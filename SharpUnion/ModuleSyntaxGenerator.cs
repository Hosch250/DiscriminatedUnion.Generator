using SharpUnion.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Accessibility = SharpUnion.Shared.Accessibility;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using SharpUnion.Parser;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime;

namespace SharpUnion;

[Generator]
public class ModuleSyntaxGenerator : IIncrementalGenerator
{
    private readonly record struct DUToGenerate(string Namespace, string Definition, bool Serializable, Accessibility Accessibility);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var dusToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName("SharpUnion.Shared.SharpUnionModuleAttribute",
            predicate: (node, _) => true,
            transform: (ctx, _) => GetSemanticTargetForGeneration(ctx));

        context.RegisterSourceOutput(dusToGenerate,
            static (spc, source) => Execute(source, spc));
    }

    private static EquatableArray<DUToGenerate>? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        var node = (CompilationUnitSyntax)context.TargetNode;

        var sourceSymbol = context.TargetSymbol as ISourceAssemblySymbol;
        if (sourceSymbol is null)
        {
            return null;
        }

        var attributes = new List<DUToGenerate>();
        foreach (var module in sourceSymbol.Modules)
        {
            var moduleAttributes = module.GetAttributes();
            foreach (var attribute in moduleAttributes)
            {
                if (attribute.AttributeClass?.IsSharpUnionModuleAttribute() == true)
                {
                    attributes.Add(GetDUToGenerate(attribute));
                }
            }
        }

        return new EquatableArray<DUToGenerate>([.. attributes]);
    }

    private static DUToGenerate GetDUToGenerate(AttributeData attribute)
    {
        var nameSpace = "";
        var declaration = "";
        var serializable = false;
        var accessibility = Accessibility.Internal;

        foreach (var arg in attribute.NamedArguments)
        {
            switch (arg.Key)
            {
                case nameof(SharpUnionModuleAttribute.Serializable):
                    serializable = arg.Value.Value as bool? ?? false;
                    break;

                case nameof(SharpUnionModuleAttribute.Accessibility):
                    accessibility = arg.Value.Value as Accessibility? ?? Accessibility.Internal;
                    break;
            };
        }

        for (int i = 0; i < attribute.ConstructorArguments.Length; i++)
        {
            var arg = attribute.ConstructorArguments[i];
            switch (i)
            {
                case 0:
                    nameSpace = (string)arg.Value!;
                    break;

                case 1:
                    declaration = (string)arg.Value!;
                    break;
            }
        }

        return new DUToGenerate(
            nameSpace,
            declaration,
            serializable,
            accessibility);
    }

    private static void Execute(EquatableArray<DUToGenerate>? dusToGenerate, SourceProductionContext context)
    {
        if (dusToGenerate is { } value)
        {
            foreach (var du in value)
            {
                var parseResult = Parse(du.Definition);
                var (typeName, output) = ParseToCSharpMapper.Map(parseResult, du.Namespace, du.Accessibility);

                if (du.Serializable)
                {
                    output = output
                        .Replace("@@UnionAttributes@@", $"\n    [System.Text.Json.Serialization.JsonConverter(typeof({typeName}Converter))]")
                        .Replace("@@UnionMembers@@", GetJsonConverter(typeName));
                }
                else
                {
                    output = output
                        .Replace("@@UnionAttributes@@", string.Empty)
                        .Replace("@@UnionMembers@@", string.Empty);
                }

                context.AddSource($"SharpUnion.{typeName}.g.cs", SourceText.From(output, Encoding.UTF8));
            }
        }

        static UnionParser.UnionStmtContext Parse(string input)
        {
            var stream = new AntlrInputStream(input);
            var lexer = new UnionLexer(stream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new UnionParser(tokens)
            {
                ErrorHandler = new BailErrorStrategy()
            };
            parser.Interpreter.PredictionMode = PredictionMode.Sll;
            var tree = parser.unionStmt();
            return tree;
        }

        static string GetJsonConverter(string typeName) =>
            $@"

                [SharpUnion.Shared.SharpUnionIgnore]
                [System.CodeDom.Compiler.GeneratedCode(""{AssemblyMetadata.AssemblyName}"", ""{AssemblyMetadata.AssemblyVersion}"")]
                private sealed class {typeName}Converter : System.Text.Json.Serialization.JsonConverter<{typeName}>
                {{
                    public override {typeName}? Read(ref System.Text.Json.Utf8JsonReader reader, System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
                    {{
                        var node = System.Text.Json.Nodes.JsonNode.Parse(ref reader);
                        foreach (var prop in typeof({typeName}).GetProperties())
                        {{
                            var value = node?[prop.Name];
                            if (value?.GetValueKind() == System.Text.Json.JsonValueKind.True)
                            {{
                                var type = typeof({typeName}).GetNestedType(prop.Name[2..]);
                                if (type is not null)
                                {{
                                    return System.Text.Json.JsonSerializer.Deserialize(node, type) as {typeName};
                                }}
                            }}
                        }}

                        return null;
                    }}

                    public override void Write(System.Text.Json.Utf8JsonWriter writer, {typeName} value, System.Text.Json.JsonSerializerOptions options)
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