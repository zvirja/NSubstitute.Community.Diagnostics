namespace NSubstitute.Community.Diagnostics.Tests
{
    public interface ISut
    {
        int Prop { get; set; }
        int Echo(int input);
        string Echo(string input);
        T EchoGen<T>(T input);
        int SumNumbers(int num1, int num2);
        ISut GetSelf(long input);
    }
}