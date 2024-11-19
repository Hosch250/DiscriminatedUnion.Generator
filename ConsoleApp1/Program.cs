using ConsoleApp1;
using static ConsoleApp1.Result;
using DiscriminatedUnion.Generator;
using static ConsoleApp1.Shape;

var c = new Circle(15.0f);
Console.WriteLine($"Area of circle with radius {c.Radius}: {Area(c)}");

var s = new Square(10.0);
Console.WriteLine($"Area of square with side {s.SideLength}: {Area(s)}");

var r = new Rectangle(5.0, 10.0);
Console.WriteLine($"Area of rect with height {r.Height} and width {r.Width}: {Area(r)}");

Shape sh = new Circle(5.0f);
Console.WriteLine($"Area of shape: {Area(sh)}");

var ok = new Result.OK<string>("");
var err = new Result.Error("");

var a = ok.IsOK<string>();
var b = err.IsError();

return 0;

namespace ConsoleApp1
{
    [DiscriminatedUnion]
    abstract partial record Shape
    {
        internal partial record Circle(float Radius);
        internal partial record EquilateralTriangle(double SideLength);
        internal partial record Square(double SideLength);
        internal partial record Rectangle(double Height, double Width);
        internal record X(double Height, double Width);

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

    [DiscriminatedUnion]
    abstract partial record Result
    {
        internal partial record OK<T>(T Value);
        internal partial record Error(string Message);
    }
}