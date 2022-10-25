using System.Collections;
using System.Text;
using InfiniteEnumFlags.Internal;

namespace InfiniteEnumFlags;

public sealed class EnumItem
{
    internal readonly BitArray Bits;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index">zero is reserved for empty bits / None.</param>
    /// <param name="length">Number of total required bits</param>
    public EnumItem(int index, int length)
    {
        // None
        if (index == 0)
        {
            Bits = new BitArray(length, false);
            return;
        }

        // Items
        Bits = new BitArray(length, false);
        Bits.Set(index - 1, true);
    }


    public EnumItem(BitArray new_value)
    {
        Bits = new_value;
    }

    public EnumItem(byte[] new_value)
    {
        Bits = new BitArray(new_value);
    }


    public static bool operator ==(EnumItem item, EnumItem item2)
    {
        return item.Equals(item2);
    }

    public static bool operator !=(EnumItem item, EnumItem item2)
    {
        return !(item == item2);
    }

    public static EnumItem operator |(EnumItem left, EnumItem right)
    {
        return new EnumItem(left.Bits.Or(right.Bits));
    }

    public static EnumItem operator &(EnumItem left, EnumItem right)
    {
        return new EnumItem(left.Bits.And(right.Bits));
    }

    public static EnumItem operator ^(EnumItem left, EnumItem right)
    {
        return new EnumItem(left.Bits.Xor(right.Bits));
    }

    public static EnumItem operator ~(EnumItem item)
    {
        var x = (BitArray)item.Bits.Clone();
        return new EnumItem(x.Not());
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        for (var i = Bits.Count - 1; i >= 0; i--)
        {
            var c = Bits[i] ? '1' : '0';
            sb.Append(c);
        }

        return sb.ToString();
    }

    public override bool Equals(object obj)
    {
        if (obj is not EnumItem item) return false;

        bool SequenceEqual(EnumItem bigger, EnumItem smaller)
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

    public string ToHexString()
    {
        var bytes = ToBytes();
        // if (BitConverter.IsLittleEndian) // Not sure is necessary
        Array.Reverse(bytes);
        return HexConverter.ToHexString(bytes);
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

    public static EnumItem FromHexString(HexString hex)
    {
        var arr = new byte[hex.Value.Length >> 1];

        for (var i = 0; i < hex.Value.Length >> 1; ++i)
        {
            arr[i] = (byte)((GetHexVal(hex.Value[i << 1]) << 4) + (GetHexVal(hex.Value[(i << 1) + 1])));
        }

        return new EnumItem(arr);
    }

    private static int GetHexVal(char hex)
    {
        var val = (int)hex;
        return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
    }
}