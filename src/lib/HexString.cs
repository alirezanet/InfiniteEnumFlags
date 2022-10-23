namespace InfiniteEnumFlags;

public sealed class HexString
{
    public string Value { get; init; }

    private HexString()
    {
        Value = string.Empty;
    }

    public static HexString Create(string hex)
    {
        if (hex.Length % 2 == 1)
            throw new Exception("The binary key cannot have an odd number of digits");

        //if (hex.Any(c => char. ))
        //	throw new Exception("Invalid hex value");

        return new HexString() { Value = hex };
    }

    public static HexString Create(int hex)
    {
        var bytes = BitConverter.GetBytes(hex);
        return new HexString() { Value = Convert.ToHexString(bytes) };
    }

    public static implicit operator HexString(string d) => HexString.Create(d);
    public static implicit operator HexString(int d) => HexString.Create(d);
    public static implicit operator string(HexString d) => d.Value;

    public override string ToString() => Value;
}