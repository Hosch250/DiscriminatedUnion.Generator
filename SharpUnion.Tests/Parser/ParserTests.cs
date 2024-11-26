using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree.Xpath;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using SharpUnion.Parser;

namespace SharpUnion.Tests.Parser;
public class ParserTests
{
    [Theory]
    [InlineData(@"union Shape =
    | Rectangle(float Width, float Length)
    | Circle(float Radius);", 2)]
    [InlineData(@"UNION Result<T, T1> =
    | OK(T Value)
    | Error(T1 Message);", 2)]
    [InlineData(@"
union Shape =
| Rectangle(float Width, float Length)
| Circle(float Radius);", 2)]
    [InlineData(@"
union Shape = 
    | Rectangle(float Width, float Length) 
    | Circle(float Radius) ;", 2)]
    [InlineData(@"union Tree =
| Node(params Tree?[] trees)
| Leaf(string Name);", 2)]
    public void UnionStatement(string input, int memberCount)
    {
        var (parser, root) = Parse(input, p => p.unionStmt());
        AssertXPath(parser, root, "//unionStmt", match => Assert.Single(match));
        AssertXPath(parser, root, "//member", match => Assert.Equal(memberCount, match.Count));
    }

    [Theory]
    [InlineData("\n| Circle(float Width)")]
    [InlineData("\n| Rectangle(double Width, double Length)")]
    [InlineData("\n | Rectangle ( double Width , double Length ) ")]
    [InlineData("\n\t| Rectangle ( double Width , double Length ) ")]
    public void MemberRule(string input)
    {
        var (parser, root) = Parse(input, p => p.member());
        AssertXPath(parser, root, "//member", match => Assert.Single(match));
    }

    [Theory]
    [InlineData("()", 0)]
    [InlineData("( )", 0)]
    [InlineData("(Foo foo)", 1)]
    [InlineData("( Foo foo )", 1)]
    [InlineData("(Foo foo, List<T> tSet)", 2)]
    [InlineData("( Foo foo , List < T > tSet )", 2)]
    [InlineData("(Foo foo, Bar bar, List<T> tSet)", 3)]
    [InlineData("(Foo foo, Bar bar, int[] i)", 3)]
    public void ParameterListRule(string input, int parameterCount)
    {
        var (parser, root) = Parse(input, p => p.parameterList());
        AssertXPath(parser, root, "//parameterList", match => Assert.Single(match));
        AssertXPath(parser, root, "//parameter", match => Assert.Equal(parameterCount, match.Count));
    }

    [Theory]
    [InlineData("Foo foo")]
    [InlineData("List<T> tSet")]
    [InlineData("params IEnumerable<int> ints")]
    [InlineData("params int[] i")]
    public void ParameterRule(string input)
    {
        var (parser, root) = Parse(input, p => p.parameter());
        AssertXPath(parser, root, "//parameter", match => Assert.Single(match));
    }

    [Theory]
    [InlineData("Foo", 1)]
    [InlineData("Foo?", 1)]
    [InlineData("Foo<T>", 2)]
    [InlineData("Foo<T?>?", 2)]
    [InlineData("Foo < T >", 2)]
    [InlineData("Foo < T ? > ?", 2)]
    [InlineData("Foo<T, T1>", 3)]
    [InlineData("Foo<T<T1>>", 3)]
    [InlineData("(int, string, List<int>)", 5)]
    [InlineData("(int, string, List<int>)?", 5)]
    [InlineData("List<(string, int)>", 4)]
    [InlineData("List<(string, int)?>", 4)]
    [InlineData("int[]", 2)]
    [InlineData("int[]?", 2)]
    [InlineData("int?[]", 2)]
    [InlineData("int?[]?", 2)]
    [InlineData("int [ ]", 2)]
    [InlineData("int [ ] ?", 2)]
    public void TypeRule(string input, int typeCount)
    {
        var (parser, root) = Parse(input, p => p.type());
        AssertXPath(parser, root, "//type", match => Assert.Equal(typeCount, match.Count));
    }

    [Theory]
    [InlineData("foo")]
    [InlineData("Foo")]
    [InlineData("FOO")]
    [InlineData("_foo1")]
    public void IdentifierRule(string input)
    {
        var (parser, root) = Parse(input, p => p.identifier());
        AssertXPath(parser, root, "//identifier", match => Assert.Single(match));
    }

    private static (UnionParser parser, T root) Parse<T>(string input, Func<UnionParser, T> rule)
    {
        var stream = new AntlrInputStream(input);
        var lexer = new UnionLexer(stream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new UnionParser(tokens)
        {
            ErrorHandler = new BailErrorStrategy()
        };
        parser.Interpreter.PredictionMode = PredictionMode.Sll;
        var tree = rule(parser);
        return (parser, tree);
    }

    private static void AssertXPath(UnionParser parser, ParserRuleContext root, string xpath, Action<ICollection<IParseTree>> constraint)
    {
        var matches = new XPath(parser, xpath).Evaluate(root);
        constraint(matches);
    }
}
