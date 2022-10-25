using System.Numerics;
using FluentAssertions;
using InfiniteEnumFlags;
using Xunit;

namespace InfiniteEnumFlagsTests;

public class EnumItemTest
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(16)]
    [InlineData(25)]
    [InlineData(31)]
    public void ToBitArray_Under32Items_ShouldReturnSameInt(int index)
    {
        // Arrange
        var x = new EnumItem(index + 1, index + 1);
        var expected = (int)Math.Pow(2, index);

        // Act
        var bitArray = x.ToBitArray();
        var array = new int[1];
        bitArray.CopyTo(array, 0);
        var actual = array[0];

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(25)]
    [InlineData(31)]
    public void ToBytes_Under32Items_ShouldReturnSameBytes(int index)
    {
        // Arrange
        var x = new EnumItem(index + 1, index + 1);
        var integerValue = (int)Math.Pow(2, index);
        var expected = BitConverter.GetBytes(integerValue);

        // Act
        var actual = x.ToBytes();

        // Assert
        expected.Should().ContainInOrder(actual);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(25)]
    [InlineData(31)]
    [InlineData(129)]
    [InlineData(257)]
    [InlineData(516)]
    [InlineData(1024)]
    [InlineData(81205)]
    // [InlineData(9120595)]
    public void ToHexString_Under32Items_ShouldReturnSameHex(int index)
    {
        // Arrange
        var x = new EnumItem(index + 1, index + 1);
        var integerValue = BigInteger.Pow(2, index);
        var expected = integerValue.ToString("X")
            .TrimStart('0');

        // Act
        var actual = x.ToHexString()
            .TrimStart('0');

        // Assert
        actual.Should().Be(expected);
        actual.Length.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(5, 6)]
    [InlineData(8, 4)]
    [InlineData(11, 150)]
    [InlineData(10, 10)]
    [InlineData(3, 4)]
    [InlineData(4, 3)]
    public void TwoEnumItemWithDifferentLengthShouldBeEqual(int firstLength, int secondLength)
    {
        // Arrange
        var valueIndex = Math.Min(firstLength, secondLength) - 2;
        var e1 = new EnumItem(valueIndex, firstLength);
        var e2 = new EnumItem(valueIndex, secondLength);

        // Assert
        (e1 == e2).Should().BeTrue();
        e1.Equals(e2).Should().BeTrue();
    }

}