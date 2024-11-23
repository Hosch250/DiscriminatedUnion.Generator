namespace DiscriminatedUnion.Generator.Examples;

class Program
{
    public static void Main() { }
}

[Shared.DiscriminatedUnion]
public abstract partial record Shape
{
    public partial record Circle(float Radius);
    public partial record EquilateralTriangle(double SideLength);
    public partial record Square(double SideLength);
    public partial record Rectangle(double Height, double Width);

    internal static double Area(Shape shape) =>
        shape switch
        {
            // alt syntax: Circle { Radius: var radius } => Math.PI * radius * radius,

            Circle(var radius) => Math.PI * radius * radius,
            EquilateralTriangle(var side) => Math.Sqrt(3.0) / 4.0 * side * side,
            Square(var side) => side * side,
            Rectangle(var height, var width) => height * width,
            _ => throw new NotImplementedException(),
        };
}

[Shared.DiscriminatedUnion]
abstract partial record Result
{
    internal partial record OK<T>(T Value);
    internal partial record Error(string Message);
}