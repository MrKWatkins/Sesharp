namespace MrKWatkins.Sesharp;

public static class EnumerableExtensions
{
    [Pure]
    public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        var index = 0;
        foreach (var item in source)
        {
            if (predicate.Invoke(item))
            {
                return index;
            }

            index++;
        }
        return -1;
    }
}