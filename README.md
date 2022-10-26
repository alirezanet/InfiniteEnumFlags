# InfiniteEnumFlags
<!--  ![Nuget](https://img.shields.io/nuget/v/InfiniteEnumFlags?label=stable) -->
![GitHub](https://img.shields.io/github/license/alirezanet/InfiniteEnumFlags) ![Nuget](https://img.shields.io/nuget/dt/InfiniteEnumFlags?color=%239100ff) ![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/InfiniteEnumFlags?label=latest) ![GitHub Workflow Status](https://img.shields.io/github/workflow/status/alirezanet/InfiniteEnumFlags/Publish%20Packages?label=checks)
[![NuGet version (InfiniteEnumFlags)](https://img.shields.io/nuget/v/InfiniteEnumFlags.svg?style=flat-square)](https://www.nuget.org/packages/InfiniteEnumFlags/)

*Status: (Project is in active development)*


The dotnet enum flags feature is amazing, but it is too limited 🙁. InfiniteEnumFlags is the same without limitation. 😊

## Introduction
Dotnet Enum has an `[Flags]` attribute that gives us the ability to have a binary enum system and use bitwise operators. 
However, enum specifically restricts to built-in numeric types which is a big problem because it is limited to 2^32 for `int` 
values which means we only can have a maximum of 32 items in our enum or 2^64 for `long` which limits us to a maximum of 64 items.

this library aims to remove these restrictions and still give us the same functionality.

## Getting started
After installing the InfiniteEnumFlags NuGet package, there are several ways to use this package. I start with the easiest one.
### Define your enum flags

#### 1. using source generator (Recommended)

To define your enum, you must create a `partial` class and extend it using `IArrayFlags` or `IIndexDictionaryFlags` and Implement the `Items` function that returns a list of strings.

`IArrayFlags`e.g.
``` csharp
public partial class FeaturesEnum : IArrayFlags
{
    public string[] Items() => new[]
    {
    //  Name -- Value - Index - Bits 
        "F1",  // 1   -   0   - 0001
        "F2",  // 2   -   1   - 0010
        "F3",  // 4   -   2   - 0100
        "F4",  // 8   -   3   - 1000
    };
}
```

In this example, F1-F4 are the enum items that give you binary sequence values using the source generator.

**Note: remember, the item's order when using `IArrayFlags` is Important**

after creating this class the below code will be generated in the background that you can use to work with your Enums.

```csharp
public partial class FeaturesEnum
{
    public const int TOTAL_ITEMS = 4;
    public static readonly EnumItem None = new(0, TOTAL_ITEMS);
    public static readonly EnumItem All = ~None;
    public static readonly EnumItem F1 = new(1, TOTAL_ITEMS);
    public static readonly EnumItem F2 = new(2, TOTAL_ITEMS);
    public static readonly EnumItem F3 = new(3, TOTAL_ITEMS);
    public static readonly EnumItem F4 = new(4, TOTAL_ITEMS);
}
```

---

You can use the `IIndexDictionaryFlags` instead of `IArrayFlags` if you wanna take control of the item's order and values.

`IIndexDictionaryFlags`e.g
```csharp
public partial class FeaturesEnum : IIndexDictionaryFlags
{
    public Dictionary<string, int> Items() => new()
    {
      // Name, Order     Index - Value - Bits
        { "F1", 2 }, //    2   -   4   - 100
        { "F2", 0 }, //    0   -   1   - 001
        { "F3", 1 }  //    1   -   2   - 010
    };
}
```

#### 2. Manual
In the previous example we saw the generated code using source generator. 
The second way of creating Enums is to manually create this class which gives us the same
functionality. but I believe it is harder to manage. 

## Usage

To use your custom enum, it is important to be familiar with the built-in dotnet enum flags capabilities
because the functionalities are almost identical. 
for example we can use all bitwise operators (`|`,`&`,`~`,`^`) in our custom enum.

e.g
```csharp
var features = FeaturesEnum.F1 | FeaturesEnum.F3;  // F1 + F3 
```

Alternatively, If you don't like bitwise Operators, you can use the EnumItem extension methods:

| Name       | Description                                                      |
|------------|------------------------------------------------------------------|
| HasFlag    | Check whatever enum has an specific flag or not, (**bitwise &**) |
| SetFlag    | Add/Set specific flag(s) to an enum, (**bitwise or**)            |
| UnsetFlag  | Remove/Unset specific flag(s) from an enum (**bitwise &~**)      |
| ToggleFlag | It toggles flag(s) from an enum (**bitwise ^**)                  |

e.g
```csharp
features.HasFlag(FeaturesEnum.F2); // false
```

### Storing EnumItem's value

Since we want to support more than 32 items in our enums, we can not store an integer
value, luckily we can use EnumItem `ToBase64Key()` function to get a unique base64 key, and to convert it back to an
EnumItem we can use `EnumItem.FromBase64()` static method.

```csharp
var features = FeaturesEnum.F1.SetFlag(FeaturesEnum.F3); 
var key = features.ToBase64Key();
var new_features = EnumItem.FromBase64(key); 
Console.WriteLine(features == new_features); // true
```

## Support

- Don't forget to give a ⭐ on [GitHub](https://github.com/alirezanet/InfiniteEnumFlags)
- Share your feedback and ideas to improve this tool
- Share InfiniteEnumFlags on your favorite social media and your friends
- Write a blog post about InfiniteEnumFlags

## Contribution

Feel free to send me a pull request!

## License

[MIT](https://github.com/alirezanet/InfiniteEnumFlags/blob/master/LICENSE)










