using System.Collections;
using System.Reflection;

namespace InfiniteEnumFlags;

public abstract class InfiniteEnum<T>
{
    public static Flag<T>? FromName(string name)
    {
        return typeof(T)
            .GetField(name, BindingFlags.Public | BindingFlags.Static)?
            .GetValue(null) as Flag<T>;
    }

    public static Flag<T> All
    {
        get
        {
            var count = typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Count(f => f.FieldType == typeof(Flag<T>) ||
                            f.FieldType.BaseType == typeof(Flag<T>));
            return new Flag<T>(new BitArray(count - 1, true));
        }
    }

    public static Flag<T> FromBase64(string base64)
    {
        return Flag<T>.FromBase64(base64);
    }

    public static Flag<T> FromUniqueId(string id, string? salt = null)
    {
        return Flag<T>.FromUniqueId(id, salt);
    }
}