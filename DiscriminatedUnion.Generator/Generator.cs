using DiscriminatedUnion.Generator.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;
using System.Text;

namespace DiscriminatedUnion.Generator;

[Generator]
public class DiscriminatedUnionGenerator : IIncrementalGenerator
{

    static readonly string assemblyVersion = Assembly.GetAssembly(typeof(DiscriminatedUnionGenerator)).GetName().Version.ToString(3);

    internal readonly record struct DUMember(string Name, Accessibility Accessibility);
    internal readonly record struct DUToGenerate(string Name, string Namespace, EquatableArray<DUMember> Children, EquatableArray<string> GenericTypeNames, bool Serializable);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var dusToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName("DiscriminatedUnion.Generator.Shared.DiscriminatedUnionAttribute",
            predicate: (node, _) => node is RecordDeclarationSyntax,
            transform: (ctx, _) => GetSemanticTargetForGeneration(ctx));

        context.RegisterSourceOutput(dusToGenerate,
            static (spc, source) => Execute(source, spc));
    }

    internal static DUToGenerate? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        var recordDeclarationSyntax = (RecordDeclarationSyntax)context.TargetNode;

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

    internal static DUToGenerate? GetDUToGenerate(SemanticModel semanticModel, RecordDeclarationSyntax recordDeclarationSyntax)
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
            foreach (var attribute in member.GetAttributes())
            {
                if (attribute.AttributeClass?.IsDiscriminatedUnionIgnoreAttribute() == true)
                {
                    continue;
                }
            }

            members.Add(new(member.Name, member.DeclaredAccessibility));
        }

        bool serializable = false;
        foreach (var attribute in recordSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.IsDiscriminatedUnionAttribute() == true)
            {
                foreach (var arg in attribute.NamedArguments)
                {
                    if (arg.Key == nameof(DiscriminatedUnionAttribute.Serializable))
                    {
                        serializable = arg.Value.Value as bool? ?? false;
                    }
                }
            }
        }

        return new DUToGenerate(
            recordSymbol.Name,
            recordSymbol.ContainingNamespace.ToDisplayString(),
            new EquatableArray<DUMember>([.. members]),
            new EquatableArray<string>([.. genericTypeNames]),
            serializable);
    }

    internal static void Execute(DUToGenerate? duToGenerate, SourceProductionContext context)
    {
        if (duToGenerate is { } value)
        {
            string result = GenerateExtensionClass(value);
            context.AddSource($"DiscriminatedUnion.{value.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }

    internal static string GenerateExtensionClass(DUToGenerate duToGenerate)
    {
        var sb = new StringBuilder();
        sb.Append(@$"
namespace {duToGenerate.Namespace}
{{
    {GetJsonConverterAttribute(duToGenerate)}
    abstract partial record {GetTypeMemberName(duToGenerate)}
    {{
        [System.CodeDom.Compiler.GeneratedCode(""DiscriminatedUnion.Generator"", ""{assemblyVersion}"")]
        private {duToGenerate.Name}() {{ }}");

        foreach (var child in duToGenerate.Children)
        {
            sb.Append(@$"

        {GetAccessibility(child.Accessibility)} sealed partial record {child.Name} : {GetTypeMemberName(duToGenerate)};

        [System.CodeDom.Compiler.GeneratedCode(""DiscriminatedUnion.Generator"", ""{assemblyVersion}"")]
        {GetAccessibility(child.Accessibility)} bool Is{child.Name} => this is {child.Name};");
        }

        if (duToGenerate.Serializable)
        {
            sb.Append(GetJsonConverter(duToGenerate));
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

        static string GetJsonConverter(DUToGenerate member) =>
            @$"

        [DiscriminatedUnion.Generator.Shared.DiscriminatedUnionIgnore]
        [System.CodeDom.Compiler.GeneratedCode(""DiscriminatedUnion.Generator"", ""{assemblyVersion}"")]
        private sealed class {member.Name}Converter : System.Text.Json.Serialization.JsonConverter<{member.Name}>
        {{
            public override {member.Name}? Read(ref System.Text.Json.Utf8JsonReader reader, System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
            {{
                var node = System.Text.Json.Nodes.JsonNode.Parse(ref reader);
                foreach (var prop in typeof({member.Name}).GetProperties())
                {{
                    var value = node?[prop.Name];
                    if (value?.GetValueKind() == System.Text.Json.JsonValueKind.True)
                    {{
                        var type = typeof({member.Name}).GetNestedType(prop.Name[2..]);
                        if (type is not null)
                        {{
                            return System.Text.Json.JsonSerializer.Deserialize(node, type) as {member.Name};
                        }}
                    }}
                }}

                return null;
            }}

            public override void Write(System.Text.Json.Utf8JsonWriter writer, {member.Name} value, System.Text.Json.JsonSerializerOptions options)
            {{
                writer.WriteStartObject();

                foreach (var prop in value.GetType().GetProperties())
                {{
                    string propertyName = prop.Name;

                    var val = value.GetType().GetProperty(propertyName)?.GetValue(value);
                    if (val is not null)
                    {{
                        writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);
                        System.Text.Json.JsonSerializer.Serialize(writer, val, options);
                    }}
                    else
                    {{
                        writer.WriteNull(propertyName);
                    }}
                }}

                writer.WriteEndObject();
            }}
        }}";
    }
}