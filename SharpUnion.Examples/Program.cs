using SharpUnion.Shared;

[module:SharpUnionModule(@"union Tree
| Node(params Tree?[] trees)
| Leaf(string Name);", Serializable = true, Accessibility = Accessibility.Public)]


[module: SharpUnionModule(@"union Tree1
| Node(params Tree?[] trees)
| Leaf(string Name);")]

namespace SharpUnion.Examples
{
    class Program
    {
        public static int Main()
        {
            //Shape type = new Shape.Circle(5f);
            //Shape type1 = new Shape.Circle(5f);
            //Shape type2 = new Shape.Square(5f);

            //var a = type == type1;
            //var b = type == type2;

            return 0;
        }
    }
}

namespace Program1
{
    [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
    internal abstract partial record Result<TResult, TException>
    {
        private Result() { }

        internal sealed partial record OK(TResult Value) : Result<TResult, TException>;
        internal bool IsOK => this is OK;

        internal sealed partial record Error(TException Message) : Result<TResult, TException>;
        internal bool IsError => this is Error;
    }
}


//[SharpUnion(Serializable = true)]
//public abstract partial record Shape
//{
//    public partial record Circle(float Radius);
//    public partial record EquilateralTriangle(double SideLength);
//    public partial record Square(double SideLength);
//    public partial record Rectangle(double Height, double Width);
//    public partial record X(params string[] X1);

//    public string IsYellow => bool.TrueString;
//    public bool IsCircle1 => true;

//    internal static double Area(Shape shape) =>
//        shape switch
//        {
//            // alt syntax: Circle { Radius: var radius } => Math.PI * radius * radius,

//            Circle(var radius) => Math.PI * radius * radius,
//            EquilateralTriangle(var side) => Math.Sqrt(3.0) / 4.0 * side * side,
//            Square(var side) => side * side,
//            Rectangle(var height, var width) => height * width,
//            _ => throw new NotImplementedException(),
//        };
//}

////[SharpUnion(Serializable = true)]
////public abstract partial record Tree
////{
////    public partial record Node(Tree? Right = null, Tree? Left = null);
////    public partial record Leaf(string Name);
////}

//[SharpUnion]
//public abstract partial record Result<T, T1>
//{
//    public partial record OK(T Value);
//    public partial record Error(T1 Message);
//}