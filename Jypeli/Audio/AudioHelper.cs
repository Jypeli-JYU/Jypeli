using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jypeli.Audio;
internal static unsafe class AudioHelper
{
    internal static AudioInfo Read(ReadOnlySpan<byte> data)
    {
        var info = new AudioInfo();

        int index = 0;
        if (data[index++] != 'R' || data[index++] != 'I' || data[index++] != 'F' || data[index++] != 'F')
        {
            ThrowIncorrectFile();
        }

        var chunkSize = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(index, 4));
        index += 4;

        if (data[index++] != 'W' || data[index++] != 'A' || data[index++] != 'V' || data[index++] != 'E')
        {
            ThrowIncorrectFile();
        }

        short numChannels = -1;
        int sampleRate = -1;
        int byteRate = -1;
        short blockAlign = -1;
        short bitsPerSample = -1;

        while (index + 4 < data.Length)
        {
            var identifier = "" + (char)data[index++] + (char)data[index++] + (char)data[index++] + (char)data[index++];
            var size = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(index, 4));
            index += 4;
            if (identifier == "fmt ")
            {
                if (size != 16)
                {
                    ThrowIncorrectFile();
                }
                else
                {
                    var audioFormat = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(index, 2));
                    index += 2;
                    if (audioFormat != 1)
                    {
                        ThrowIncorrectFile();
                    }
                    else
                    {
                        numChannels = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(index, 2));
                        index += 2;
                        sampleRate = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(index, 4));
                        index += 4;
                        byteRate = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(index, 4));
                        index += 4;
                        blockAlign = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(index, 2));
                        index += 2;
                        bitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(index, 2));
                        index += 2;

                        if(!(bitsPerSample == 8 || bitsPerSample == 16) || !(numChannels == 1 || numChannels == 2))
                            ThrowIncorrectFile();
                    }
                }
            }
            else if (identifier == "data")
            {
                var dataArea = data.Slice(index, size);
                index += size;

                info.data = dataArea;

                Debug.WriteLine($"Read {size} bytes Data");
            }
            else if (identifier == "JUNK")
            {
                // this exists to align things
                index += size;
            }
            else if (identifier == "iXML")
            {
                var v = data.Slice(index, size);
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

        info.numChannels = numChannels;
        info.sampleRate = sampleRate;
        info.byteRate = byteRate;
        info.blockAlign = blockAlign;
        info.bitsPerSample = bitsPerSample;

        return info;
    }
    private static void ThrowIncorrectFile()
    {
        throw new FormatException("Annettu äänitiedosto on virheellisessä formaatissa. Toistaiseksi vain 8- ja 16-bittiset mono ja stereo wav-muotoiset tiedostot sallitaan.");
    }
}

internal ref struct AudioInfo
{
    public short numChannels = -1;
    public int sampleRate = -1;
    public int byteRate = -1;
    public short blockAlign = -1;
    public short bitsPerSample = -1;
    public ReadOnlySpan<byte> data = null;

    public AudioInfo()
    {
    }

    public override string ToString()
    {
        return $"Channels: {numChannels}, SampleRate: {sampleRate}, ByteRate: {byteRate}, BlockAlign: {blockAlign}, BitsPerSample: {bitsPerSample}, DataLen: {data.Length}";
    }
}
