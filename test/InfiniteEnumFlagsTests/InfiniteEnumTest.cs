using FluentAssertions;
using InfiniteEnumFlags;
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
    public void AllExcept_Should_Return_AllDeclaredFlagsMinusGiven()
    {
        // Arrange
        var removed = TestEnum.F2 | TestEnum.F5;
        var expected = TestEnum.F1 | TestEnum.F3 | TestEnum.F4 |
                       TestEnum.F6 | TestEnum.F7 | TestEnum.F8;

        // Act
        var actual = TestEnum.AllExcept(removed);

        // Assert
        actual.Should().Be(expected);
        actual.HasFlag(removed).Should().BeFalse();
    }

    [Fact]
    public void AllExcept_None_Should_Equal_All()
    {
        TestEnum.AllExcept(TestEnum.None).Should().Be(TestEnum.All);
    }

    [Fact]
    public void AllExcept_All_Should_Equal_None()
    {
        TestEnum.AllExcept(TestEnum.All).Should().Be(TestEnum.None);
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
        names.Should().NotContain("None");
        names.Should().NotContain("F3");
    }

    [Fact]
    public void GetNames_WithNone_ShouldReturnNone()
    {
        // Act
        var names = TestEnum.GetNames(TestEnum.None).ToList();

        // Assert
        names.Should().ContainSingle().Which.Should().Be("None");
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

    [Fact]
    public void FromName_ShouldReturnFlagWhenNameExists()
    {
        // Act
        var flag = TestEnum.FromName("F3");

        // Assert
        flag.Should().Be(TestEnum.F3);
    }

    [Fact]
    public void FromName_ShouldReturnNullWhenNameDoesNotExist()
    {
        // Act
        var flag = TestEnum.FromName("Missing");

        // Assert
        flag.Should().BeNull();
    }

    [Fact]
    public void TryFromName_ShouldReturnFalseWhenNameDoesNotExist()
    {
        // Act
        var result = TestEnum.TryFromName("Missing", out var flag);

        // Assert
        result.Should().BeFalse();
        flag.Should().Be(TestEnum.None);
    }

    [Fact]
    public void FromNames_ShouldCombineKnownFlags()
    {
        // Act
        var flags = TestEnum.FromNames("F2", "F4", "F8");

        // Assert
        flags.Should().Be(TestEnum.F2 | TestEnum.F4 | TestEnum.F8);
    }

    [Fact]
    public void FromNames_WithUnknownName_ShouldThrow()
    {
        // Act
        var action = () => TestEnum.FromNames("F2", "Missing");

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryFromNames_WithUnknownName_ShouldReturnFalse()
    {
        // Act
        var result = TestEnum.TryFromNames(new[] { "F2", "Missing" }, out var flags);

        // Assert
        result.Should().BeFalse();
        flags.Should().Be(TestEnum.None);
    }
}
