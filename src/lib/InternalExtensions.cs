using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace InfiniteEnumFlags;

internal static class InternalExtensions
{
    private const int BitShiftPerInt32 = 5;
    private const int BitsPerInt32 = 32;

    internal static BitArray LeftShift(this BitArray @this, int count)
    {
        int[] m_array = @this.GetFieldValue<int[]>("m_array") ??
                        throw new Exception("BitArray's array field not found");


        var m_length = @this.Length;


        if (count <= 0)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            return @this;
        }

        int lengthToClear;
        if (count < m_length)
        {
            int lastIndex = (m_length - 1) >> BitShiftPerInt32; // Divide by 32.

            // We can not use Math.DivRem without taking a dependency on System.Runtime.Extensions
            lengthToClear = Div32Rem(count, out int shiftCount);

            if (shiftCount == 0)
            {
                Array.Copy(m_array, 0, m_array, lengthToClear, lastIndex + 1 - lengthToClear);
            }
            else
            {
                int fromindex = lastIndex - lengthToClear;
                unchecked
                {
                    while (fromindex > 0)
                    {
                        int left = m_array[fromindex] << shiftCount;
                        uint right = (uint)m_array[--fromindex] >> (BitsPerInt32 - shiftCount);
                        m_array[lastIndex] = left | (int)right;
                        lastIndex--;
                    }

                    m_array[lastIndex] = m_array[fromindex] << shiftCount;
                }
            }
        }
        else
        {
            lengthToClear = GetInt32ArrayLengthFromBitLength(m_length); // Clear all
        }

        m_array.AsSpan(0, lengthToClear).Clear();

        return @this;
    }

    private static T? GetFieldValue<T>(this object obj, string name)
    {
        // Set the flags so that private and public fields from instances will be found
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var field = obj.GetType().GetField(name, bindingFlags);
        return (T)field?.GetValue(obj)!;
    }

    private static int Div32Rem(int number, out int remainder)
    {
        uint quotient = (uint)number / 32;
        remainder = number & (32 - 1); // equivalent to number % 32, since 32 is a power of 2
        return (int)quotient;
    }

    /// <summary>
    /// Used for conversion between different representations of bit array.
    /// Returns (n + (32 - 1)) / 32, rearranged to avoid arithmetic overflow.
    /// For example, in the bit to int case, the straightforward calc would
    /// be (n + 31) / 32, but that would cause overflow. So instead it's
    /// rearranged to ((n - 1) / 32) + 1.
    /// Due to sign extension, we don't need to special case for n == 0, if we use
    /// bitwise operations (since ((n - 1) >> 5) + 1 = 0).
    /// This doesn't hold true for ((n - 1) / 32) + 1, which equals 1.
    ///
    /// Usage:
    /// GetArrayLength(77): returns how many ints must be
    /// allocated to store 77 bits.
    /// </summary>
    /// <param name="n"></param>
    /// <returns>how many ints are required to store n bytes</returns>
    private static int GetInt32ArrayLengthFromBitLength(int n)
    {
        Debug.Assert(n >= 0);
        return (int)((uint)(n - 1 + (1 << BitShiftPerInt32)) >> BitShiftPerInt32);
    }
}