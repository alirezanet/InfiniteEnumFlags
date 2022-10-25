using InfiniteEnumFlags.Internal;

namespace InfiniteEnumFlags;

public sealed class HexString
{
    public string Value { get; private set; }

    private HexString()
    {
        Value = string.Empty;
    }

    public static HexString Create(string hex)
    {
        // TODO: add some hex validation 

        return new HexString() { Value = hex };
    }

    public static HexString Create(int hex)
    {
        var bytes = BitConverter.GetBytes(hex);
        return new HexString() { Value = HexConverter.ToHexString(bytes) };
    }

    public static implicit operator HexString(string d) => Create(d);
    public static implicit operator HexString(int d) => Create(d);
    public static implicit operator string(HexString d) => d.Value;

    public override string ToString() => Value;
}