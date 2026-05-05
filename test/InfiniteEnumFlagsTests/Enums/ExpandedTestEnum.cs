using InfiniteEnumFlags;

namespace InfiniteEnumFlagsTests.Enums;

/// <summary>
/// Simulates TestEnum after a future expansion: same indices for all original
/// flags, new flags added at indices 8..11. Used to verify backward compatibility
/// of stored IDs across enum evolution.
/// </summary>
public sealed class ExpandedTestEnum : InfiniteEnum<ExpandedTestEnum>
{
    // --- original flags: identical indices to TestEnum (simulates "before") ---
    public static readonly Flag<ExpandedTestEnum> None = new(-1);
    public static readonly Flag<ExpandedTestEnum> F1 = new(0);
    public static readonly Flag<ExpandedTestEnum> F2 = new(1);
    public static readonly Flag<ExpandedTestEnum> F3 = new(2);
    public static readonly Flag<ExpandedTestEnum> F4 = new(3);
    public static readonly Flag<ExpandedTestEnum> F5 = new(4);
    public static readonly Flag<ExpandedTestEnum> F6 = new(5);
    public static readonly Flag<ExpandedTestEnum> F7 = new(6);
    public static readonly Flag<ExpandedTestEnum> F8 = new(7);

    // --- newly added flags (simulates "after") ---
    public static readonly Flag<ExpandedTestEnum> F9 = new(8);
    public static readonly Flag<ExpandedTestEnum> F10 = new(9);
    public static readonly Flag<ExpandedTestEnum> F11 = new(10);
    public static readonly Flag<ExpandedTestEnum> F12 = new(11);
}
