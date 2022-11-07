using System.Collections;
using System.Text;

namespace InfiniteEnumFlags;

public class Flag<T>
{
    internal readonly BitArray Bits;

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

    public override string ToString() => ToBase64Key();

    public string ToBinaryString()
    {
        var sb = new StringBuilder();

        for (var i = Bits.Count - 1; i >= 0; i--)
        {
            var c = Bits[i] ? '1' : '0';
            sb.Append(c);
        }

        return sb.ToString();
    }

    public string ToBase64Key()
    {
        var bytes = ToBytes().AsSpan();
        var index = 0;
        for (var i = bytes.Length - 1; i >= 0; i--)
        {
            if (bytes[i] == 0) continue;
            index = i + 1;
            break;
        }

        var key = bytes.Slice(0, index).ToArray();
        return Convert.ToBase64String(key);
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

    public string ToBase64String()
    {
        var bytes = ToBytes();
        return Convert.ToBase64String(bytes);
    }

    public static Flag<T> FromBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        return new Flag<T>(bytes);
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