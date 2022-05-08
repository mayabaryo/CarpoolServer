using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration; //This one adds the GetConnectionString method to Configuration
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using CarpoolServerBL.Models;

//for the live carpool
using CarpoolServer.Hubs;

namespace CarpoolServer
{
    public class Startup
    {
        public static string ServerEmail {get; private set;}
        public static string ServerEmailPassword { get; private set; }

        #region Add Constructor with Configuration using Dependency Injection
        //This constructor need to be added to get an instance of configuration file (appsettings.json)
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        #endregion

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            //Add Controllers and set the Json Serializer to handle loop referencing
            services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions
                        .ReferenceHandler = ReferenceHandler.Preserve);
            //The following two commands set the Session state to work!
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(180);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            //The following set the connection string to the DB and DB context!
            #region Add DB Context Support
            string connectionString = this.Configuration.GetConnectionString("CarpoolDB");

            services.AddDbContext<CarpoolDBContext>(options => options
                                                                .UseSqlServer(connectionString));
            //.UseLazyLoadingProxies());
            #endregion

            #region Read Email and email password
            Object email = this.Configuration.GetValue(typeof(string), "ServerEmail");
            Object emailPassword = this.Configuration.GetValue(typeof(string), "ServerEmailPassword");

            ServerEmail = email.ToString();
            ServerEmailPassword = emailPassword.ToString();
            #endregion

            //Add SignalR
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(); //Added to have the wwwroot folder and server to accept calls to static files
            app.UseRouting();
            app.UseSession(); //Added to tell the server to use sessions!

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //for the live carpool
                endpoints.MapHub<CarpoolHub>("/carpool");
            });
        }
    }
}
