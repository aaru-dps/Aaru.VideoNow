// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Program.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : VideoNow analyzing and decoding tools.
//
// --[ Description ] ----------------------------------------------------------
//
//     Main program loop.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DiscImageChef.VideoNow
{
    static class MainClass
    {
        const  int                                   max_size = 635040000;
        static string                                AssemblyCopyright;
        static string                                AssemblyTitle;
        static AssemblyInformationalVersionAttribute AssemblyVersion;
        //
        /// <summary>
        ///     This is some kind of header. Every 10 bytes there's an audio byte. Here it is without reordering from little
        ///     endian, so the first appearence is at 9th byte.
        /// </summary>
        static readonly byte[] frameMarker =
        {
            0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3,
            0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3,
            0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3,
            0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81,
            0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81,
            0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81,
            0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00,
            0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7,
            0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7,
            0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7,
            0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3,
            0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3,
            0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3,
            0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0x00, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81,
            0x00, 0xC7, 0x00, 0x00, 0x01, 0x02, 0x02, 0x04, 0x03, 0x06, 0x00, 0xFF, 0x04, 0x08, 0x05, 0x0A, 0x06,
            0x0C, 0x07, 0x0E, 0x00, 0xFF, 0x08, 0x11, 0x09, 0x13, 0x0A, 0x15, 0x0B, 0x17, 0x00, 0xFF, 0x0C, 0x19,
            0x0D, 0x1B, 0x0E, 0x1D, 0x0F, 0x1F, 0x00, 0xFF, 0x28, 0x00, 0x29, 0x02, 0x2A, 0x04, 0x2B, 0x06, 0x00,
            0xFF, 0x2C, 0x08, 0x2D, 0x0A, 0x2E, 0x0C, 0x2F, 0x0E, 0x00, 0xFF, 0x30, 0x11, 0x31, 0x13, 0x32, 0x15,
            0x33, 0x17, 0x00, 0xFF, 0x34, 0x19, 0x35, 0x1B, 0x36, 0x1D, 0x37, 0x1F, 0x00, 0xFF, 0x38, 0x00, 0x39,
            0x02, 0x3A, 0x04, 0x3B, 0x06, 0x00, 0xFF, 0x3C, 0x08, 0x3D, 0x0A, 0x3E, 0x0C, 0x3F, 0x0E, 0x00, 0xFF,
            0x40, 0x11, 0x41, 0x13, 0x42, 0x15, 0x43, 0x17, 0x00, 0xFF, 0x44, 0x19, 0x45, 0x1B, 0x46, 0x1D, 0x47,
            0x1F, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF
        };

        static readonly byte[] bitmapHeader
        = {
            0x42, 0x4D, 0x36, 0x87, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
            0x00, 0x00, 0x90, 0x00, 0x00, 0x00, 0x50, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x87, 0x00, 0x00, 0x23, 0x2E, 0x00, 0x00, 0x23, 0x2E, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };


        public static void Main(string[] args)
        {
            object[] attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            AssemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
            attributes    = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            AssemblyVersion =
                Attribute.GetCustomAttribute(typeof(MainClass).Assembly, typeof(AssemblyInformationalVersionAttribute))
                    as AssemblyInformationalVersionAttribute;
            AssemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            PrintCopyright();

            if(args.Length != 1)
            {
                Console.WriteLine("Usage: DiscImageChef.VideoNow dump.raw");
                return;
            }

            if(!File.Exists(args[0]))
            {
                Console.WriteLine("Specified file does not exist.");
                return;
            }

            FileStream fs;
            try { fs = File.Open(args[0], FileMode.Open, FileAccess.Read, FileShare.Read); }
            catch
            {
                Console.WriteLine("Could not open specified file.");
                return;
            }

            if(fs.Length > max_size)
            {
                Console.WriteLine("File is too big, not continuing.");
                return;
            }

            Console.WriteLine("File: {0}", args[0]);
            Console.WriteLine("Searching for first frame....");

            long   framePosition = 0;
            byte[] buffer        = new byte[frameMarker.Length];

            while(framePosition < 19600)
            {
                fs.Position = framePosition;
                fs.Read(buffer, 0, buffer.Length);

                for(int ab = 8; ab < buffer.Length; ab += 10) buffer[ab] = 0;

                if(buffer.SequenceEqual(frameMarker)) break;

                framePosition++;
            }

            for(int ab = 8; ab < buffer.Length; ab += 10) buffer[ab] = 0;

            if(!buffer.SequenceEqual(frameMarker))
            {
                Console.WriteLine("Could not find any frame!");
                return;
            }

            Console.WriteLine("First frame found at {0}",             framePosition);
            Console.WriteLine("First frame {0} at a sector boundary", framePosition % 2352 == 0 ? "is" : "is not");
            char progress = ' ';

            FileStream outFs = new FileStream(args[0] + ".audio.wav", FileMode.Create, FileAccess.ReadWrite,
                                              FileShare.Read);

            WaveHeader waveHeader = new WaveHeader {id = 0x46464952, format = 0x45564157};
            FormatChunk fmtChunk = new FormatChunk
            {
                id            = 0x20746D66,
                size          = 16,
                format        = 1,
                channels      = 2,
                sampleRate    = 17640,
                bitsPerSample = 8
            };
            fmtChunk.byteRate   = fmtChunk.sampleRate * fmtChunk.channels * fmtChunk.bitsPerSample / 8;
            fmtChunk.blockAlign = (ushort)(fmtChunk.channels * fmtChunk.bitsPerSample / 8);
            DataChunkHeader dataChunkHeader = new DataChunkHeader {id = 0x61746164};

            long headersSize = Marshal.SizeOf(waveHeader) + Marshal.SizeOf(fmtChunk) + Marshal.SizeOf(dataChunkHeader);
            outFs.Write(new byte[headersSize], 0, (int)headersSize);

            fs.Position = framePosition;
            byte[] frameBuffer = new byte[19600];
            fs.Read(frameBuffer, 0, frameBuffer.Length);
            frameBuffer = SwapBuffer(frameBuffer);

            for(int i = 9; i <= frameBuffer.Length; i += 10)
            {
                switch(i / 10 % 4)
                {
                    case 0:
                        progress = '-';
                        break;
                    case 1:
                        progress = '\\';
                        break;
                    case 2:
                        progress = '|';
                        break;
                    case 3:
                        progress = '/';
                        break;
                }

                Console.Write("\rExtracting audio {0}        ", progress);
                outFs.WriteByte(frameBuffer[i]);
            }

            FileStream frameFs = new FileStream($"{args[0]}.frame0000.bmp", FileMode.Create, FileAccess.ReadWrite,
                                              FileShare.Read);
            frameFs.Write(bitmapHeader, 0, bitmapHeader.Length);
            frameBuffer = DecodeFrame(frameBuffer);
            frameFs.Write(frameBuffer, 0, frameBuffer.Length);
            frameFs.Close();

            int totalFrames = 1;
            framePosition += 19600;
            buffer = new byte[frameMarker.Length];

            while(framePosition + 19600 < fs.Length)
            {
                switch(totalFrames % 4)
                {
                    case 0:
                        progress = '-';
                        break;
                    case 1:
                        progress = '\\';
                        break;
                    case 2:
                        progress = '|';
                        break;
                    case 3:
                        progress = '/';
                        break;
                }

                Console.Write("\rLooking for more frames {0}", progress);

                for(int ab = 8; ab < buffer.Length; ab += 10) buffer[ab] = 0;

                if(!buffer.SequenceEqual(frameMarker))
                {
                    Console.Write("\r                            \r");
                    Console.WriteLine("Frame {0} and the next one are not aligned...", totalFrames);
                    long expectedFramePosition = framePosition;

                    while(framePosition < fs.Length)
                    {
                        fs.Position = framePosition;
                        fs.Read(buffer, 0, buffer.Length);

                        for(int ab = 8; ab < buffer.Length; ab += 10) buffer[ab] = 0;

                        if(buffer.SequenceEqual(frameMarker))
                        {
                            Console.Write("\r                            \r");

                            fs.Position = framePosition;
                            frameBuffer = new byte[19600];
                            fs.Read(frameBuffer, 0, frameBuffer.Length);
                            frameBuffer = SwapBuffer(frameBuffer);

                            for(int i = 9; i <= frameBuffer.Length; i += 10)
                            {
                                switch(i / 10 % 4)
                                {
                                    case 0:
                                        progress = '-';
                                        break;
                                    case 1:
                                        progress = '\\';
                                        break;
                                    case 2:
                                        progress = '|';
                                        break;
                                    case 3:
                                        progress = '/';
                                        break;
                                }

                                Console.Write("\rExtracting audio {0}        ", progress);
                                outFs.WriteByte(frameBuffer[i]);
                            }
                            frameFs = new FileStream($"{args[0]}.frame{totalFrames:D6}.bmp", FileMode.Create, FileAccess.ReadWrite,
                                                                FileShare.Read);
                            frameFs.Write(bitmapHeader, 0, bitmapHeader.Length);
                            frameBuffer = DecodeFrame(frameBuffer);
                            frameFs.Write(frameBuffer, 0, frameBuffer.Length);
                            frameFs.Close();

                            totalFrames++;
                            Console.Write("\r                            \r");
                            Console.WriteLine("Frame {1} found at {0}, {2} bytes apart", framePosition, totalFrames,
                                              framePosition - expectedFramePosition);
                            Console.WriteLine("Frame {1} {0} at a sector boundary",
                                              framePosition % 2352 == 0 ? "is" : "is not", totalFrames);
                            framePosition += 19600;

                            break;
                        }

                        framePosition++;
                    }

                    continue;
                }

                if(framePosition % 2352 == 0)
                {
                    Console.Write("\r                            \r");
                    Console.WriteLine("Frame {0} is at a sector boundary", totalFrames);
                }

                Console.Write("\r                            \r");
                fs.Position = framePosition;
                frameBuffer = new byte[19600];
                fs.Read(frameBuffer, 0, frameBuffer.Length);
                frameBuffer = SwapBuffer(frameBuffer);

                for(int i = 9; i <= frameBuffer.Length; i += 10)
                {
                    switch(i / 10 % 4)
                    {
                        case 0:
                            progress = '-';
                            break;
                        case 1:
                            progress = '\\';
                            break;
                        case 2:
                            progress = '|';
                            break;
                        case 3:
                            progress = '/';
                            break;
                    }

                    Console.Write("\rExtracting audio {0}        ", progress);
                    outFs.WriteByte(frameBuffer[i]);
                }
                frameFs = new FileStream($"{args[0]}.frame{totalFrames:D6}.bmp", FileMode.Create, FileAccess.ReadWrite,
                                         FileShare.Read);
                frameFs.Write(bitmapHeader, 0, bitmapHeader.Length);
                frameBuffer = DecodeFrame(frameBuffer);
                frameFs.Write(frameBuffer, 0, frameBuffer.Length);
                frameFs.Close();

                totalFrames++;
                fs.Position = framePosition;
                fs.Read(buffer, 0, buffer.Length);
                framePosition += 19600;
            }

            Console.Write("\r                            \r");
            Console.WriteLine("Found {0} frames", totalFrames);

            fs.Close();
            outFs.Position = 0;

            dataChunkHeader.size = (uint)(outFs.Length - headersSize);
            waveHeader.size      = (uint)(outFs.Length - 8);

            buffer = new byte[Marshal.SizeOf(waveHeader)];
            IntPtr ptr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.StructureToPtr(waveHeader, ptr, false);
            Marshal.Copy(ptr, buffer, 0, buffer.Length);
            Marshal.FreeHGlobal(ptr);
            outFs.Write(buffer, 0, buffer.Length);

            buffer = new byte[Marshal.SizeOf(fmtChunk)];
            ptr    = Marshal.AllocHGlobal(buffer.Length);
            Marshal.StructureToPtr(fmtChunk, ptr, false);
            Marshal.Copy(ptr, buffer, 0, buffer.Length);
            Marshal.FreeHGlobal(ptr);
            outFs.Write(buffer, 0, buffer.Length);

            buffer = new byte[Marshal.SizeOf(dataChunkHeader)];
            ptr    = Marshal.AllocHGlobal(buffer.Length);
            Marshal.StructureToPtr(dataChunkHeader, ptr, false);
            Marshal.Copy(ptr, buffer, 0, buffer.Length);
            Marshal.FreeHGlobal(ptr);
            outFs.Write(buffer, 0, buffer.Length);

            outFs.Close();
        }

        static void PrintCopyright()
        {
            Console.WriteLine("{0} {1}", AssemblyTitle, AssemblyVersion?.InformationalVersion);
            Console.WriteLine("{0}",     AssemblyCopyright);
            Console.WriteLine();
        }

        static byte[] SwapBuffer(byte[] buffer)
        {
            byte[] tmp = new byte[buffer.Length];
            for(int i = 0; i < buffer.Length; i += 2)
            {
                tmp[i] = buffer[i + 1];
                tmp[i             + 1] = buffer[i];
            }
            return tmp;
        }

        static byte[] DecodeFrame(byte[] frameBuffer)
        {
            char progress = ' ';
            MemoryStream videoFs = new MemoryStream();
            Array.Reverse(frameBuffer);
            byte r, g, b;

            int index = 1;
            int indexBlock2;
            for(int i = 0; i < 19200; i += 240)
            {
                for(int k = 0; k < 120; k += 10)
                {
                    for(int j = 1; j < 9; j += 3)
                    {
                        indexBlock2 = index + 120;
                        switch(index / 10 % 4)
                        {
                            case 0:
                                progress = '-';
                                break;
                            case 1:
                                progress = '\\';
                                break;
                            case 2:
                                progress = '|';
                                break;
                            case 3:
                                progress = '/';
                                break;
                        }

                        Console.Write("\rExtracting video {0}        ", progress);
                        r = (byte)((frameBuffer[index] & 0xF0) + ((frameBuffer[index] & 0xF0) >> 4));
                        b = (byte)((frameBuffer[indexBlock2] & 0xF0) +
                                   ((frameBuffer[indexBlock2] & 0xF0) >> 4));
                        g = (byte)((frameBuffer[indexBlock2] & 0x0F) +
                                   ((frameBuffer[indexBlock2] & 0x0F) << 4));
                        videoFs.WriteByte(r);
                        videoFs.WriteByte(g);
                        videoFs.WriteByte(b);

                        r = (byte)((frameBuffer[indexBlock2 + 1] & 0xF0) +
                                   ((frameBuffer[indexBlock2 + 1] & 0xF0) >> 4));
                        b = (byte)((frameBuffer[index] & 0x0F) + ((frameBuffer[index] & 0x0F) << 4));
                        g = (byte)((frameBuffer[index + 1] & 0xF0) + ((frameBuffer[index + 1] & 0xF0) >> 4));
                        videoFs.WriteByte(r);
                        videoFs.WriteByte(g);
                        videoFs.WriteByte(b);

                        r = (byte)((frameBuffer[index + 1] & 0x0F) + ((frameBuffer[index + 1] & 0x0F) << 4));
                        b = (byte)((frameBuffer[indexBlock2 + 1] & 0x0F) +
                                   ((frameBuffer[indexBlock2 + 1] & 0x0F) << 4));
                        g = (byte)((frameBuffer[indexBlock2 + 2] & 0xF0) +
                                   ((frameBuffer[indexBlock2 + 2] & 0xF0) >> 4));
                        videoFs.WriteByte(r);
                        videoFs.WriteByte(g);
                        videoFs.WriteByte(b);

                        r = (byte)((frameBuffer[index + 120 + 2] & 0x0F) +
                                   ((frameBuffer[index + 120 + 2] & 0x0F) << 4));
                        b = (byte)((frameBuffer[index + 2] & 0xF0) + ((frameBuffer[index + 2] & 0xF0) >> 4));
                        g = (byte)((frameBuffer[index + 2] & 0x0F) + ((frameBuffer[index + 2] & 0x0F) << 4));
                        videoFs.WriteByte(r);
                        videoFs.WriteByte(g);
                        videoFs.WriteByte(b);

                        index += 3;
                    }

                    index += 1;
                }

               index += 120;
            }

            frameBuffer = videoFs.ToArray();
            byte[] tmp = new byte[144 * 3];
            for(int i = 0; i < 80; i++)
            {
                Array.Copy(frameBuffer, i * tmp.Length, tmp, 0, tmp.Length);
                Array.Reverse(tmp);
                Array.Copy(tmp, 0, frameBuffer, i *tmp.Length, tmp.Length);
            }
            return frameBuffer;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WaveHeader
    {
        public uint id;
        public uint size;
        public uint format;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FormatChunk
    {
        public uint   id;
        public uint   size;
        public ushort format;
        public ushort channels;
        public uint   sampleRate;
        public uint   byteRate;
        public ushort blockAlign;
        public ushort bitsPerSample;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct DataChunkHeader
    {
        public uint id;
        public uint size;
    }
}