using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;

namespace TaskBoard;

public class MostRecentEventIdentityGrouper<TEvent, TPastEvent>(
    Func<IEvent<TEvent>, Guid> currentEventJoinId,
    Func<IEvent<TPastEvent>, Guid> pastEventJoinId,
    Func<TPastEvent, Guid> pastEventSelectionId)
    : IAggregateGrouper<Guid>
    where TEvent : notnull
    where TPastEvent : notnull
{
    private static Guid FetchEventStreamId(IEvent evt) => evt.StreamId;

    public MostRecentEventIdentityGrouper(Func<TPastEvent, Guid> pastEventSelectionId) : 
        this(FetchEventStreamId, FetchEventStreamId, pastEventSelectionId)
    {
        JoinEventsByStreamId = true;
    }

    private bool JoinEventsByStreamId { get; }
    
    private Func<IEvent<TEvent>, Guid> CurrentEventJoinId { get; } = currentEventJoinId;

    private Func<IEvent<TPastEvent>, Guid> PastEventJoinId { get; } = pastEventJoinId;

    private Func<TPastEvent, Guid> PastEventSelectionId { get; } = pastEventSelectionId;

    public async Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<Guid> grouping)
    {
        var pastEventTypeName = EventMappingExtensions.GetEventTypeName<TPastEvent>();

        var targetEvents = events.OfType<IEvent<TEvent>>().ToArray();

        if (targetEvents.Length == 0) return;
        
        var targetEventIds = targetEvents.Select(e => CurrentEventJoinId(e)).Distinct().ToList();

        var pastEventsQuery = session.Events.QueryAllRawEvents().Where(e => e.EventTypeName == pastEventTypeName);
        if (JoinEventsByStreamId)
            pastEventsQuery = pastEventsQuery.Where(e => targetEventIds.Contains(e.StreamId));

        var pastEventsUnfiltered = (await pastEventsQuery.ToListAsync()).Cast<IEvent<TPastEvent>>();
        
        var pastEvents = pastEventsUnfiltered
            .Where(e => targetEventIds.Contains(PastEventJoinId(e)))
            .Select(e => (PastEventJoinId(e), e.Sequence, e.Data))
            .GroupBy(p => p.Item1)
            .ToDictionary(p => p.Key);

        foreach (var @event in targetEvents)
        {
            var eventId = CurrentEventJoinId(@event);
            var pastEvent = pastEvents[eventId].LastOrDefault(e => e.Sequence < @event.Sequence).Data;
            if (pastEvent is null) continue;
            var pastEventId = PastEventSelectionId(pastEvent);
            
            grouping.AddEvent(pastEventId, @event);
        }
    }
}