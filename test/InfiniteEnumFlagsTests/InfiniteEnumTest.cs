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

    [Fact]
    public void GetNames_Should_Return_EnumNames()
    {
        // Act
        var items = TestEnum.GetNames().ToList();

        // Assert
        items.Count().Should().Be(9);
        items.First().Should().Be("None");
    }

    [Fact]
    public void GetNames_WithCustomFlags_Should_Return_EnumNames()
    {
        // Arrange
        var features = TestEnum.F2 | TestEnum.F4;

        // Act
        var names = TestEnum.GetNames(features).ToList();

        // Assert
        names.Should().Contain("F2");
        names.Should().Contain("F4");
        names.Should().NotContain("F3");
    }

    [Fact]
    public void GetKeyValues_ShouldReturnNameAndValues()
    {
        // Act
        var items = TestEnum.GetKeyValues();

        // Assert
        items.Count.Should().Be(9);

        var first = items.First();
        first.Key.Should().Be("None");
        first.Value.Should().Be(TestEnum.None);

        var last = items.Last();
        last.Key.Should().Be("F8");
        last.Value.Should().Be(TestEnum.F8);
    }
}