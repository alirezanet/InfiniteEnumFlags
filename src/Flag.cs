using System.Collections;
using System.IO.Compression;
using System.Text;

namespace InfiniteEnumFlags;

public class Flag<T>
{
    internal readonly BitArray Bits;
    public int Length => Bits.Length;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index">minus one (-1) is reserved for empty bits / None.</param>
    /// <param name="length">Number of total required bits</param>
    public Flag(int index, int? length = null)
    {
        index++;
        length ??= index + 1;

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
        Bits = new_value;
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

    public static bool operator ==(Flag<T> item, Flag<T> item2)
    {
        return item.Equals(item2);
    }

    public static bool operator !=(Flag<T> item, Flag<T> item2)
    {
        return !(item == item2);
    }

    public static Flag<T> operator |(Flag<T> left, Flag<T> right)
    {
        var (nLeft, nRight) = FixLength(left, right);
        return new Flag<T>(nLeft.Or(nRight));
    }

    private static (BitArray nLeft, BitArray nRight) FixLength(Flag<T> left, Flag<T> right)
    {
        var length = Math.Max(left.Bits.Length, right.Bits.Length);
        var nLeft = new BitArray(length, false);

        for (var i = 0; i < left.Bits.Length; i++)
            if (left.Bits[i])
                nLeft.Set(i, true);

        var nRight = new BitArray(length, false);
        for (var i = 0; i < right.Bits.Length; i++)
            if (right.Bits[i])
                nRight.Set(i, true);

        return (nLeft, nRight);
    }

    public static Flag<T> operator &(Flag<T> left, Flag<T> right)
    {
        var (nLeft, nRight) = FixLength(left, right);
        return new Flag<T>(nLeft.And(nRight));
    }

    public static Flag<T> operator ^(Flag<T> left, Flag<T> right)
    {
        var (nLeft, nRight) = FixLength(left, right);
        return new Flag<T>(nLeft.Xor(nRight));
    }

    public static Flag<T> operator ~(Flag<T> item)
    {
        var x = (BitArray)item.Bits.Clone();
        return new Flag<T>(x.Not());
    }

    public override string ToString() => ToUniqueId();

    public string ToBinaryString()
    {
        var sb = new StringBuilder();

        for (var i = 0; i < Bits.Count; i++)
        {
            var c = Bits[i] ? '1' : '0';
            sb.Append(c);
        }

        return sb.ToString();
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
        var bytes = ToBytes().AsSpan();
        var index = 0;
        for (var i = bytes.Length - 1; i >= 0; i--)
        {
            if (bytes[i] == 0) continue;
            index = i + 1;
            break;
        }

        var key = bytes[..index].ToArray();
        return Convert.ToBase64String(key);
    }

    public string ToBase64String()
    {
        var bytes = ToBytes();
        return Convert.ToBase64String(bytes);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Flag<T> item) return false;

        bool SequenceEqual(Flag<T> bigger, Flag<T> smaller)
        {
            var bytes = new byte[(bigger.Bits.Length - 1) / 8 + 1];
            smaller.Bits.CopyTo(bytes, 0);
            return bigger.ToBytes().SequenceEqual(bytes);
        }

        if (item.Bits.Length > Bits.Length)
            return SequenceEqual(item, this);

        if (item.Bits.Length < Bits.Length)
            return SequenceEqual(this, item);

        return item.Bits.Length == Bits.Length &&
               ((BitArray)item.Bits.Clone()).Xor(Bits).OfType<bool>().All(e => !e);
    }

    public override int GetHashCode()
    {
        return Bits.GetHashCode() * 31;
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
}