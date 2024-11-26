//HintName: SharpUnion.Result.g.cs

namespace Project1
{
    
    abstract partial record Result<TResult, TException>
    {
        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
        private Result() { }

        internal sealed partial record OK : Result<TResult, TException>;

        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
        internal bool IsOK => this is OK;

        internal sealed partial record Error : Result<TResult, TException>;

        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
        internal bool IsError => this is Error;
    }
}