using app.DAL.Managers;
using app.Web.Utils;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;

namespace app.Web
{
    public class Startup
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(Startup));

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static RedisStruct RedisInfo;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddTransient(typeof(IDBManager<>), typeof(DynamoDBManager<>));

            InitAddDataProtection(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();

            LoggingStuffOnLog4NetInitCompleted();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// To share antiforgery tokens, we set up the Data Protection service with a shared location.
        /// It could be a shared directory, a Redis cache ..
        /// https://stackoverflow.com/questions/43860631/how-do-i-handle-validateantiforgerytoken-across-linux-servers
        /// https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/implementation/key-storage-providers?view=aspnetcore-2.1&tabs=visual-studio#azure-and-redis
        /// </summary>
        /// <param name="services"></param>
        void InitAddDataProtection(IServiceCollection services)
        {
            /*** Shared Directory by EC2 instances ***/

            //var dirTokensPath = Configuration.GetValue<string>("AntiforgeryTokensPath");
            //services.AddDataProtection()
            //    .PersistKeysToFileSystem(new DirectoryInfo(dirTokensPath));

            /*** Shared Redis Cache ***/
            //_redisUrl = Configuration.GetSection("Redis").GetValue<string>("Url");

            try
            {
                string keyname = Configuration.GetSection("Redis").GetValue<string>("ParamStoreKeyname");
                RedisStruct.HistoryLogs.Add($"Start reading parameter : '{keyname}'");
                RedisStruct.Url = AWSParameterHelper.GetConfiguration(keyname).Result;                

                RedisStruct.HistoryLogs.Add($"Start connecting to Redis url : '{RedisStruct.Url}'");
                RedisStruct.Connection = ConnectionMultiplexer.Connect(RedisStruct.Url);
                RedisStruct.HistoryLogs.Add($"Connected on Redis url : '{RedisStruct.Url}'");

                if (RedisStruct.Connection != null)
                {
                    RedisStruct.HistoryLogs.Add($"Redis connection on url '{RedisStruct.Url}'. Status is : {RedisStruct.ConnectionState} | {RedisStruct.Connection.GetStatus()}");

                    services.AddDataProtection()
                                .PersistKeysToStackExchangeRedis(RedisStruct.Connection, "DataProtection-Keys");
                }
                else
                    RedisStruct.HistoryLogs.Add($"Not connected on Redis url : '{RedisStruct.Url}' !");

            }
            catch (RedisConnectionException ex)
            {
                RedisStruct.HistoryLogs.Add($"An error occured when connecting to Redis url : '{RedisStruct.Url}' : {ex}");
            }
            catch (Exception ex)
            {
                RedisStruct.HistoryLogs.Add($"An error has occured ! : { ex}");
            }
        }

        void LoggingStuffOnLog4NetInitCompleted()
        {
            string cwd = Directory.GetCurrentDirectory();
            if (!Directory.Exists(Path.Combine(cwd, "wwwroot")))
                _log.Error($"wwwroot not found in current directory '{cwd}'. It should contain wwwroot to serve static files");

            foreach (var l in RedisStruct.HistoryLogs)
                _log.Info(l);
        }
    }

    public class RedisStruct
    {
        public static string Url;

        public static ConnectionMultiplexer Connection;

        public static string ConnectionState => IsConnected ? "Connected" : "Not Connected";

        public static bool IsConnected => (Connection != null && Connection.IsConnected);

        public static List<string> HistoryLogs = new List<string>();
    }
}
