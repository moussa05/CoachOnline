using CoachOnline.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace CoachOnline.Workers
{
    public class TimeBot
    {

        private Timer bot;
        public TimeBot(int Period)
        {

            GetvideosTimes().Wait();
            bot = new Timer();
            bot.Interval = Period;
            bot.Elapsed += Bot_Elapsed;
            bot.Start();

        }

        private static bool Locked = false;
        private void Bot_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Locked)
            {

                try
                {

                    GetvideosTimes().Wait();
                }
                catch (Exception er)
                {
                    Console.WriteLine("Bot time problem");
                }
            }
        }

        private async Task GetvideosTimes()
        {
            Locked = true;
            using (var cnx = new DataContext())
            {
                var z = cnx.Episodes.Where(z => !string.IsNullOrEmpty(z.MediaId) && z.MediaLenght <= 1).ToList();
                foreach (var p in z)
                {
                    try
                    {

                        Task<double> task = ConverterService.GetVideoLenght(p.MediaId);
                        task.Wait();
                        p.MediaLenght = task.Result;
                    }
                    catch (Exception e)
                    {
                        Locked = false;
                    }
                }
                cnx.SaveChanges();
            }
            Locked = false;
        }
    }
}
