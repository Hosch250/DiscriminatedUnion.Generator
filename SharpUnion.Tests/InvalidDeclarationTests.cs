using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    SharpUnion.Analyzers.InvalidDeclaration,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace SharpUnion.Tests;

public class InvalidDeclarationTests
{
    [Fact]
    public async Task CreatesDiagOnAttribute_NullValue_Async()
    {
        await Verify.VerifyAnalyzerAsync($@"
using SharpUnion.Shared;

[module:{{|DU5:SharpUnionModule|}}(""ConsoleApp1"", null)]

namespace System.Runtime.CompilerServices
{{
    internal static class IsExternalInit {{ }}
}}

namespace SharpUnion.Shared
{{
    [System.AttributeUsage(System.AttributeTargets.Module, AllowMultiple = true)]
    public class SharpUnionModuleAttribute(string nameSpace, string definition) : System.Attribute
    {{
        public string NameSpace {{ get; }} = nameSpace;
        public string Definition {{ get; }} = definition;

        public bool Serializable {{ get; set; }} = false;
        public Accessibility Accessibility {{ get; set; }} = Accessibility.Public;
    }}

    public enum Accessibility
    {{
        Public,
        Internal
    }}
}}");
    }

    [Fact]
    public async Task CreatesDiagOnAttribute_MalformedValue_Async()
    {
        await Verify.VerifyAnalyzerAsync($@"
using SharpUnion.Shared;

[module:{{|DU5:SharpUnionModule|}}(""ConsoleApp1"", @""union Tree() =
| Node(params Tree?[] trees)
| Leaf(string Name);"")]

namespace System.Runtime.CompilerServices
{{
    internal static class IsExternalInit {{ }}
}}

namespace SharpUnion.Shared
{{
    [System.AttributeUsage(System.AttributeTargets.Module, AllowMultiple = true)]
    public class SharpUnionModuleAttribute(string nameSpace, string definition) : System.Attribute
    {{
        public string NameSpace {{ get; }} = nameSpace;
        public string Definition {{ get; }} = definition;

        public bool Serializable {{ get; set; }} = false;
        public Accessibility Accessibility {{ get; set; }} = Accessibility.Public;
    }}

    public enum Accessibility
    {{
        Public,
        Internal
    }}
}}");
    }

    [Fact]
    public async Task DoesNotCreatesDiagOnWellFormedMemberAsync()
    {
        await Verify.VerifyAnalyzerAsync($@"
using SharpUnion.Shared;

[module:SharpUnionModule(""ConsoleApp1"", @""union Tree =
| Node(params Tree?[] trees)
| Leaf(string Name);"")]

namespace System.Runtime.CompilerServices
{{
    internal static class IsExternalInit {{ }}
}}

namespace SharpUnion.Shared
{{
    [System.AttributeUsage(System.AttributeTargets.Module, AllowMultiple = true)]
    public class SharpUnionModuleAttribute(string nameSpace, string definition) : System.Attribute
    {{
        public string NameSpace {{ get; }} = nameSpace;
        public string Definition {{ get; }} = definition;

        public bool Serializable {{ get; set; }} = false;
        public Accessibility Accessibility {{ get; set; }} = Accessibility.Public;
    }}

    public enum Accessibility
    {{
        Public,
        Internal
    }}
}}");
    }
}