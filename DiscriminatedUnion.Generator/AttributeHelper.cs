using Microsoft.CodeAnalysis;

namespace DiscriminatedUnion.Generator;

public static class AttributeHelper
{
    public static bool IsDiscriminatedUnionAttribute(this ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol
        {
            MetadataName: "DiscriminatedUnionAttribute",
            ContainingNamespace:
            {
                Name: "Shared",
                ContainingNamespace:
                {
                    Name: "Generator",
                    ContainingNamespace:
                    {
                        Name: "DiscriminatedUnion",
                        ContainingNamespace.IsGlobalNamespace: true
                    }
                }
            }
        };

    public static bool IsDiscriminatedUnionIgnoreAttribute(this ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol
        {
            MetadataName: "DiscriminatedUnionIgnoreAttribute",
            ContainingNamespace:
            {
                Name: "Shared",
                ContainingNamespace:
                {
                    Name: "Generator",
                    ContainingNamespace:
                    {
                        Name: "DiscriminatedUnion",
                        ContainingNamespace.IsGlobalNamespace: true
                    }
                }
            }
        };
}
