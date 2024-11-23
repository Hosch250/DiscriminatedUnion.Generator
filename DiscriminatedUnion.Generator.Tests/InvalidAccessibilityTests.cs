using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DiscriminatedUnion.Generator.Analyzers.InvalidAccessibility,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DiscriminatedUnion.Generator.Tests;

public class InvalidAccessibilityTests
{
    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    public async Task CreatesDiagOnPrivateMemberAsync(string accessibility)
    {
        await Verify.VerifyAnalyzerAsync(@$"
namespace System.Runtime.CompilerServices
{{
    internal static class IsExternalInit {{ }}
}}

namespace DiscriminatedUnion.Generator.Shared
{{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DiscriminatedUnionAttribute : System.Attribute
    {{
    }}
}}

namespace ConsoleApp1
{{
    using DiscriminatedUnion.Generator.Shared;

    [DiscriminatedUnion]
    abstract partial record Result
    {{
        internal partial record A(int Value);
        {accessibility} partial record {{|DU2:B|}}(string Message);
    }}
}}");
    }
}