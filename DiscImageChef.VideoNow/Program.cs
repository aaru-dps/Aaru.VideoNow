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

namespace DiscImageChef.VideoNow
{
    static class MainClass
    {
        const  int                                   max_size = 635040000;
        static string                                AssemblyCopyright;
        static string                                AssemblyTitle;
        static AssemblyInformationalVersionAttribute AssemblyVersion;
        static readonly byte[] frameMarker =
            {
                0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81
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

                if(buffer.SequenceEqual(frameMarker)) break;

                framePosition++;
            }

            if(!buffer.SequenceEqual(frameMarker))
            {
                Console.WriteLine("Could not find any frame!");
                return;
            }

            Console.WriteLine("First frame found at {0}",             framePosition);
            Console.WriteLine("First frame {0} at a sector boundary", framePosition % 2352 == 0 ? "is" : "is not");

            int  totalFrames = 1;
            char progress    = ' ';
            framePosition += 19600;

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

                if(!buffer.SequenceEqual(frameMarker))
                {
                    Console.Write("\r                            \r");
                    Console.WriteLine("Frame {0} and the next one are not aligned...", totalFrames);
                    long expectedFramePosition = framePosition;

                    while(framePosition < fs.Length)
                    {
                        fs.Position = framePosition;
                        fs.Read(buffer, 0, buffer.Length);

                        if(buffer.SequenceEqual(frameMarker))
                        {
                            totalFrames++;
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

                totalFrames++;
                fs.Position = framePosition;
                fs.Read(buffer, 0, buffer.Length);
                framePosition += 19600;
            }

            Console.Write("\r                            \r");
            Console.WriteLine("Found {0} frames", totalFrames);
        }

        static void PrintCopyright()
        {
            Console.WriteLine("{0} {1}", AssemblyTitle, AssemblyVersion?.InformationalVersion);
            Console.WriteLine("{0}",     AssemblyCopyright);
            Console.WriteLine();
        }
    }
}