using System.Collections;

namespace InfiniteEnumFlags;

/// <summary>
/// Untyped variant of <see cref="Flag{T}"/> (uses <c>object</c> as the type parameter).
/// Prefer <see cref="Flag{T}"/> for type-safe enum flag values.
/// </summary>
public class Flag : Flag<object>
{
    public Flag() : base()
    {
    }

    public Flag(int index, int? length = null) : base(index, length)
    {
    }

    public Flag(BitArray new_value) : base(new_value)
    {
    }

    public Flag(byte[] new_value) : base(new_value)
    {
    }

    internal Flag(ulong[] canonicalWords) : base(canonicalWords, true)
    {
    }
}
