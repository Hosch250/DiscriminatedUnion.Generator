using System.Collections.Immutable;
using SharpUnion.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace SharpUnion.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvalidDeclaration : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        "DU5",
        "Invalid declaration of DU",
        "DU declaration not formatted correctly.",
        "SharpUnion",
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterSyntaxNodeAction(context =>
        {
            if (context.Node is AttributeSyntax node)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(node).Symbol?.ContainingSymbol;
                if (symbol is INamedTypeSymbol namedTypeSymbol)
                {
                    if (namedTypeSymbol.IsSharpUnionModuleAttribute() == true)
                    {
                        if (node.ArgumentList == null || node.ArgumentList.Arguments.Count < 2)
                        {
                            return;     // this will be a compiler error; just don't crash
                        }

                        if (node.ArgumentList?.Arguments[1].Expression is not LiteralExpressionSyntax arg)
                        {
                            return;     // this will be a compiler error; just don't crash
                        }

                        if (!arg.Token.IsKind(SyntaxKind.StringLiteralToken))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, node.Name.GetLocation()));
                            return;
                        }

                        try
                        {
                            var text = (string) arg.Token.Value!;
                            ParseToCSharpMapper.Parse(text);
                        }
                        catch
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, node.Name.GetLocation()));
                        }
                    }
                }
            }
        }, SyntaxKind.Attribute);
    }
}