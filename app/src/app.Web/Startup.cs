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

            // To share antiforgery tokens, we set up the Data Protection service with a shared location.
            // https://stackoverflow.com/questions/43860631/how-do-i-handle-validateantiforgerytoken-across-linux-servers

            var dirTokensPath = Configuration.GetValue<string>("AntiforgeryTokensPath");
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dirTokensPath));
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
