using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;

namespace TaskBoard;

public class MostRecentEventIdentityGrouper<TEvent, TPastEvent> : IAggregateGrouper<Guid>
    where TEvent : notnull
    where TPastEvent : notnull
{
    public MostRecentEventIdentityGrouper(Func<TEvent, Guid> eventIdMapping, Func<TPastEvent, Guid> pastEventJoinId, Func<TPastEvent, Guid> pastEventTargetId)
    {
        EventIdMapping = eventIdMapping;
        PastEventJoinId = pastEventJoinId;
        PastEventTargetId = pastEventTargetId;
    }

    private Func<TEvent, Guid> EventIdMapping { get; }

    private Func<TPastEvent, Guid> PastEventJoinId { get; }
    
    private Func<TPastEvent, Guid> PastEventTargetId { get; }

    public async Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<Guid> grouping)
    {
        var pastEventTypeName = EventMappingExtensions.GetEventTypeName<TPastEvent>();

        var targetEvents = events.OfType<IEvent<TEvent>>().ToArray();

        if (targetEvents.Length == 0) return;
        
        var targetEventIds = targetEvents.Select(e => EventIdMapping(e.Data)).Distinct();
        
        var pastEvents = (await session.Events.QueryAllRawEvents()
                .Where(e => e.EventTypeName == pastEventTypeName)
                .ToListAsync())
            .Cast<IEvent<TPastEvent>>()
            .Where(e => targetEventIds.Contains(PastEventJoinId(e.Data)))
            .Select(e => (e.Sequence, e.Timestamp, e.Data))
            .GroupBy(p => PastEventJoinId(p.Data))
            .ToDictionary(p => p.Key);

        foreach (var @event in targetEvents)
        {
            var eventId = EventIdMapping(@event.Data);
            var pastEvent = pastEvents[eventId].LastOrDefault(e => e.Sequence < @event.Sequence).Data;
            if (pastEvent is null) continue;
            var pastEventId = PastEventTargetId(pastEvent);
            
            grouping.AddEvent(pastEventId, @event);
        }
    }
}