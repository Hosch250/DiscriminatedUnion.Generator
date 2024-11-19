using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DiscriminatedUnion.Generator.Analyzers.NonPartialMember,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DiscriminatedUnion.Generator.Tests;

public class NonPartialMemberTests
{
    [Fact]
    public async Task CreatesDiagOnNonPartialMemberAsync()
    {
        await Verify.VerifyAnalyzerAsync(@"
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace DiscriminatedUnion.Generator
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DiscriminatedUnionAttribute : System.Attribute
    {
    }
}

namespace ConsoleApp1
{
    using DiscriminatedUnion.Generator;

    [DiscriminatedUnion]
    abstract partial record Result
    {
        internal partial record OK<T>(T Value);
        internal record {|DU1:Error|}(string Message);
    }
}");   
    }
}