# InfiniteEnumFlags

![GitHub](https://img.shields.io/github/license/alirezanet/InfiniteEnumFlags)
![Nuget](https://img.shields.io/nuget/dt/InfiniteEnumFlags?color=%239100ff)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/InfiniteEnumFlags?label=latest)
[![NuGet version (InfiniteEnumFlags)](https://img.shields.io/nuget/v/InfiniteEnumFlags.svg?style=flat-square)](https://www.nuget.org/packages/InfiniteEnumFlags/)

InfiniteEnumFlags gives you `[Flags]`-style behavior without the 32-bit or 64-bit limit of built-in .NET enums.

Use it when native C# flags are the right model, but `int`, `long`, or `ulong` are too small. This is common for permissions, feature switches, policy rules, and other systems where the list can grow past 64 values.

You still get named values, bitwise operators, and compact storage, but the flags are backed by a growable bit array instead of a fixed-size numeric enum.

## Install

```bash
dotnet add package InfiniteEnumFlags
```

## Supported frameworks

The package targets:

- `net10.0`
- `net8.0`
- `net7.0`
- `net6.0`
- `netstandard2.1`

## Define flags

Create a class that inherits from `InfiniteEnum<T>` and expose each value as a static `Flag<T>`.

`-1` is reserved for `None`. Every other number is the zero-based bit index.

```csharp
using InfiniteEnumFlags;

public sealed class Permissions : InfiniteEnum<Permissions>
{
    public static readonly Flag<Permissions> None = new(-1);      // 0      -> 0
    public static readonly Flag<Permissions> ReadUsers = new(0);  // 1      -> 1
    public static readonly Flag<Permissions> CreateUsers = new(1); // 2    -> 10
    public static readonly Flag<Permissions> DeleteUsers = new(2); // 4    -> 100
    public static readonly Flag<Permissions> ViewReports = new(100);
}
```

The constructor value is the bit index, not the decimal value. For example, `new(2)` means "turn on bit 2", which is the same idea as `1 << 2`.

| Flag | Index | Decimal value | Binary shape |
|---|---:|---:|---|
| `None` | `-1` | `0` | `0` |
| `ReadUsers` | `0` | `1` | `1` |
| `CreateUsers` | `1` | `2` | `10` |
| `DeleteUsers` | `2` | `4` | `100` |
| `ViewReports` | `100` | very large | bit 100 is on |

Native `[Flags]` enums store this shape inside an `int`, `long`, or `ulong`. InfiniteEnumFlags stores the same shape in a growable bit array, so high indexes like `100`, `500`, or `10_000` are still valid.

## Combine and check flags

You can use familiar bitwise operators.

```csharp
var permissions = Permissions.ReadUsers | Permissions.CreateUsers;
// binary: 1 | 10 = 11

bool canRead = permissions.HasFlag(Permissions.ReadUsers); // true
bool canDelete = permissions.HasFlag(Permissions.DeleteUsers); // false
```

Supported operators:

| Operator | Meaning | Example |
|---|---|---|
| `|` | add/combine flags | `ReadUsers | CreateUsers` |
| `&` | keep only shared flags | `permissions & required` |
| `^` | toggle flags | `permissions ^ DeleteUsers` |
| `~` | invert known bits in the current value length | `~permissions` |

The helper methods mirror the common operators:

| Method | Operator | Meaning |
|---|---|---|
| `SetFlag` | `|` | add flags |
| `UnsetFlag` | `& ~` | remove flags |
| `ToggleFlag` | `^` | toggle flags |

`HasFlag` checks for any overlap. This is the default because most permission-style checks ask: "does this value contain at least one of these flags?"

```csharp
var required = Permissions.ReadUsers | Permissions.CreateUsers;

permissions.HasFlag(required); // true
Permissions.ReadUsers.HasFlag(required); // true
```

Use `HasAllFlags` when every requested flag must be present:

```csharp
permissions.HasAllFlags(required); // true
Permissions.ReadUsers.HasAllFlags(required); // false
```

## Set, unset, and toggle

```csharp
var permissions = Permissions.None
    .SetFlag(Permissions.ReadUsers, Permissions.ViewReports);

permissions = permissions.UnsetFlag(Permissions.ViewReports);
permissions = permissions.ToggleFlag(Permissions.DeleteUsers);
```

## Work with names

`InfiniteEnum<T>` can read the static flag fields you define.

```csharp
var names = Permissions.GetNames().ToList();
// None, ReadUsers, CreateUsers, DeleteUsers, ViewReports

var selectedNames = Permissions.GetNames(
    Permissions.ReadUsers | Permissions.DeleteUsers);
// ReadUsers, DeleteUsers

var read = Permissions.FromName("ReadUsers");

var combined = Permissions.FromNames("ReadUsers", "ViewReports");

if (Permissions.TryFromName("DeleteUsers", out var deleteUsers))
{
    // use deleteUsers
}
```

`All` returns all non-empty flags:

```csharp
var allPermissions = Permissions.All;
```

## Store and restore values

Use `ToUniqueId` when you need a compact string value for storage.

```csharp
var permissions = Permissions.ReadUsers | Permissions.ViewReports;

string id = permissions.ToUniqueId();
var restored = Permissions.FromUniqueId(id);

Console.WriteLine(permissions == restored); // true
```

You can also add a salt:

```csharp
string id = permissions.ToUniqueId("my-app");
var restored = Permissions.FromUniqueId(id, "my-app");
```

For plain base64 storage, use `ToBase64String`, `ToBase64Trimmed`, and `FromBase64`.

## Notes

- `Flag<T>` values are immutable from public APIs.
- Equality ignores trailing zero bits, so the same logical flags compare equal even if they were created with different internal lengths.
- `None` is an empty flag set.
- `HasFlag(None)` returns `false` because there are no bits to overlap.
- `HasAllFlags(None)` returns `true`, because an empty requirement is always satisfied.

## License

[MIT](https://github.com/alirezanet/InfiniteEnumFlags/blob/master/LICENSE)
