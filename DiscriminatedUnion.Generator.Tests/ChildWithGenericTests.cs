using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DiscriminatedUnion.Generator.Analyzers.ChildWithGeneric,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DiscriminatedUnion.Generator.Tests;

public class ChildWithGenericTests
{
    [Fact]
    public async Task CreatesDiagOnMemberWithGenericAsync()
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
    public abstract partial record Result<T>
    {{
        public partial record A(T Value);
        internal partial record {{|DU1:B|}}<T1>(T1 Message);
    }}
}}");
    }
}