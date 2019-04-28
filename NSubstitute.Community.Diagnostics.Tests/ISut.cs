namespace Samples
{
    public interface ISut
    {
        int Echo(int msg);
        
        int Prop { get; set; }
    }
}