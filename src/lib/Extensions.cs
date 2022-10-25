namespace InfiniteEnumFlags;

public static class Extensions
{
    public static bool HasFlag(this EnumItem a, EnumItem b)
    {
        return (a & b) == b;
    }

    public static EnumItem SetFlag(this EnumItem a, params EnumItem[] b)
    {
        return b.Aggregate(a, (current, item) => current | item);
    }

    public static EnumItem UnsetFlag(this EnumItem a, params EnumItem[] b)
    {
        return b.Aggregate(a, (current, item) => current & ~item);
    }

    public static EnumItem ToggleFlag(this EnumItem a, params EnumItem[] b)
    {
        return b.Aggregate(a, (current, item) => current ^ item);
    }
}