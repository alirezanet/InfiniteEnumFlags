using System.Collections;
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
    public void EnumItem_ToBitArray_Under32Values_ShouldReturnSameInt(int index)
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
    public void EnumItem_ToBytes_Under32Values_ShouldReturnSameBytes(int index)
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



}