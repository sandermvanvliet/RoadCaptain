using System.Runtime.InteropServices;

namespace RoadCaptain.DataExtractor
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0xC0)]
    public unsafe struct WadFileHeader
    {
        public uint m_nameICRC32;
    
        // char m_filePath[80] assuming FILE_PATH_SIZE is 80 
        // based on 0xC0 total size and subsequent field offsets
        public fixed byte m_filePath[80]; 
    
        public WadAssetType m_assetType;
        public uint m_fileLength;
        public uint m_seqNo;
    
        public IntPtr m_nextFileSameHash; // WAD_FILE_HEADER*
        public IntPtr m_nextFileSameAsset; // WAD_FILE_HEADER*
    
        public ulong f80;
        public IntPtr m_link; // WAD_FILE_HEADER*
    
        public int m_crypted;
        public int f94;
    
        public ulong f98;
        public ulong fA0;
        public ulong fA8;
        public ulong fB0;
        public ulong fB8;

        // Helper to get the data pointer immediately following this header
        public byte* FirstChar()
        {
            fixed (WadFileHeader* p = &this)
            {
                return (byte*)p + sizeof(WadFileHeader);
            }
        }

        // Helper to get the file path as a string
        public string GetFilePath()
        {
            fixed (byte* p = m_filePath)
            {
                return Marshal.PtrToStringAnsi((IntPtr)p);
            }
        }
    }
}