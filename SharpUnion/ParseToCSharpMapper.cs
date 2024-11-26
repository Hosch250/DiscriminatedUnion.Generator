using SharpUnion.Parser;
using SharpUnion.Shared;
using System.Text;

namespace SharpUnion;

public static class ParseToCSharpMapper
{
    public record MapResult(string TypeName, string Output);

    public static MapResult Map(UnionParser.UnionStmtContext unionStmt, string ns, Accessibility accessibility)
    {
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