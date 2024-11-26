//HintName: SharpUnion.Result.g.cs

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