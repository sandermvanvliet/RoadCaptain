namespace RoadCaptain.WixComponentFileGenerator
{
    public class Program
    {
        static void Main(string[] args)
        {
            var sources = new WixFileGenerator(args[2]);

            sources.Generate(args[0], args[1]);
        }
    }
}