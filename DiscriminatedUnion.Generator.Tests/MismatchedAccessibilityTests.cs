using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DiscriminatedUnion.Generator.Analyzers.MismatchedAccessibility,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DiscriminatedUnion.Generator.Tests;

public class MismatchedAccessibilityTests
{
    [Fact]
    public async Task CreatesDiagOnPrivateMemberAsync()
    {
        await Verify.VerifyAnalyzerAsync(@$"
namespace System.Runtime.CompilerServices
{{
    internal static class IsExternalInit {{ }}
}}

namespace DiscriminatedUnion.Generator
{{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DiscriminatedUnionAttribute : System.Attribute
    {{
    }}
}}

namespace ConsoleApp1
{{
    using DiscriminatedUnion.Generator;

    [DiscriminatedUnion]
    public abstract partial record Result
    {{
        public partial record A(int Value);
        internal partial record {{|DU3:B|}}(string Message);
    }}
}}");
    }
}