using SharpUnion.Shared;
using System.Text.Json;

namespace SharpUnion.Examples;

class Program
{
    public static int Main()
    {
        Shape type = new Shape.Circle(5f);
        Shape type1 = new Shape.Circle(5f);
        Shape type2 = new Shape.Square(5f);

        var a = type == type1;
        var b = type == type2;

        return 0;
    }
}

[SharpUnion(Serializable = true)]
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

[SharpUnion(Serializable = true)]
public abstract partial record Tree
{
    public partial record Node(Tree? Right = null, Tree? Left = null);
    public partial record Leaf(string Name);
}

[SharpUnion]
public abstract partial record Result<T, T1>
{
    public partial record OK(T Value);
    public partial record Error(T1 Message);
}