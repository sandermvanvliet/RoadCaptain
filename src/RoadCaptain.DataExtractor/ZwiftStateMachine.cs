using System.Text;

namespace RoadCaptain.DataExtractor
{
    public class ZwiftStateMachine
    {
        public void Parse(byte[] data)
        {
            ParserState currentState = ParserState.Seeking;
            List<byte> buffer = new List<byte>();

            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];

                switch (currentState)
                {
                    case ParserState.Seeking:
                        if (b == 0x5A)
                        {
                            // Header start
                            currentState = ParserState.ReadingHeader;
                            buffer.Add(b);
                        }
                        else if (b == 0x3C) { // '<' Start of XML Literal
                            currentState = ParserState.ReadingLiteral;
                            buffer.Add(b);
                        }
                        else if (b == 0xE0) { // Control Tokens
                            currentState = ParserState.ReadingEntry;
                        }
                        else if (b >= 0x30 && b <= 0x39) { // ASCII Numbers (0-9)
                            currentState = ParserState.ReadingCoordinate;
                            buffer.Add(b);
                        }
                        break;
                    
                    case ParserState.SeekingToTable:
                        if (i == 240)
                        {
                            currentState = ParserState.ReadingTable;
                        }
                        break;
                    case ParserState.ReadingTable:
                        if (b == 0x02)
                        {
                            
                        }
                        break;
                    case ParserState.ReadingEntry:
                        if (b == 0x27)
                        {
                            break;
                        }

                        if (b != 0xE0)
                        {
                            buffer.Add(b);
                        }
                        else
                        {
                            Console.WriteLine("Read full entry: " + Convert.ToHexString(buffer.ToArray()));
                            buffer.Clear();
                            break;
                        }

                        // Not an entry, resume seeking
                        currentState = ParserState.Seeking;
                        buffer.Clear();
                        
                        break;
                    case ParserState.ReadingHeader:
                        if (buffer.Count == 1 && b == 0x57)
                        {
                            buffer.Add(b);
                        } else if (buffer.Count == 2 && b == 0x46)
                        {
                            buffer.Add(b);
                        }
                        else if (buffer.Count == 3 && b == 0x21)
                        {
                            currentState = ParserState.ReadingPath;
                            buffer.Clear();
                        }
                        else
                        {
                            buffer.Clear();
                            currentState = ParserState.Seeking;
                        }
                        break;

                    case ParserState.ReadingPath:
                        if (b != 0x0)
                        {
                            buffer.Add(b);
                        }
                        else
                        {
                            Console.WriteLine("Data source path: " + Encoding.UTF8.GetString(buffer.ToArray()));
                            currentState = ParserState.SeekingToTable;
                            buffer.Clear();
                        }

                        break;
                    case ParserState.ReadingLiteral:
                        buffer.Add(b);
                        if (b == 0x3E) { // '>' End of XML tag
                            Console.WriteLine($"Found Tag: {Encoding.ASCII.GetString(buffer.ToArray())}");
                            buffer.Clear();
                            currentState = ParserState.Seeking;
                        }
                        break;

                    case ParserState.ReadingCoordinate:
                        // Read until a non-numeric/non-dot byte is found
                        if ((b >= 0x30 && b <= 0x39) || b == 0x2E || b == 0x2C || b == 0x2D) {
                            buffer.Add(b);
                        } else {
                            Console.WriteLine($"Found Metric: {Encoding.ASCII.GetString(buffer.ToArray())}");
                            buffer.Clear();
                            i--; // Re-evaluate this byte in Seeking state
                            currentState = ParserState.Seeking;
                        }
                        break;

                    case ParserState.ReadingToken:
                        // Tokens are usually fixed length (e.g., E0 + 1 byte)
                        Console.WriteLine($"Processed Control Token: {b:X2}");
                        currentState = ParserState.Seeking;
                        break;
                }
            }
        }
    }
}