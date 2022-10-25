using InfiniteEnumFlags;

namespace InfiniteEnumFlagsTests.Enums;

public partial class TestIndexDictionaryFlags : IIndexDictionaryFlags
{
    public Dictionary<string, int> Items() => new()
    {
        { "K2", 0 },
        { "K3", 2 },
        { "K1", 1 }
    };
}