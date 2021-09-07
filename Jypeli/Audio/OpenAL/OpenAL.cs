#region Licence
/*
MIT License

- Copyright (c) 2019-2020 Ultz Limited
- Copyright (c) 2021- .NET Foundation and Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Original code from: https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenAL%20Demos/WavePlayer/Program.cs
*/

#endregion

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.OpenAL;

namespace Jypeli.Audio.OpenAL // Laitetaan omaan nimiavaruuteen siltä varalta jos tulevaisuudessa tulee jokin muukin alusta äänen toistoa varten.
{
    public static unsafe class OpenAL
    {
        private static AL al;
        private static uint source;

        internal static void Init()
        {
            var alc = ALContext.GetApi(true);
            al = AL.GetApi(true);
            var device = alc.OpenDevice("");
            if (device == null)
            {
                throw new AudioDeviceException("Unable to initialize OpenAL device.");
            }

            var context = alc.CreateContext(device, null);
            alc.MakeContextCurrent(context);

            al.GetError();

        }

        public static uint LoadSound(string filename)
        {
            ReadOnlySpan<byte> file = File.ReadAllBytes(filename);
            int index = 0;
            if (file[index++] != 'R' || file[index++] != 'I' || file[index++] != 'F' || file[index++] != 'F')
            {
                Debug.WriteLine("Given file is not in RIFF format"); // TODO: Haluttaisiinko tämmöisissä tilanteissa heittää poikkeus?
                return 0;
            }

            var chunkSize = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
            index += 4;

            if (file[index++] != 'W' || file[index++] != 'A' || file[index++] != 'V' || file[index++] != 'E')
            {
                Debug.WriteLine("Given file is not in WAVE format");
                return 0;
            }

            short numChannels = -1;
            int sampleRate = -1;
            int byteRate = -1;
            short blockAlign = -1;
            short bitsPerSample = -1;
            BufferFormat format = 0;

            source = al.GenSource();
            var buffer = al.GenBuffer();
            al.SetSourceProperty(source, SourceBoolean.Looping, false);
            al.SetSourceProperty(source, SourceFloat.Gain, 0.2f);

            while (index + 4 < file.Length)
            {
                var identifier = "" + (char)file[index++] + (char)file[index++] + (char)file[index++] + (char)file[index++];
                var size = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
                index += 4;
                if (identifier == "fmt ")
                {
                    if (size != 16)
                    {
                        Debug.WriteLine($"Unknown Audio Format with subchunk1 size {size}");
                    }
                    else
                    {
                        var audioFormat = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                        index += 2;
                        if (audioFormat != 1)
                        {
                            Debug.WriteLine($"Unknown Audio Format with ID {audioFormat}");
                        }
                        else
                        {
                            numChannels = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                            index += 2;
                            sampleRate = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
                            index += 4;
                            byteRate = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
                            index += 4;
                            blockAlign = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                            index += 2;
                            bitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                            index += 2;

                            if (numChannels == 1)
                            {
                                if (bitsPerSample == 8)
                                    format = BufferFormat.Mono8;
                                else if (bitsPerSample == 16)
                                    format = BufferFormat.Mono16;
                                else
                                {
                                    Debug.WriteLine($"Can't Play mono {bitsPerSample} sound.");
                                }
                            }
                            else if (numChannels == 2)
                            {
                                if (bitsPerSample == 8)
                                    format = BufferFormat.Stereo8;
                                else if (bitsPerSample == 16)
                                    format = BufferFormat.Stereo16;
                                else
                                {
                                    Debug.WriteLine($"Can't Play stereo {bitsPerSample} sound.");
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"Can't play audio with {numChannels} sound");
                            }
                        }
                    }
                }
                else if (identifier == "data")
                {
                    var data = file.Slice(44, size);
                    index += size;

                    fixed (byte* pData = data)
                        al.BufferData(buffer, format, pData, size, sampleRate);
                    Debug.WriteLine($"Read {size} bytes Data");
                }
                else if (identifier == "JUNK")
                {
                    // this exists to align things
                    index += size;
                }
                else if (identifier == "iXML")
                {
                    var v = file.Slice(index, size);
                    var str = Encoding.ASCII.GetString(v);
                    Debug.WriteLine($"iXML Chunk: {str}");
                    index += size;
                }
                else
                {
                    Debug.WriteLine($"Unknown Section: {identifier}");
                    index += size;
                }
            }

            Debug.WriteLine
            (
                $"Success. Detected RIFF-WAVE audio file, PCM encoding. {numChannels} Channels, {sampleRate} Sample Rate, {byteRate} Byte Rate, {blockAlign} Block Align, {bitsPerSample} Bits per Sample"
            );

            al.SetSourceProperty(source, SourceInteger.Buffer, buffer);

            return source;
        }

        public static void Play(uint source)
        {
            al.SourcePlay(source);
        }

        public static void Stop(uint source)
        {
            al.SourceStop(source);
        }
    }
}
