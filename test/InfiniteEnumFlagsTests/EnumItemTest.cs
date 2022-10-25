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
    [InlineData(15)] // AIAAAA== - 32768
    [InlineData(16)]
    [InlineData(25)]
    [InlineData(31)]
    [InlineData(47)]
    [InlineData(48)]
    [InlineData(129)]
    [InlineData(257)]
    [InlineData(516)]
    [InlineData(1024)]
    [InlineData(81205)]
    // [InlineData(9120595)]
    public void ToHexString_Under32Items_ShouldReturnSameHex(int index)
    {
        // Arrange
        var x = new EnumItem(index + 1, index + 2);
        var integerValue = BigInteger.Pow(2, index);
        var expected = Convert.ToBase64String(integerValue.ToByteArray());

        // Act
        var actual = x.ToBase64String();

        // Assert
        actual.Should().Be(expected, x.ToString());
        actual.Length.Should().BeGreaterThan(0);

        x.Should().Be(EnumItem.FromBase64(actual));
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

    [Fact]
    public void TwoNoneShouldBeEqual()
    {
        // Arrange
        var e1 = new EnumItem(0, 3);
        var e2 = new EnumItem(0, 3);

        // Assert
        (e1 == e2).Should().BeTrue();
        e1.Equals(e2).Should().BeTrue();
        e1.Should().Be(Enums.TestArrayFlags.None);
    }

    [Fact]
    public void FromBase64String_SameLength_MustHaveEqualBase64String()
    {
        // Arrange
        var flags = Enums.TestArrayFlags.F2 | Enums.TestArrayFlags.F129 | Enums.TestArrayFlags.F172;
        var hex = flags.ToBase64String();

        // Act
        var newFlags = EnumItem.FromBase64(hex);

        // Assert
        flags.Should().Be(newFlags);
    }

    [Fact]
    public void FromBase64String_WithDifferentLength_MustHaveEqualItems()
    {
        // Arrange
        var e1 = new EnumItem(5, 10);
        var base1 = e1.ToBase64String();

        var e2 = new EnumItem(5, 50);
        var base2 = e2.ToBase64String();

        // Act
        var newEnum1 = EnumItem.FromBase64(base1);
        var newEnum2 = EnumItem.FromBase64(base2);

        // Assert
        newEnum1.Should().Be(e1);
        newEnum1.Should().Be(e2);

        newEnum2.Should().Be(e1);
        newEnum2.Should().Be(e2);
        newEnum1.Should().Be(newEnum2);

        base1.Should().NotBe(base2);
    }

    [Theory]
    [InlineData(5, 6, 3)]
    [InlineData(2, 3, 1)]
    [InlineData(11, 150, 8)]
    [InlineData(10, 10, 9)]
    [InlineData(3, 4, 2)]
    [InlineData(4, 3, 2)]
    [InlineData(1200, 380, 253)]
    public void ToBase64Key_WithDifferentLength_MustHaveEqualKeyAndItems(int firstLength, int secondLength, int index)
    {
        // Arrange
        var e1 = new EnumItem(index, firstLength);
        var key1 = e1.ToBase64Key();

        var e2 = new EnumItem(index, secondLength);
        var key2 = e2.ToBase64Key();

        // Act
        var newEnum1 = EnumItem.FromBase64(key1);
        var newEnum2 = EnumItem.FromBase64(key2);

        // Assert
        key1.Should().Be(key2);
        newEnum1.ToBase64Key().Should().Be(key1);

        newEnum1.Should().Be(e1);
        newEnum1.Should().Be(e2);

        newEnum2.Should().Be(e1);
        newEnum2.Should().Be(e2);
        newEnum1.Should().Be(newEnum2);

    }


}