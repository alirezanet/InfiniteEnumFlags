namespace InfiniteEnumFlags;

public static class Extensions
{
    public static bool HasFlag(this EnumItem source, EnumItem flag)
    {
        return (source & flag) != new EnumItem(0, source.Bits.Length);
    }
}