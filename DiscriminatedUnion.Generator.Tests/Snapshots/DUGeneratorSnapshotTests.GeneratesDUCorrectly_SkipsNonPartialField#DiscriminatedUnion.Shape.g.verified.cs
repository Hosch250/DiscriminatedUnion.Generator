//HintName: DiscriminatedUnion.Shape.g.cs

namespace Project1
{
    abstract partial record Shape
    {
        private Shape() {}

        internal sealed partial record Circle : Shape;
        internal bool IsCircle() => this is Circle;

        internal sealed partial record EquilateralTriangle : Shape;
        internal bool IsEquilateralTriangle() => this is EquilateralTriangle;

        internal sealed partial record Square : Shape;
        internal bool IsSquare() => this is Square;
    }
}