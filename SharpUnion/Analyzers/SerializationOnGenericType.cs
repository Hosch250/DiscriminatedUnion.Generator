using System.Collections.Immutable;
using SharpUnion.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SharpUnion.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SerializationOnGenericType : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        "DU4",
        "Serialization flag on Generic DU",
        "Serialization is not supported on generic DU types.",
        "SharpUnion",
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterSymbolStartAction(context =>
        {
            if (context.Symbol is INamedTypeSymbol symbol)
            {
                foreach (var attribute in symbol.GetAttributes())
                {
                    if (attribute.AttributeClass?.IsSharpUnionAttribute() == true)
                    {
                        foreach (var arg in attribute.NamedArguments)
                        {
                            if (arg.Key == nameof(SharpUnionAttribute.Serializable))
                            {
                                if (arg.Value.Value as bool? ?? false)
                                {
                                    context.RegisterSymbolEndAction(context => Analyze(context, symbol));
                                }
                            }
                        }

                        return;
                    }
                }
            }
        }, SymbolKind.NamedType);
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol symbol)
    {
        if (symbol.TypeArguments.Length > 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations.FirstOrDefault()));
        }
    }
}