using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DiscriminatedUnion.Generator.Analyzers.SerializationOnGenericType,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DiscriminatedUnion.Generator.Tests;

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

namespace DiscriminatedUnion.Generator.Shared
{{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DiscriminatedUnionAttribute : System.Attribute
    {{
        public bool Serializable {{ get; set; }}
    }}
}}

namespace ConsoleApp1
{{
    using DiscriminatedUnion.Generator.Shared;

    [DiscriminatedUnion(Serializable = true)]
    public abstract partial record {{|DU4:Result|}}<T>
    {{
        public partial record A(T Value);
        internal partial record B(string Message);
    }}
}}");
    }
}
