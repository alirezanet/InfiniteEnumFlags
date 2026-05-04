using System.Collections;
using System.IO.Compression;
using System.Text;

namespace InfiniteEnumFlags;

public class Flag<T> : IEquatable<Flag<T>>
{
    internal readonly BitArray Bits;
    public int Length => Bits.Length;
    public bool IsEmpty
    {
        get
        {
            for (var i = 0; i < Bits.Length; i++)
                if (Bits[i])
                    return false;

            return true;
        }
    }

    public int Count
    {
        get
        {
            var count = 0;
            for (var i = 0; i < Bits.Length; i++)
                if (Bits[i])
                    count++;

            return count;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index">minus one (-1) is reserved for empty bits / None.</param>
    /// <param name="length">Number of total required bits</param>
    public Flag(int index, int? length = null)
    {
        if (index < -1)
            throw new ArgumentOutOfRangeException(nameof(index), "Flag index must be -1 or greater.");

        index++;
        length ??= index + 1;

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be zero or greater.");

        if (length < index)
            throw new ArgumentOutOfRangeException(
                nameof(length),
                "Length must be large enough to contain the flag index.");

        // None
        if (index == 0)
        {
            Bits = new BitArray(length.Value, false);
            return;
        }

        // Items
        Bits = new BitArray(length.Value, false);
        Bits.Set(index - 1, true);
    }

    public Flag(BitArray new_value)
    {
        Bits = (BitArray)new_value.Clone();
    }

    public Flag(byte[] new_value)
    {
        Bits = new BitArray(new_value);
    }

    public Flag()
    {
        Bits = new BitArray(1, false);
    }

    public static explicit operator Flag(Flag<T> item)
    {
        return new Flag(item.Bits);
    }

    public static explicit operator Flag<T>(Flag item)
    {
        return new Flag<T>(item.Bits);
    }

    public static bool operator ==(Flag<T>? item, Flag<T>? item2)
    {
        return item is null ? item2 is null : item.Equals(item2);
    }

    public static bool operator !=(Flag<T>? item, Flag<T>? item2)
    {
        return !(item == item2);
    }

    public static Flag<T> operator |(Flag<T> left, Flag<T> right)
    {
        if (left is null) throw new ArgumentNullException(nameof(left));
        if (right is null) throw new ArgumentNullException(nameof(right));

        var (nLeft, nRight) = FixLength(left, right);
        return new Flag<T>(nLeft.Or(nRight));
    }

    private static (BitArray nLeft, BitArray nRight) FixLength(Flag<T> left, Flag<T> right)
    {
        var length = Math.Max(left.Bits.Length, right.Bits.Length);
        var nLeft = (BitArray)left.Bits.Clone();
        nLeft.Length = length;

        var nRight = (BitArray)right.Bits.Clone();
        nRight.Length = length;

        return (nLeft, nRight);
    }

    public static Flag<T> operator &(Flag<T> left, Flag<T> right)
    {
        if (left is null) throw new ArgumentNullException(nameof(left));
        if (right is null) throw new ArgumentNullException(nameof(right));

        var (nLeft, nRight) = FixLength(left, right);
        return new Flag<T>(nLeft.And(nRight));
    }

    public static Flag<T> operator ^(Flag<T> left, Flag<T> right)
    {
        if (left is null) throw new ArgumentNullException(nameof(left));
        if (right is null) throw new ArgumentNullException(nameof(right));

        var (nLeft, nRight) = FixLength(left, right);
        return new Flag<T>(nLeft.Xor(nRight));
    }

    public static Flag<T> operator ~(Flag<T> item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));

        var x = (BitArray)item.Bits.Clone();
        return new Flag<T>(x.Not());
    }

    public override string ToString() => ToUniqueId();

    public string ToBinaryString()
    {
        var chars = new char[Bits.Count];

        for (var i = 0; i < Bits.Count; i++)
            chars[i] = Bits[i] ? '1' : '0';

        return new string(chars);
    }

    public string ToUniqueId() => ToUniqueId(null);

    public string ToUniqueId(string? salt)
    {
        var data = ToBytes();
        using var compressedStream = new MemoryStream();
        using var zipStream = new DeflateStream(compressedStream, CompressionLevel.Fastest);
        zipStream.Write(data, 0, data.Length);
        if (salt is not null)
        {
            var saltBytes = Encoding.UTF8.GetBytes(salt);
            zipStream.Write(saltBytes);
        }
        zipStream.Close();
        return Convert.ToBase64String(compressedStream.ToArray());
    }

    public string ToBase64Trimmed()
    {
        return Convert.ToBase64String(GetNormalizedBytes());
    }

    public string ToBase64String()
    {
        var bytes = ToBytes();
        return Convert.ToBase64String(bytes);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Flag<T>);
    }

    public bool Equals(Flag<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return GetNormalizedBytes().SequenceEqual(other.GetNormalizedBytes());
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach (var value in GetNormalizedBytes())
            hashCode.Add(value);

        return hashCode.ToHashCode();
    }

    public static Flag<T> FromBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        return new Flag<T>(bytes);
    }

    public static Flag<T> FromUniqueId(string id) => FromUniqueId(id, null);
    public static Flag<T> FromUniqueId(string id, string? salt)
    {
        var data = Convert.FromBase64String(id);
        using var compressedStream = new MemoryStream(data);
        using var outputStream = new MemoryStream();
        using var zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        zipStream.CopyTo(outputStream);
        zipStream.Close();

        var bytes = outputStream.ToArray();
        if (salt is null) return new Flag<T>(bytes);

        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var saltIndex = bytes.Length - saltBytes.Length;
        if (saltIndex < 0)
            throw new InvalidOperationException("salt is not valid");

        var actualSalt = bytes.AsSpan(saltIndex);
        if (!actualSalt.SequenceEqual(saltBytes))
            throw new InvalidOperationException("salt is not valid");

        return new Flag<T>(bytes.AsSpan(0, saltIndex).ToArray());
    }

    public byte[] ToBytes()
    {
        var ret = new byte[(Bits.Length - 1) / 8 + 1];
        Bits.CopyTo(ret, 0);
        return ret;
    }

    /// <inheritdoc cref="BitArray.CopyTo" />
    public void CopyTo(Array array, int index = 0)
    {
        Bits.CopyTo(array, index);
    }

    public BitArray ToBitArray()
    {
        return (Bits.Clone() as BitArray)!;
    }

    private byte[] GetNormalizedBytes()
    {
        var bytes = ToBytes();
        var length = bytes.Length;

        while (length > 0 && bytes[length - 1] == 0)
            length--;

        if (length == bytes.Length) return bytes;
        return bytes[..length];
    }
}
