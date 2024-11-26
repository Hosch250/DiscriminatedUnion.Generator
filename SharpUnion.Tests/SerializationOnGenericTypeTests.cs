using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    SharpUnion.Analyzers.SerializationOnGenericType,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace SharpUnion.Tests;

public class SerializationOnGenericTypeTests
{
    [Fact]
    public async Task CreatesDiagOnGenericMemberAsync()
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
        public bool Serializable {{ get; set; }}
    }}
}}

namespace ConsoleApp1
{{
    using SharpUnion.Shared;

    [SharpUnion(Serializable = true)]
    public abstract partial record {{|DU4:Result|}}<T>
    {{
        public partial record A(T Value);
        internal partial record B(string Message);
    }}
}}");
    }
}
