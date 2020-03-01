// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Reflection;

// ReSharper disable LocalizableElement

namespace Aaru.VideoNow
{
    internal static class MainClass
    {
        const  int                                   MAX_SIZE = 635040000;
        static string                                assemblyCopyright;
        static string                                assemblyTitle;
        static AssemblyInformationalVersionAttribute assemblyVersion;

        static readonly byte[] FrameStart =
        {
            0xE3, 0x81, 0xC7, 0xE3, 0x81, 0xC7, 0xE3, 0x81
        };

        static readonly byte[] SwappedFrameStart =
        {
            0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3
        };

        public static void Main(string[] args)
        {
            object[] attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            assemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
            attributes    = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

            assemblyVersion =
                Attribute.GetCustomAttribute(typeof(MainClass).Assembly, typeof(AssemblyInformationalVersionAttribute))
                    as AssemblyInformationalVersionAttribute;

            assemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            PrintCopyright();

            if(args.Length != 1)
            {
                Console.WriteLine(Localization.Usage);

                return;
            }

            if(!File.Exists(args[0]))
            {
                Console.WriteLine(Localization.FileDoesNotExist);

                return;
            }

            FileStream fs;

            try
            {
                fs = File.Open(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch
            {
                Console.WriteLine(Localization.FileCannotBeOpened);

                return;
            }

            if(fs.Length > MAX_SIZE)
            {
                Console.WriteLine(Localization.FileIsTooBig);

                return;
            }

            Console.WriteLine(Localization.FileName, args[0]);
            Console.WriteLine(Localization.SearchingFirstFrame);

            long   framePosition   = 0;
            byte[] buffer          = new byte[Color.FrameMarker.Length];
            byte[] swappedBuffer   = new byte[Color.FrameMarker.Length];
            bool   swapped         = false;
            bool   xp              = false;
            byte[] startBuffer     = new byte[FrameStart.Length];
            byte[] xpBuffer        = new byte[Xp.FrameMarker.Length];
            byte[] xpSwappedBuffer = new byte[Xp.FrameMarker.Length];

            while(framePosition < 19760)
            {
                fs.Position = framePosition;
                fs.Read(startBuffer, 0, startBuffer.Length);

                if(!startBuffer.SequenceEqual(FrameStart) &&
                   !startBuffer.SequenceEqual(SwappedFrameStart))
                {
                    framePosition++;

                    continue;
                }

                fs.Position = framePosition;
                fs.Read(buffer, 0, buffer.Length);

                for(int ab = 8; ab < buffer.Length; ab += 10)
                    buffer[ab] = 0;

                if(buffer.SequenceEqual(Color.FrameMarker))
                    break;

                fs.Position = framePosition;
                fs.Read(swappedBuffer, 0, swappedBuffer.Length);

                for(int ab = 9; ab < swappedBuffer.Length; ab += 10)
                    swappedBuffer[ab] = 0;

                if(swappedBuffer.SequenceEqual(Color.SwappedFrameMarker))
                {
                    swapped = true;

                    break;
                }

                fs.Position = framePosition;
                fs.Read(xpBuffer, 0, xpBuffer.Length);

                for(int i = 0; i < xpBuffer.Length; i++)
                    xpBuffer[i] &= Xp.FrameMask[i];

                if(xpBuffer.SequenceEqual(Xp.FrameMarker))
                {
                    xp = true;

                    break;
                }

                fs.Position = framePosition;
                fs.Read(xpSwappedBuffer, 0, xpSwappedBuffer.Length);

                for(int i = 0; i < xpSwappedBuffer.Length; i++)
                    xpSwappedBuffer[i] &= Xp.SwappedFrameMask[i];

                if(xpSwappedBuffer.SequenceEqual(Xp.SwappedFrameMarker))
                {
                    swapped = true;
                    xp      = true;

                    break;
                }

                framePosition++;
            }

            if(!buffer.SequenceEqual(Color.FrameMarker)               &&
               !swappedBuffer.SequenceEqual(Color.SwappedFrameMarker) &&
               !xpBuffer.SequenceEqual(Xp.FrameMarker)                &&
               !xpSwappedBuffer.SequenceEqual(Xp.SwappedFrameMarker))
            {
                Console.WriteLine(Localization.NoFrameFound);

                return;
            }

            Console.WriteLine(Localization.FirstFrameFoundAt, framePosition);

            Console.WriteLine(framePosition % 2352 == 0 ? Localization.FirstFrameIsAtSectorBoundary
                                  : Localization.FirstFrameIsNotAtSectorBoundary);

            if(xp)
                Xp.Decode(args[0], fs, swapped, framePosition);
            else
                Color.Decode(args[0], fs, swapped, framePosition);

            fs.Close();
        }

        static void PrintCopyright()
        {
            Console.WriteLine("{0} {1}", assemblyTitle, assemblyVersion?.InformationalVersion);
            Console.WriteLine("{0}", assemblyCopyright);
            Console.WriteLine();
        }
    }
}