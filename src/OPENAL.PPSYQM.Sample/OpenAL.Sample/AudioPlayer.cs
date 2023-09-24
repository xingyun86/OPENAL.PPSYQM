using Avalonia.Platform;
using OPENAL.PPSYQM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAL.Sample
{
    class AudioPlayer
    {
        public static async void Play(string resFilePath)
        {
            await Task.Run(() =>
            {
                IntPtr sound_data_ptr = IntPtr.Zero;
                try
                {
                    var alDevice = ALC.OpenDevice(string.Empty);
                    var alContext = ALC.CreateContext(alDevice, new int[] { });
                    ALC.MakeContextCurrent(alContext);
                    AL.GetError();

                    int buffer = AL.GenBuffer();
                    int source = AL.GenSource();
                    int state = 0;
                    bool looping = false;
                    int channels, bits_per_sample, sample_rate;
                    var fsAssist = AssetLoader.Open(new Uri("avares://OpenAl.Sample/Assets/" + resFilePath));
                    byte[] sound_data = LoadWave(fsAssist, out channels, out bits_per_sample, out sample_rate);
                    sound_data_ptr = Marshal.AllocHGlobal(sound_data.Length);

                    Marshal.Copy(sound_data, 0, sound_data_ptr, sound_data.Length);
                    AL.BufferData(buffer, GetSoundFormat(channels, bits_per_sample), sound_data_ptr, sound_data.Length, sample_rate);

                    AL.Source(source, ALSourceb.Looping, looping);
                    AL.Source(source, ALSourcei.Buffer, buffer);
                    AL.SourcePlay(source);

                    Trace.Write("Playing");

                    do
                    {
                        Thread.Sleep(100);
                        Trace.Write(".");
                        AL.GetSource(source, ALGetSourcei.SourceState, out state);
                    }
                    while ((ALSourceState)state == ALSourceState.Playing);
                    Trace.WriteLine("FIN");

                    AL.SourceStop(source);

                    AL.DeleteSource(source);
                    AL.DeleteBuffer(buffer);

                    ALC.DestroyContext(alContext);
                    ALC.CloseDevice(alDevice);
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (sound_data_ptr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(sound_data_ptr);
                    }
                }
            });
        }
        public static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        public static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
    }
}
