using SharpUnion.Shared;

[module: SharpUnionModule(
    "SharpUnion.Examples",
    @"union Tree =
| Node(params Tree?[] trees)
| Leaf(string Name);",
    Serializable = true,
    Accessibility = Accessibility.Public)]

namespace SharpUnion.Examples;

class Program
{
    public static int Main()
    {
        Tree tree = new Tree.Node(
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

        return 0;
    }
}

// alternative syntax
//[SharpUnion(Serializable = true)]
//public abstract partial record Tree
//{
//    public partial record Node(Tree? Right = null, Tree? Left = null);
//    public partial record Leaf(string Name);
//}