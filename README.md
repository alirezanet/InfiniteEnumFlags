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
| `\|` | add/combine flags | `ReadUsers \| CreateUsers` |
| `&` | keep only shared flags | `permissions & required` |
| `^` | toggle flags | `permissions ^ DeleteUsers` |
| `~` | invert known bits in the current value length | `~permissions` |

The helper methods mirror the common operators:

| Method | Operator | Meaning |
|---|---|---|
| `SetFlag` | `\|` | add flags |
| `UnsetFlag` | `&~` | remove flags |
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

Use `ToId` when you need a compact string value for storage. IDs are URL-safe,
filename-safe, and database-friendly (alphabet `A-Z a-z 0-9 - _`, plus literal
`"0"` for `None`).

```csharp
var permissions = Permissions.ReadUsers | Permissions.ViewReports;

string id = permissions.ToId();
var restored = Permissions.FromId(id);

Console.WriteLine(permissions == restored); // true
```

### Guarantees

- **Canonical.** Equal flags always produce the same ID, regardless of the
  internal bit length they were constructed with.
- **Round-trip safe.** `Flag.FromId(flag.ToId()).Equals(flag)` for every value.
- **Unique.** Different flag values always produce different IDs.
- **Compact even at high indices.** The encoder picks between a dense byte
  representation and a sparse delta-varint representation per value, whichever
  is shorter. High-index flags don't blow up the ID size.

| Value | ID |
|---|---|
| `None` | `0` |
| `ReadUsers` (bit 0) | `AAE` |
| `CreateUsers` (bit 1) | `AAI` |
| `ReadUsers \| CreateUsers` | `AAM` |
| `DeleteUsers` (bit 2) | `AAQ` |
| `ViewReports` (bit 100) | `AWQ` |

A flag at bit `10000` is roughly `4` characters, not `~1700`. This makes
`InfiniteEnumFlags` safe to use as a primary key, query parameter, or document
identifier even for very sparse, high-index flag sets.

> **Wire format**: `[varint K][body]`. `K == 0` means dense canonical bytes
> follow. `K > 0` means `K` delta-encoded bit indices follow. The format is
> self-describing — the decoder doesn't need a tag byte.

For plain padded base64 storage (e.g. when integrating with systems that already
expect base64), use `ToBase64String`, `ToBase64Trimmed`, and `FromBase64`.

### Scoped IDs

`ToId` stores only the flag value. That keeps IDs tiny and close to native enum
behavior, but it also means an ID from one enum class can be syntactically valid
for another.

If values from different enum classes may share the same database column, queue,
or API field — or if you want to detect IDs that were generated for a different
context — use scoped IDs:

```csharp
string id = permissions.ToScopedId();
var restored = Permissions.FromScopedId(id);
```

The default scope is the enum class name. You can override it when multiple enum
classes should intentionally share the same ID space, or when you want to bind
IDs to a logical version:

```csharp
string id = permissions.ToScopedId("permissions-v1");
var restored = Permissions.FromScopedId(id, "permissions-v1");
```

A scoped ID carries a 2-byte scope fingerprint and a masked payload, so:

- The raw value ID does **not** appear verbatim inside a scoped ID.
- `FromScopedId` throws `InvalidOperationException` when the scope doesn't match,
  giving you a cheap sanity check against ID misuse across contexts.

Scoped IDs are not a security boundary — they are a tamper-evident routing tag.
Anything that needs cryptographic integrity should be signed separately.

## Performance notes

- Storage is a packed `ulong[]`, canonicalized so trailing zero words are
  trimmed. `Flag(5, length: 10000)` and `Flag(5)` share the same single-word
  representation.
- Bitwise operators (`|`, `&`, `^`, `~`) operate 64 bits at a time and the JIT
  auto-vectorizes the loops on x86 (AVX2) and ARM64 (NEON).
- `Count` uses hardware `POPCNT` via `BitOperations.PopCount`.
- `IsEmpty` is `O(1)`.
- Equality is a single SIMD `Span<ulong>.SequenceEqual`.
- Set-bit enumeration uses `TrailingZeroCount` — one CPU instruction per set bit
  and zero work for empty words. This makes the sparse ID encoder cheap even for
  very wide flag sets.

## Notes

- `Flag<T>` values are immutable from public APIs.
- Equality is canonical: equal logical flags always compare equal *and* hash
  equal, regardless of how they were constructed.
- `None` is an empty flag set.
- `HasFlag(None)` returns `false` because there are no bits to overlap.
- `HasAllFlags(None)` returns `true`, because an empty requirement is always
  satisfied.

## Supported targets

`net6.0`, `net7.0`, `net8.0`, `net10.0`.

## License

[MIT](https://github.com/alirezanet/InfiniteEnumFlags/blob/master/LICENSE)
