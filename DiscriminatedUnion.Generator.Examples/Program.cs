﻿using DiscriminatedUnion.Generator.Shared;
using System.Text.Json;

namespace DiscriminatedUnion.Generator.Examples;

class Program
{
    public static int Main()
    {
        Shape type = new Shape.Circle(5f);
        var json = JsonSerializer.Serialize(type);
        var resultCircle = JsonSerializer.Deserialize<Shape.Circle>(json);
        var resultShape = JsonSerializer.Deserialize<Shape>(json);
        var jsona = JsonSerializer.Serialize(resultCircle);
        var jsonb = JsonSerializer.Serialize(resultShape);
        var w = json == jsona && json == jsonb;


        Tree type1 = new Tree.Node(
            new Tree.Node(
                new Tree.Node(
                    new Tree.Leaf("A")),
                new Tree.Node(
                    new Tree.Node(
                        new Tree.Leaf("B"),
                        new Tree.Leaf("C")),
                    new Tree.Node(
                        new Tree.Node(
                            new Tree.Leaf("D"))))),
            new Tree.Leaf("E"));

        var json1 = JsonSerializer.Serialize(type1);
        var result1 = JsonSerializer.Deserialize<Tree>(json1);
        var json2 = JsonSerializer.Serialize(result1);
        var works = json1 == json2;

        return 0;
    }
}

[DiscriminatedUnion(Serializable = true)]
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

[DiscriminatedUnion(Serializable = true)]
public abstract partial record Tree
{
    public partial record Node(Tree? Right = null, Tree? Left = null);
    public partial record Leaf(string Name);
}

[DiscriminatedUnion]
public abstract partial record Result<T, T1>
{
    public partial record OK(T Value);
    public partial record Error(T1 Message);
}