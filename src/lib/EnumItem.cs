using System.Collections;
using System.Text;

namespace InfiniteEnumFlags;

public sealed class EnumItem
{
    private readonly BitArray _bits;

    public EnumItem(int index, int length)
    {
        // None
        if (index == 0)
        {
            _bits = new BitArray(length, false);
            return;
        }

        // Items
        _bits = new BitArray(length, false);
        _bits.Set(index - 1, true);
    }


    public EnumItem(BitArray new_value)
    {
        _bits = new_value;
    }

    public EnumItem(byte[] new_value)
    {
        _bits = new BitArray(new_value);
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
        return new EnumItem(left._bits.Or(right._bits));
    }

    public static EnumItem operator &(EnumItem left, EnumItem right)
    {
        return new EnumItem(left._bits.And(right._bits));
    }

    public static EnumItem operator ^(EnumItem left, EnumItem right)
    {
        return new EnumItem(left._bits.Xor(right._bits));
    }

    public static EnumItem operator ~(EnumItem item)
    {
        var x = (BitArray)item._bits.Clone();
        return new EnumItem(x.Not());
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        for (var i = _bits.Count - 1; i >= 0; i--)
        {
            var c = _bits[i] ? '1' : '0';
            sb.Append(c);
        }

        return sb.ToString();
    }

    public override bool Equals(object obj)
    {
        if (obj is not EnumItem item) return false;

        bool SequenceEqual(EnumItem bigger, EnumItem smaller)
        {
            var bytes = new byte[(bigger._bits.Length - 1) / 8 + 1];
            smaller._bits.CopyTo(bytes, 0);
            return bigger.ToBytes().SequenceEqual(bytes);
        }

        if (item._bits.Length > _bits.Length)
            return SequenceEqual(item, this);

        if (item._bits.Length < _bits.Length)
            return SequenceEqual(this, item);

        return item._bits.Length == _bits.Length &&
               item._bits.Xor(_bits).OfType<bool>().All(e => !e);
    }

    public override int GetHashCode()
    {
        return _bits.GetHashCode() * 31;
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
        var ret = new byte[(_bits.Length - 1) / 8 + 1];
        _bits.CopyTo(ret, 0);
        return ret;
    }

    public void CopyTo(Array array, int index = 0)
    {
        _bits.CopyTo(array, index);
    }

    public BitArray ToBitArray()
    {
        return (_bits.Clone() as BitArray)!;
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