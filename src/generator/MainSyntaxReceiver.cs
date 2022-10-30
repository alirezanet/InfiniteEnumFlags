using Microsoft.CodeAnalysis;
namespace InfiniteEnumFlags.Generator;

internal class MainSyntaxReceiver : ISyntaxReceiver
{
    public ClassSyntaxReceiver ClassSyntax { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        ClassSyntax.OnVisitSyntaxNode(syntaxNode);
    }
}