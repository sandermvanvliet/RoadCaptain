using System.Runtime.InteropServices;

namespace RoadCaptain.DataExtractor
{
    public class WadUnpacker
    {
        private byte[] _decompBuf;
        private const int WAD_VERSION = 11;

        public WadUnpacker(string fileName, bool bDumpMode = true)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    if (bDumpMode) Console.Error.WriteLine($"wad_unpack error: cannot open '{fileName}' for read");
                    // Global return/error codes should ideally be handled via Properties or Exceptions
                    return;
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    // 1. Read Header
                    WadHeader wadHdr = ReadHeader(reader);

                    // 2. Validate Signature ("ZWF!")
                    if (wadHdr.GetSignature() != "ZWF!")
                    {
                        if (bDumpMode) Console.Error.WriteLine("wad_unpack error: unexpected wad file signature");
                        return;
                    }

                    // 3. Validate Version
                    if (wadHdr.Version != WAD_VERSION)
                    {
                        if (bDumpMode)
                            Console.Error.WriteLine(
                                $"wad_unpack error: unexpected version. Support: {WAD_VERSION}, Found: {wadHdr.Version}");
                        return;
                    }

                    // 4. Prepare Decompression Buffer
                    // C++ logic: ((decompSize + 263) & 0xFFFFFFF8) + 0x100020
                    uint decompBufSz = ((wadHdr.DecompressedSize + 263) & 0xFFFFFFF8) + 0x100020;
                    _decompBuf = new byte[decompBufSz];

                    // 5. Read Compressed Data
                    byte[] compressedData = reader.ReadBytes((int)wadHdr.CompressedSize);
                    if (compressedData.Length != wadHdr.CompressedSize)
                    {
                        if (bDumpMode) Console.Error.WriteLine("wad_unpack error: could not read compressed block");
                        return;
                    }

                    // 6. Decompress
                    uint crc32 = 0;
                    // Assuming TJZIP_Decompress is a DLL import or a custom C# method
                    uint resultLength = TJZIP_Decompress(
                        compressedData,
                        _decompBuf,
                        Marshal.SizeOf(wadHdr), // offset in destination
                        wadHdr.CompressedSize,
                        out crc32
                    );

                    // 7. Validate Result
                    if (resultLength == wadHdr.DecompressedSize)
                    {
                        if (wadHdr.Crc32 != crc32)
                        {
                            if (bDumpMode)
                                Console.Error.WriteLine(
                                    $"wad_unpack error: CRC32 mismatch. Found: {crc32}, Header: {wadHdr.Crc32}");
                        }

                        // Logic for WAD_OffsetsToPointers would go here
                        ProcessDecompressedData(_decompBuf);
                    }
                    else
                    {
                        if (bDumpMode)
                            Console.Error.WriteLine(
                                $"wad_unpack error: Decompressed length error. Result: {resultLength}, Expected: {wadHdr.DecompressedSize}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (bDumpMode) Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        private static WadHeader ReadHeader(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(WadHeader)));

            var header = BytesToStruct<WadHeader>(bytes);
            
            Console.WriteLine(header.Dump());
            
            return header;
        }

        private static TStruct BytesToStruct<TStruct>(byte[] bytes)
        {
            // Pin the managed memory while, copy it out the data, then unpin it
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            TStruct theStructure = (TStruct)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(TStruct));
            handle.Free();
            return theStructure;
        }

        private void ProcessDecompressedData(byte[] data)
        {
            var wh = new WadHeader();
            var ptr = new IntPtr();
            Marshal.StructureToPtr(wh, ptr, false);
            Marshal.Copy(data, 0, ptr, Marshal.SizeOf(wh));

            // // 2. Iterate through Asset Types
            // for (int assetIdx = 0; assetIdx < (int)WadAssetType.CNT; assetIdx++)
            // {
            //     // wh->m_assets[assetIdx] is currently an offset (stored as a pointer/long)
            //     long assetOffset = (long)wh.m_assets[assetIdx];
            //
            //     if (assetOffset != 0)
            //     {
            //         // Calculate absolute memory address
            //         WadFileHeader* pfh = (WadFileHeader*)(bwh + assetOffset);
            //
            //         // Validation and Assignment
            //         System.Diagnostics.Debug.Assert((int)pfh->m_assetType == assetIdx);
            //         wh->m_assets[assetIdx] = (IntPtr)pfh;
            //
            //         if (pfh->m_crypted != 0)
            //             EncryptDecryptWadString(pfh->FirstChar(), pfh->m_fileLength);
            //
            //         DoDump(pfh);
            //
            //         // Handle Link pointer
            //         if ((long)pfh->m_link != 0)
            //         {
            //             pfh->m_link = (IntPtr)(bwh + (long)pfh->m_link);
            //         }
            //
            //         // Iterate through linked list of files of the same asset type
            //         while ((long)pfh->m_nextFileSameAsset != 0)
            //         {
            //             WadFileHeader* pnfh = (WadFileHeader*)(bwh + (long)pfh->m_nextFileSameAsset);
            //             System.Diagnostics.Debug.Assert(pfh->m_assetType == pnfh->m_assetType);
            //
            //             pfh->m_nextFileSameAsset = (IntPtr)pnfh;
            //
            //             if ((long)pnfh->m_link != 0)
            //             {
            //                 pnfh->m_link = (IntPtr)(bwh + (long)pnfh->m_link);
            //             }
            //
            //             if (pnfh->m_crypted != 0)
            //                 EncryptDecryptWadString(pnfh->FirstChar(), pnfh->m_fileLength);
            //
            //             DoDump(pnfh);
            //             pfh = pnfh;
            //         }
            //     }
            // }
            //
            // // 3. Handle Hash Buckets (The Directory)
            // // The hash table follows immediately after the header
            // long* ptrAfterHeader = (long*)(bwh + sizeof(WadHeader));
            //
            // for (int dirIdx = 0; dirIdx < HASH_BUCKETS; dirIdx++)
            // {
            //     long dirOffset = ptrAfterHeader[dirIdx];
            //     if (dirOffset != 0)
            //     {
            //         WadFileHeader* dirPtr = (WadFileHeader*)(bwh + dirOffset);
            //         DoDump(dirPtr);
            //
            //         // Convert offset to absolute address in the hash table
            //         ptrAfterHeader[dirIdx] = (long)dirPtr;
            //
            //         // Follow the collision chain (linked list for the same hash)
            //         while ((long)dirPtr->m_nextFileSameHash != 0)
            //         {
            //             WadFileHeader* filePtr = (WadFileHeader*)(bwh + (long)dirPtr->m_nextFileSameHash);
            //             DoDump(filePtr);
            //
            //             dirPtr->m_nextFileSameHash = (IntPtr)filePtr;
            //             dirPtr = filePtr;
            //         }
            //     }
            // }
        }


        private unsafe uint TJZIP_Decompress(byte[] compressedData, byte[] decompressedBuf, int outputOffset,
            uint compressedSize, out uint crc32)
        {
            // Initialize pointers/indices
            int srcIndex = 0;
            int destIndex = outputOffset;
            uint currentCrc = 0;

            // We use Spans to safely view parts of our arrays
            Span<byte> src = compressedData;
            Span<byte> dest = decompressedBuf;

            // Initial Raw Data Block
            TJZIP_ParseRawDataBlock(src, ref srcIndex, dest, ref destIndex, ref currentCrc);

            while (srcIndex < (int)compressedSize)
            {
                int dCode = TJZIP_ParseDictionaryCode(src, ref srcIndex, dest, ref destIndex, ref currentCrc);

                if (dCode != 3) // Assuming 3 is a 'stop' or 'special' code
                {
                    if (dCode == 0)
                    {
                        TJZIP_ParseRawDataBlock(src, ref srcIndex, dest, ref destIndex, ref currentCrc);
                    }
                    else
                    {
                        // Ensure we don't read past the source buffer
                        if (srcIndex < (int)compressedSize)
                        {
                            // Copy dCode number of bytes
                            for (int i = 0; i < dCode; i++)
                            {
                                byte chr = src[srcIndex++];
                                dest[destIndex++] = chr;

                                // CRC32 Calculation
                                currentCrc = g_crc32Table[(byte)currentCrc ^ chr] ^ (currentCrc >> 8);
                            }
                        }
                    }
                }

                // Logic check: ensuring pointers are moving forward (the C++ ptr comparison)
                if ((0 < destIndex) != (srcIndex > 0))
                {
                    crc32 = 0;
                    return 0;
                }
            }

            crc32 = ~currentCrc;
            return (uint)(destIndex - outputOffset);
        }

        private void TJZIP_ParseRawDataBlock(
            ReadOnlySpan<byte> src,
            ref int srcIdx,
            Span<byte> dest,
            ref int destIdx,
            ref uint crc32)
        {
            byte chr0 = src[srcIdx++];
            int len;

            if (chr0 != 0)
            {
                len = chr0 + 2;
            }
            else
            {
                byte chr1 = src[srcIdx++];
                byte chr2 = src[srcIdx++];

                if (chr2 != 0)
                {
                    len = (chr2 << 8) | chr1;
                }
                else
                {
                    byte chr3 = src[srcIdx++];
                    byte chr4 = src[srcIdx++];

                    if (chr3 != 0)
                    {
                        len = (chr3 << 16) | (chr4 << 8) | chr1;
                    }
                    else
                    {
                        byte chr5 = src[srcIdx++];
                        byte chr6 = src[srcIdx++];
                        // Note: Following the original C++ logic exactly:
                        // chr4 shifted 24, chr5 shifted 8, chr6 shifted 16, chr1 base
                        len = (chr4 << 24) | (chr5 << 8) | (chr6 << 16) | chr1;
                    }
                }
            }

            // Copy bytes and update CRC
            for (int i = 0; i < len; i++)
            {
                byte b = src[srcIdx++];
                dest[destIdx++] = b;

                // Update CRC32 using the lookup table
                crc32 = g_crc32Table[(byte)crc32 ^ b] ^ (crc32 >> 8);
            }
        }

        private int TJZIP_ParseDictionaryCode(
            ReadOnlySpan<byte> src,
            ref int srcIdx,
            Span<byte> dest,
            ref int destIdx,
            ref uint crc32)
        {
            int backOff = 0;
            int len = 0;
            int result;

            byte code = src[srcIdx++];
            byte chr1 = src[srcIdx++];
            byte chr2, chr3;

            // Use bitmask 0xE0 to determine the compression "mode"
            switch (code & 0xE0)
            {
                case 0xE0:
                    chr2 = src[srcIdx++];
                    if ((code & 0xF) != 0)
                    {
                        len = (code & 0xF) + 3;
                        backOff = ((code << 10) & 0x4000) | ((chr1 >> 2) << 8) | chr2;
                        result = chr1 & 3;
                    }
                    else
                    {
                        chr3 = src[srcIdx++];
                        if (chr1 != 0)
                        {
                            len = chr1 + 18;
                            backOff = ((code << 10) & 0x4000) | ((chr2 >> 2) << 8) | chr3;
                            result = chr2 & 3;
                        }
                        else
                        {
                            byte chr4 = src[srcIdx++];
                            byte chr5 = src[srcIdx++];
                            len = chr3 | (chr2 << 8);
                            result = chr4 & 3;
                            if (len != 0)
                            {
                                backOff = ((code << 10) & 0x4000) | ((chr4 >> 2) << 8) | chr5;
                            }
                        }
                    }

                    break;

                case 0xC0:
                    chr2 = src[srcIdx++];
                    len = (code & 0x1F) + 4;
                    backOff = chr2 | ((chr1 >> 2) << 8);
                    result = chr1 & 3;
                    break;

                default:
                    len = (code >> 5) + 4;
                    result = code & 3;
                    backOff = chr1 | (((code >> 2) & 3) << 8);
                    break;
            }

            // Calculate the dictionary pointer (lookback position)
            // pDict = (*pDestPtr) - backOff
            int dictIdx = destIdx - backOff;

            // Copy the dictionary data to the current destination
            while (len-- > 0)
            {
                // In C#, accessing the same Span (dest) at an older index
                byte chr = dest[dictIdx++];
                dest[destIdx++] = chr;

                // Update CRC32
                crc32 = g_crc32Table[(byte)crc32 ^ chr] ^ (crc32 >> 8);
            }

            return result;
        }


        private static readonly uint[] g_crc32Table = new uint[]
        {
            0, 0x77073096, 0xEE0E612C, 0x990951BA, 0x76DC419, 0x706AF48F,
            0xE963A535, 0x9E6495A3, 0xEDB8832, 0x79DCB8A4, 0xE0D5E91E,
            0x97D2D988, 0x9B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91,
            0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE, 0x1ADAD47D,
            0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856, 0x646BA8C0,
            0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9, 0xFA0F3D63,
            0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172,
            0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA,
            0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3, 0x45DF5C75,
            0xDCD60DCF, 0xABD13D59, 0x26D930AC, 0x51DE003A, 0xC8D75180,
            0xBFD06116, 0x21B4F4B5, 0x56B3C423, 0xCFBA9599, 0xB8BDA50F,
            0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 0x2F6F7C87,
            0x58684C11, 0xC1611DAB, 0xB6662D3D, 0x76DC4190, 0x1DB7106,
            0x98D220BC, 0xEFD5102A, 0x71B18589, 0x6B6B51F, 0x9FBFE4A5,
            0xE8B8D433, 0x7807C9A2, 0xF00F934, 0x9609A88E, 0xE10E9818,
            0x7F6A0DBB, 0x86D3D2D, 0x91646C97, 0xE6635C01, 0x6B6B51F4,
            0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 0x1B01A57B,
            0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950, 0x8BBEB8EA,
            0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65,
            0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2, 0x4ADFA541,
            0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9FC,
            0xAD678846, 0xDA60B8D0, 0x44042D73, 0x33031DE5, 0xAA0A4C5F,
            0xDD0D7CC9, 0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086,
            0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F, 0x5EDEF90E,
            0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81,
            0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6, 0x3B6E20C,
            0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x4DB2615, 0x73DC1683,
            0xE3630B12, 0x94643B84, 0xD6D6A3E, 0x7A6A5AA8, 0xE40ECF0B,
            0x9309FF9D, 0xA00AE27, 0x7D079EB1, 0xF00F9344, 0x8708A3D2,
            0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 0x196C3671,
            0x6E6B06E7, 0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC,
            0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 0xD6D6A3E8,
            0xA1D1937E, 0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767,
            0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6,
            0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79,
            0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236, 0xCC0C7795,
            0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28,
            0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B,
            0x5BDEAE1D, 0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x26D930A,
            0x9C0906A9, 0xEB0E363F, 0x72076785, 0x5005713, 0x95BF4A82,
            0xE2B87A14, 0x7BB12BAE, 0xCB61B38, 0x92D28E9B, 0xE5D5BE0D,
            0x7CDCEFB7, 0xBDBDF21, 0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8,
            0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
            0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF,
            0xF862AE69, 0x616BFFD3, 0x166CCF45, 0xA00AE278, 0xD70DD2EE,
            0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 0x4969474D,
            0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC, 0x40DF0B66, 0x37D83BF0,
            0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9, 0xBDBDF21C,
            0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605, 0xCDD70693,
            0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02,
            0x2A6F2B94, 0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
        };
    }
}