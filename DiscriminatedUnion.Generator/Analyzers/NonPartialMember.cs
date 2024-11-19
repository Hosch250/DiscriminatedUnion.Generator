using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DiscriminatedUnion.Generator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NonPartialMember : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        "DU1",
        "Non-Partial Member",
        "A DU member is not marked as partial",
        "Discriminated Union",
        DiagnosticSeverity.Warning,
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
        var recordMembers = symbol.GetTypeMembers();
        foreach (var member in recordMembers)
        {
            var syntaxNode = member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as RecordDeclarationSyntax;
            if (syntaxNode?.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)) != true)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, member.Locations.FirstOrDefault()));
            }
        }
    }
}