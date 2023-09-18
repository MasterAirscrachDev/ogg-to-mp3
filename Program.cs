using NAudio.Wave;
using System;
using System.IO;
using NAudio.Lame;
using NVorbis;

namespace oggtomp3
{
    class Program
    {
        static void Main(string[] args)
        {
            bool deleteOgg = false;
            //check for a file in this directory called deleteOnConvert.cfg
            if (File.Exists("deleteOnConvert.cfg"))
            {
                //if the file exists, check if it contains the word "true"
                string deleteOnConvert = File.ReadAllText("deleteOnConvert.cfg");
                if (deleteOnConvert.Contains("true"))
                {
                    //if it does, set deleteOgg to true
                    deleteOgg = true;
                }
            }
            else{
                //if the file doesn't exist, create it and write "false" to it
                File.WriteAllText("deleteOnConvert.cfg", "false");
            }
            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    if (IsOggFile(arg))
                    {
                        ConvertOggToMp3(arg);
                        if (deleteOgg)
                        {
                            File.Delete(arg);
                        }
                    }
                }
            }
            else
            {
                // Your regular application code here
                Console.WriteLine(":(");
            }
        }

        static bool IsOggFile(string filePath)
        {
            return Path.GetExtension(filePath).Equals(".ogg", StringComparison.OrdinalIgnoreCase);
        }

        static void ConvertOggToMp3(string oggFilePath)
        {
            // Get the directory of the input OGG file
            string directory = Path.GetDirectoryName(oggFilePath);

            // Create the output MP3 file path by replacing the file extension
            string mp3FilePath = Path.Combine(directory, Path.GetFileNameWithoutExtension(oggFilePath) + ".mp3");

            // Load the OGG file using NVorbis
            using (var oggReader = new VorbisReader(oggFilePath))
            {
                
                // Create an MP3 file writer
                using (var mp3Writer = new LameMP3FileWriter(mp3FilePath, WaveFormat.CreateIeeeFloatWaveFormat(oggReader.SampleRate, oggReader.Channels), LAMEPreset.STANDARD))
                {
                    // Read and convert OGG data to MP3
                    var buffer = new float[oggReader.SampleRate * oggReader.Channels];
                    int bytesRead;
                    while ((bytesRead = oggReader.ReadSamples(buffer, 0, buffer.Length)) > 0)
                    {
                        var bytes = new byte[bytesRead * sizeof(float)];
                        Buffer.BlockCopy(buffer, 0, bytes, 0, bytes.Length);
                        mp3Writer.Write(bytes, 0, bytes.Length);
                    }
                }
            }
        }
    }
}
