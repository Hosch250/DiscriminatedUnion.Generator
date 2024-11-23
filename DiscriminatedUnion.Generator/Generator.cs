using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace DiscriminatedUnion.Generator;

[Generator]
public class DiscriminatedUnionGenerator : IIncrementalGenerator
{
    public readonly record struct DUMember(string Name, Accessibility Accessibility);
    public readonly record struct DUToGenerate(string Name, string Namespace, List<DUMember> Children, List<string> GenericTypeNames);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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
                if (attributeContainingTypeSymbol.IsDiscriminatedUnionAttribute())
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

        var genericTypeNames = new List<string>(recordSymbol.TypeArguments.Length);
        foreach (var arg in recordSymbol.TypeArguments)
        {
            genericTypeNames.Add(arg.Name);
        }

        var recordMembers = recordSymbol.GetTypeMembers();
        var members = new List<DUMember>(recordMembers.Length);
        foreach (var member in recordMembers)
        {
            members.Add(new(member.Name, member.DeclaredAccessibility));
        }

        return new DUToGenerate(recordSymbol.Name, recordSymbol.ContainingNamespace.ToDisplayString(), members, genericTypeNames);
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
    abstract partial record {GetTypeMemberName(duToGenerate)}
    {{
        private {duToGenerate.Name}() {{ }}");

        foreach (var child in duToGenerate.Children)
        {
            sb.Append(@$"

        {GetAccessibility(child.Accessibility)} sealed partial record {child.Name} : {GetTypeMemberName(duToGenerate)};
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

        static string GetTypeMemberName(DUToGenerate member) =>
            member.GenericTypeNames.Count > 0 ? $"{member.Name}<{string.Join(", ", member.GenericTypeNames)}>" : member.Name;
    }
}
