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
        var x = new TestClass();
        var field = x.GetType().GetField("TOTAL_ITEMS");

        // Assert
        field.Should().NotBeNull();
        field!.GetValue(x).Should().Be(3);
    }
}

public partial class TestClass : IArrayFlags
{
    public string[] Items() => new[]
    {
        "F1",
        "F2",
        "F3"
    };
}
