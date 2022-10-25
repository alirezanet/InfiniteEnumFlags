namespace InfiniteEnumFlags;

public static class Extensions
{
    public static EnumItem SetFlag(this EnumItem a, EnumItem b)
    {
        return a | b;
    }

    public static EnumItem UnsetFlag(this EnumItem a, EnumItem b)
    {
        return a & (~b);
    }

    public static bool HasFlag(this EnumItem a, EnumItem b)
    {
        return (a & b) == b;
    }

    public static EnumItem ToggleFlag(this EnumItem a, EnumItem b)
    {
        return a ^ b;
    }
}