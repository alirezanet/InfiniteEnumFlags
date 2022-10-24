using FluentAssertions;
using InfiniteEnumFlags;
using Xunit;

namespace InfiniteEnumFlagsTests;

public class GeneratorTests
{
    [Fact]
    public void TestClass_MustHave_TotalItems_WithValueThree()
    {
        // Arrange
        var x = new TestArrayFlags();
        var field = x.GetType().GetField("TOTAL_ITEMS");

        // Assert
        field.Should().NotBeNull();
        field!.GetValue(x).Should().Be(3);
    }
}

public partial class TestArrayFlags : IArrayFlags
{
    public string[] Items() => new[]
    {
        "F1",
        "F2",
        "F3"
    };
}
public partial class TestArrayFlagsWithoutItems : IArrayFlags
{
    public string[] Items() => throw new Exception("OOPS");
}

public partial class TestIndexDictionaryFlags : IIndexDictionaryFlags
{
    public Dictionary<string, int> Items() => new()
    {
        { "K2", 0 },
        { "K3", 2 },
        { "K1", 1 }
    };
}