using System.Reflection;

namespace InfiniteEnumFlags;

public abstract class InfiniteEnum<T>
{
    public static IEnumerable<string> GetNames()
    {
        return GetNames(BindingFlags.Public | BindingFlags.Static);
    }

    public static IEnumerable<string> GetNames(BindingFlags bindingFlags)
    {
        return typeof(T)
            .GetFields(bindingFlags)
            .Where(IsFlagField)
            .Select(f => f.Name);
    }

    public static IEnumerable<string> GetNames(Flag<T> enumFlag, BindingFlags bindingFlags)
    {
        return GetKeyValues(bindingFlags)
            .Where(x => x.Value.IsEmpty ? enumFlag.IsEmpty : enumFlag.HasAllFlags(x.Value))
            .Select(x => x.Key);
    }

    public static IEnumerable<string> GetNames(Flag<T> enumFlag)
    {
        return GetNames(enumFlag, BindingFlags.Public | BindingFlags.Static);
    }

    public static Dictionary<string, Flag<T>> GetKeyValues()
    {
        return GetKeyValues(BindingFlags.Public | BindingFlags.Static);
    }

    public static Dictionary<string, Flag<T>> GetKeyValues(BindingFlags bindingFlags)
    {
        return typeof(T)
            .GetFields(bindingFlags)
            .Where(IsFlagField)
            .ToDictionary(f => f.Name, f => (Flag<T>)f.GetValue(null)!);
    }

    public static Flag<T>? FromName(string name)
    {
        return FromName(name, BindingFlags.Public | BindingFlags.Static);
    }

    public static Flag<T>? FromName(string name, BindingFlags bindingFlags)
    {
        var field = typeof(T).GetField(name, bindingFlags);
        return field is not null && IsFlagField(field)
            ? field.GetValue(null) as Flag<T>
            : null;
    }

    public static bool TryFromName(string name, out Flag<T> flag)
    {
        return TryFromName(name, BindingFlags.Public | BindingFlags.Static, out flag);
    }

    public static bool TryFromName(string name, BindingFlags bindingFlags, out Flag<T> flag)
    {
        var result = FromName(name, bindingFlags);
        flag = result ?? new Flag<T>(-1);
        return result is not null;
    }

    public static Flag<T> FromNames(params string[] names)
    {
        return FromNames((IEnumerable<string>)names);
    }

    public static Flag<T> FromNames(IEnumerable<string> names)
    {
        var keyValues = GetKeyValues();
        var result = new Flag<T>(-1);

        foreach (var name in names)
        {
            if (!keyValues.TryGetValue(name, out var flag))
                throw new ArgumentException($"Unknown flag name '{name}'.", nameof(names));

            result |= flag;
        }

        return result;
    }

    public static bool TryFromNames(IEnumerable<string> names, out Flag<T> flag)
    {
        var keyValues = GetKeyValues();
        var result = new Flag<T>(-1);

        foreach (var name in names)
        {
            if (!keyValues.TryGetValue(name, out var item))
            {
                flag = new Flag<T>(-1);
                return false;
            }

            result |= item;
        }

        flag = result;
        return true;
    }

    public static Flag<T> All
    {
        get
        {
            var flags = typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(IsFlagField)
                .Select(f => (Flag<T>)f.GetValue(null)!)
                .Where(f => !f.IsEmpty);

            return flags.Aggregate(new Flag<T>(-1), (current, flag) => current | flag);
        }
    }

    /// <summary>
    /// Returns every declared flag on <typeparamref name="T"/> except the bits
    /// in <paramref name="flag"/>. This is the "infinite enum" replacement for
    /// the native enum idiom <c>~SomeEnum.X</c>, which has no well-defined
    /// meaning over an unbounded bit space.
    /// </summary>
    public static Flag<T> AllExcept(Flag<T> flag)
    {
        if (flag is null) throw new ArgumentNullException(nameof(flag));
        var all = All;
        return flag.IsEmpty ? all : all & ~flag;
    }

    public static Flag<T> FromBase64(string base64)
    {
        return Flag<T>.FromBase64(base64);
    }

    public static Flag<T> FromId(string id)
    {
        return Flag<T>.FromId(id);
    }

    public static Flag<T> FromScopedId(string id)
    {
        return Flag<T>.FromScopedId(id);
    }

    public static Flag<T> FromScopedId(string id, string scope)
    {
        return Flag<T>.FromScopedId(id, scope);
    }

    private static bool IsFlagField(FieldInfo field)
    {
        return typeof(Flag<T>).IsAssignableFrom(field.FieldType);
    }
}
