using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    SharpUnion.Analyzers.MismatchedAccessibility,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace SharpUnion.Tests;

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
    public abstract partial record Result
    {{
        public partial record A(int Value);
        internal partial record {{|DU3:B|}}(string Message);
    }}
}}");
    }
}
