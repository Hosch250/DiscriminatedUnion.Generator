namespace DiscriminatedUnion.Generator;

public static class AttributeHelper
{
    public const string Attribute = @"
namespace DiscriminatedUnion.Generator
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DiscriminatedUnionAttribute : System.Attribute
    {
    }
}";
}
