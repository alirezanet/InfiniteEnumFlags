using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace InfinateEnumFlags;

/// <summary>
/// Created on demand before each generation pass
/// </summary>
internal class ClassSyntaxReceiver : ISyntaxReceiver
{
    public readonly List<ClassDeclarationSyntax> TargetClasses = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        var lst = new List<string>()
        {
            "IArrayFlags",
            "IIndexDictionaryFlags"
        };

        if (syntaxNode is ClassDeclarationSyntax { BaseList.Types.Count: > 0 } classDeclarationSyntax &&
            classDeclarationSyntax.BaseList.Types.Any(q => lst.Contains(q.ToString())))
        {
            TargetClasses.Add(classDeclarationSyntax);
        }
    }
}