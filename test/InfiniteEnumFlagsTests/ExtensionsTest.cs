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
        var flags = Enums.TestEnum.F1 | Enums.TestEnum.F2;

        // Assert
        flags.HasFlag(Enums.TestEnum.F2).Should().BeTrue();
        flags.HasFlag(Enums.TestEnum.F1).Should().BeTrue();
        (Enums.TestEnum.F1 | Enums.TestEnum.F3 | Enums.TestEnum.F2)
            .HasFlag(Enums.TestEnum.F3)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void HasFlag_FalseCondition()
    {
        // Arrange
        var flags = Enums.TestEnum.F1 | Enums.TestEnum.F2;

        // Assert
        flags.HasFlag(Enums.TestEnum.F3).Should().BeFalse();
        Enums.TestEnum.None.HasFlag(Enums.TestEnum.F3).Should().BeFalse();
    }

    [Fact]
    public void HasFlag_MultipleFeatures()
    {
        // Arrange
        var flag = Enums.TestEnum.F1;
        var features = Enums.TestEnum.F5 | Enums.TestEnum.F1 | Enums.TestEnum.F2;

        // Act
        var result = flag.HasFlag(features);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ToggleFlag()
    {
        // Arrange
        var flags = Enums.TestEnum.F1 | Enums.TestEnum.F2;

        // Act
        var actual = flags.ToggleFlag(Enums.TestEnum.F1, Enums.TestEnum.F3);

        // Assert
        actual.Should().Be(Enums.TestEnum.F2 | Enums.TestEnum.F3);
        actual.Should().NotBe(Enums.TestEnum.F1);
        actual.Should().NotBe(Enums.TestEnum.None);
    }

    [Fact]
    public void SetFlag()
    {
        // Arrange
        var flags = Enums.TestEnum.F1;

        // Act
        var actual = flags.SetFlag(Enums.TestEnum.F3);

        // Assert
        actual.HasFlag(Enums.TestEnum.F3);
    }

    [Fact]
    public void UnSetFlag()
    {
        // Arrange
        var flags = Enums.TestEnum.F1 | Enums.TestEnum.F2;

        // Act
        var actual = flags.UnsetFlag(Enums.TestEnum.F1, Enums.TestEnum.F3);

        // Assert
        actual.Should().Be(Enums.TestEnum.F2);
        actual.Should().NotBe(Enums.TestEnum.F1);
        actual.Should().NotBe(Enums.TestEnum.None);
    }
}