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
        Enums.TestArrayFlags.All.HasFlag(Enums.TestArrayFlags.F3).Should().BeTrue();
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
}

