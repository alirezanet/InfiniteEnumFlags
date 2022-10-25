using InfiniteEnumFlags;

namespace InfiniteEnumFlagsTests.Enums;

public partial class TestArrayFlagsWithoutItems : IArrayFlags
{
    public string[] Items() => throw new Exception("OOPS");
}