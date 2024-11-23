using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DiscriminatedUnion.Generator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MismatchedAccessibility : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        "DU3",
        "Mismatched Accessibility",
        "A DU member is marked with a different accessibility modifier than the parent",
        "Discriminated Union",
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
                    if (attribute.AttributeClass?.IsDiscriminatedUnionAttribute() == true)
                    {
                        context.RegisterSymbolEndAction(context => Analyze(context, symbol));
                        return;
                    }
                }
            }
        }, SymbolKind.NamedType);
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol symbol)
    {
        var expectedAccessibility = symbol.DeclaredAccessibility;

        var recordMembers = symbol.GetTypeMembers();
        foreach (var member in recordMembers)
        {
            foreach (var attribute in member.GetAttributes())
            {
                if (attribute.AttributeClass?.IsDiscriminatedUnionIgnoreAttribute() == true)
                {
                    continue;
                }
            }

            if (member.DeclaredAccessibility != expectedAccessibility)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, member.Locations.FirstOrDefault()));
            }
        }
    }
}