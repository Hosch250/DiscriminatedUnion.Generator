using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace DiscriminatedUnion.Generator;

[Generator]
public class DiscriminatedUnionGenerator : IIncrementalGenerator
{
    public readonly record struct DUMember(string Name, Accessibility Accessibility, bool IsGenericType, List<string> GenericTypeNames);
    public readonly record struct DUToGenerate(string Name, string Namespace, List<DUMember> Children);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "DiscriminatedUnionAttribute.g.cs", SourceText.From(AttributeHelper.Attribute, Encoding.UTF8)));

        var dusToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(dusToGenerate,
            static (spc, source) => Execute(source, spc));
    }

    static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is RecordDeclarationSyntax m && m.AttributeLists.Count > 0;

    static DUToGenerate? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a RecordDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        var recordDeclarationSyntax = (RecordDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (var attributeListSyntax in recordDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                if (IsDiscriminatedUnionAttribute(attributeContainingTypeSymbol))
                {
                    return GetDUToGenerate(context.SemanticModel, recordDeclarationSyntax);
                }
            }
        }

        return null;

        static bool IsDiscriminatedUnionAttribute(ITypeSymbol typeSymbol) =>
            typeSymbol is INamedTypeSymbol
            {
                MetadataName: "DiscriminatedUnionAttribute",
                ContainingNamespace:
                {
                    Name: "Generator",
                    ContainingNamespace:
                    {
                        Name: "DiscriminatedUnion",
                        ContainingNamespace.IsGlobalNamespace: true
                    }
                }
            };
    }

    static DUToGenerate? GetDUToGenerate(SemanticModel semanticModel, RecordDeclarationSyntax recordDeclarationSyntax)
    {
        if (semanticModel.GetDeclaredSymbol(recordDeclarationSyntax) is not INamedTypeSymbol recordSymbol)
        {
            return null;
        }

        var recordMembers = recordSymbol.GetTypeMembers();
        var members = new List<DUMember>(recordMembers.Length);

        foreach (var member in recordMembers)
        {
            var syntaxNode = member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as RecordDeclarationSyntax;

            var genericTypeNames = new List<string>(member.TypeArguments.Length);
            foreach (var arg in member.TypeArguments)
            {
                genericTypeNames.Add(arg.Name);
            }

            members.Add(new(member.Name, member.DeclaredAccessibility, member.IsGenericType, genericTypeNames));
        }

        return new DUToGenerate(recordSymbol.Name, recordSymbol.ContainingNamespace.ToDisplayString(), members);
    }

    static void Execute(DUToGenerate? duToGenerate, SourceProductionContext context)
    {
        if (duToGenerate is { } value)
        {
            string result = GenerateExtensionClass(value);
            context.AddSource($"DiscriminatedUnion.{value.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }

    public static string GenerateExtensionClass(DUToGenerate duToGenerate)
    {
        var sb = new StringBuilder();
        sb.Append(@$"
namespace {duToGenerate.Namespace}
{{
    abstract partial record {duToGenerate.Name}
    {{
        private {duToGenerate.Name}() {{ }}");

        foreach (var child in duToGenerate.Children)
        {
            sb.Append(@$"

        {GetAccessibility(child.Accessibility)} sealed partial record {GetTypeMemberName(child)} : {duToGenerate.Name};
        {GetAccessibility(child.Accessibility)} bool Is{GetTypeMemberName(child)}() => this is {GetTypeMemberName(child)};");
        }

        sb.Append("\n    }\n}");

        return sb.ToString();

        static string GetAccessibility(Accessibility accessibility) =>
            accessibility switch
            {
                Accessibility.NotApplicable => "internal",
                Accessibility.Private => "private",
                Accessibility.ProtectedAndInternal => "private protected",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.Public => "public",
                _ => throw new NotImplementedException()
            };

        static string GetTypeMemberName(DUMember member) =>
            member.IsGenericType ? $"{member.Name}<{string.Join(", ", member.GenericTypeNames)}>" : member.Name;
    }
}
