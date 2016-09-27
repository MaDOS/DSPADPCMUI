using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NAudio.Wave;

namespace DSPADPCMUI
{
    public class WAVFile
    {
        public byte[] data;
        public FileInfo file;
        public WaveFormat format;
        public Header header;

        public WAVFile(FileInfo file)
        {
            this.file = file;
            this.format = new WaveFormat(new BinaryReader(file.OpenRead()));

            BinaryReader br = new BinaryReader(file.OpenRead());
            this.header.fileDescriptionHeader = br.ReadChars(4);
            this.header.fileSize = br.ReadUInt32();
            this.header.wavDescriptionHeader = br.ReadChars(4);
            this.header.formatDescriptionHeader = br.ReadChars(4);
            this.header.waveSectionChunckSize = br.ReadUInt32();
            this.header.waveTypeFormat = br.ReadUInt16();
            this.header.channelCount = br.ReadUInt16();
            this.header.samplesPerSecond = br.ReadUInt32();
            this.header.bytesPerSecond = br.ReadUInt32();
            this.header.blockAlignment = br.ReadUInt16();
            this.header.bitsPerSample = br.ReadUInt16();
            this.header.dataDescriptionHeader = br.ReadChars(4);
            this.header.dataSize = br.ReadUInt32();

            this.data = br.ReadBytes((int)this.header.dataSize);

            br.Close();
        }

        public void toMono()
        {
            byte[] newBytes = new byte[this.header.dataSize / this.header.channelCount];
            for (uint i = 0, j = 0; i < this.header.dataSize; i = i + this.header.channelCount * (uint)(this.header.bitsPerSample / 8), j++)
            {
                newBytes[j] = data[i];
            }
            data = newBytes;
        }

        public struct Header
        {
            public char[] fileDescriptionHeader;
            public uint fileSize;
            public char[] wavDescriptionHeader;
            public char[] formatDescriptionHeader;
            public uint waveSectionChunckSize;
            public ushort waveTypeFormat;
            public ushort channelCount;
            public uint samplesPerSecond;
            public uint bytesPerSecond;
            public ushort blockAlignment;
            public ushort bitsPerSample;
            public char[] dataDescriptionHeader;
            public uint dataSize;
        }
    }
}
