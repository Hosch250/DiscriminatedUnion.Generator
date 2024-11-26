using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace SharpUnion.Tests;

public class ModuleGeneratorSnapshotTests
{
    [Fact]
    public Task GeneratesDUCorrectly()
    {
        var source = @"
using SharpUnion.Shared;

[module:SharpUnionModule(""Program1"", @""union Tree =
| Node(params Tree?[] trees)
| Leaf(string Name);"")]";

        return Verify(source);
    }

    [Fact]
    public Task GeneratesDUCorrectly_WithGenerics()
    {
        var source = @"
using SharpUnion.Shared;

[module:SharpUnionModule(""Program1"", @""union Result<TResult, TException> =
| OK(TResult Value)
| Error(TException Message);"")]";

        return Verify(source);
    }

    [Fact]
    public Task GeneratesDUCorrectly_Serializable()
    {
        var source = @"
using SharpUnion.Shared;

[module:SharpUnionModule(""Program1"", @""union Tree =
| Node(params Tree?[] trees)
| Leaf(string Name);"", Serializable = true)]";

        return Verify(source);
    }

    [Fact]
    public Task GeneratesDUCorrectly_PublicAccessibility()
    {
        var source = @"
using SharpUnion.Shared;

[module:SharpUnionModule(""Program1"", @""union Tree =
| Node(params Tree?[] trees)
| Leaf(string Name);"", Accessibility = Accessibility.Public)]";

        return Verify(source);
    }

    private static readonly string dotNetAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
    internal static Task Verify(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        IEnumerable<PortableExecutableReference> references = AppDomain.CurrentDomain.GetAssemblies()
        .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
        .Select(_ => MetadataReference.CreateFromFile(_.Location))
        .Concat(
        [
            MetadataReference.CreateFromFile(typeof(Shared.SharpUnionAttribute).Assembly.Location),
        ])
        .ToList();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references);

        var generator = new ModuleSyntaxGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return Verifier.Verify(driver).UseDirectory("ModuleSnapshots");
    }
}