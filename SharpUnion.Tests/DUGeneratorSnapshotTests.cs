using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SharpUnion.Tests;

public class DUGeneratorSnapshotTests
{
    [Fact]
    public Task GeneratesDUCorrectly()
    {
        var source = @"
using SharpUnion.Shared;

namespace Project1;

[SharpUnion]
abstract partial record Shape
{
    internal partial record Circle(float Radius);
    internal partial record EquilateralTriangle(double SideLength);
    internal partial record Square(double SideLength);
    internal partial record Rectangle(double Height, double Width);
}";

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task GeneratesDUCorrectly_IncludesNonPartialField()
    {
        var source = @"
using SharpUnion.Shared;

namespace Project1;

[SharpUnion]
abstract partial record Shape
{
    internal partial record Circle(float Radius);
    internal partial record EquilateralTriangle(double SideLength);
    internal partial record Square(double SideLength);
    internal record Rectangle(double Height, double Width);
}";

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task GeneratesDUCorrectly_WithGenerics()
    {
        var source = @"
using SharpUnion.Shared;

namespace Project1;

[SharpUnion]
abstract partial record Result<TResult, TException>
{
    internal partial record OK(TResult Value);
    internal partial record Error(TException Message);
}";

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task GeneratesDUCorrectly_Serializable()
    {
        var source = @"
using SharpUnion.Shared;

namespace Project1;

[SharpUnion(Serializable = true)]
abstract partial record Shape
{
    internal partial record Circle(float Radius);
    internal partial record EquilateralTriangle(double SideLength);
    internal partial record Square(double SideLength);
    internal partial record Rectangle(double Height, double Width);
}";

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task GeneratesDUCorrectly_IgnoresTypeWithIgnoreAttribute()
    {
        var source = @"
using SharpUnion.Shared;

namespace Project1;

[SharpUnion]
abstract partial record Shape
{
    internal partial record Circle(float Radius);
    internal partial record EquilateralTriangle(double SideLength);
    internal partial record Square(double SideLength);

    [SharpUnionIgnore]
    internal partial record Rectangle(double Height, double Width);
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
    private static readonly string dotNetAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

    public static Task Verify(string source)
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

        var generator = new Generator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return Verifier.Verify(driver).UseDirectory("Snapshots");
    }
}