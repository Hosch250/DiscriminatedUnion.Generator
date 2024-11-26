using Microsoft.CodeAnalysis;

namespace SharpUnion;

internal static class AttributeHelper
{
    internal static bool IsSharpUnionAttribute(this ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol
        {
            MetadataName: "SharpUnionAttribute",
            ContainingNamespace:
            {
                Name: "Shared",
                ContainingNamespace:
                {
                    Name: "SharpUnion",
                    ContainingNamespace.IsGlobalNamespace: true
                }
            }
        };

    internal static bool IsSharpUnionModuleAttribute(this ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol
        {
            MetadataName: "SharpUnionModuleAttribute",
            ContainingNamespace:
            {
                Name: "Shared",
                ContainingNamespace:
                {
                    Name: "SharpUnion",
                    ContainingNamespace.IsGlobalNamespace: true
                }
            }
        };

    internal static bool IsSharpUnionIgnoreAttribute(this ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol
        {
            MetadataName: "SharpUnionIgnoreAttribute",
            ContainingNamespace:
            {
                Name: "Shared",
                ContainingNamespace:
                {
                    Name: "SharpUnion",
                    ContainingNamespace.IsGlobalNamespace: true
                }
            }
        };
}
