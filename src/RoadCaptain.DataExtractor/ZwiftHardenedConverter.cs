using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RoadCaptain.DataExtractor;

public static class ZwiftHardenedConverter
{
    public static void ExportZtxToPng(byte[] tgaxData, string outPath)
    {
        // 1. We MUST use a NativeWindow to get a valid context on the current thread
        var settings = new NativeWindowSettings() { StartVisible = false, ClientSize = (1, 1) };
        using (var window = new NativeWindow(settings))
        {
            // IMPORTANT: Make the context current on this thread
            window.Context.MakeCurrent();

            // Parse Header
            var header = GetHeader(tgaxData);
            int width = header.wWidth;
            int height = header.wHeight;
            int offset = 18; // Size of TGAX_HEADER

            // Identify Format
            InternalFormat internalFormat = (header.wType == 24) ? 
                InternalFormat.CompressedRgbaS3tcDxt1Ext : 
                InternalFormat.CompressedRgbaS3tcDxt5Ext;

            // 2. Texture Setup
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            // Set parameters so OpenGL considers the texture "Complete"
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            // 3. Upload Data
            int dataLen = tgaxData.Length - offset;
            byte[] pixelData = new byte[dataLen];
            Array.Copy(tgaxData, offset, pixelData, 0, dataLen);

            // Upload compressed
            GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, dataLen, pixelData);

            // Check for errors immediately after upload
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception($"OpenGL Upload Error: {error}");

            // 4. Force synchronization
            GL.Flush();
            GL.Finish();

            // 5. Read back as RGBA
            byte[] rgbaPixels = new byte[width * height * 4];
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, rgbaPixels);

            // CHECK: If first 100 bytes are still 0, something is wrong with the GPU read-back
            bool allZeros = true;
            for (int i = 0; i < Math.Min(rgbaPixels.Length, 100); i++) {
                if (rgbaPixels[i] != 0) { allZeros = false; break; }
            }

            if (allZeros)
                throw new Exception("GPU returned an empty buffer. Check if your GPU drivers support S3TC/DXT decompression.");

            // 6. Save
            using (var image = Image.LoadPixelData<Rgba32>(rgbaPixels, width, height))
            {
                image.Mutate(x => {
                    x.Flip(FlipMode.Vertical);
                    // Ensure visibility
                    x.ProcessPixelRowsAsVector4(row => {
                        for (int i = 0; i < row.Length; i++) row[i].W = 1.0f; 
                    });
                });
                image.SaveAsPng(outPath);
            }

            GL.DeleteTexture(tex);
        }
    }

    private static TgaxHeader GetHeader(byte[] data)
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try { return Marshal.PtrToStructure<TgaxHeader>(handle.AddrOfPinnedObject()); }
        finally { handle.Free(); }
    }
}