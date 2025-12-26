using System.Text;
using System.Text.RegularExpressions;

namespace RoadCaptain.DataExtractor
{
    public class ZwiftDataMapper
    {
        private const byte VALUE_START = 0xCF;
        private const byte VALUE_END = 0xCA;

        public Dictionary<string, string> ExtractPairs(byte[] rawData)
        {
            var results = new Dictionary<string, string>();
            string lastLabel = "HeaderInfo";
            int i = 0;

            while (i < rawData.Length)
            {
                // 1. Identify Label (Last string seen before a value)
                if (rawData[i] >= 0x20 && rawData[i] <= 0x7E)
                {
                    StringBuilder sb = new StringBuilder();
                    while (i < rawData.Length && rawData[i] >= 0x20 && rawData[i] <= 0x7E)
                    {
                        sb.Append((char)rawData[i]);
                        i++;
                    }
                    string candidate = sb.ToString().Trim();
                    if (candidate.Length > 2) lastLabel = candidate.TrimEnd('>');
                    continue;
                }

                // 2. Extract Data between CF and CA tokens
                if (rawData[i] == VALUE_START)
                {
                    i += 3; // Skip CF and the 2 identification bytes
                    int start = i;
                    while (i < rawData.Length && rawData[i] != VALUE_END) i++;

                    if (i < rawData.Length)
                    {
                        string rawVal = Encoding.ASCII.GetString(rawData, start, i - start);
                        string cleanVal = Regex.Replace(rawVal, @"[^0-9.\-,]", "");
                    
                        if (!string.IsNullOrEmpty(cleanVal))
                        {
                            string key = lastLabel;
                            // Prevent duplicate keys by adding the offset
                            if (results.ContainsKey(key)) key += $"_{start}";
                            results[key] = cleanVal;
                        }
                    }
                }
                i++;
            }
            return results;
        }
    }
}