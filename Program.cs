using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoachOnline.Implementation;
using CoachOnline.Model;
using CoachOnline.Mongo;
using CoachOnline.Statics;
using CoachOnline.Workers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rollbar;
using Sentry;
using Serilog;

namespace CoachOnline
{
    public class Program
    {
       // public static bool UseSQLite = !true;


        public static void Main(string[] args)
        {
            Statics.ConfigData.UpdateConfig();
#if DEBUG
            //Statics.ConfigData.Config.EnviromentPath = "./";
#endif
            //ConverterService.ConvertFFMpegSharp("./wwwroot/uploads/0586f40729614a9b88a69b88e37759f4.m4v", "./wwwroot/uploads/udalosie.mp4").Wait();

            //RemoveTestAccounts();
            Recycler.RecyclerSetup();
            // new AdminImplementation().RemoveDrafts().Wait();
            ConverterService.SetTimer(5000);
   
                new DataContext().MigrateDb();
                Console.WriteLine($"{DateTime.Now} Using PostgreSQL");
           
            using (var cnx = new DataContext())
            {
                cnx.MigrateDb();
                var admin = cnx.Admins.Where(x => x.Email == "admin@coachs.com").FirstOrDefault();
                var admin2 = cnx.Admins.Where(x => x.Email == "admin2@coachs.com").FirstOrDefault();
                var admin3 = cnx.Admins.Where(x => x.Email == "admin3@coachs.com").FirstOrDefault();
                var admin4 = cnx.Admins.Where(x => x.Email == "penelope@lafabriquedufutur.global").FirstOrDefault();
                var admin5 = cnx.Admins.Where(x => x.Email == "olivier@cyberscaling.com").FirstOrDefault();
                if (admin == null)
                {
                    cnx.Admins.Add(new Model.Admin
                    {
                        Id = 1,
                        Email = "admin@coachs.com",
                        Password = LetsHash.ToSHA512("Ziomal11!")
                    });
                }
                if (admin2 == null)
                {
                    cnx.Admins.Add(new Model.Admin
                    {
                        Id = 2,
                        Email = "admin2@coachs.com",
                        Password = LetsHash.ToSHA512("D8@:zm>F")
                    });
                }
                if (admin3 == null)
                {
                    cnx.Admins.Add(new Model.Admin
                    {
                        Id = 3,
                        Email = "admin3@coachs.com",
                        Password = LetsHash.ToSHA512("_Utp>3Ak")
                    });
                }
                if (admin4 == null)
                {
                    cnx.Admins.Add(new Model.Admin
                    {
                        Id = 4,
                        Email = "penelope@lafabriquedufutur.global",
                        Password = LetsHash.ToSHA512("83A5eb7d6a!")
                    });
                }
                else if (admin4 != null && admin4.Password == LetsHash.ToSHA512("83a5eb7d6a"))
                {
                    admin4.Password = LetsHash.ToSHA512("83A5eb7d6a!");
                }
                if (admin5 == null)
                {
                    cnx.Admins.Add(new Model.Admin
                    {
                        Id = 5,
                        Email = "olivier@cyberscaling.com",
                        Password = LetsHash.ToSHA512("81Bef7f7e1!")
                    });
                }
                else if(admin5 != null && admin5.Password == LetsHash.ToSHA512("81bef7f7e1"))
                {
                    admin5.Password = LetsHash.ToSHA512("81Bef7f7e1!");
                }



                if (!cnx.Professions.Any())
                {
                    cnx.Professions.Add(new Profession { Name = "Agriculteurs exploitants" });
                    cnx.Professions.Add(new Profession { Name = "Artisans, commer�ants, chefs d'entreprise" });
                    cnx.Professions.Add(new Profession { Name = "Cadres et professions intellectuelles sup�rieures" });
                    cnx.Professions.Add(new Profession { Name = "Employ�s non qualifi�s" });
                    cnx.Professions.Add(new Profession { Name = "Employ�s qualifi�s" });
                    cnx.Professions.Add(new Profession { Name = "Non d�termin�" });
                    cnx.Professions.Add(new Profession { Name = "Ouvriers non qualifi�s" });
                    cnx.Professions.Add(new Profession { Name = "Ouvriers qualifi�s" });
                    cnx.Professions.Add(new Profession { Name = "Professions interm�diaires" });
                }

                var users = cnx.users.Where(x => x.SocialLogin.HasValue && x.SocialLogin.Value && x.SocialProvider == "accounts.google.com").ToList();
                //var usersNotConfirmed = cnx.users.Where(x => x.Status == UserAccountStatus.AWAITING_EMAIL_CONFIRMATION).ToList();
                foreach(var usr in users)
                {
                    usr.SocialProvider = "GOOGLE";
                }

                cnx.SaveChanges();

                var userNicks = cnx.users.Where(x => x.Status != UserAccountStatus.DELETED).ToList();
                var repeats = new List<string>();
                foreach(var u in userNicks)
                {
                    if (u.Nick != null)
                    {
                        repeats.Add(u.Nick);
                    }
                    else
                    {
                        var emailFirstPart = u.EmailAddress.Split('@').FirstOrDefault();

                        if(!repeats.Any(t=>t == emailFirstPart))
                        {
                            u.Nick = emailFirstPart;
                            repeats.Add(emailFirstPart);
                        }
                        else
                        {   string temp = emailFirstPart;
                            int i = 1;
                            while(repeats.Any(t => t == temp))
                            {
                                temp += i.ToString();
                                i++;
                            }
                            u.Nick = temp;
                        }
                    }

                }

                cnx.SaveChanges();


                var eps =  cnx.Episodes.Where(t=>t.EpisodeState == EpisodeState.BEFORE_UPLOAD).ToList();

                foreach(var ep in eps)
                {
                    if(string.IsNullOrEmpty(ep.MediaId))
                    {
                        ep.EpisodeState = EpisodeState.BEFORE_UPLOAD;             
                    }
                    else
                    {
                        if (File.Exists($"{ConfigData.Config.EnviromentPath}wwwroot/uploads/{ep.MediaId}"))
                        {
                            Console.WriteLine($"Ok. File from episode {ep.Id} exists.");
                            ep.EpisodeState = EpisodeState.UPLOADED;

                            if (ep.MediaId.ToLower().EndsWith("_c.mp4"))
                            {
                                ep.EpisodeState = EpisodeState.CONVERTED;
                                Console.WriteLine($"Ok. File from episode {ep.Id} is converted.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"File from episode {ep.Id} does not exists");
                        }
                    }
                }

                cnx.SaveChanges();

                var courses = cnx.courses.Where(t => t.State == CourseState.APPROVED && !t.PublishedCount.HasValue).ToList();

                foreach(var c in courses)
                {
                    c.PublishedCount = 1;
                }

             
                cnx.SaveChanges();
                    
            }
            new TimeBot(10000);
            CreateFolders();
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {

#if DEBUG
#else
                Console.WriteLine("Not debugging");
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("STRIPE_API_KEY")))
                {
                    Statics.ConfigData.Config.StripeRk = Environment.GetEnvironmentVariable("STRIPE_API_KEY");
                }
                else
                {
                    Console.WriteLine("Using stripe api from config file.");
                }

                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("StripeWebhookKeyAccount")))
                {
                    Statics.ConfigData.Config.StripeWebhookKeyAccount = Environment.GetEnvironmentVariable("StripeWebhookKeyAccount");
                }
                else
                {
                    Console.WriteLine("Using stripe api from config file.");
                }

                ////PAYPAL
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PayPalClientID")))
                {
                    Statics.ConfigData.Config.PayPalClientID = Environment.GetEnvironmentVariable("PayPalClientID");
                }
                else
                {
                    Console.WriteLine("Using paypal client ID from config file.");
                }

                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PayPalSecret")))
                {
                    Statics.ConfigData.Config.PayPalSecret = Environment.GetEnvironmentVariable("PayPalSecret");
                }
                else
                {
                    Console.WriteLine("Using paypal secret from config file.");
                }

                ////GOOGLE
                ///
                 if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GoogleSecret")))
                {
                    Statics.ConfigData.Config.GoogleSecret = Environment.GetEnvironmentVariable("GoogleSecret");
                }
                else
                {
                    Console.WriteLine("Using google secret from config file.");
                }

                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GoogleClientId")))
                {
                    Statics.ConfigData.Config.GoogleClientId = Environment.GetEnvironmentVariable("GoogleClientId");
                }
                else
                {
                    Console.WriteLine("Using google client ID from config file.");
                }


                ///stripe
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("STRIPE_ACCOUNT_WEBHOOK")))
                {
                    Statics.ConfigData.Config.StripeWebhookKey = Environment.GetEnvironmentVariable("STRIPE_ACCOUNT_WEBHOOK");
                }
                else
                {
                    Console.WriteLine("Using stripe webhook from config file.");
                }
                if (string.IsNullOrEmpty(Statics.ConfigData.Config.StripeRk))
                {
                    Console.WriteLine("Stripe key not set");
                }
                if (string.IsNullOrEmpty(Statics.ConfigData.Config.StripeWebhookKey))
                {
                    Console.WriteLine("Stripe webhook key not set");
                }
#endif
            }
            Console.WriteLine("Config created");
            using (var cnx = new DataContext())
            {
                var terms = cnx.Terms.Select(x => x).ToList();
                if (terms == null || terms.Count == 0)
                {
                    cnx.Terms.Add(new Model.Terms { Created = DateTime.Now, Url = "local" });
                }
                //cnx.CreateDb();
                //var user = cnx.users.Where(x => x.EmailAddress == "kamilkostrzewski@itsharkz.com").FirstOrDefault();
                //if (user == null)
                //{
                //    cnx.users.Add(new Model.User
                //    {
                //        EmailAddress = "kamilkostrzewski@itsharkz.com",
                //        Password = Statics.LetsHash.ToSHA512("Ziomal11!")
                //    });
                //    cnx.SaveChanges();
                //}
            }
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig("f2aeed49548e485c85cd77e5e29d9d7f"));
            RollbarLocator.RollbarInstance.Info("Rollbar is configured properly.");

            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               //.Enrich.FromLogContext()
               //.Enrich.WithCaller()
               .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] ({SourceContext} => {Method}): {Message:lj}{NewLine}{Exception}")
               .WriteTo.File("auth.log", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] ({SourceContext} => {Method}): {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
               .Enrich.FromLogContext()
               .CreateLogger();
            new DataContext().MigrateDb();
            Console.WriteLine("Migrated");
            Console.WriteLine("Now running");
            var host = CreateHostBuilder(args).Build();
            CloseAllActiveConnections(host);

            host.Run();
        }


        private static void CloseAllActiveConnections(IHost host)
        {
            try
            {
                var mongoSvc = host.Services.GetService(typeof(MongoCtx)) as MongoCtx;

                if (mongoSvc != null)
                {
                    var conns = mongoSvc.InstitureUsersCollection.FindAllConnected();
                    conns.Wait();
                    var allActive = conns.Result;

                    foreach (var c in allActive)
                    {
                        c.ConnectionEndTime = DateTime.Now;
                        var up = mongoSvc.InstitureUsersCollection.UpdateAsync(c);
                        up.Wait();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static void CreateFolders()
        {
            var folders = new[] { $"{ConfigData.Config.EnviromentPath}/wwwroot", $"{ConfigData.Config.EnviromentPath}/wwwroot/attachments", $"{ConfigData.Config.EnviromentPath}/wwwroot/uploads", $"{ConfigData.Config.EnviromentPath}/wwwroot/images", $"{ConfigData.Config.EnviromentPath}/wwwroot/student_cards", $"{ConfigData.Config.EnviromentPath}/wwwroot/regulations", $"{ConfigData.Config.EnviromentPath}/wwwroot/tempfiles", $"{ConfigData.Config.EnviromentPath}/wwwroot/documents" };

            foreach (var f in folders)
            {
                if (!Directory.Exists(f))
                {
                    Directory.CreateDirectory(f);
                }
            }

            Console.WriteLine("Folders created");

        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    webBuilder.UseSentry(x =>
                    {
                        x.Dsn = "https://059122e555bb45ceb42225e9ba3097af@o722164.ingest.sentry.io/5782399";
                        x.DiagnosticLevel = SentryLevel.Error;
                        //x.SendDefaultPii = true;
                        //x.MaxRequestBodySize = Sentry.Extensibility.RequestSize.Always;
                    });
                    //webBuilder.UseSentry("https://059122e555bb45ceb42225e9ba3097af@o722164.ingest.sentry.io/5782399");

                    webBuilder.UseUrls("http://0.0.0.0:5050");
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Limits.MaxRequestBodySize = 1610612736;
                        serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(15); 
                    });
                    //webBuilder.UseKestrel(
                    //    x => x.Limits.MaxRequestBodySize = 1610612736
                    //    );
                    Console.WriteLine("app started");

                });



    }
}

