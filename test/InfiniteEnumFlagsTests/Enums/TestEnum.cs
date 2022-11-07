using InfiniteEnumFlags;

namespace InfiniteEnumFlagsTests.Enums;

public sealed class TestEnum : InfiniteEnum<TestEnum>
{
    public static readonly Flag<TestEnum> None = new(-1);
    public static readonly Flag<TestEnum> F1 = new(0);
    public static readonly Flag<TestEnum> F2 = new(1);
    public static readonly Flag<TestEnum> F3 = new(2);
    public static readonly Flag<TestEnum> F4 = new(3);
    public static readonly Flag<TestEnum> F5 = new(4);
    public static readonly Flag<TestEnum> F6 = new(5);
    public static readonly Flag<TestEnum> F7 = new(6);
    public static readonly Flag<TestEnum> F8 = new(7);
}