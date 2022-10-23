using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace InfiniteEnumFlags.Generator;

[Generator]
public partial class InfiniteEnumGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        //if (!Debugger.IsAttached) Debugger.Launch();

        context.RegisterForSyntaxNotifications(() => new ClassSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var receiver = (ClassSyntaxReceiver)context.SyntaxReceiver;
        foreach (var target in receiver!.TargetClasses)
        {
            var source = new StringBuilder();
            source.AppendLine("// auto-generated file");
            source.AppendLine($"namespace {GetNamespaceFrom(target)};");
            var cleanTarget = target.RemoveNodes(target.ChildNodes(), SyntaxRemoveOptions.KeepNoTrivia);
            cleanTarget = GenerateClassMembers(target, cleanTarget);
            source.AppendLine(cleanTarget.ToString());
            context.AddSource($"{target.Identifier.ToString()}.g.cs", source.ToString());
        }
    }

    private ClassDeclarationSyntax GenerateClassMembers(ClassDeclarationSyntax target,
        ClassDeclarationSyntax cleanTarget)
    {
        var itemsMethod = target.Members.OfType<MethodDeclarationSyntax>()
            .Single(q => q.Identifier.Text == "Items");
        var items = ParseEnumItems(itemsMethod) ?? new List<string>();
        var enumItems = new List<MemberDeclarationSyntax>
        {
            FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                    .WithVariables(
                        SingletonSeparatedList(VariableDeclarator(Identifier("TOTAL_ITEMS"))
                            .WithInitializer(EqualsValueClause(LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                Literal(items.Count)))))))
                .WithModifiers(TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.ConstKeyword))),
            FieldDeclaration(
                    VariableDeclaration(
                            IdentifierName("EnumItem"))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                        Identifier("None"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            ImplicitObjectCreationExpression()
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SeparatedList<ArgumentSyntax>(
                                                            new SyntaxNodeOrToken[]
                                                            {
                                                                Argument(
                                                                    LiteralExpression(
                                                                        SyntaxKind.NumericLiteralExpression,
                                                                        Literal(0))),
                                                                Token(SyntaxKind.CommaToken),
                                                                Argument(
                                                                    IdentifierName("TOTAL_ITEMS"))
                                                            }))))))))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.ReadOnlyKeyword))),
            FieldDeclaration(
                    VariableDeclaration(
                            IdentifierName("EnumItem"))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                        Identifier("All"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            PrefixUnaryExpression(
                                                SyntaxKind.BitwiseNotExpression,
                                                IdentifierName("None")))))))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.ReadOnlyKeyword)))
        };

        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < items.Count; i++)
        {
            enumItems.Add(
                FieldDeclaration(VariableDeclaration(IdentifierName("EnumItem"))
                        .WithVariables(SingletonSeparatedList(
                            VariableDeclarator(Identifier(items[i]))
                                .WithInitializer(EqualsValueClause(
                                    ImplicitObjectCreationExpression()
                                        .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]
                                            {
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression,
                                                        Literal(i + 1))),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    IdentifierName("TOTAL_ITEMS"))
                                            }))))))))
                    .WithModifiers(TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.ReadOnlyKeyword))));
        }

        return cleanTarget!.WithMembers(List(enumItems)).NormalizeWhitespace();
    }

    public List<string> ParseEnumItems(MethodDeclarationSyntax method)
    {
        return method.ReturnType.ToString() switch
        {
            "string[]" => ParseArrayItems(method),
            "Dictionary<string, int>" => ParseIndexDictionaryItems(method),
            _ => null
        };
    }

    private List<string> ParseIndexDictionaryItems(MethodDeclarationSyntax method)
    {
        var items = method
            .DescendantNodes().OfType<InitializerExpressionSyntax>()
            .Single(q => q.IsKind(SyntaxKind.CollectionInitializerExpression))
            .DescendantNodes().Where(q => q.IsKind(SyntaxKind.ComplexElementInitializerExpression));

        var lst = items.Select(item => item.DescendantNodes().OfType<LiteralExpressionSyntax>().ToArray())
            .Select(x => new
            {
                Name = x.Single(q => q.IsKind(SyntaxKind.StringLiteralExpression)).Token.Value!.ToString(),
                Index = int.Parse(x.Single(q => q.IsKind(SyntaxKind.NumericLiteralExpression)).ToString())
            }).ToList();
        return lst.OrderBy(q => q.Index).Select(q => q.Name).ToList();
    }

    public List<string> ParseArrayItems(MethodDeclarationSyntax method)
    {
        var items = method
            .DescendantNodes().OfType<InitializerExpressionSyntax>().Single()
            .DescendantTokens().Where(q => q.IsKind(SyntaxKind.StringLiteralToken));

        return items.Select(q => q.Value?.ToString()).ToList();
    }

    public static string GetNamespaceFrom(SyntaxNode s) =>
        s.Parent switch
        {
            NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax.Name.ToString(),
            FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax =>
                fileScopedNamespaceDeclarationSyntax.Name.ToString(),
            null => throw new Exception("Namespace not found"),
            _ => GetNamespaceFrom(s.Parent)
        };
}