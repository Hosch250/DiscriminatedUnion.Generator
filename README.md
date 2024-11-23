# DiscriminatedUnion.Generator
Bringing the power of F# discriminated unions to C#.

## Use
Write discriminated unions with nested records:
```cs
using DiscriminatedUnion.Generator.Shared;

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
internal abstract partial record Result<T>
{
    internal partial record OK(T Value);
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
        internal bool IsCircle => this is Circle;

        internal sealed partial record EquilateralTriangle : Shape;
        internal bool IsEquilateralTriangle => this is EquilateralTriangle;

        internal sealed partial record Square : Shape;
        internal bool IsSquare => this is Square;

        internal sealed partial record Rectangle : Shape;
        internal bool IsRectangle => this is Rectangle;
    }
}

namespace MyApp
{
    abstract partial record Result<T>
    {
        private Result() {}

        internal sealed partial record OK : Result<T>;
        internal bool IsOK => this is OK;

        internal sealed partial record Error : Result<T>;
        internal bool IsError => this is Error;
    }
}
```

Logic:
- Force a private constructor on the parent type so nobody can create a new instance of it.
- Mark each implementation as `sealed`, so nobody can derive from them.
- Implement `Is*` properties. *Favor pattern matching when using, though.*

### Serialization
Set the `Serializable` property on your attribute: `[DiscriminatedUnion(Serializable = true)]`. This will add support for System.Text.Json serialization. When this flag is set, the following code will work:
```cs
var type = new Shape.Circle(5f);
var json = JsonSerializer.Serialize(type);
var resultCircle = JsonSerializer.Deserialize<Shape.Circle>(json);
var resultShape = JsonSerializer.Deserialize<Shape>(json);
```

## Analyzers
Some analyzers are included to help prevent issues.

### Child Member with Generic
Generics must be included on the parent type; defining generic child types is not allowed (child members may use generics included on the parent type, however). This analyzer will raise an error.

### Invalid Accessibility
DUs must be internal or public. Private types don't make sense in this case, because the derived members will not be visible, and protected types don't make sense because DUs cannot be further derived. This analyzer will raise an error.

### Mismatched Accessibility
DUs and their members must have the same accessibility modifier. This analyzer will raise an error.

### Serialization flag on Generic DU
Generic DUs (such as the `Result<T>` shown above), cannot be deserialized.