using System.Text;

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
        
        using var stream = File.OpenRead(inputFilePath);

        byte currentByte = (byte)stream.ReadByte();
        var fileHeader = new List<byte>();

        while (currentByte != 0 && stream.CanRead)
        {
            fileHeader.Add((byte)currentByte);
            currentByte = (byte)stream.ReadByte();
        }

        if (!stream.CanRead && currentByte != 0)
        {
            Console.Error.WriteLine("No null terminator found for header, this is a format I don't understand...");
            Environment.ExitCode = 1;
            return;
        }
        
        Console.WriteLine(Encoding.UTF8.GetString(fileHeader.ToArray()));

        // Skip to next bit of real data
        while (stream.Position < stream.Length && currentByte == 0)
        {
            currentByte = (byte)stream.ReadByte();
        }

        if (stream.Position >= stream.Length)
        {
            Console.Error.WriteLine("Only null bytes found, this is a format I don't understand...");
            Environment.ExitCode = 1;
            return;
        }

        var seekingToFirst = true;
        var sections = new Dictionary<long, byte[]>();
        long currentSectionStart = 0;
        var currentSection = new List<byte>();

        byte prevByte;
        
        while (stream.Position < stream.Length)
        {
            prevByte = currentByte;
            currentByte = (byte)stream.ReadByte();
            
            // Read till we find 0xe027 which seems to be a marker
            if (prevByte == 0xe0 && currentByte == 0x27)
            {
                // Found a section marker
                // If we're seeking to the first marker then ignore anything that came before
                if (seekingToFirst)
                {
                    seekingToFirst = false;
                    currentSectionStart = stream.Position - 1;
                }
                else
                {
                    sections.Add(currentSectionStart, currentSection.Take(currentSection.Count - 1).ToArray());

                    currentSectionStart = stream.Position - 1;
                    currentSection = [];
                }
            }
            else
            {
                // Only push bytes to the current section if we're not
                // seeking for the first marker.
                if (!seekingToFirst)
                {
                    currentSection.Add(currentByte);
                }
            }
            
        }
        
        Console.WriteLine("Found these sections:\n");
        foreach (var section in sections.Take(100).OrderBy(kv => kv.Key))
        {
            Console.WriteLine($"{section.Key:#########0}: {DumpBytes(section)}");
        }
    }

    private static string DumpBytes(KeyValuePair<long, byte[]> section)
    {
        var bytesToHex = section.Value.Length > 64
            ? section.Value.Take(64).ToArray()
            : section.Value;
        
        var textValue = Encoding.UTF8.GetString(bytesToHex);
        
        return $"({section.Value.Length:#####}): {Convert.ToHexString(bytesToHex)} : {textValue}";
    }
}