using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RoadCaptain.DataExtractor
{
    // --- Headers ---
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TgaxHeader {
        public ushort d1, d2, wType, d3, d4, d5, wWidth, wHeight;
        public byte bpp, desc;
    }
    
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct DDS_HEADER {
        [FieldOffset(0)]
        public uint           dwSize;
        [FieldOffset(1)]
        public uint           dwFlags;
        [FieldOffset(2)]
        public uint           dwHeight;
        [FieldOffset(3)]
        public uint           dwWidth;
        [FieldOffset(4)]
        public uint           dwPitchOrLinearSize;
        [FieldOffset(5)]
        public uint           dwDepth;
        [FieldOffset(6)]
        public uint           dwMipMapCount;
        [FieldOffset(7)]
        public uint[]           dwReserved1;
        [FieldOffset(18)]
        public DDS_PIXELFORMAT ddspf;
        [FieldOffset(26)]
        public uint           dwCaps;
        [FieldOffset(27)]
        public uint           dwCaps2;
        [FieldOffset(28)]
        public uint           dwCaps3;
        [FieldOffset(29)]
        public uint           dwCaps4;
        [FieldOffset(30)]
        public uint           dwReserved2;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DDS_PIXELFORMAT
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwFourCC;
        public uint dwRGBBitCount;
        public uint dwRBitMask;
        public uint dwGBitMask;
        public uint dwBBitMask;
        public uint dwABitMask;
    }

    public class TgxConverter
    {
        public static void Run(string[] args)
        {
            if (args.Length == 0) {
                Console.WriteLine("Usage: converter.exe <file.ztx>");
                return;
            }

            string inputPath = args[0];
            string outputPath = Path.ChangeExtension(Path.GetFileName(inputPath), ".dds");

            try {
                // 1. Read and Decompress ZTX
                byte[] tgaxData = DecompressZtx(inputPath);

                ExtractDdsFromTgax(tgaxData, outputPath);
                // // 2. Initialize a "Headless" OpenGL context (No window shown)
                // using (var gameWindow = new GameWindow(GameWindowSettings.Default, new NativeWindowSettings { StartVisible = true }))
                // {
                //     gameWindow.MakeCurrent();
                //     
                //     ConvertTgaxToPng(tgaxData, outputPath);
                // }
                
                //ZwiftHardenedConverter.ExportZtxToPng(tgaxData, outputPath);
                
                Console.WriteLine($"Successfully converted to: {outputPath}");
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void ExtractDdsFromTgax(byte[] tgaxData, string outputPath)
        {
            var tgaxHeaderSize = Marshal.SizeOf<TgaxHeader>();

            var offset = tgaxHeaderSize;

            int g_nSkipMipCount = 0; //ultra gfx profile, load all qualities
            uint max_width = h->wWidth;
            uint max_height = h->wHeight;
            int div = 0;
            while ((max_width >> div) > 4) ++div;
            int   nSkipMipCount = g_nSkipMipCount;
            if (div <= 6)
            {
                nSkipMipCount = div + g_nSkipMipCount - 6;
            }

            if (nSkipMipCount < 0)
            {
                nSkipMipCount = 0;
            }
            
            int size_divider = 0;
            uint tdata_mult = 16, fmt_idx = 6;
            if (h->wType == 24) {
                fmt_idx = 5;
            }
            
            using var out_dds = File.OpenWrite(outputPath);
            
            out_dds.Write("DDS "u8);
            
            DDS_HEADER dds_header = new();
            dds_header.dwSize = (uint)Marshal.SizeOf<DDS_HEADER>();
            dds_header.dwFlags = 1 /*DDSD_CAPS*/ | 2 /*DDSD_HEIGHT*/ | 4 /*DDSD_WIDTH*/ | 0x1000 /*DDSD_PIXELFORMAT*/ | 0x20000 /*DDSD_MIPMAPCOUNT*/ /*| 0x80000 DDSD_LINEARSIZE*/;
            dds_header.dwHeight = max_height; dds_header.dwWidth = max_width;
            dds_header.dwMipMapCount = (uint)div;
            dds_header.dwCaps = 8 /*DDSCAPS_COMPLEX*/ | 0x1000 /*DDSCAPS_TEXTURE*/ | 0x400000 /*DDSCAPS_MIPMAP*/;
            dds_header.ddspf.dwSize = (uint)Marshal.SizeOf<DDS_PIXELFORMAT>();
            dds_header.ddspf.dwFlags = 1 /*DDPF_ALPHAPIXELS*/ | 4 /*DDPF_FOURCC*/;
            dds_header.ddspf.dwFourCC = (uint)(fmt_idx == 5 ? 0x31545844 /*DXT1*/ : 0x35545844 /*DXT5*/);
            
            var headerBytes = new byte[dds_header.dwSize];
            var headerPtr = Marshal.AllocHGlobal((int)dds_header.dwSize);
            Marshal.StructureToPtr(dds_header, headerPtr, true);
            Marshal.Copy(headerPtr, headerBytes, 0, headerBytes.Length);
            out_dds.Write(headerBytes);
            
            out_dds.Write((const char*)data + sizeof(TGAX_HEADER), size - sizeof(TGAX_HEADER));
        }

        static byte[] DecompressZtx(string path)
        {
            byte[] fileData = File.ReadAllBytes(path);
            // Zwift ZTX Header: "ZHR\x1" (4 bytes) + Decompressed Size (8 bytes) + Padding (4 bytes)
            long decomprSize = BitConverter.ToInt64(fileData, 4);
            
            using var msInput = new MemoryStream(fileData, 16, fileData.Length - 16);
            using var msOutput = new MemoryStream();
            using var zlib = new ZLibStream(msInput, CompressionMode.Decompress);
            zlib.CopyTo(msOutput);
            
            return msOutput.ToArray();
        }
        
        static void ConvertTgaxToPng(byte[] data, string outPath)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var header = Marshal.PtrToStructure<TgaxHeader>(handle.AddrOfPinnedObject());
            handle.Free();

            int width = header.wWidth;
            int height = header.wHeight;
            int offset = Marshal.SizeOf<TgaxHeader>();

            // FIX 1: Set Alignment to 1 to handle non-power-of-two textures safely
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);

            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            InternalFormat format = (header.wType == 24) ? 
                InternalFormat.CompressedRgbaS3tcDxt1Ext : 
                InternalFormat.CompressedRgbaS3tcDxt5Ext;

            int dataLen = data.Length - offset;
            byte[] pixelData = new byte[dataLen];
            Array.Copy(data, offset, pixelData, 0, dataLen);

            GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, format, width, height, 0, dataLen, pixelData);

            // FIX 2: Ensure GPU has finished the command before we request pixels back
            GL.Finish(); 

            byte[] rgbaPixels = new byte[width * height * 4];
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, rgbaPixels);

            using (var image = Image.LoadPixelData<Rgba32>(rgbaPixels, width, height))
            {
                image.Mutate(x => x.Flip(FlipMode.Vertical));

                // FIX 3: Debugging - Force alpha to 255 (Opaque) 
                // If the image was "invisible" because of the alpha channel, this will fix it.
                image.Mutate(x => x.ProcessPixelRowsAsVector4(row => {
                    for (int i = 0; i < row.Length; i++) {
                        row[i].W = 1.0f; // Set Alpha to 100%
                    }
                }));

                image.SaveAsPng(outPath);
            }

            GL.DeleteTexture(tex);
        }

        static void OrigConvertTgaxToPng(byte[] data, string outPath)
        {
            // Parse Header
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var header = Marshal.PtrToStructure<TgaxHeader>(handle.AddrOfPinnedObject());
            handle.Free();

            int width = header.wWidth;
            int height = header.wHeight;
            int offset = Marshal.SizeOf<TgaxHeader>();

            // Identify Format
            InternalFormat format = (header.wType == 24) ? 
                InternalFormat.CompressedRgbaS3tcDxt1Ext : 
                InternalFormat.CompressedRgbaS3tcDxt5Ext;

            // 1. Upload to GPU
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);
            
            int dataLen = data.Length - offset;
            byte[] pixelData = new byte[dataLen];
            Array.Copy(data, offset, pixelData, 0, dataLen);

            GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, format, width, height, 0, dataLen, pixelData);

            // 2. Download from GPU as standard RGBA (Hardware decompress happens here)
            byte[] rgbaPixels = new byte[width * height * 4];
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, rgbaPixels);

            // 3. Save using ImageSharp
            // OpenGL is bottom-to-top, ImageSharp is top-to-bottom, so we flip during creation
            using (var image = Image.LoadPixelData<Rgba32>(rgbaPixels, width, height))
            {
                // Flip vertically to correct OpenGL orientation
                image.Mutate(x => x.Flip(FlipMode.Vertical));
                image.SaveAsPng(outPath);
            }

            GL.DeleteTexture(tex);
        }
    }
}
