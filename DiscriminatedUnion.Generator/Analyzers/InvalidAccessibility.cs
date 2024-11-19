using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DiscriminatedUnion.Generator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvalidAccessibility : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        "DU2",
        "Invalid Accessibility",
        "A DU or one of it's members member is neither marked as public or internal",
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
                    if (attribute.AttributeClass?.ToDisplayString() == "DiscriminatedUnion.Generator.DiscriminatedUnionAttribute")
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
        if (!HasValidAccessibilityModifier(symbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations.FirstOrDefault()));
        }

        var recordMembers = symbol.GetTypeMembers();
        foreach (var member in recordMembers)
        {
            if (!HasValidAccessibilityModifier(member))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, member.Locations.FirstOrDefault()));
            }
        }

        bool HasValidAccessibilityModifier(INamedTypeSymbol symbol) =>
            symbol.DeclaredAccessibility == Accessibility.Public || symbol.DeclaredAccessibility == Accessibility.Internal;
    }
}