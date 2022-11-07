namespace InfiniteEnumFlags;

public static class Extensions
{
    public static bool HasFlag<T>(this Flag<T> a, Flag<T> b)
    {
        return (a & b) == b;
    }

    public static Flag<T> SetFlag<T>(this Flag<T> a, params Flag<T>[] b)
    {
        return b.Aggregate(a, (current, item) => current | item);
    }

    public static Flag<T> UnsetFlag<T>(this Flag<T> a, params Flag<T>[] b)
    {
        return b.Aggregate(a, (current, item) => current & ~item);
    }

    public static Flag<T> ToggleFlag<T>(this Flag<T> a, params Flag<T>[] b)
    {
        return b.Aggregate(a, (current, item) => current ^ item);
    }

}