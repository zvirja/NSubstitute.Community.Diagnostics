namespace NSubstitute.Community.Diagnostics.Tests
{
    public interface ITargetInterface
    {
        string StrProp { get; set; }
        string Echo(string input);
        T EchoGen<T>(T input);
        int SumNumbers(int num1, int num2);
        ITargetInterface GetSelf(long input);
    }
}