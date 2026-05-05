using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Numerics;

namespace InfiniteEnumFlags;

/// <summary>
/// Growable, immutable flag value backed by a packed <c>ulong[]</c> for fast
/// 64-bit-parallel bitwise operations. The internal representation is canonical:
/// trailing all-zero words are removed so equal flag values always have identical
/// storage regardless of how they were constructed.
/// </summary>
public class Flag<T> : IEquatable<Flag<T>>
{
    private const int BitsPerWord = 64;
    private const int Log2BitsPerWord = 6;

    // Canonical: never has a trailing zero word. Empty flag => zero-length array.
    private readonly ulong[] _words;

    // ------------------------------------------------------------------ ctors

    private protected Flag(ulong[] canonicalWords, bool _)
    {
        _words = canonicalWords;
    }

    internal Flag(ulong[] canonicalWords) : this(canonicalWords, true) { }

    public Flag()
    {
        _words = Array.Empty<ulong>();
    }

    /// <summary>
    /// Creates a flag with a single bit set at <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Zero-based bit index. <c>-1</c> creates an empty flag.</param>
    /// <param name="length">
    /// Optional minimum number of bits the underlying storage must accommodate. The
    /// canonical representation is independent of <paramref name="length"/>; it is
    /// validated only to catch obvious sizing mistakes.
    /// </param>
    public Flag(int index, int? length = null)
    {
        if (index < -1)
            throw new ArgumentOutOfRangeException(nameof(index), "Flag index must be -1 or greater.");

        if (length.HasValue)
        {
            if (length.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be zero or greater.");
            if (index >= 0 && length.Value <= index)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be large enough to contain the flag index.");
        }

        if (index == -1)
        {
            _words = Array.Empty<ulong>();
            return;
        }

        var wordIndex = index >> Log2BitsPerWord;
        var words = new ulong[wordIndex + 1];
        words[wordIndex] = 1UL << (index & (BitsPerWord - 1));
        _words = words;
    }

    public Flag(BitArray bits)
    {
        if (bits is null) throw new ArgumentNullException(nameof(bits));
        _words = WordsFromBitArray(bits);
    }

    public Flag(byte[] bytes)
    {
        if (bytes is null) throw new ArgumentNullException(nameof(bytes));
        _words = WordsFromBytes(bytes.AsSpan());
    }

    // ------------------------------------------------------------------ properties

    public bool IsEmpty => _words.Length == 0;

    public int Count
    {
        get
        {
            var c = 0;
            for (var i = 0; i < _words.Length; i++)
                c += BitOperations.PopCount(_words[i]);
            return c;
        }
    }

    /// <summary>
    /// Number of bits in the canonical underlying storage (always a multiple of 64,
    /// or 0 for an empty flag). Two equal flags always report the same length.
    /// </summary>
    public int Length => _words.Length * BitsPerWord;

    // ------------------------------------------------------------------ operators

    public static explicit operator Flag(Flag<T> item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));
        return new Flag(CloneWords(item._words));
    }

    public static explicit operator Flag<T>(Flag item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));
        return new Flag<T>(CloneWords(item.GetWords()), true);
    }

    public static bool operator ==(Flag<T>? a, Flag<T>? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Flag<T>? a, Flag<T>? b) => !(a == b);

    public static Flag<T> operator |(Flag<T> a, Flag<T> b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));
        var (longer, shorter) = a._words.Length >= b._words.Length
            ? (a._words, b._words)
            : (b._words, a._words);

        if (longer.Length == 0) return new Flag<T>();
        var result = new ulong[longer.Length];
        for (var i = 0; i < shorter.Length; i++) result[i] = longer[i] | shorter[i];
        for (var i = shorter.Length; i < longer.Length; i++) result[i] = longer[i];
        // OR with longer never shrinks; canonical because longer was canonical.
        return new Flag<T>(result, true);
    }

    public static Flag<T> operator &(Flag<T> a, Flag<T> b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));
        var min = Math.Min(a._words.Length, b._words.Length);
        if (min == 0) return new Flag<T>();
        var result = new ulong[min];
        for (var i = 0; i < min; i++) result[i] = a._words[i] & b._words[i];
        return new Flag<T>(Canonicalize(result), true);
    }

    public static Flag<T> operator ^(Flag<T> a, Flag<T> b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));
        var (longer, shorter) = a._words.Length >= b._words.Length
            ? (a._words, b._words)
            : (b._words, a._words);

        if (longer.Length == 0) return new Flag<T>();
        var result = new ulong[longer.Length];
        for (var i = 0; i < shorter.Length; i++) result[i] = longer[i] ^ shorter[i];
        for (var i = shorter.Length; i < longer.Length; i++) result[i] = longer[i];
        return new Flag<T>(Canonicalize(result), true);
    }

    public static Flag<T> operator ~(Flag<T> a)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (a._words.Length == 0) return new Flag<T>();
        var result = new ulong[a._words.Length];
        for (var i = 0; i < a._words.Length; i++) result[i] = ~a._words[i];
        return new Flag<T>(Canonicalize(result), true);
    }

    // ------------------------------------------------------------------ equality

    public override bool Equals(object? obj) => Equals(obj as Flag<T>);

    public bool Equals(Flag<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_words.Length != other._words.Length) return false;
        return _words.AsSpan().SequenceEqual(other._words.AsSpan());
    }

    public override int GetHashCode()
    {
        var h = new HashCode();
        for (var i = 0; i < _words.Length; i++) h.Add(_words[i]);
        return h.ToHashCode();
    }

    // ------------------------------------------------------------------ conversions

    /// <summary>
    /// Returns the canonical little-endian byte representation with trailing zero
    /// bytes removed. An empty flag returns an empty array.
    /// </summary>
    public byte[] ToBytes()
    {
        if (_words.Length == 0) return Array.Empty<byte>();
        // Compute trimmed length first.
        var lastWord = _words[_words.Length - 1];
        var trailingZeroBits = BitOperations.LeadingZeroCount(lastWord); // 0..63 (lastWord != 0)
        var lastWordBytes = 8 - (trailingZeroBits >> 3);
        var totalBytes = (_words.Length - 1) * 8 + lastWordBytes;
        var bytes = new byte[totalBytes];
        for (var i = 0; i < _words.Length - 1; i++)
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i * 8), _words[i]);
        // Last word: write only the needed bytes.
        var lastBaseOffset = (_words.Length - 1) * 8;
        for (var i = 0; i < lastWordBytes; i++)
            bytes[lastBaseOffset + i] = (byte)(lastWord >> (i * 8));
        return bytes;
    }

    public BitArray ToBitArray()
    {
        if (_words.Length == 0) return new BitArray(0);
        var bytes = new byte[_words.Length * 8];
        for (var i = 0; i < _words.Length; i++)
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i * 8), _words[i]);
        return new BitArray(bytes);
    }

    /// <inheritdoc cref="BitArray.CopyTo" />
    public void CopyTo(Array array, int index = 0) => ToBitArray().CopyTo(array, index);

    public string ToBase64String() => Convert.ToBase64String(ToBytesPadded());
    public string ToBase64Trimmed() => Convert.ToBase64String(ToBytes());

    public static Flag<T> FromBase64(string base64)
    {
        if (base64 is null) throw new ArgumentNullException(nameof(base64));
        return new Flag<T>(Convert.FromBase64String(base64));
    }

    public string ToBinaryString()
    {
        if (_words.Length == 0) return string.Empty;
        var bytes = ToBytes();
        var totalBits = bytes.Length * 8;
        var chars = new char[totalBits];
        for (var i = 0; i < totalBits; i++)
            chars[i] = (bytes[i >> 3] & (1 << (i & 7))) != 0 ? '1' : '0';
        return new string(chars);
    }

    // ------------------------------------------------------------------ Id encoding
    //
    // Wire format (decoded bytes):
    //   - Empty flag is encoded as the single literal string "0" (not base64).
    //   - Otherwise: [varint K] [body]
    //         K == 0  => DENSE  body = canonical little-endian trimmed bytes
    //         K  > 0  => SPARSE body = K delta-encoded varint indices
    //                            (delta_i = index_i - index_{i-1} - 1, with index_{-1} = -1)
    //   - Encoder picks whichever format is shorter. Tie => DENSE.
    //
    // Rationale:
    //   - Dense is optimal for densely-set / low-index flags (typical).
    //   - Sparse is dramatically smaller for high-index flags
    //     (e.g. bit 1000 alone => 3 bytes instead of 126).
    //   - The leading varint K disambiguates the two formats with no extra tag byte.

    public override string ToString() => ToId();

    public string ToId()
    {
        if (_words.Length == 0) return "0";
        var payload = EncodePayload();
        return Base64UrlEncode(payload);
    }

    public static Flag<T> FromId(string id)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        if (id == "0") return new Flag<T>();
        var bytes = Base64UrlDecode(id);
        return new Flag<T>(DecodePayload(bytes), true);
    }

    public string ToScopedId() => ToScopedId(GetDefaultScope());

    public string ToScopedId(string scope)
    {
        if (scope is null) throw new ArgumentNullException(nameof(scope));
        var fingerprint = GetScopeFingerprint(scope);
        // For empty flag we still write a one-byte payload (varint 0 + zero dense bytes)
        // so the scope check applies even to empty values.
        var payload = _words.Length == 0 ? new byte[] { 0 } : EncodePayload();
        var output = new byte[2 + payload.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(output, fingerprint);
        MaskPayload(payload, output.AsSpan(2), fingerprint);
        return Base64UrlEncode(output);
    }

    public static Flag<T> FromScopedId(string id) => FromScopedId(id, GetDefaultScope());

    public static Flag<T> FromScopedId(string id, string scope)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        if (scope is null) throw new ArgumentNullException(nameof(scope));
        var bytes = Base64UrlDecode(id);
        if (bytes.Length < 2) throw new FormatException("Invalid scoped flag id.");
        var expected = GetScopeFingerprint(scope);
        var actual = BinaryPrimitives.ReadUInt16LittleEndian(bytes);
        if (expected != actual)
            throw new InvalidOperationException("Flag id scope is not valid.");
        var maskedLen = bytes.Length - 2;
        var payload = new byte[maskedLen];
        MaskPayload(bytes.AsSpan(2), payload, actual);
        return new Flag<T>(DecodePayload(payload), true);
    }

    // ------------------------------------------------------------------ encoding internals

    private byte[] EncodePayload()
    {
        // Collect indices once; we need them to size sparse and to write it.
        var bitCount = Count;
        var indices = bitCount == 0 ? Array.Empty<int>() : new int[bitCount];
        var pos = 0;
        for (var w = 0; w < _words.Length; w++)
        {
            var word = _words[w];
            while (word != 0)
            {
                var bit = BitOperations.TrailingZeroCount(word);
                indices[pos++] = (w << Log2BitsPerWord) + bit;
                word &= word - 1;
            }
        }

        var trimmed = ToBytes();
        var denseLen = VarIntSize(0) + trimmed.Length;

        var sparseLen = VarIntSize((uint)bitCount);
        var lastIdx = -1;
        for (var i = 0; i < indices.Length; i++)
        {
            sparseLen += VarIntSize((uint)(indices[i] - lastIdx - 1));
            lastIdx = indices[i];
        }

        if (sparseLen < denseLen)
        {
            var buf = new byte[sparseLen];
            var p = WriteVarInt(buf, 0, (uint)bitCount);
            lastIdx = -1;
            for (var i = 0; i < indices.Length; i++)
            {
                p = WriteVarInt(buf, p, (uint)(indices[i] - lastIdx - 1));
                lastIdx = indices[i];
            }
            return buf;
        }
        else
        {
            var buf = new byte[denseLen];
            var p = WriteVarInt(buf, 0, 0);
            Buffer.BlockCopy(trimmed, 0, buf, p, trimmed.Length);
            return buf;
        }
    }

    private static ulong[] DecodePayload(byte[] payload)
    {
        if (payload.Length == 0) throw new FormatException("Invalid flag id.");
        var pos = 0;
        var count = ReadVarInt(payload, ref pos);

        if (count == 0)
        {
            // Dense
            return WordsFromBytes(payload.AsSpan(pos));
        }

        // Sparse
        var indices = new int[count];
        var lastIdx = -1;
        for (uint i = 0; i < count; i++)
        {
            var delta = ReadVarInt(payload, ref pos);
            var idx = lastIdx + 1 + (int)delta;
            if (idx < 0) throw new FormatException("Invalid flag id.");
            indices[i] = idx;
            lastIdx = idx;
        }
        if (pos != payload.Length) throw new FormatException("Invalid flag id.");

        var maxIdx = indices[indices.Length - 1];
        var wordCount = (maxIdx >> Log2BitsPerWord) + 1;
        var words = new ulong[wordCount];
        for (var i = 0; i < indices.Length; i++)
        {
            var idx = indices[i];
            words[idx >> Log2BitsPerWord] |= 1UL << (idx & (BitsPerWord - 1));
        }
        return words;
    }

    private static int VarIntSize(uint value)
    {
        var size = 1;
        while (value >= 0x80) { value >>= 7; size++; }
        return size;
    }

    private static int WriteVarInt(byte[] buf, int pos, uint value)
    {
        while (value >= 0x80)
        {
            buf[pos++] = (byte)(value | 0x80);
            value >>= 7;
        }
        buf[pos++] = (byte)value;
        return pos;
    }

    private static uint ReadVarInt(byte[] payload, ref int pos)
    {
        uint value = 0;
        var shift = 0;
        while (true)
        {
            if (pos >= payload.Length)
                throw new FormatException("Invalid flag id.");
            var b = payload[pos++];
            value |= (uint)(b & 0x7F) << shift;
            if ((b & 0x80) == 0) return value;
            shift += 7;
            if (shift > 28)
                throw new FormatException("Invalid flag id.");
        }
    }

    // ------------------------------------------------------------------ scope helpers

    private static string GetDefaultScope() => typeof(T).FullName ?? typeof(T).Name;

    private static ushort GetScopeFingerprint(string scope)
    {
        // FNV-1a over UTF-16 code units, folded to 16 bits.
        const uint offset = 2166136261;
        const uint prime = 16777619;
        var hash = offset;
        foreach (var c in scope)
        {
            hash ^= (byte)c; hash *= prime;
            hash ^= (byte)(c >> 8); hash *= prime;
        }
        return (ushort)((hash >> 16) ^ (hash & 0xFFFF));
    }

    private static void MaskPayload(ReadOnlySpan<byte> source, Span<byte> destination, ushort fingerprint)
    {
        // SplitMix64-style keystream seeded from the fingerprint. Deterministic
        // and avalanching enough to obscure the raw payload from casual inspection.
        ulong state = ((ulong)fingerprint << 48) | ((ulong)fingerprint << 32)
                      | ((ulong)fingerprint << 16) | fingerprint;
        ulong block = 0;
        for (var i = 0; i < source.Length; i++)
        {
            var lane = i & 7;
            if (lane == 0) block = NextMaskBlock(ref state);
            destination[i] = (byte)(source[i] ^ (byte)(block >> (lane * 8)));
        }
    }

    private static ulong NextMaskBlock(ref ulong state)
    {
        state += 0x9E3779B97F4A7C15UL;
        var r = state;
        r = (r ^ (r >> 30)) * 0xBF58476D1CE4E5B9UL;
        r = (r ^ (r >> 27)) * 0x94D049BB133111EBUL;
        return r ^ (r >> 31);
    }

    // ------------------------------------------------------------------ word helpers

    private byte[] ToBytesPadded()
    {
        if (_words.Length == 0) return Array.Empty<byte>();
        var bytes = new byte[_words.Length * 8];
        for (var i = 0; i < _words.Length; i++)
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i * 8), _words[i]);
        return bytes;
    }

    internal ulong[] GetWords() => _words;

    private static ulong[] CloneWords(ulong[] words)
    {
        if (words.Length == 0) return Array.Empty<ulong>();
        var copy = new ulong[words.Length];
        Array.Copy(words, copy, words.Length);
        return copy;
    }

    private static ulong[] Canonicalize(ulong[] words)
    {
        var len = words.Length;
        while (len > 0 && words[len - 1] == 0) len--;
        if (len == words.Length) return words;
        if (len == 0) return Array.Empty<ulong>();
        var trimmed = new ulong[len];
        Array.Copy(words, trimmed, len);
        return trimmed;
    }

    private static ulong[] WordsFromBytes(ReadOnlySpan<byte> bytes)
    {
        var end = bytes.Length;
        while (end > 0 && bytes[end - 1] == 0) end--;
        if (end == 0) return Array.Empty<ulong>();
        var wordCount = (end + 7) >> 3;
        var words = new ulong[wordCount];
        var fullWords = end >> 3;
        for (var w = 0; w < fullWords; w++)
            words[w] = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(w * 8, 8));
        var rem = end & 7;
        if (rem != 0)
        {
            ulong last = 0;
            var baseOffset = fullWords * 8;
            for (var i = 0; i < rem; i++)
                last |= (ulong)bytes[baseOffset + i] << (i * 8);
            words[fullWords] = last;
        }
        return words;
    }

    private static ulong[] WordsFromBitArray(BitArray bits)
    {
        if (bits.Length == 0) return Array.Empty<ulong>();
        var byteLen = (bits.Length + 7) >> 3;
        var bytes = new byte[byteLen];
        bits.CopyTo(bytes, 0);
        return WordsFromBytes(bytes);
    }

    // ------------------------------------------------------------------ base64url

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0) return string.Empty;
        var b64Len = ((bytes.Length + 2) / 3) * 4;
        char[]? rented = null;
        Span<char> chars = b64Len <= 256
            ? stackalloc char[b64Len]
            : (rented = ArrayPool<char>.Shared.Rent(b64Len));
        try
        {
            if (!Convert.TryToBase64Chars(bytes, chars, out var written))
                throw new InvalidOperationException("Unable to encode flag id.");
            var n = written;
            while (n > 0 && chars[n - 1] == '=') n--;
            for (var i = 0; i < n; i++)
            {
                chars[i] = chars[i] switch
                {
                    '+' => '-',
                    '/' => '_',
                    _ => chars[i]
                };
            }
            return new string(chars[..n]);
        }
        finally
        {
            if (rented is not null) ArrayPool<char>.Shared.Return(rented);
        }
    }

    private static byte[] Base64UrlDecode(string id)
    {
        var pad = (id.Length & 3) switch
        {
            0 => 0,
            2 => 2,
            3 => 1,
            _ => throw new FormatException("Invalid flag id length.")
        };
        var b64Len = id.Length + pad;
        char[]? rented = null;
        Span<char> chars = b64Len <= 256
            ? stackalloc char[b64Len]
            : (rented = ArrayPool<char>.Shared.Rent(b64Len));
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
            for (var i = id.Length; i < b64Len; i++) chars[i] = '=';

            var maxBytes = (b64Len >> 2) * 3;
            var bytes = new byte[maxBytes];
            if (!Convert.TryFromBase64Chars(chars[..b64Len], bytes, out var written))
                throw new FormatException("Invalid flag id.");
            if (written == maxBytes) return bytes;
            var trimmed = new byte[written];
            Array.Copy(bytes, trimmed, written);
            return trimmed;
        }
        finally
        {
            if (rented is not null) ArrayPool<char>.Shared.Return(rented);
        }
    }
}
