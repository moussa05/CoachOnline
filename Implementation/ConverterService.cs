using CoachOnline.Statics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Xabe.FFmpeg;


namespace CoachOnline.Implementation
{

    public class ConverterService
    {




        public static List<int> EpisodesToSkip = new List<int>();
        private static List<int> CurrentConverting = new List<int>();
        private static bool Locked { get { return CurrentConverting.Count >= 3; } }
        private static Timer timer = new Timer();


        public static async Task<double> GetVideoLenght(string fileName)
        {
            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo($"{ConfigData.Config.EnviromentPath}wwwroot/uploads/{fileName}");
            return mediaInfo.Duration.TotalSeconds;
        }
        public class Arg : FFMpegCore.Arguments.IArgument
        {
            public string Text => "-map 0:v";
        }

        public static async Task<bool> ConvertFFMpegSharp(string input, string output)
        {
           // FFmpeg.SetExecutablesPath("/snap/bin/ffmpeg");
            var filepath = $"{input}";
            var video = await FFMpegCore.FFProbe.AnalyseAsync(filepath);
            string outputFileName = $"{output}";

            if (video.PrimaryVideoStream.Rotation != 0)
            {
                video.PrimaryVideoStream.Rotation = 0;
            }

            await FFMpegCore.FFMpegArguments
            .FromFileInput(filepath)
            .OutputToFile(outputFileName, true, options => options
                .OverwriteExisting()
                .WithVideoCodec(FFMpegCore.Enums.VideoCodec.LibX264)
                .OverwriteExisting()
                //.WithVideoFilters(x => x.Arguments = )
                //.WithConstantRateFactor(21)
                .WithAudioCodec(FFMpegCore.Enums.AudioCodec.Aac)
                //.WithVariableBitrate(4)
                .WithVideoFilters(filterOptions => filterOptions
                    .Scale(FFMpegCore.Enums.VideoSize.Original))
                .WithFastStart()
                )
            .ProcessAsynchronously();

            //var video = VideoInfo.FromPath(filepath);


            return true;
        }

        public static bool RunCommandLine(string input, string output)
        {

            Console.WriteLine("Starting from command line.");
            var proc = new Process();
            
            proc.StartInfo.FileName = "ffmpeg";
            proc.StartInfo.Arguments = $"-y -i {input} {output}";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            string outPut = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            var exitCode = proc.ExitCode;
            return exitCode == 0;



        }

        public static void UpdateMediaLength(int videoId, double length)
        {
            try
            {
                if (videoId != 0)
                {
                    using (var cnx = new DataContext())
                    {
                        var vid =  cnx.Episodes.FirstOrDefault(z => z.Id == videoId);
                        if (vid != null)
                        {
                            vid.MediaLenght = length;
                            cnx.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static async Task<bool> Run(string fileName, int videoId = 0)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Console.WriteLine("Start converting " + fileName);
            var filepath = $"{ConfigData.Config.EnviromentPath}wwwroot/uploads/{fileName}";


            if (!File.Exists(filepath))
            {
                Console.WriteLine("File does not exist " + filepath);
                return false;
            }


            var file = new FileInfo(filepath);
            string outputFileName = $"{ConfigData.Config.EnviromentPath}wwwroot/uploads/{fileName.Replace($"{file.Extension}", "")}_c.mp4";



            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(filepath);

            if (mediaInfo != null)
            {
                UpdateMediaLength(videoId, mediaInfo.Duration.TotalSeconds);
            }

            var videoStream = mediaInfo.VideoStreams
                .FirstOrDefault();
            videoStream
                ?.SetCodec(VideoCodec.h264)
                ?.SetSize(VideoSize.Hd720);
            bool hasRotation = false;
            if (videoStream != null && videoStream.Rotation.HasValue && videoStream.Rotation == 90)
            {
                videoStream?.Rotate(RotateDegrees.Clockwise);
                hasRotation = true;
            }


            try
            {

                videoStream.SetWatermark($"{ConfigData.Config.EnviromentPath}wwwroot/uploads/coach.png", Position.BottomRight);
                IStream audioStream = mediaInfo?.AudioStreams?.FirstOrDefault()?.SetCodec(AudioCodec.aac);
                var conversion = FFmpeg.Conversions.New();
                conversion.AddStream(videoStream, audioStream)
                    .SetOverwriteOutput(true)
                .SetOutput(outputFileName);
                conversion.UseMultiThread(true);
                conversion.OnProgress += Conversion_OnProgress;
                await conversion.Start();
            }
            catch (Exception e)
            {
                try
                {
                    if (!RunCommandLine(filepath, outputFileName))
                        return false;
                }
                catch (Exception er)
                {
                    Console.WriteLine("Second conversion try failed." + er.Message);
                    return false;
                }
            }




            if (hasRotation)
            {
                IMediaInfo mediaInfoAfter = await FFmpeg.GetMediaInfo(outputFileName);
                var videoStreamAfter = mediaInfoAfter?.VideoStreams?
                    .FirstOrDefault();
                var audioStreamAfter = mediaInfoAfter?.AudioStreams?.FirstOrDefault();

                var conversionAfter = FFmpeg.Conversions.New();

                videoStreamAfter.SetWatermark($"{ConfigData.Config.EnviromentPath}wwwroot/uploads/coach.png", Position.BottomRight);
                string outputFileNameAfter = $"{ConfigData.Config.EnviromentPath}wwwroot/uploads/{fileName.Replace($"{file.Extension}", "")}_c2.mp4";


                conversionAfter.AddStream(videoStreamAfter).AddStream(audioStreamAfter)
                .SetOutput(outputFileNameAfter);

                await conversionAfter.Start();

                if (File.Exists(outputFileNameAfter) && File.Exists(outputFileName))
                {
                    File.Delete(outputFileName);
                    File.Copy(outputFileNameAfter, outputFileName);
                    File.Delete(outputFileNameAfter);
                }
            }



            //   AddParameter(hasRotation? $"-i {ConfigData.Config.EnviromentPath}wwwroot/uploads/coach.png -filter_complex \"[1:v]transpose = 1[transposed]; [transposed][0:v]overlay = 10:10[out]\" \\ -map \"[out]\" -map 1:a" 
            //  : "-i {ConfigData.Config.EnviromentPath}wwwroot/uploads/coach.png -filter_complex \"[0:v]overlay = 10:10[out]\" \\ -map \"[out]\" -map 1:a", ParameterPosition.PreInput)


            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            var elapsedS = watch.Elapsed.TotalSeconds;
            file.MoveTo(file.FullName + "_copy_of_converted");
            Console.WriteLine($"Finished converion file {file.FullName} ({Convert.ToDouble(file.Length) / 1024 / 1024} mb) Converting time {elapsedS} in ms {elapsedMs}");

            return true;

        }



        public static void SetTimer(int miliseconds)
        {
            Console.Write("Converter started");
            timer.Interval = miliseconds;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            if (!Locked)
            {
                try
                {
                    ConvertMachine().Wait();
                }
                catch (Exception er)
                {
                    Console.WriteLine("Can't convert" + er);
                }
            }
        }

        public static async Task ConvertMachine()
        {
            if (!Locked)
            {

                List<string> files = new List<string>();

                using (var cnx = new DataContext())
                {

                    var filesList = await cnx.Episodes.Where(z => !z.MediaId.Contains("_c") && !string.IsNullOrEmpty(z.MediaId) && !EpisodesToSkip.Any(p => p == z.Id) && !CurrentConverting.Any(p => p == z.Id) /*&& z.MediaId.Contains("8a0d103738d54e55acc478f776bfdb17.mp4")*/).OrderByDescending(x => x.Created).ToListAsync();
                    var toConvert = filesList.FirstOrDefault();
                    if (toConvert != null)
                    {
                        Console.WriteLine("Converting title: " + toConvert.Title);
                        FileInfo info = new FileInfo(toConvert.MediaId);
                        string toConvertNewName = toConvert.MediaId.Replace($"{info.Extension}", "") + "_c.mp4";
                        try
                        {
                            CurrentConverting.Add(toConvert.Id);
                            var result = await Run(toConvert.MediaId, toConvert.Id);
                            if (!result)
                            {
                                toConvert.EpisodeState = Model.EpisodeState.ERROR_WITH_CONVERSION;
                                await cnx.SaveChangesAsync();
                                return;
                            }
                            toConvert.MediaId = toConvertNewName;
                            toConvert.MediaNeedsConverting = false;
                            toConvert.EpisodeState = Model.EpisodeState.CONVERTED;

                            Console.WriteLine("Conversion done " + toConvert.MediaId);
                            if (CurrentConverting.Any(z => z == toConvert.Id))
                            {

                                CurrentConverting.Remove(toConvert.Id);
                            }
                            //Locked = false; depraced
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            if (CurrentConverting.Any(z => z == toConvert.Id))
                            {

                                CurrentConverting.Remove(toConvert.Id);
                            }
                            if (File.Exists($"{ConfigData.Config.EnviromentPath}wwwroot/uploads/{toConvertNewName}"))
                            {
                                File.Delete($"{ConfigData.Config.EnviromentPath}wwwroot/uploads/{toConvertNewName}");
                                Console.WriteLine("Deleted corrupted file");
                            }
                            else
                            {

                                EpisodesToSkip.Add(toConvert.Id);
                            }

                            //Locked = false; depraced
                        }
                        finally
                        {
                            cnx.SaveChanges();
                        }

                    }


                }



            }

        }




        private async static void Conversion_OnProgress(object sender, Xabe.FFmpeg.Events.ConversionProgressEventArgs args)
        {


            if (args.Percent % 10 == 0)
            {
                Console.Write($"Converting {CurrentConverting.Count} Videos, Queue Status:" + (Locked ? " locked" : " unlocked"));
                await Console.Out.WriteLineAsync($"[{args.Duration}/{args.TotalLength}][{args.Percent}%] ");
            }

        }
    }

}
