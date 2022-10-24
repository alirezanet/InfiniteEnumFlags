# InfiniteEnumFlags

The dotnet enum flags feature is amazing, but it is too limited 🙁. InfiniteEnumFlags is the same without limitation. 😊

## Introduction
Dotnet Enum has a `[Flags]` attribute that gives up the ability to have a binary enum system and use bitwise operators.
However, enum specifically restricts to built-in numeric types which is a big problem 
because it is limited to 2^32 for `int` values that mean we only can have a maximum of 32 items
in our enum or 2^64 for `long` that limits us to a maximum of 64 items. 

this library aims to remove this restrictions and still give us the same functionality.


## Getting started
After installing the InfiniteEnumFlags NuGet package, there are several ways to use this package. I start with the easiest one.
### Define your enum flags

#### 1. using source generator (Recommended)

To define your enum, you must create a `partial` class and extend it using `IArrayFlags` or `IIndexDictionaryFlags` and Implement the `Items` function that returns a list of strings.

`IArrayFlags`e.g.
``` csharp
public partial class YourCustomEnumName : IArrayFlags
{
    public string[] Items() => new[]
    {
    //  Name -- Value 
        "F1",  // 1   
        "F2",  // 2
        "F3",  // 4
        "F4",  // 8
    };
}
```

In this example, F1-F4 are the enum items that give you binary sequence values using the source generator.

**Note: remember, the item's order when using `IArrayFlags` is Important**

after creating this class the below code will be generated in the background that you can use to work with your Enums.

```csharp
public partial class YourCustomEnumName
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

#### 2. Manual
In the previous example we saw the generated code using source generator. 
The second way of creating Enums is to manually create this class which gives us the same
functionality. but I believe it is harder to manage. 


### Usage

To use your custom enum, it is important to be familiar with the built-in enum flags capabilities
because the functionalities are almost identical. 
for example we can use all bitwise operators (`|`,`&`,`~`,`^`) in our custom enum.

e.g
```csharp
var features = YourCustomEnumName.F1 | YourCustomEnumName.F3;  // (+) F1 + F3 
```












