using System.Reflection;

namespace InfiniteEnumFlags;

/// <summary>
/// Base class for infinite-bit enum types. Inherit from this and declare
/// <c>static readonly <see cref="Flag{T}"/></c> fields to define named flag values.
/// </summary>
public abstract class InfiniteEnum<T>
{
    /// <summary>Returns the names of all declared flag fields using public static binding.</summary>
    public static IEnumerable<string> GetNames()
    {
        return GetNames(BindingFlags.Public | BindingFlags.Static);
    }

    /// <summary>Returns the names of all declared flag fields matching the given <paramref name="bindingFlags"/>.</summary>
    public static IEnumerable<string> GetNames(BindingFlags bindingFlags)
    {
        return typeof(T)
            .GetFields(bindingFlags)
            .Where(IsFlagField)
            .Select(f => f.Name);
    }

    /// <summary>Returns the names of declared flags that are fully contained in <paramref name="enumFlag"/>.</summary>
    public static IEnumerable<string> GetNames(Flag<T> enumFlag, BindingFlags bindingFlags)
    {
        return GetKeyValues(bindingFlags)
            .Where(x => x.Value.IsEmpty ? enumFlag.IsEmpty : enumFlag.HasAllFlags(x.Value))
            .Select(x => x.Key);
    }

    /// <summary>Returns the names of declared flags that are fully contained in <paramref name="enumFlag"/> using public static binding.</summary>
    public static IEnumerable<string> GetNames(Flag<T> enumFlag)
    {
        return GetNames(enumFlag, BindingFlags.Public | BindingFlags.Static);
    }

    /// <summary>Returns a dictionary of all declared flag names and their values using public static binding.</summary>
    public static Dictionary<string, Flag<T>> GetKeyValues()
    {
        return GetKeyValues(BindingFlags.Public | BindingFlags.Static);
    }

    /// <summary>Returns a dictionary of all declared flag names and their values matching the given <paramref name="bindingFlags"/>.</summary>
    public static Dictionary<string, Flag<T>> GetKeyValues(BindingFlags bindingFlags)
    {
        return typeof(T)
            .GetFields(bindingFlags)
            .Where(IsFlagField)
            .ToDictionary(f => f.Name, f => (Flag<T>)f.GetValue(null)!);
    }

    /// <summary>Returns the declared flag with the given <paramref name="name"/>, or <c>null</c> if not found.</summary>
    public static Flag<T>? FromName(string name)
    {
        return FromName(name, BindingFlags.Public | BindingFlags.Static);
    }

    /// <summary>Returns the declared flag with the given <paramref name="name"/> using the specified <paramref name="bindingFlags"/>, or <c>null</c> if not found.</summary>
    public static Flag<T>? FromName(string name, BindingFlags bindingFlags)
    {
        var field = typeof(T).GetField(name, bindingFlags);
        return field is not null && IsFlagField(field)
            ? field.GetValue(null) as Flag<T>
            : null;
    }

    /// <summary>Tries to find a declared flag by <paramref name="name"/>. Returns <c>false</c> and sets <paramref name="flag"/> to <c>None</c> if not found.</summary>
    public static bool TryFromName(string name, out Flag<T> flag)
    {
        return TryFromName(name, BindingFlags.Public | BindingFlags.Static, out flag);
    }

    /// <inheritdoc cref="TryFromName(string, out Flag{T})"/>
    public static bool TryFromName(string name, BindingFlags bindingFlags, out Flag<T> flag)
    {
        var result = FromName(name, bindingFlags);
        flag = result ?? new Flag<T>(-1);
        return result is not null;
    }

    /// <summary>Combines all named flags into a single flag. Throws <see cref="ArgumentException"/> for unknown names.</summary>
    public static Flag<T> FromNames(params string[] names)
    {
        return FromNames((IEnumerable<string>)names);
    }

    /// <inheritdoc cref="FromNames(string[])"/>
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

    /// <summary>Tries to combine all named flags. Returns <c>false</c> and sets <paramref name="flag"/> to <c>None</c> if any name is unknown.</summary>
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

    /// <summary>Returns the union of all non-empty declared flags on <typeparamref name="T"/>.</summary>
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

    /// <summary>Restores a flag from a Base64 string. See <see cref="Flag{T}.FromBase64"/>.</summary>
    public static Flag<T> FromBase64(string base64)
    {
        return Flag<T>.FromBase64(base64);
    }

    /// <summary>Restores a flag from an ID string produced by <see cref="Flag{T}.ToId"/>.</summary>
    public static Flag<T> FromId(string id)
    {
        return Flag<T>.FromId(id);
    }

    /// <summary>Restores a flag from a scoped ID using the full type name of <typeparamref name="T"/> as scope.</summary>
    public static Flag<T> FromScopedId(string id)
    {
        return Flag<T>.FromScopedId(id);
    }

    /// <summary>Restores a flag from a scoped ID produced with the given <paramref name="scope"/>.</summary>
    public static Flag<T> FromScopedId(string id, string scope)
    {
        return Flag<T>.FromScopedId(id, scope);
    }

    private static bool IsFlagField(FieldInfo field)
    {
        return typeof(Flag<T>).IsAssignableFrom(field.FieldType);
    }
}
