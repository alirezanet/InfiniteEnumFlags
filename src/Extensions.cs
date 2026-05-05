namespace InfiniteEnumFlags;

/// <summary>Bitwise helper extensions for <see cref="Flag{T}"/>.</summary>
public static class Extensions
{
    /// <summary>Returns <c>true</c> when <paramref name="a"/> has any bit in common with <paramref name="b"/>.</summary>
    public static bool HasFlag<T>(this Flag<T> a, Flag<T> b)
    {
        return !(a & b).IsEmpty;
    }

    /// <summary>Returns <c>true</c> when every bit set in <paramref name="b"/> is also set in <paramref name="a"/>.
    /// Always returns <c>true</c> when <paramref name="b"/> is empty.</summary>
    public static bool HasAllFlags<T>(this Flag<T> a, Flag<T> b)
    {
        return (a & b) == b;
    }

    /// <summary>Returns a new flag with all bits from <paramref name="a"/> plus every flag in <paramref name="b"/> set.</summary>
    public static Flag<T> SetFlag<T>(this Flag<T> a, params Flag<T>[] b)
    {
        return b.Aggregate(a, (current, item) => current | item);
    }

    /// <summary>Returns a new flag with every bit in <paramref name="b"/> cleared from <paramref name="a"/>.</summary>
    public static Flag<T> UnsetFlag<T>(this Flag<T> a, params Flag<T>[] b)
    {
        return b.Aggregate(a, (current, item) => current & ~item);
    }

    /// <summary>Returns a new flag with bits in <paramref name="b"/> toggled in <paramref name="a"/>: set bits become unset and vice versa.</summary>
    public static Flag<T> ToggleFlag<T>(this Flag<T> a, params Flag<T>[] b)
    {
        return b.Aggregate(a, (current, item) => current ^ item);
    }
}
