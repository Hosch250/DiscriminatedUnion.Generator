using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace DiscriminatedUnion.Generator;

public static class SourceGenerationHelper
{
    public const string Attribute = @"
namespace DiscriminatedUnion.Generator
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DiscriminatedUnionAttribute : System.Attribute
    {
    }
}";
}


[Generator]
public class DUGenerator : IIncrementalGenerator
{
    public readonly record struct DUToGenerate(string Name, string Namespace, List<(string Name, Accessibility Accessibility)> Children);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "DiscriminatedUnionAttribute.g.cs", SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        IncrementalValuesProvider<DUToGenerate?> dusToGenerate = context.SyntaxProvider
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
        foreach (AttributeListSyntax attributeListSyntax in recordDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == "DiscriminatedUnion.Generator.DiscriminatedUnionAttribute")
                {
                    return GetDUToGenerate(context.SemanticModel, recordDeclarationSyntax);
                }
            }
        }

        return null;
    }

    static DUToGenerate? GetDUToGenerate(SemanticModel semanticModel, RecordDeclarationSyntax recordDeclarationSyntax)
    {
        if (semanticModel.GetDeclaredSymbol(recordDeclarationSyntax) is not INamedTypeSymbol recordSymbol)
        {
            return null;
        }

        var recordMembers = recordSymbol.GetTypeMembers();
        var members = new List<(string Name, Accessibility Accessibility)>(recordMembers.Length);

        foreach (var member in recordMembers)
        {
            var syntaxNode = member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as RecordDeclarationSyntax;
            if (syntaxNode?.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)) != true)
            {
                continue;
            }

            members.Add((member.Name, member.DeclaredAccessibility));
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
        private {duToGenerate.Name}() {{}}");

        foreach (var child in duToGenerate.Children)
        {
            sb.Append(@$"

        {GetAccessibility(child.Accessibility)} sealed partial record {child.Name} : {duToGenerate.Name};
        {GetAccessibility(child.Accessibility)} bool Is{child.Name} => this is {child.Name};");
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

    }
}
