using Antlr4.Runtime.Atn;
using Antlr4.Runtime;
using SharpUnion.Parser;

namespace SharpUnion.Tests;
public class ParseToCSharpMapperTests
{
    [Fact]
    public void VerifyOutput()
    {
        var input = @"
union Shape =
| Rectangle(float Width, float Length)
| Circle(float Radius);";

        var output = ParseToCSharpMapper.Map(Parse(input), "My.Namespace", Shared.Accessibility.Public);

        var expected = @"
namespace My.Namespace
{@@UnionAttributes@@
    [System.CodeDom.Compiler.GeneratedCode(""SharpUnion"", ""2.0.0"")]
    public abstract partial record Shape
    {
        private Shape() { }

        public sealed partial record Rectangle(float Width, float Length) : Shape;
        public bool IsRectangle => this is Rectangle;

        public sealed partial record Circle(float Radius) : Shape;
        public bool IsCircle => this is Circle;

        @@UnionMembers@@
    }
}";

        Assert.Equal(expected, output);
    }

    [Fact]
    public void VerifyOutput_WithGenerics()
    {
        var input = @"
union Result<TResult, TException> =
| OK(TResult Value)
| Error(TException Message);";

        var output = ParseToCSharpMapper.Map(Parse(input), "My.Namespace", Shared.Accessibility.Internal);

        var expected = @"
namespace My.Namespace
{@@UnionAttributes@@
    [System.CodeDom.Compiler.GeneratedCode(""SharpUnion"", ""2.0.0"")]
    internal abstract partial record Result<TResult, TException>
    {
        private Result() { }

        internal sealed partial record OK(TResult Value) : Result<TResult, TException>;
        internal bool IsOK => this is OK;

        internal sealed partial record Error(TException Message) : Result<TResult, TException>;
        internal bool IsError => this is Error;

        @@UnionMembers@@
    }
}";

        Assert.Equal(expected, output);
    }

    private static UnionParser.UnionStmtContext Parse(string input)
    {
        var stream = new AntlrInputStream(input);
        var lexer = new UnionLexer(stream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new UnionParser(tokens)
        {
            ErrorHandler = new BailErrorStrategy()
        };
        parser.Interpreter.PredictionMode = PredictionMode.Sll;
        var tree = parser.unionStmt();
        return tree;
    }
}



//namespace Project1
//{

//    abstract partial record Shape
//    {
//        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
//        private Shape() { }

//        internal sealed partial record Circle : Shape;

//        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
//        internal bool IsCircle => this is Circle;

//        internal sealed partial record EquilateralTriangle : Shape;

//        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
//        internal bool IsEquilateralTriangle => this is EquilateralTriangle;

//        internal sealed partial record Square : Shape;

//        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
//        internal bool IsSquare => this is Square;

//        internal sealed partial record Rectangle : Shape;

//        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
//        internal bool IsRectangle => this is Rectangle;
//    }
//}