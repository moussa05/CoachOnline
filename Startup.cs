using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.S3;
using CoachOnline.ElasticSearch.ESConfig;
using CoachOnline.ElasticSearch.Services;
using CoachOnline.Helpers;
using CoachOnline.Hubs;
using CoachOnline.Hubs.Model;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Mongo;
using CoachOnline.PayPalIntegration;
using CoachOnline.Services;
using CoachOnline.Statics;
using CoachOnline.Workers;
//using ITSAuth.Implementation.Email;

using ITSAuth.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;
using Microsoft.Owin.Cors;
using MongoDB.Driver;
using tusdotnet;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;

namespace CoachOnline
{
    public class Startup
    {
        public static string SendgridKey = "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";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {

                    builder
                      .SetIsOriginAllowed((host) => true)
                      .AllowCredentials().AllowAnyHeader().AllowAnyMethod()
                    .WithExposedHeaders("Location", "Upload-Offset", "Upload-Length");
                });
                options.DefaultPolicyName = "CorsPolicy";
            });


            services.AddControllers();
            services.AddControllers().AddNewtonsoftJson();

            //services.AddAuthentication("BasicAuthentication")
            // .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuthentication", null);



            services.AddAuthentication(opts =>
            {

                opts.AddScheme<BasicAuthHandler>("BasicAuthentication", "");
                opts.DefaultScheme = "BasicAuthentication";
                opts.DefaultSignInScheme = "External";

            })
            .AddCookie("External",options =>
            {
                options.LoginPath = "/api/authentication/google/challange"; // Must be lowercase
                                                                                  //options.ExpireTimeSpan = new TimeSpan(0, 1, 0);
                options.EventsType = typeof(RevokeAuthenticationEvents);

            })

             .AddOpenIdConnect(GoogleDefaults.AuthenticationScheme,
               GoogleDefaults.DisplayName,
               options =>
               {
                   options.SignInScheme = "External";
                   options.Authority = "https://accounts.google.com";
                   options.ClientId = ConfigData.Config.GoogleClientId;
                   options.ClientSecret = ConfigData.Config.GoogleSecret;
                   options.ResponseType = OpenIdConnectResponseType.IdToken;
                   options.CallbackPath = $"/signin-google";
          

                   options.SaveTokens = true; //this has to be true to get the token value
                   options.Scope.Add("email");
                   options.Scope.Add("profile");
                   options.Scope.Add("openid");
                   

  
               
                });
            //.AddIdentityCookies();

            services.AddDataProtection();



            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Coach Online API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IUser, UserService>();
            services.AddSingleton<IUserInfo, UserInfoService>();
            services.AddSingleton<ISearch, SearchSvc>();
            services.AddElasticSearch();
            services.AddSingleton<IBalance, BalanceService>();
            services.AddSingleton<ICounter, WatchTimeCounterService>();
            services.AddSingleton<ICoach, CoachService>();
            services.AddSingleton<IAuthAsync, DataImplementation>();
            services.AddSingleton<ICoachService, DataImplementation>();
            services.AddSingleton<IEmailApiService>(new SendgridImplementation(SendgridKey));
            services.AddSingleton<IAdmin, AdminImplementation>();
            services.AddSingleton<IPaymentService, PaymentService>();
            services.AddSingleton<IPayPal, PayPalService>();
            services.AddSingleton<IPlayerMedia, PlayerService>();
            services.AddSingleton<IWebhook, WebhookImpl>();
            services.AddSingleton<ISubscription, SubscriptionService>();
            services.AddSingleton<IStream, StreamingService>();
            services.AddSingleton<IEvent, EventService>();
            services.AddSingleton<IAffiliate, AffiliateService>();
            services.AddSingleton<IB2BManagement, B2BManagementService>();
            services.AddSingleton<ILibraryManagement, LibraryManagementService>();
            services.AddSingleton<IInstitution, InstitutionService>();
            services.AddSingleton<IProductManage, ProductManageService>();
            services.AddSingleton<IMongoClient>(new MongoClient(ConfigData.Config.MongoDBServer));
            services.AddSingleton<MongoCtx>();
            services.AddSingleton<IHubUserInfoInMemory, HubUsersInMemory>();
            services.AddSingleton<IFAQ, FAQService>();
            services.AddSingleton<IContract, ContractsService>();
            services.AddSingleton<IComment, CommentsService>();
            services.AddSingleton<IQuestionnaire, QuestionnaireService>();
            services.AddSingleton<IRequestedPayments, RequestedPaymentsService>();
            services.AddSignalR();
            services.AddScoped<RevokeAuthenticationEvents>();
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();
            services.AddHostedService<VideoStatusChecker>();
            services.AddHostedService<BalanceWorkerSvc>();
            services.AddHostedService<ElasticReindexWorker>();
            services.AddHostedService<AffiliatesWorker>();
            services.AddHostedService<InstitutionWorker>();
            services.AddHostedService<UserServicesWorker>();

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            

            app.UseCors("CorsPolicy");
            app.UseCorsMiddleware();


            ////manual redirection to https
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
                
              

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCookiePolicy(new CookiePolicyOptions()
            {
                MinimumSameSitePolicy = SameSiteMode.None,
                //Secure = CookieSecurePolicy.None,
                
                
            });


            app.UseTus(httpContext =>
            {

                //check tokens etc here
                return new DefaultTusConfiguration()
                {
                    Store = new TusDiskStore($"{ConfigData.Config.EnviromentPath}/wwwroot/uploads"),
                    UrlPath = "/video",


                    Events = new Events
                    {
                        //OnAuthorizeAsync = async auth =>
                        //{


                        //    var authSvc = app.ApplicationServices.GetService(typeof(BasicAuthHandler)) as BasicAuthHandler;
                        //    if (authSvc != null)
                        //    {
                        //        await authSvc.AuthenticateAsync();

                        //    }
                        //},
                        OnBeforeCreateAsync = async ec =>
                        {

                            string key = "";
                            int userId = 0;
                            int courseId = 0;
                            int lessonId = 0;
                            Console.WriteLine("On before async started");
                            if (httpContext.Request.Headers.ContainsKey("secret_token"))
                            {

                                key = httpContext.Request.Headers["secret_token"].ToString();
                            }
                            else
                            {

                                ec.HttpContext.Response.StatusCode = 400;
                                throw new CoachOnlineException("Not authorized.", CoachOnlineExceptionState.NotAuthorized);
                            }

                            if (httpContext.Request.Headers.ContainsKey("course_id"))
                            {

                                bool parsed = int.TryParse(httpContext.Request.Headers["course_id"].ToString(), out courseId);
                                if (!parsed)
                                {
                                    throw new CoachOnlineException("Can't parse course Id. It has to be int", CoachOnlineExceptionState.DataNotValid);
                                }
                            }
                            else
                            {
                                ec.HttpContext.Response.StatusCode = 400;
                                throw new CoachOnlineException("course_id is mandatory.", CoachOnlineExceptionState.WrongDataSent);

                            }

                            if (httpContext.Request.Headers.ContainsKey("lesson_id"))
                            {
                                bool parsed = int.TryParse(httpContext.Request.Headers["lesson_id"].ToString(), out lessonId);
                                if (!parsed)
                                {
                                    ec.HttpContext.Response.StatusCode = 400;
                                    throw new CoachOnlineException("Can't parse lesson Id. It has to be int", CoachOnlineExceptionState.DataNotValid);
                                }

                            }
                            else
                            {
                                ec.HttpContext.Response.StatusCode = 400;
                                throw new CoachOnlineException("lesson_id is mandatory.", CoachOnlineExceptionState.WrongDataSent);
                            }

                            try
                            {
                                // userId = ClaimsPrincipal.Current.GetUserId().Value;

                                var userSvc = app.ApplicationServices.GetService(typeof(IUser)) as IUser;
                                //Console.WriteLine(key.Trim('"'));
                                var user = await userSvc.GetUserByTokenAsync(key.Trim('"'), courseId, lessonId);
                                userId = user.Id;
                                //Console.WriteLine("User ok");
                            }
                            catch (CoachOnlineException e)
                            {
                                ec.HttpContext.Response.StatusCode = 400;

                                throw e;
                            }
                        },

                        OnFileCompleteAsync = async eventContext =>
                        {
                            string key = "";
                            int userId = 0;
                            int courseId = 0;
                            int lessonId = 0;
                            Console.WriteLine("On file completed async started");
                            if (httpContext.Request.Headers.ContainsKey("secret_token"))
                            {

                                key = httpContext.Request.Headers["secret_token"].ToString();
                            }
                            else
                            {

                                throw new CoachOnlineException("Not authorized.", CoachOnlineExceptionState.NotAuthorized);
                            }

                            if (httpContext.Request.Headers.ContainsKey("course_id"))
                            {

                                bool parsed = int.TryParse(httpContext.Request.Headers["course_id"].ToString(), out courseId);
                                if (!parsed)
                                {
                                    throw new CoachOnlineException("Can't parse course Id. It has to be int", CoachOnlineExceptionState.DataNotValid);
                                }
                            }
                            else
                            {
                                throw new CoachOnlineException("course_id is mandatory.", CoachOnlineExceptionState.WrongDataSent);

                            }

                            if (httpContext.Request.Headers.ContainsKey("lesson_id"))
                            {

                                bool parsed = int.TryParse(httpContext.Request.Headers["lesson_id"].ToString(), out lessonId);
                                if (!parsed)
                                {
                                    throw new CoachOnlineException("Can't parse course Id. It has to be int", CoachOnlineExceptionState.DataNotValid);
                                }
                            }
                            else
                            {
                                throw new CoachOnlineException("lesson_id is mandatory.", CoachOnlineExceptionState.WrongDataSent);
                            }

                            try
                            {

                                var userSvc = app.ApplicationServices.GetService(typeof(IUser)) as IUser;
                                Console.WriteLine(key.Trim('"'));
                                var user = await userSvc.GetUserByTokenAsync(key.Trim('"'), courseId, lessonId);
                                userId = user.Id;
                                Console.WriteLine("User ok");
                            }
                            catch (CoachOnlineException e)
                            {

                                throw e;
                            }


                            ITusFile file = await eventContext.GetFileAsync();

                            var meta = await file.GetMetadataAsync(new System.Threading.CancellationToken());
                            string filename = "";

                            if (meta.Keys.Contains("filename"))
                            {

                                filename = meta["filename"].GetString(System.Text.Encoding.UTF8);
                            }
                            else
                            {
                                filename = LetsHash.RandomHash("lesson");
                            }




                            var videoStream = await file.GetContentAsync(new System.Threading.CancellationToken());

                            string finalFileName = $"{file.Id}.{filename.Split(".").Last()}";



                            using (var fileStream = new FileStream($"{ConfigData.Config.EnviromentPath}/wwwroot/uploads/{finalFileName}", FileMode.Create, FileAccess.Write))
                            {
                                videoStream.CopyTo(fileStream);
                            }


                            //using (var data = new DataImplementation())
                            //{
                            try
                            {
                                var dataImplSvc = app.ApplicationServices.GetService(typeof(ICoachService)) as ICoachService;
                                // await DataImplementation.Instance.UpdateEpisodeAttachment(key, courseId, lessonId, finalFileName);
                                if (dataImplSvc != null)
                                {
                                    await dataImplSvc.UpdateEpisodeAttachment(key, courseId, lessonId, finalFileName, true);
                                }
                            }
                            catch (CoachOnlineException e)
                            {
                                throw e;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw new CoachOnlineException("Unknown error.", CoachOnlineExceptionState.UNKNOWN);
                            }
                            //}


                        },
                        OnCreateCompleteAsync = async ev =>

                        {

                            var file = await ev.GetFileAsync();
                            bool isVideo = file.IsVideo();
                            bool isAudio = file.IsAudio();
                            if (!isVideo && !isAudio)
                            {
                                string result = "\n\n";
                                var meta = await file.GetMetadataAsync(new System.Threading.CancellationToken { });
                                foreach (var m in meta.Keys)
                                {
                                    try
                                    {
                                        result += $"{m}: {meta[m]} \n";
                                        Console.WriteLine($"{m}: {meta[m]}");
                                    }
                                    catch (Exception) { }
                                }
                                ev.HttpContext.Response.StatusCode = 403;
                                throw new CoachOnlineException("File is not a video or audio." + result, CoachOnlineExceptionState.DataNotValid);

                            }
                        }

                    }
                };

            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<VideoHub>("/signalr/video");
                endpoints.MapHub<ActiveUsersHub>("/signalr/users");
                endpoints.MapRazorPages();
            });



            app.UseSwagger();

#if DEBUG
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoachOnline API");
                c.RoutePrefix = string.Empty;
            });

#endif    

            app.UseStaticFiles(new StaticFileOptions
            {

                OnPrepareResponse = ctx =>
                {
                    try
                    {

                        if (ctx.Context.Request.Path.HasValue && ctx.Context.Request.Path.Value.Contains("uploads"))
                        {
                            if (ctx.Context.Request.Query.ContainsKey("authtoken") && !string.IsNullOrEmpty(ctx.Context.Request.Query.First(x => x.Key.ToLower() == "authtoken").Value))
                            {
                                Console.WriteLine("inside checking for auth token");
                                var userToken = ctx.Context.Request.Query.First(x => x.Key.ToLower() == "authtoken");
                                var userSvc = app.ApplicationServices.GetService(typeof(IUser)) as IUser;
                                var userResponse = userSvc.Authenticate(userToken.Value);
                                userResponse.Wait();
                                var user = userResponse.Result;
                                if (user.UserRole == UserRoleType.ADMIN)
                                {
                                    //free watching
                                }
                                else if (user.UserRole == UserRoleType.COACH && (!ctx.Context.Request.Query.ContainsKey("token") || !ctx.Context.Request.Query.ContainsKey("id")))
                                {
                                    var attachment_hash = ctx.Context.Request.Path.Value.Replace("/uploads/", "").Split(".").FirstOrDefault();
                                    bool isOwner = false;
                                    if (attachment_hash != null)
                                    {
                                        var playerSvc = app.ApplicationServices.GetService(typeof(IPlayerMedia)) as IPlayerMedia;
                                        var isOwnerResponse = playerSvc.UserIsAttachmentOwner(attachment_hash, user.Id);
                                        isOwnerResponse.Wait();
                                        isOwner = isOwnerResponse.Result;
                                    }
                                    if (!isOwner)
                                    {
                                        ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                        ctx.Context.Response.ContentLength = 0;
                                        ctx.Context.Response.Body = Stream.Null;
                                    }
                                }
                                else if (ctx.Context.Request.Query.ContainsKey("token") && ctx.Context.Request.Query.ContainsKey("id"))
                                {
                                    var subscriptionSvc = app.ApplicationServices.GetService(typeof(ISubscription)) as ISubscription;
                                    var episode = ctx.Context.Request.Query.First(x => x.Key.ToLower() == "id");
                                    var episodeId = episode.Value.First().ToInteger();
                                    bool isOwner = false;
                                    bool isPromo = false;
                                    var subActive = subscriptionSvc.IsUserSubscriptionActive(user.Id);
                                    subActive.Wait();
                                    if (!subActive.Result)
                                    {

                                        var playerSvc2 = app.ApplicationServices.GetService(typeof(IPlayerMedia)) as IPlayerMedia;
                                        var isPromotionalEp = playerSvc2.IsEpisodeAPromo(episodeId);
                                        isPromotionalEp.Wait();

                                        isPromo = isPromotionalEp.Result;

                                        if (!isPromo)
                                        {

                                            var isOwnerResponse = userSvc.IsUserOwnerOfEpisode(user.Id, episodeId);
                                            isOwnerResponse.Wait();
                                            isOwner = isOwnerResponse.Result;
                                        }

                                    }


                                    if (!subActive.Result && !isOwner && !isPromo)
                                    {
                                        ctx.Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                        ctx.Context.Response.ContentLength = 0;
                                        ctx.Context.Response.Body = Stream.Null;
                                        return;
                                    }
                                    if (!isOwner)
                                    {
                                        var token = ctx.Context.Request.Query.First(x => x.Key.ToLower() == "token");

                                        var userId = user.Id;

                                        var playerSvc = app.ApplicationServices.GetService(typeof(IPlayerMedia)) as IPlayerMedia;

                                        var getMediaTokenResponse = playerSvc.GetUserTokenForEpisodeMedia(userId, episodeId, token.Value);

                                        getMediaTokenResponse.Wait();
                                        var getMediaToken = getMediaTokenResponse.Result;
                                        if (getMediaToken == null || token.Value != getMediaToken)
                                        {
                                            ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                            ctx.Context.Response.ContentLength = 0;
                                            ctx.Context.Response.Body = Stream.Null;
                                        }
                                        playerSvc.DisposeUserTokenForEpisodeMedia(userId, episodeId).Wait();
                                    }
                                }
                            }
                            else if(ctx.Context.Request.Query.ContainsKey("id"))
                            {
                                Console.WriteLine("checking for promo");
                                var episode = ctx.Context.Request.Query.First(x => x.Key.ToLower() == "id");
                                var episodeId = episode.Value.First().ToInteger();

                                var playerSvc2 = app.ApplicationServices.GetService(typeof(IPlayerMedia)) as IPlayerMedia;
                                var isPromotionalEp = playerSvc2.IsEpisodeAPromo(episodeId);
                                isPromotionalEp.Wait();

                                var isPromo = isPromotionalEp.Result;

                                if (!isPromo)
                                {
                                    ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                    ctx.Context.Response.ContentLength = 0;
                                    ctx.Context.Response.Body = Stream.Null;
                                }
                            }
                            else
                            {
                                ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                ctx.Context.Response.ContentLength = 0;
                                ctx.Context.Response.Body = Stream.Null;
                            }
                        }
                        else if (ctx.Context.Request.Path.HasValue && ctx.Context.Request.Path.Value.Contains("attachments"))
                        {

                            if (ctx.Context.Request.Query.ContainsKey("authtoken"))
                            {
                                var userToken = ctx.Context.Request.Query.First(x => x.Key.ToLower() == "authtoken");
                                var userSvc = app.ApplicationServices.GetService(typeof(IUser)) as IUser;
                                var userResponse = userSvc.Authenticate(userToken.Value);
                                userResponse.Wait();
                                var user = userResponse.Result;
                            }
                        }
                        else if (ctx.Context.Request.Path.HasValue && ctx.Context.Request.Path.Value.Contains("tempfiles"))
                        {
                            ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            ctx.Context.Response.ContentLength = 0;
                            ctx.Context.Response.Body = Stream.Null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        ctx.Context.Response.ContentLength = 0;
                        ctx.Context.Response.Body = Stream.Null;
                    }
                }
            });


        }
    }



    public class RevokeAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        public RevokeAuthenticationEvents(
          IMemoryCache cache,
          ILogger<RevokeAuthenticationEvents> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public override Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            return base.ValidatePrincipal(context);
            
            //foreach(var c in context.HttpContext.Request.Cookies)
            //{
            //    context.Options.CookieManager.DeleteCookie(context.HttpContext, c.Key, new CookieOptions() { Expires = DateTime.Today });
            //}
            
            //return Task.CompletedTask;
        }


    }
}
