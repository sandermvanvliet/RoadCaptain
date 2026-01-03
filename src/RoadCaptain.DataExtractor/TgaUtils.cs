// using System.IO.Compression;
// using System.Runtime.InteropServices;
// using System.Text;
//
// namespace RoadCaptain.DataExtractor
// {
//     // Replicating the TGAX Header Structure
//     [StructLayout(LayoutKind.Sequential, Pack = 1)]
//     public struct TgaxHeader
//     {
//         public ushort wDummy1;
//         public ushort wDummy2;
//         public ushort wType; // 24 = DXT1, others often DXT5
//         public ushort wDummy3;
//         public ushort wDummy4;
//         public ushort wDummy5;
//         public ushort wWidth;
//         public ushort wHeight;
//         public byte bBitsPerPixel;
//         public byte bDescriptor;
//     }
//
//     [StructLayout(LayoutKind.Sequential, Pack = 1)]
//     public struct DdsPixelFormat
//     {
//         public uint dwSize;
//         public uint dwFlags;
//         public uint dwFourCC;
//         public uint dwRGBBitCount;
//         public uint dwRBitMask;
//         public uint dwGBitMask;
//         public uint dwBBitMask;
//         public uint dwABitMask;
//     }
//
//     [StructLayout(LayoutKind.Sequential, Pack = 1)]
//     public struct DdsHeader
//     {
//         public uint dwSize;
//         public uint dwFlags;
//         public uint dwHeight;
//         public uint dwWidth;
//         public uint dwPitchOrLinearSize;
//         public uint dwDepth;
//         public uint dwMipMapCount;
//         [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
//         public uint[] dwReserved1;
//         public DdsPixelFormat ddspf;
//         public uint dwCaps;
//         public uint dwCaps2;
//         public uint dwCaps3;
//         public uint dwCaps4;
//         public uint dwReserved2;
//     }
//
//     public class TgaUtils
//     {
//         static string g_in_path;
//         static string g_out_path;
//         static int g_nSkipMipCount = 0;
//
//         public static void Run(string[] args)
//         {
//             if (args.Length < 1)
//             {
//                 Console.Error.WriteLine("Zwift tgax utility.\nUsage example:\n1. tgax.exe file.ztx\n2. tgax.exe file.tgax\n3. tgax.exe file.dds");
//                 return;
//             }
//
//             foreach (var path in args)
//             {
//                 string phase = "File open";
//                 g_in_path = path;
//                 try
//                 {
//                     byte[] fileData = File.ReadAllBytes(g_in_path);
//                     if (fileData.Length < 4) continue;
//
//                     // Check for ZTX (ZHR\x1)
//                     if (IsValidCompAssetHeader(fileData))
//                     {
//                         phase = "ztx processing";
//                         long decomprSize = BitConverter.ToInt64(fileData, 4);
//                         byte[] compressedPart = new byte[fileData.Length - 16];
//                         Array.Copy(fileData, 16, compressedPart, 0, compressedPart.Length);
//
//                         byte[] decompressed = DecompressZlib(compressedPart);
//
//                         if (decompressed.Length == decomprSize)
//                         {
//                             g_out_path = CreateOutputPath(".tgax");
//                             File.WriteAllBytes(g_out_path, decompressed);
//                             ProcessTgax(decompressed);
//                         }
//                     }
//                     // Check for DDS
//                     else if (Encoding.ASCII.GetString(fileData, 0, 4) == "DDS ")
//                     {
//                         phase = "dds processing";
//                         ProcessDds(fileData);
//                     }
//                     // Treat as raw TGAX
//                     else
//                     {
//                         phase = "tgax processing";
//                         ProcessTgax(fileData);
//                     }
//                 }
//                 catch (Exception e)
//                 {
//                     Console.Error.WriteLine($"Error during {phase} for {g_in_path}: {e.Message}");
//                 }
//             }
//         }
//
//         static bool IsValidCompAssetHeader(byte[] data)
//         {
//             return data.Length > 4 && data[0] == 'Z' && data[1] == 'H' && data[2] == 'R' && data[3] == 1;
//         }
//
//         static byte[] DecompressZlib(byte[] data)
//         {
//             // C# ZLibStream expects the data without the 2-byte Zlib header if using DeflateStream, 
//             // but .NET 6+ ZLibStream handles the header automatically.
//             using var msInput = new MemoryStream(data);
//             using var msOutput = new MemoryStream();
//             using var zlib = new ZLibStream(msInput, CompressionMode.Decompress);
//             zlib.CopyTo(msOutput);
//             return msOutput.ToArray();
//         }
//
//         static byte[] CompressZlib(byte[] data)
//         {
//             using var msOutput = new MemoryStream();
//             using (var zlib = new ZLibStream(msOutput, CompressionLevel.Optimal))
//             {
//                 zlib.Write(data, 0, data.Length);
//             }
//             return msOutput.ToArray();
//         }
//
//         static void ProcessTgax(byte[] data)
//         {
//             TgaxHeader h = ByteArrayToStruct<TgaxHeader>(data);
//             int div = 0;
//             while ((h.wWidth >> div) > 4) div++;
//
//             // Mimicking the original logic for DDS creation
//             g_out_path = CreateOutputPath(".dds");
//             using var fs = new FileStream(g_out_path, FileMode.Create);
//             using var bw = new BinaryWriter(fs);
//
//             bw.Write(Encoding.ASCII.GetBytes("DDS "));
//             
//             DdsHeader dds = new DdsHeader
//             {
//                 dwSize = (uint)Marshal.SizeOf<DdsHeader>(),
//                 dwFlags = 0x1 | 0x2 | 0x4 | 0x1000 | 0x20000,
//                 dwHeight = h.wHeight,
//                 dwWidth = h.wWidth,
//                 dwMipMapCount = (uint)div,
//                 dwCaps = 0x8 | 0x1000 | 0x400000,
//                 ddspf = new DdsPixelFormat
//                 {
//                     dwSize = (uint)Marshal.SizeOf<DdsPixelFormat>(),
//                     dwFlags = 0x1 | 0x4,
//                     dwFourCC = (h.wType == 24) ? 0x31545844u : 0x35545844u // DXT1 or DXT5
//                 }
//             };
//
//             bw.Write(StructToBytes(dds));
//             bw.Write(data, Marshal.SizeOf<TgaxHeader>(), data.Length - Marshal.SizeOf<TgaxHeader>());
//             Console.WriteLine($"{g_out_path}: written OK");
//         }
//
//         static void ProcessDds(byte[] data)
//         {
//             // Logic to convert DDS back to TGAX/ZTX
//             int headerSize = Marshal.SizeOf<DdsHeader>();
//             DdsHeader dds = ByteArrayToStruct<DdsHeader>(data.AsSpan(4, headerSize).ToArray());
//             
//             TgaxHeader tgax = new TgaxHeader
//             {
//                 wType = (ushort)(dds.ddspf.dwFourCC == 0x31545844 ? 24 : 0x820),
//                 wDummy2 = 2,
//                 wWidth = (ushort)dds.dwWidth,
//                 wHeight = (ushort)dds.dwHeight
//             };
//
//             byte[] pixelData = new byte[data.Length - 4 - headerSize];
//             Array.Copy(data, 4 + headerSize, pixelData, 0, pixelData.Length);
//
//             // Save TGAX
//             byte[] tgaxFile = new byte[Marshal.SizeOf<TgaxHeader>() + pixelData.Length];
//             Buffer.BlockCopy(StructToBytes(tgax), 0, tgaxFile, 0, Marshal.SizeOf<TgaxHeader>());
//             Buffer.BlockCopy(pixelData, 0, tgaxFile, Marshal.SizeOf<TgaxHeader>(), pixelData.Length);
//             
//             File.WriteAllBytes(CreateOutputPath(".tgax"), tgaxFile);
//
//             // Save ZTX (Compressed)
//             g_out_path = CreateOutputPath(".ztx");
//             using var fs = new FileStream(g_out_path, FileMode.Create);
//             using var bw = new BinaryWriter(fs);
//             bw.Write(Encoding.ASCII.GetBytes("ZHR\x1"));
//             bw.Write((long)tgaxFile.Length);
//             bw.Write(new byte[4]); // Dummy padding
//             bw.Write(CompressZlib(tgaxFile));
//             Console.WriteLine($"{g_out_path}: written OK");
//         }
//
//         static string CreateOutputPath(string ext)
//         {
//             return Path.Combine(Environment.CurrentDirectory, Path.GetFileNameWithoutExtension(g_in_path) + ext);
//         }
//
//         static T ByteArrayToStruct<T>(byte[] bytes) where T : struct
//         {
//             GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
//             try { return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject()); }
//             finally { handle.Free(); }
//         }
//
//         static byte[] StructToBytes<T>(T str) where T : struct
//         {
//             int size = Marshal.SizeOf(str);
//             byte[] arr = new byte[size];
//             IntPtr ptr = Marshal.AllocHGlobal(size);
//             try
//             {
//                 Marshal.StructureToPtr(str, ptr, true);
//                 Marshal.Copy(ptr, arr, 0, size);
//                 return arr;
//             }
//             finally { Marshal.FreeHGlobal(ptr); }
//         }
//     }
// }