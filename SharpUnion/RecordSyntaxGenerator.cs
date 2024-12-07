using SharpUnion.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using Accessibility = Microsoft.CodeAnalysis.Accessibility;

namespace SharpUnion;

[Generator]
public class RecordSyntaxGenerator : IIncrementalGenerator
{
    private readonly record struct DUMember(string Name, Accessibility Accessibility);
    private readonly record struct DUToGenerate(string Name, string Namespace, EquatableArray<DUMember> Children, EquatableArray<string> GenericTypeNames, bool Serializable);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var dusToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName("SharpUnion.Shared.SharpUnionAttribute",
            predicate: (node, _) => node is RecordDeclarationSyntax,
            transform: (ctx, _) => GetSemanticTargetForGeneration(ctx));

        context.RegisterSourceOutput(dusToGenerate,
            static (spc, source) => Execute(source, spc));
    }

    private static DUToGenerate? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        var recordDeclarationSyntax = (RecordDeclarationSyntax)context.TargetNode;
        return GetDUToGenerate(context.SemanticModel, recordDeclarationSyntax, context.Attributes[0]);
    }

    private static DUToGenerate? GetDUToGenerate(SemanticModel semanticModel, RecordDeclarationSyntax recordDeclarationSyntax, AttributeData attributeData)
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
            var ignoreMember = false;
            foreach (var attribute in member.GetAttributes())
            {
                if (attribute.AttributeClass?.IsSharpUnionIgnoreAttribute() == true)
                {
                    ignoreMember = true;
                }
            }

            if (!ignoreMember)
            {
                members.Add(new(member.Name, member.DeclaredAccessibility));
            }
        }

        bool serializable = false;
        foreach (var arg in attributeData.NamedArguments)
        {
            if (arg.Key == nameof(SharpUnionAttribute.Serializable))
            {
                serializable = arg.Value.Value as bool? ?? false;
            }
        }

        return new DUToGenerate(
            recordSymbol.Name,
            recordSymbol.ContainingNamespace.ToDisplayString(),
            new EquatableArray<DUMember>([.. members]),
            new EquatableArray<string>([.. genericTypeNames]),
            serializable);
    }

    private static void Execute(DUToGenerate? duToGenerate, SourceProductionContext context)
    {
        if (duToGenerate is { } value)
        {
            string result = GenerateExtensionClass(value);
            context.AddSource($"SharpUnion.{value.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }

    private static string GenerateExtensionClass(DUToGenerate duToGenerate)
    {
        var sb = new StringBuilder();
        sb.Append($@"
namespace {duToGenerate.Namespace}
{{
    {GetJsonConverterAttribute(duToGenerate)}
    abstract partial record {GetTypeMemberName(duToGenerate)}
    {{
        [System.CodeDom.Compiler.GeneratedCode(""{AssemblyMetadata.AssemblyName}"", ""{AssemblyMetadata.AssemblyVersion}"")]
        private {duToGenerate.Name}() {{ }}");

        foreach (var child in duToGenerate.Children)
        {
            sb.Append($@"

        {GetAccessibility(child.Accessibility)} sealed partial record {child.Name} : {GetTypeMemberName(duToGenerate)};

        [System.CodeDom.Compiler.GeneratedCode(""SharpUnion"", ""{AssemblyMetadata.AssemblyVersion}"")]
        {GetAccessibility(child.Accessibility)} bool Is{child.Name} => this is {child.Name};");
        }

        if (duToGenerate.Serializable)
        {
            sb.Append(DeserializationHelper.GetJsonConverter(duToGenerate.Name));
        }

        sb.Append($"\n    }}\n}}");

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

        static string GetJsonConverterAttribute(DUToGenerate member) =>
            member.Serializable ? $"[System.Text.Json.Serialization.JsonConverter(typeof({member.Name}Converter))]" : "";
    }
}