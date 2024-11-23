//HintName: DiscriminatedUnion.Result.g.cs

namespace Project1
{
    abstract partial record Result<TResult, TException>
    {
        private Result() { }

        internal sealed partial record OK : Result<TResult, TException>;
        internal bool IsOK => this is OK;

        internal sealed partial record Error : Result<TResult, TException>;
        internal bool IsError => this is Error;
    }
}