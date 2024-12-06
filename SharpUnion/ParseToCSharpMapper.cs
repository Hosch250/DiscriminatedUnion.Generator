using Antlr4.Runtime.Atn;
using Antlr4.Runtime;
using SharpUnion.Parser;
using SharpUnion.Shared;
using System.Text;

namespace SharpUnion;

public static class ParseToCSharpMapper
{
    public record MapResult(string TypeName, string Output);

    public static UnionParser.UnionStmtContext Parse(string input)
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

    public static MapResult Map(string ns, string declaration, Accessibility accessibility)
    {
        var unionStmt = Parse(declaration);

        var sb = new StringBuilder();
        sb.Append($@"
namespace {ns}
{{@@UnionAttributes@@
    [System.CodeDom.Compiler.GeneratedCode(""{AssemblyMetadata.AssemblyName}"", ""{AssemblyMetadata.AssemblyVersion}"")]
    {Enum.GetName(typeof(Accessibility), accessibility).ToLower()} abstract partial record {unionStmt.type().GetText()}
    {{
        private {Identifier(unionStmt.type())}() {{ }}");

        foreach (var member in unionStmt.member())
        {
            sb.Append(GetMember(member, unionStmt.type(), accessibility));
        }

        sb.Append($@"@@UnionMembers@@
    }}
}}");

        return new MapResult(Identifier(unionStmt.type()), sb.ToString());
    }

    private static string Identifier(UnionParser.TypeContext type) => type switch
    {
        UnionParser.BasicTypeContext bc => bc.identifier().GetText(),
        UnionParser.GenericTypeContext gc => gc.identifier().GetText(),
        _ => throw new NotImplementedException()
    };

    private static string GetMember(UnionParser.MemberContext member, UnionParser.TypeContext parentType, Accessibility accessibility)
    {
        return $@"

        {Enum.GetName(typeof(Accessibility), accessibility).ToLower()} sealed partial record {member.identifier().GetText()}{member.parameterList().GetText().Replace('\t', ' ')} : {parentType.GetText()};
        {Enum.GetName(typeof(Accessibility), accessibility).ToLower()} bool Is{member.identifier().GetText()} => this is {member.identifier().GetText()};";
    }
}