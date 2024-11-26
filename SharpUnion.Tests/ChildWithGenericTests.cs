using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    SharpUnion.Analyzers.ChildWithGeneric,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace SharpUnion.Tests;

public class ChildWithGenericTests
{
    [Fact]
    public async Task CreatesDiagOnMemberWithGenericAsync()
    {
        await Verify.VerifyAnalyzerAsync($@"
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
    public abstract partial record Result<T>
    {{
        public partial record A(T Value);
        internal partial record {{|DU1:B|}}<T1>(T1 Message);
    }}
}}");
    }

    [Fact]
    public async Task DoesNotCreateDiagOnIgnoredMemberWithGenericAsync()
    {
        await Verify.VerifyAnalyzerAsync($@"
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

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class SharpUnionIgnoreAttribute : System.Attribute
    {{
    }}
}}

namespace ConsoleApp1
{{
    using SharpUnion.Shared;

    [SharpUnion]
    public abstract partial record Result<T>
    {{
        public partial record A(T Value);

        [SharpUnionIgnore]
        internal partial record B<T1>(T1 Message);
    }}
}}");
    }
}