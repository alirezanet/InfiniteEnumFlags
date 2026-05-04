using System.Buffers;
using System.Collections;

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

    public override string ToString() => ToId();

    public string ToBinaryString()
    {
        var chars = new char[Bits.Count];

        for (var i = 0; i < Bits.Count; i++)
            chars[i] = Bits[i] ? '1' : '0';

        return new string(chars);
    }

    public string ToId()
    {
        var bytes = ToBytes();
        var length = GetNormalizedByteLength(bytes);
        return length == 0 ? "0" : Base64UrlEncode(bytes.AsSpan(0, length));
    }

    public string ToScopedId() => ToScopedId(GetDefaultScope());

    public string ToScopedId(string scope)
    {
        if (scope is null) throw new ArgumentNullException(nameof(scope));

        var bytes = ToBytes();
        var length = GetNormalizedByteLength(bytes);
        var fingerprint = GetScopeFingerprint(scope);
        var payloadLength = sizeof(ulong) + length;

        byte[]? rentedPayload = null;
        Span<byte> payload = payloadLength <= 256
            ? stackalloc byte[payloadLength]
            : rentedPayload = ArrayPool<byte>.Shared.Rent(payloadLength);

        try
        {
            WriteUInt64LittleEndian(payload, fingerprint);
            WriteMaskedValue(bytes.AsSpan(0, length), payload[sizeof(ulong)..payloadLength], fingerprint);
            return Base64UrlEncode(payload[..payloadLength]);
        }
        finally
        {
            if (rentedPayload is not null)
                ArrayPool<byte>.Shared.Return(rentedPayload);
        }
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

        var bytes = ToBytes();
        var otherBytes = other.ToBytes();
        return bytes.AsSpan(0, GetNormalizedByteLength(bytes))
            .SequenceEqual(otherBytes.AsSpan(0, GetNormalizedByteLength(otherBytes)));
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        var bytes = ToBytes();
        var normalizedBytes = bytes.AsSpan(0, GetNormalizedByteLength(bytes));
        foreach (var value in normalizedBytes)
            hashCode.Add(value);

        return hashCode.ToHashCode();
    }

    public static Flag<T> FromBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        return new Flag<T>(bytes);
    }

    public static Flag<T> FromId(string id)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        return id == "0"
            ? new Flag<T>(-1)
            : new Flag<T>(Base64UrlDecode(id));
    }

    public static Flag<T> FromScopedId(string id) => FromScopedId(id, GetDefaultScope());

    public static Flag<T> FromScopedId(string id, string scope)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        if (scope is null) throw new ArgumentNullException(nameof(scope));

        var payload = Base64UrlDecode(id);
        if (payload.Length < sizeof(ulong))
            throw new FormatException("Invalid scoped flag id.");

        var expectedFingerprint = GetScopeFingerprint(scope);
        var actualFingerprint = ReadUInt64LittleEndian(payload);
        if (actualFingerprint != expectedFingerprint)
            throw new InvalidOperationException("Flag id scope is not valid.");

        var valueLength = payload.Length - sizeof(ulong);
        if (valueLength == 0)
            return new Flag<T>(-1);

        var valueBytes = new byte[valueLength];
        WriteMaskedValue(payload.AsSpan(sizeof(ulong), valueLength), valueBytes, actualFingerprint);
        return new Flag<T>(valueBytes);
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
        var length = GetNormalizedByteLength(bytes);

        if (length == 0) return Array.Empty<byte>();
        if (length == bytes.Length) return bytes;

        var normalizedBytes = new byte[length];
        Buffer.BlockCopy(bytes, 0, normalizedBytes, 0, length);
        return normalizedBytes;
    }

    private static int GetNormalizedByteLength(byte[] bytes)
    {
        var length = bytes.Length;

        while (length > 0 && bytes[length - 1] == 0)
            length--;

        return length;
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes)
    {
        var base64Length = ((bytes.Length + 2) / 3) * 4;
        char[]? rentedChars = null;
        Span<char> chars = base64Length <= 256
            ? stackalloc char[base64Length]
            : rentedChars = ArrayPool<char>.Shared.Rent(base64Length);

        try
        {
            if (!Convert.TryToBase64Chars(bytes, chars, out var charsWritten))
                throw new InvalidOperationException("Unable to encode flag id.");

            var outputLength = charsWritten;
            while (outputLength > 0 && chars[outputLength - 1] == '=')
                outputLength--;

            for (var i = 0; i < outputLength; i++)
            {
                chars[i] = chars[i] switch
                {
                    '+' => '-',
                    '/' => '_',
                    _ => chars[i]
                };
            }

            return new string(chars[..outputLength]);
        }
        finally
        {
            if (rentedChars is not null)
                ArrayPool<char>.Shared.Return(rentedChars);
        }
    }

    private static byte[] Base64UrlDecode(string id)
    {
        var padding = (id.Length % 4) switch
        {
            0 => 0,
            2 => 2,
            3 => 1,
            _ => throw new FormatException("Invalid flag id length.")
        };

        var base64Length = id.Length + padding;
        char[]? rentedChars = null;
        Span<char> chars = base64Length <= 256
            ? stackalloc char[base64Length]
            : rentedChars = ArrayPool<char>.Shared.Rent(base64Length);

        byte[]? rentedBytes = null;
        var maxByteLength = (base64Length / 4) * 3;
        Span<byte> bytes = maxByteLength <= 256
            ? stackalloc byte[maxByteLength]
            : rentedBytes = ArrayPool<byte>.Shared.Rent(maxByteLength);

        try
        {
            for (var i = 0; i < id.Length; i++)
            {
                chars[i] = id[i] switch
                {
                    '-' => '+',
                    '_' => '/',
                    _ => id[i]
                };
            }

            for (var i = id.Length; i < base64Length; i++)
                chars[i] = '=';

            if (!Convert.TryFromBase64Chars(chars[..base64Length], bytes, out var bytesWritten))
                throw new FormatException("Invalid flag id.");

            return bytes[..bytesWritten].ToArray();
        }
        finally
        {
            if (rentedChars is not null)
                ArrayPool<char>.Shared.Return(rentedChars);

            if (rentedBytes is not null)
                ArrayPool<byte>.Shared.Return(rentedBytes);
        }
    }

    private static string GetDefaultScope()
    {
        return typeof(T).FullName ?? typeof(T).Name;
    }

    private static ulong GetScopeFingerprint(string scope)
    {
        const ulong offset = 14695981039346656037;
        const ulong prime = 1099511628211;

        var hash = offset;
        foreach (var c in scope)
        {
            hash ^= (byte)c;
            hash *= prime;
            hash ^= (byte)(c >> 8);
            hash *= prime;
        }

        return hash;
    }

    private static void WriteMaskedValue(ReadOnlySpan<byte> source, Span<byte> destination, ulong fingerprint)
    {
        var state = fingerprint;
        for (var i = 0; i < source.Length; i++)
        {
            if ((i & 7) == 0)
                state = NextMaskState(state);

            destination[i] = (byte)(source[i] ^ (byte)(state >> ((i & 7) * 8)));
        }
    }

    private static ulong NextMaskState(ulong state)
    {
        state += 0x9E3779B97F4A7C15;
        var result = state;
        result = (result ^ (result >> 30)) * 0xBF58476D1CE4E5B9;
        result = (result ^ (result >> 27)) * 0x94D049BB133111EB;
        return result ^ (result >> 31);
    }

    private static void WriteUInt64LittleEndian(Span<byte> destination, ulong value)
    {
        for (var i = 0; i < sizeof(ulong); i++)
            destination[i] = (byte)(value >> (i * 8));
    }

    private static ulong ReadUInt64LittleEndian(ReadOnlySpan<byte> source)
    {
        ulong value = 0;
        for (var i = 0; i < sizeof(ulong); i++)
            value |= (ulong)source[i] << (i * 8);

        return value;
    }
}
