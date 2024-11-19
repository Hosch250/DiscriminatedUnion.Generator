# DiscriminatedUnion.Generator
Bringing the power of F# discriminated unions to C#.

## Use
Write discriminated unions with nested records:
```cs
using DiscriminatedUnion.Generator;

namespace MyApp;

[DiscriminatedUnion]
public abstract partial record Shape
{
    public partial record Circle(float Radius);
    public partial record EquilateralTriangle(double SideLength);
    public partial record Square(double SideLength);
    public partial record Rectangle(double Height, double Width);

    internal static double Area(Shape shape) =>
        shape switch
        {
            // alt pattern matching syntax: Circle { Radius: var radius } => Math.PI * radius * radius,

            Circle(var radius) => Math.PI * radius * radius,
            EquilateralTriangle(var side) => Math.Sqrt(3.0) / 4.0 * side * side,
            Square(var side) => side * side,
            Rectangle(var height, var width) => height * width,
            _ => throw new NotImplementedException(),
        };
}

[DiscriminatedUnion]
internal abstract partial record Result
{
    internal partial record OK<T>(T Value);
    internal partial record Error(string Message);
}
```

The generator will generate code that looks like the following:
```cs
namespace MyApp
{
    abstract partial record Shape
    {
        private Shape() {}

        internal sealed partial record Circle : Shape;
        internal bool IsCircle() => this is Circle;

        internal sealed partial record EquilateralTriangle : Shape;
        internal bool IsEquilateralTriangle() => this is EquilateralTriangle;

        internal sealed partial record Square : Shape;
        internal bool IsSquare() => this is Square;

        internal sealed partial record Rectangle : Shape;
        internal bool IsRectangle() => this is Rectangle;
    }
}

namespace MyApp
{
    abstract partial record Result
    {
        private Result() {}

        internal sealed partial record OK<T> : Result;
        internal bool IsOK<T>() => this is OK<T>;

        internal sealed partial record Error : Result;
        internal bool IsError() => this is Error;
    }
}
```

Logic:
- Force a private constructor on the parent type so nobody can create a new instance of it.
- Mark each implementation as `sealed`, so nobody can derive from them.
- Implement `Is*` functions to be feature comparable with F#. Favor pattern matching when using, though.

## Analyzers
Some analyzers are included to help prevent issues.

### Non-Partial Member
If you don't mark one of your members as partial, the generator will ignore it. However, this was probably a mistake, so this will raise a warning.

### Invalid Accessibility
DUs must be internal or public. Private types don't make sense in this case, because the derived members will not be visible, and protected types don't make sense because DUs cannot be further derived. This analyzer will raise an error.

### Mismatched Accessibility
DUs and their members must have the same accessibility modifier. This analyzer will raise an error.