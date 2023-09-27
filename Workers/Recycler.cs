using CoachOnline.Statics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace CoachOnline.Workers
{
    public static class Recycler
    {
        private static List<string> NotWantedExtensions = new List<string> { ".chunkcomplete", ".chunkstart", ".metadata", ".uploadlength" };
        private static Timer CleanerVideos = new Timer();


        public static void RecyclerSetup()
        {
            CleanerVideos.Interval = 60 * 1000 * 10;
            CleanerVideos.Elapsed += CleanerVideos_Elapsed;
            CleanerVideos.Start();

            Console.WriteLine("Recycler runs");
            CleanVideos();
        }

        private static void CleanerVideos_Elapsed(object sender, ElapsedEventArgs e)
        {
            CleanVideos();
            CleanTempFiles();
        }

        public static void CleanTempFiles()
        {
            string Path = $"{ConfigData.Config.EnviromentPath}/wwwroot/tempfiles";
            DateTime DeadLine = DateTime.UtcNow;
            DirectoryInfo info = new DirectoryInfo(Path);
            List<FileInfo> files = info.GetFiles()
                .Where(x => x.CreationTimeUtc.AddDays(1) <= DeadLine)
                .Where(x => x.Name.Contains(".xlsx"))
                .ToList();

            foreach(var f in files)
            {
                if(File.Exists(f.FullName))
                {
                    File.Delete(f.FullName);
                }
            }

        }

        public static void CleanVideos()
        {
            string Path = $"{ConfigData.Config.EnviromentPath}/wwwroot/uploads";
            DateTime DeadLine = DateTime.UtcNow;
            DirectoryInfo info = new DirectoryInfo(Path);
            List<FileInfo> files = info.GetFiles()
                .Where(x => x.CreationTimeUtc.AddMinutes(60) <= DeadLine)
                .Where(x => !x.Name.Contains(".") || NotWantedExtensions.Any(z => x.Name.Contains(z)))
                .OrderBy(x => x.CreationTime)
                .ToList();

            FileInfo[] Doubles = info.GetFiles()
                .Where(x => x.CreationTimeUtc.AddMinutes(240) <= DeadLine)
                .Where(z => z.Name.Contains("_copy_of_converted"))
                .ToArray();

            foreach (var f in Doubles)
            {
                
                if (f != null)
                {
                    files.Add(f);
                }
            }

            if (files.Count > 0)
            {
                Console.WriteLine($"Removing {files.Count} files: ");
            }
            foreach (var f in files)
            {
                try
                {

                    if (File.Exists(f.FullName))
                    {
                        File.Delete(f.FullName);
                        Console.WriteLine($"{f.Name} Removed");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"File {f.Name ?? ""} can't be deleted." +
                        $" Reason: {e.Message} Exception {e.GetType()}");
                }
            }

        }






    }
}
