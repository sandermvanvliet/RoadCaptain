namespace RoadCaptain.DataExtractor;

class Program
{
    static void Main(string[] args)
    {
        var inputFilePath = args[0];

        if (!File.Exists(inputFilePath))
        {
            Console.Error.WriteLine("File does not exist");
            Environment.ExitCode = 1;
            return;
        }

        var unpacker = new WadUnpacker(inputFilePath);
    }
}