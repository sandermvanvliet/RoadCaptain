// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
