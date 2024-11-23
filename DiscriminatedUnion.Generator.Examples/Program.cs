using System.Text.Json;

namespace DiscriminatedUnion.Generator.Examples;

class Program
{
    public static int Main()
    {
        var type = new Shape.Circle(5f);
        var json = JsonSerializer.Serialize(type);
        var resultCircle = JsonSerializer.Deserialize<Shape.Circle>(json);
        var resultShape = JsonSerializer.Deserialize<Shape>(json);

        return 0;
    }
}

[Shared.DiscriminatedUnion(Serializable = true)]
public abstract partial record Shape
{
    public partial record Circle(float Radius);
    public partial record EquilateralTriangle(double SideLength);
    public partial record Square(double SideLength);
    public partial record Rectangle(double Height, double Width);

    public string IsYellow => bool.TrueString;
    public bool IsCircle1 => true;

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
abstract partial record Result<T, T1>
{
    internal partial record OK(T Value);
    internal partial record Error(T1 Message);
}