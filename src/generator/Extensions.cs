#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace InfiniteEnumFlags.Generator;

public static class Extensions
{
    public static string? GetNamespace(this SyntaxNode s) =>
        s.Parent switch
        {
            NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax.Name.ToString(),
            FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax =>
                fileScopedNamespaceDeclarationSyntax.Name.ToString(),
            null => null,
            _ => GetNamespace(s.Parent)
        };
}