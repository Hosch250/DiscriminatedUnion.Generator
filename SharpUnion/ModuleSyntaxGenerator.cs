using SharpUnion.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Accessibility = SharpUnion.Shared.Accessibility;
using Microsoft.CodeAnalysis.Text;
using System.Text;

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
        var attributes = new List<DUToGenerate>();
        foreach (var attribute in context.Attributes)
        {
            attributes.Add(GetDUToGenerate(attribute));
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
                    accessibility = (Accessibility)(arg.Value.Value as int? ?? 0);
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
                try
                {
                    var (typeName, output) = ParseToCSharpMapper.Map(du.Namespace, du.Definition, du.Accessibility);

                    if (du.Serializable)
                    {
                        output = output
                            .Replace("@@UnionAttributes@@", $"\n    [System.Text.Json.Serialization.JsonConverter(typeof({typeName}Converter))]")
                            .Replace("@@UnionMembers@@", DeserializationHelper.GetJsonConverter(typeName));
                    }
                    else
                    {
                        output = output
                            .Replace("@@UnionAttributes@@", string.Empty)
                            .Replace("@@UnionMembers@@", string.Empty);
                    }

                    context.AddSource($"SharpUnion.{typeName}.g.cs", SourceText.From(output, Encoding.UTF8));
                }
                catch { }
            }
        }
    }
}