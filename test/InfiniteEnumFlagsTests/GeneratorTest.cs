using FluentAssertions;
using Xunit;

namespace InfiniteEnumFlagsTests;

public class GeneratorTests
{
    [Fact]
    public void TestArrayFlags_MustHave_TotalItems_WithValue179()
    {
        // Arrange
        var x = new Enums.TestArrayFlags();
        var field = x.GetType().GetField("TOTAL_ITEMS");

        // Assert
        field.Should().NotBeNull();
        field!.GetValue(x).Should().Be(179);
    }

    [Fact]
    public void PrivateEnums_MustHave_TotalItems_WithValue3()
    {
        // Arrange
        var x = new Enums.Features();

        // Act

        // Assert
    }
}