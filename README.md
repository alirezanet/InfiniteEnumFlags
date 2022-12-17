# InfiniteEnumFlags
<!--  ![Nuget](https://img.shields.io/nuget/v/InfiniteEnumFlags?label=stable) -->
![GitHub](https://img.shields.io/github/license/alirezanet/InfiniteEnumFlags) ![Nuget](https://img.shields.io/nuget/dt/InfiniteEnumFlags?color=%239100ff) ![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/InfiniteEnumFlags?label=latest)
[![NuGet version (InfiniteEnumFlags)](https://img.shields.io/nuget/v/InfiniteEnumFlags.svg?style=flat-square)](https://www.nuget.org/packages/InfiniteEnumFlags/)
<!-- ![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/alirezanet/InfiniteEnumFlags/.github/workflows/publish.yml?branch=master) -->

*Status: (Project is in active development)*


The dotnet enum flags feature is amazing, but it is too limited 🙁. InfiniteEnumFlags is the same without limitation. 😊

## Introduction
Dotnet Enum has an `[Flags]` attribute that gives us the ability to have a binary enum system and use bitwise operators. 
However, enum specifically restricts to built-in numeric types which is a big problem because it is limited to 2^32 for `int` 
values which means we only can have a maximum of 32 items in our enum or 2^64 for `long` which limits us to a maximum of 64 items.

this library aims to remove these restrictions and still give us the same functionality.

## Getting started

To define an enum class, you can create a class/record and inherit it from `InfiniteEnum<ClassName>`,
this base class, will help you to access the list of defined enum items.
then by adding static fields of type `Flag<ClassName>` you can create your enums.
setting the values is imperative to provide values as powers of two. 

e.g

```csharp
public class Features : InfiniteEnum<Features>
{
    public static readonly Flag<Features> None = new(-1);  // 0  - 0
    public static readonly Flag<Features> F1 = new(0);     // 1  - 1
    public static readonly Flag<Features> F2 = new(1);     // 2  - 10
    public static readonly Flag<Features> F3 = new(2);     // 4  - 100
    public static readonly Flag<Features> F4 = new(3);     // 8  - 1000
    public static readonly Flag<Features> F5 = new(4);     // 16 - 10000
    public static readonly Flag<Features> F6 = new(5);     // 32 - 100000
    public static readonly Flag<Features> F7 = new(6);     // 64 - 1000000
    public static readonly Flag<Features> F8 = new(7);     // 128 - 10000000

    // We can support up to 2,147,483,647 items
}
```
 
## Usage

To use your custom enum, it is important to be familiar with the built-in dotnet enum flags capabilities
because the functionalities are almost identical. 
for example we can use all bitwise operators (`|`,`&`,`~`,`^`) in our custom enum.

e.g
```csharp
var features = Features.F1 | Features.F3;  // F1 + F3 
```

Alternatively, If you don't like bitwise Operators, you can use the Flag<> extension methods:

| Name       | Description                                                      |
|------------|------------------------------------------------------------------|
| HasFlag    | Check whatever enum has an specific flag or not, (**bitwise &**) |
| SetFlag    | Add/Set specific flag(s) to an enum, (**bitwise or**)            |
| UnsetFlag  | Remove/Unset specific flag(s) from an enum (**bitwise &~**)      |
| ToggleFlag | It toggles flag(s) from an enum (**bitwise ^**)                  |

e.g
```csharp
var features = Features.F1.SetFlag(Features.F3);
features.HasFlag(Features.F2); // false
```

### Storing Flag's value

Since we want to support more than 32 items in our enums, we can not store an integer
value, luckily we can use `ToUniqueId()` function to get a unique base64 key, and to convert it back to an
Flag, we can use `FromUniqueId()` static method.

```csharp
var features = Features.F1 | Features.F3; 
string id = features.ToUniqueId();
var new_features = Features.FromUniqueId(id); 
Console.WriteLine(features == new_features); // true
```

## Example
One of the reasonable use cases that shows why we need this library is developing a flexible authorization in asp.net core applications that [@Json Taylor](https://github.com/jasontaylordev) presented very well in [this video](https://www.youtube.com/watch?v=BVJVhceN3N4&list=PLxCsNpHrt57uwt229Frf1tYt2gywF__xg&index=1&t=1265s). However, his implementation using dotnet [Flags] had a massive limitation in big projects when we need more than 64 permissions. I provided an example of the same demo in the video using InfiniteEnumFlags that doesn't have these limitations.

To see flexible aspnetcore authorization example using InfiniteEnumFlags, first clone the repo using below command
```
 git clone --recurse-submodules https://github.com/alirezanet/InfiniteEnumFlags.git
```
then you can open `\InfiniteEnumFlags\Example\flexible-aspnetcore-authorization\FlexibleAuth\FlexibleAuth.sln` and then run the `Server` project.

Admin Username: `admin@localhost`

Auditor Username: `auditor@localhost`

Default Password: `Password123!`

## Support

- Don't forget to give a ⭐ on [GitHub](https://github.com/alirezanet/InfiniteEnumFlags)
- Share your feedback and ideas to improve this library
- Share InfiniteEnumFlags on your favorite social media and your friends
- Write a blog post about InfiniteEnumFlags

## Contribution

Feel free to send me a pull request!

## License

[MIT](https://github.com/alirezanet/InfiniteEnumFlags/blob/master/LICENSE)










