using System.Text;

public class ZwiftAdvancedParser
{
    // This dictionary acts as the "lookup table" for common Zwift binary tokens
    private static readonly Dictionary<byte, string> TokenMap = new Dictionary<byte, string>
    {
        { 0xE0, "RECORD_SEPARATOR" },
        { 0xC1, "PROPERTY_ID" },
        { 0xCF, "VALUE_START" },
        { 0xCA, "VALUE_END" },
        { 0xD5, "NODE_LINK" },
        { 0xF2, "ENTITY_START" }
    };

    public void ParseWithLogic(byte[] data)
    {
        int i = 0;
        while (i < data.Length)
        {
            byte current = data[i];

            // 1. Handle Known Dictionary Tokens
            if (TokenMap.ContainsKey(current))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"[{TokenMap[current]}] ");
                Console.ResetColor();
                i++;
                continue;
            }

            // 2. Handle ASCII String Literals (The "Human Readable" parts)
            if (current >= 0x20 && current <= 0x7E)
            {
                StringBuilder sb = new StringBuilder();
                while (i < data.Length && data[i] >= 0x20 && data[i] <= 0x7E)
                {
                    sb.Append((char)data[i]);
                    i++;
                }
                string result = sb.ToString().Trim();
                if (result.Length > 1) 
                    Console.WriteLine(result);
            }
            else
            {
                // 3. Handle unknown binary junk/padding
                i++;
            }
        }
    }
}