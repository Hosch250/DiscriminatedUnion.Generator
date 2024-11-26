//HintName: DiscriminatedUnion.Shape.g.cs

namespace Project1
{
    
    abstract partial record Shape
    {
        [System.CodeDom.Compiler.GeneratedCode("DiscriminatedUnion.Generator", "1.0.0")]
        private Shape() { }

        internal sealed partial record Circle : Shape;

        [System.CodeDom.Compiler.GeneratedCode("DiscriminatedUnion.Generator", "1.0.0")]
        internal bool IsCircle => this is Circle;

        internal sealed partial record EquilateralTriangle : Shape;

        [System.CodeDom.Compiler.GeneratedCode("DiscriminatedUnion.Generator", "1.0.0")]
        internal bool IsEquilateralTriangle => this is EquilateralTriangle;

        internal sealed partial record Square : Shape;

        [System.CodeDom.Compiler.GeneratedCode("DiscriminatedUnion.Generator", "1.0.0")]
        internal bool IsSquare => this is Square;

        internal sealed partial record Rectangle : Shape;

        [System.CodeDom.Compiler.GeneratedCode("DiscriminatedUnion.Generator", "1.0.0")]
        internal bool IsRectangle => this is Rectangle;
    }
}