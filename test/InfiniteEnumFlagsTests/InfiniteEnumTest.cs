using FluentAssertions;
using InfiniteEnumFlagsTests.Enums;
using Xunit;

namespace InfiniteEnumFlagsTests;

public class InfiniteEnumTest
{
    [Fact]
    public void All_Should_Return_AllFlags()
    {
        // Arrange
        var expected = TestEnum.F1 | TestEnum.F2 | TestEnum.F3 | TestEnum.F4 |
                       TestEnum.F5 | TestEnum.F6 | TestEnum.F7 | TestEnum.F8;
        // Act
        var actual = TestEnum.All;

        // Assert
        actual.Should().Be(expected);
    }
}