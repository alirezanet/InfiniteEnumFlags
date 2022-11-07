using FluentAssertions;
using InfiniteEnumFlags;
using Xunit;

namespace InfiniteEnumFlagsTests;

public class ExtensionsTest
{
    [Fact]
    public void HasFlag_TrueCondition()
    {
        // Arrange
        var flags = Enums.TestArrayFlags.F1 | Enums.TestArrayFlags.F2;

        // Assert
        flags.HasFlag(Enums.TestArrayFlags.F2).Should().BeTrue();
        flags.HasFlag(Enums.TestArrayFlags.F1).Should().BeTrue();
        (Enums.TestArrayFlags.F1 | Enums.TestArrayFlags.F3 | Enums.TestArrayFlags.F2)
            .HasFlag(Enums.TestArrayFlags.F3)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void HasFlag_FalseCondition()
    {
        // Arrange
        var flags = Enums.TestArrayFlags.F1 | Enums.TestArrayFlags.F2;

        // Assert
        flags.HasFlag(Enums.TestArrayFlags.F3).Should().BeFalse();
        Enums.TestArrayFlags.None.HasFlag(Enums.TestArrayFlags.F3).Should().BeFalse();
    }

    [Fact]
    public void ToggleFlag()
    {
        // Arrange
        var flags = Enums.TestArrayFlags.F1 | Enums.TestArrayFlags.F2;

        // Act
        var actual = flags.ToggleFlag(Enums.TestArrayFlags.F1, Enums.TestArrayFlags.F3);

        // Assert
        actual.Should().Be(Enums.TestArrayFlags.F2 | Enums.TestArrayFlags.F3);
        actual.Should().NotBe(Enums.TestArrayFlags.F1);
        actual.Should().NotBe(Enums.TestArrayFlags.None);
    }

    [Fact]
    public void SetFlag()
    {
        // Arrange
        var flags = Enums.TestArrayFlags.F1;

        // Act
        var actual = flags.SetFlag(Enums.TestArrayFlags.F3);

        // Assert
        actual.HasFlag(Enums.TestArrayFlags.F3);
    }

    [Fact]
    public void UnSetFlag()
    {
        // Arrange
        var flags = Enums.TestArrayFlags.F1 | Enums.TestArrayFlags.F2;

        // Act
        var actual = flags.UnsetFlag(Enums.TestArrayFlags.F1, Enums.TestArrayFlags.F3);

        // Assert
        actual.Should().Be(Enums.TestArrayFlags.F2);
        actual.Should().NotBe(Enums.TestArrayFlags.F1);
        actual.Should().NotBe(Enums.TestArrayFlags.None);
    }
}