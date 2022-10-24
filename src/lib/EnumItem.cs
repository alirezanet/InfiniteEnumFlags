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

    public string ToHexString()
    {
        return HexConverter.ToHexString(ToBytes());
    }

    public byte[] ToBytes()
    {
        var ret = new byte[(_bits.Length - 1) / 8 + 1];
        _bits.CopyTo(ret, 0);
        return ret;
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