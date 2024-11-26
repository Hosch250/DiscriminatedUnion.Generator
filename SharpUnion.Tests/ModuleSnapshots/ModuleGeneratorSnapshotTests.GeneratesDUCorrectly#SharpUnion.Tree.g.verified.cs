//HintName: SharpUnion.Tree.g.cs

namespace Program1
{
    [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
    internal abstract partial record Tree
    {
        private Tree() { }

        internal sealed partial record Node(params Tree?[] trees) : Tree;
        internal bool IsNode => this is Node;

        internal sealed partial record Leaf(string Name) : Tree;
        internal bool IsLeaf => this is Leaf;
    }
}