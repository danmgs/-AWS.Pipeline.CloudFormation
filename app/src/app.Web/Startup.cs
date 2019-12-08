using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using app.DAL.Managers;
using app.Models;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddTransient(typeof(IDBManager<>), typeof(DynamoDBManager<>));

            InitAddDataProtection(services);
        }

        /// <summary>
        /// To share antiforgery tokens, we set up the Data Protection service with a shared location.
        /// It could be a shared directory, a Redis cache ..
        /// https://stackoverflow.com/questions/43860631/how-do-i-handle-validateantiforgerytoken-across-linux-servers
        /// </summary>
        /// <param name="services"></param>
        void InitAddDataProtection(IServiceCollection services)
        {
            /*** Shared Directory by EC2 instances ***/
            //var dirTokensPath = Configuration.GetValue<string>("AntiforgeryTokensPath");
            //services.AddDataProtection()
            //    .PersistKeysToFileSystem(new DirectoryInfo(dirTokensPath));

            /*** Shared Redis Cache ***/
            var redisUrl = Configuration.GetSection("Redis").GetValue<string>("Url");

            try
            {
                var redis = ConnectionMultiplexer.Connect(redisUrl);
                services.AddDataProtection()
                    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
            }
            catch (RedisConnectionException ex)
            {
                _log.Error($"Could not connect to Redis : {redisUrl}", ex);
            }
            catch (Exception ex)
            {
                _log.Error($"Could not connect to Redis : {redisUrl}", ex);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();

            var cwd = Directory.GetCurrentDirectory();
            _log.Info(" *** Current working directory should contain wwwroot to server static files ***");
            _log.Info($"Current working directory : {cwd}");
            

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
    }
}
