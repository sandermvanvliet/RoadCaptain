using System.Runtime.InteropServices;
using System.Text;

namespace RoadCaptain.DataExtractor
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
    public unsafe struct WadHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] FileSignatureChars;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
        public byte[] WadFilePath; // Assuming same FILE_PATH_SIZE
        public UInt32 m_wadFilePathCrc32;
    
        // Array of pointers to headers for each asset type
        // In C#, fixed arrays of pointers/IntPtr require 'fixed' or explicit expansion
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)WadAssetType.CNT)]
        public IntPtr[] m_assets; 
    
        public uint Crc32;
        public uint Version;
        public uint DecompressedSize;
        public uint CompressedSize;

        public string GetSignature()
        {
            fixed (byte* p = FileSignatureChars) return Encoding.ASCII.GetString(p, 4);
        }

        public string Dump()
        {
            var sig = Encoding.ASCII.GetString(this.FileSignatureChars);
            return $"sig: {sig}, compressed size: {CompressedSize}, decompressed size: {DecompressedSize}, Version: {Version}";
        }
    }
}