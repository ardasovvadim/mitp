namespace ConsoleApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new LabI().Run();
        }
    }

    public interface ILab
    {
        void Run();
    }
}