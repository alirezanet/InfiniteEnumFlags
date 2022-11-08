using System.Numerics;
using FluentAssertions;
using InfiniteEnumFlags;
using InfiniteEnumFlagsTests.Enums;
using Xunit;

namespace InfiniteEnumFlagsTests;

public class FlagTest
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(16)]
    [InlineData(25)]
    [InlineData(30)]
    public void ToBitArray_Under32Items_ShouldReturnSameInt(int index)
    {
        // Arrange
        var x = new Flag(index);
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
    [InlineData(30)]
    public void ToBytes_Under32Items_ShouldReturnSameBytes(int index)
    {
        // Arrange
        var x = new Flag(index);
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
    public void ToBase64_Under32Items_ShouldReturnSameHex(int index)
    {
        // Arrange
        var x = new Flag(index);
        var integerValue = BigInteger.Pow(2, index);
        var expected = Convert.ToBase64String(integerValue.ToByteArray());

        // Act
        var actual = x.ToBase64String();

        // Assert
        expected.Should().Be(actual);
        actual.Length.Should().BeGreaterThan(0);

        x.Should().Be(Flag.FromBase64(actual));
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
        var e1 = new Flag(valueIndex, firstLength);
        var e2 = new Flag(valueIndex, secondLength);

        // Assert
        (e1 == e2).Should().BeTrue();
        e1.Equals(e2).Should().BeTrue();
    }

    [Fact]
    public void TwoNoneShouldBeEqual()
    {
        // Arrange
        var e1 = new Flag(-1, 3);
        var e2 = new Flag(-1, 5);

        // Assert
        (e1 == e2).Should().BeTrue();
        e1.Equals(e2).Should().BeTrue();
    }

    [Fact]
    public void FromBase64_SameLength_MustHaveEqualBase64String()
    {
        // Arrange
        var flags = TestEnum.F2 | TestEnum.F7 | TestEnum.F8;
        var hex = flags.ToBase64Trimmed();

        // Act
        var newFlags = Flag<TestEnum>.FromBase64(hex);

        // Assert
        flags.Should().Be(newFlags);
    }

    [Fact]
    public void FromBase64_WithDifferentLength_MustHaveEqualItems()
    {
        // Arrange
        var e1 = new Flag(5, 10);
        var base1 = e1.ToBase64String();

        var e2 = new Flag(5, 50);
        var base2 = e2.ToBase64String();

        // Act
        var newEnum1 = Flag.FromBase64(base1);
        var newEnum2 = Flag.FromBase64(base2);

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
    public void ToBase64Trimmed_WithDifferentLength_MustHaveEqualKeyAndItems(int firstLength, int secondLength,
        int index)
    {
        // Arrange
        var e1 = new Flag(index, firstLength);
        var key1 = e1.ToBase64Trimmed();

        var e2 = new Flag(index, secondLength);
        var key2 = e2.ToBase64Trimmed();

        // Act
        var newEnum1 = Flag.FromBase64(key1);
        var newEnum2 = Flag.FromBase64(key2);

        // Assert
        key1.Should().Be(key2);
        newEnum1.ToBase64Trimmed().Should().Be(key1);

        newEnum1.Should().Be(e1);
        newEnum1.Should().Be(e2);

        newEnum2.Should().Be(e1);
        newEnum2.Should().Be(e2);
        newEnum1.Should().Be(newEnum2);
    }

    [Fact]
    public void ToUniqueId_FromUniqueId_WithoutSalt_ShouldBeGiveOriginalFlag()
    {
        var features = new Flag(125) | new Flag(122) | new Flag(10);
        var id = features.ToUniqueId();
        var new_features = Flag.FromUniqueId(id);

        features.Should().Be(new_features);
    }

    [Fact]
    public void ToUniqueId_FromUniqueId_WithSalt_ShouldBeGiveOriginalFlag()
    {
        const string salt = "salt";
        var features = new Flag(15) | new Flag(12) | new Flag(110);
        var id = features.ToUniqueId(salt);
        var new_features = Flag.FromUniqueId(id, salt);

        features.Should().Be(new_features);
    }
}