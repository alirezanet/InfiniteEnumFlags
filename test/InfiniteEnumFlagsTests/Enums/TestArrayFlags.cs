using InfiniteEnumFlags;

namespace InfiniteEnumFlagsTests.Enums;

public partial class TestArrayFlags : IArrayFlags
{
    public string[] Items() => new[]
    {
        "F1",
        "F2",
        "F3"
    };
}