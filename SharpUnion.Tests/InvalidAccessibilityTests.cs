using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    SharpUnion.Analyzers.InvalidAccessibility,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace SharpUnion.Tests;

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

namespace SharpUnion.Shared
{{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class SharpUnionAttribute : System.Attribute
    {{
    }}
}}

namespace ConsoleApp1
{{
    using SharpUnion.Shared;

    [SharpUnion]
    abstract partial record Result
    {{
        internal partial record A(int Value);
        {accessibility} partial record {{|DU2:B|}}(string Message);
    }}
}}");
    }
}