# Release Notes v0.5.0

## Breaking Changes

### 1. Scoped ID Format Change

The internal fingerprinting mechanism for scoped IDs has been updated from `ulong` to `uint` for improved efficiency and reduced payload size.

**Impact**: Scoped IDs generated with v0.4.x are **not compatible** with v0.5.0. If you have persisted scoped IDs (from `ToUniqueId` with a salt parameter), you will need to regenerate them after upgrading.

**Migration**:
- If you were using `ToUniqueId(salt)` or `FromUniqueId(id, salt)`, regenerate all stored scoped IDs after upgrading
- Regular IDs (without salt/scope) remain compatible

### 2. Method Renaming

The scoped ID methods have been renamed for clarity and consistency:

| Old Method (v0.4.x) | New Method (v0.5.0) |
|---------------------|---------------------|
| `ToUniqueId()` | `ToId()` |
| `ToUniqueId(salt)` | `ToScopedId(scope)` |
| `FromUniqueId(id)` | `FromId(id)` |
| `FromUniqueId(id, salt)` | `FromScopedId(id, scope)` |

**Migration**:
```csharp
// Before (v0.4.x)
var id = permissions.ToUniqueId();
var scoped = permissions.ToUniqueId("my-salt");
var restored = Permissions.FromUniqueId(id);
var scopedRestored = Permissions.FromUniqueId(scoped, "my-salt");

// After (v0.5.0)
var id = permissions.ToId();
var scoped = permissions.ToScopedId("my-scope");
var restored = Permissions.FromId(id);
var scopedRestored = Permissions.FromScopedId(scoped, "my-scope");
```

### 3. HasFlag Behavior Change

`HasFlag` now uses **any-overlap semantics** instead of requiring all flags to be present.

**Before (v0.4.x)**:
```csharp
var permissions = Permissions.ReadUsers | Permissions.CreateUsers;
var required = Permissions.ReadUsers | Permissions.DeleteUsers;

permissions.HasFlag(required); // false - not all flags present
```

**After (v0.5.0)**:
```csharp
var permissions = Permissions.ReadUsers | Permissions.CreateUsers;
var required = Permissions.ReadUsers | Permissions.DeleteUsers;

permissions.HasFlag(required); // true - at least one flag matches (ReadUsers)
permissions.HasAllFlags(required); // false - not all flags present
```

**Migration**: If you need the old "all flags must be present" behavior, use the new `HasAllFlags` method.

## New Features

### .NET 10 Support

Added support for .NET 10 while maintaining compatibility with:
- .NET 10
- .NET 8
- .NET 7
- .NET 6
- .NET Standard 2.1

### HasAllFlags Method

New method for strict flag checking where all requested flags must be present:

```csharp
var permissions = Permissions.ReadUsers | Permissions.CreateUsers;
var required = Permissions.ReadUsers | Permissions.CreateUsers;

permissions.HasAllFlags(required); // true - all flags present
Permissions.ReadUsers.HasAllFlags(required); // false - only one flag present
```

### Enhanced Scoped IDs

The scoped ID system has been completely redesigned:
- More efficient fingerprinting with `uint` instead of `ulong`
- Smaller payload size (4 bytes overhead instead of 8)
- Clearer API with `ToScopedId`/`FromScopedId` naming
- Better security through XOR masking

### Improved Name Parsing

Enhanced methods for working with flag names:
- Better error handling
- `TryFromName` for safe parsing
- `FromNames` for parsing multiple flag names at once

## Improvements

- Fixed flag equality and hashing edge cases
- Improved null handling throughout the API
- Defensive bit array copying to prevent mutations
- Stricter validation for all-flags checks
- Expanded test coverage for edge cases
- Updated documentation with clearer examples

## Migration Guide

### Quick Migration Steps

1. **Update NuGet package**:
   ```bash
   dotnet add package InfiniteEnumFlags --version 0.5.0
   ```

2. **Rename method calls**:
   - Replace `ToUniqueId()` with `ToId()`
   - Replace `ToUniqueId(salt)` with `ToScopedId(scope)`
   - Replace `FromUniqueId(id)` with `FromId(id)`
   - Replace `FromUniqueId(id, salt)` with `FromScopedId(id, scope)`

3. **Update HasFlag usage** (if needed):
   - Review all `HasFlag` calls
   - Replace with `HasAllFlags` where you need strict "all flags present" checking

4. **Regenerate scoped IDs**:
   - If you persisted scoped IDs with salt, regenerate them using the new `ToScopedId` method

### Breaking Change Checklist

- [ ] Updated method calls from `ToUniqueId`/`FromUniqueId` to `ToId`/`FromId`
- [ ] Updated scoped ID calls to use `ToScopedId`/`FromScopedId`
- [ ] Reviewed all `HasFlag` usage and replaced with `HasAllFlags` where needed
- [ ] Regenerated any persisted scoped IDs
- [ ] Tested flag comparison and serialization logic
- [ ] Updated documentation and examples

## Full Changelog

**Commits since v0.4.3**:
- `3feda46` - refactor: update fingerprint handling to use uint instead of ulong for improved efficiency
- `6fc9fc0` - refactor: rename ToUniqueId to ToId and update related methods for clarity and consistency
- `9b2679a` - chore: remove supported frameworks section from README
- `5cd2a2c` - feat: harden flags API and modernize project
- Multiple dependency updates and CI improvements

**GitHub Release**: https://github.com/alirezanet/InfiniteEnumFlags/releases/tag/v0.5.0

---

For questions or issues, please visit: https://github.com/alirezanet/InfiniteEnumFlags/issues
