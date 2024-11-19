using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DiscriminatedUnion.Generator.Tests;

public class DUGeneratorSnapshotTests
{
    [Fact]
    public Task GeneratesDUCorrectly()
    {
        var source = @"
using DiscriminatedUnion.Generator;

namespace Project1;

[DiscriminatedUnion]
abstract partial record Shape
{
    partial internal record Circle(float Radius);
    partial internal record EquilateralTriangle(double SideLength);
    partial internal record Square(double SideLength);
    partial internal record Rectangle(double Height, double Width);
    internal record X(double Height, double Width);
}";

        return TestHelper.Verify(source);
    }
}

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}

public static class TestHelper
{
    public static Task Verify(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        IEnumerable<PortableExecutableReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        ];

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references);

        var generator = new DUGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return Verifier.Verify(driver).UseDirectory("Snapshots");
    }
}