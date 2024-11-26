using Microsoft.CodeAnalysis;

namespace SharpUnion;

public static class AttributeHelper
{
    public static bool IsSharpUnionAttribute(this ITypeSymbol typeSymbol) =>
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

    public static bool IsSharpUnionIgnoreAttribute(this ITypeSymbol typeSymbol) =>
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
