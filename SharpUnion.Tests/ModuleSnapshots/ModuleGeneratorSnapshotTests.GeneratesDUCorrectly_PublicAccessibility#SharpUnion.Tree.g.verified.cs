//HintName: SharpUnion.Tree.g.cs

namespace Program1
{
    [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
    public abstract partial record Tree
    {
        private Tree() { }

        public sealed partial record Node(params Tree?[] trees) : Tree;
        public bool IsNode => this is Node;

        public sealed partial record Leaf(string Name) : Tree;
        public bool IsLeaf => this is Leaf;
    }
}