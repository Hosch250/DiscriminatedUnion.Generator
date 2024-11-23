//HintName: DiscriminatedUnion.Result.g.cs

namespace Project1
{
    abstract partial record Result
    {
        private Result() { }

        internal sealed partial record OK<T> : Result;
        internal bool IsOK<T>() => this is OK<T>;

        internal sealed partial record OK<T, T1, T2> : Result;
        internal bool IsOK<T, T1, T2>() => this is OK<T, T1, T2>;

        internal sealed partial record Error : Result;
        internal bool IsError() => this is Error;
    }
}