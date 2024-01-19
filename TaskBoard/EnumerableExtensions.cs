namespace TaskBoard;

public static class EnumerableExtensions
{
    public static T[] AsArray<T>(this IEnumerable<T> src) => src as T[] ?? src.ToArray();
}