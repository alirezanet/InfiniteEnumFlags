using Microsoft.CodeAnalysis;


namespace InfiniteEnumFlags.Generator;

[Generator]
public class InfiniteEnumGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        //if (!Debugger.IsAttached) Debugger.Launch();

        context.RegisterForSyntaxNotifications(() => new MainSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var receiver = (MainSyntaxReceiver)context.SyntaxReceiver;

        foreach (var classes in receiver!.ClassSyntax.Sources)
        {
            context.AddSource($"{classes.FileName}.g.cs", classes.Code);
        }

        foreach (var enums in receiver!.EnumSyntax.Sources)
        {
            context.AddSource($"{enums.FileName}.g.cs", enums.Code);
        }
    }
}
