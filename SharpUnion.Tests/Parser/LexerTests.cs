using Antlr4.Runtime;
using SharpUnion.Parser;

namespace SharpUnion.Tests.Parser;
public class LexerTests
{
    [Theory]
    [InlineData("PARAMS", UnionLexer.PARAMS)]
    [InlineData("params", UnionLexer.PARAMS)]
    [InlineData("UNION", UnionLexer.UNION)]
    [InlineData("union", UnionLexer.UNION)]
    [InlineData("[", UnionLexer.OPENBRACKET)]
    [InlineData("]", UnionLexer.CLOSEBRACKET)]
    [InlineData("<", UnionLexer.OPENCARET)]
    [InlineData(">", UnionLexer.CLOSECARET)]
    [InlineData("(", UnionLexer.OPENPAREN)]
    [InlineData(")", UnionLexer.CLOSEPAREN)]
    [InlineData("|", UnionLexer.PIPE)]
    [InlineData(";", UnionLexer.SEMICOLON)]
    [InlineData("=", UnionLexer.EQUALS)]
    [InlineData(",", UnionLexer.COMMA)]
    [InlineData("?", UnionLexer.QUESTIONMARK)]
    [InlineData("test", UnionLexer.IDENTIFIER)]
    [InlineData("test_test1", UnionLexer.IDENTIFIER)]
    [InlineData("_1test1", UnionLexer.IDENTIFIER)]
    public void Literals(string token, int type)
    {
        var tokenStream = GenerateTokens(token);
        Assert.Equal(tokenStream[0].Type, type);
    }

    private IList<IToken> GenerateTokens(string input)
    {
        var stream = new AntlrInputStream(input);
        var lexer = new UnionLexer(stream);

        var tokenStream = new CommonTokenStream(lexer);
        tokenStream.Fill();

        return tokenStream.GetTokens();
    }
}
