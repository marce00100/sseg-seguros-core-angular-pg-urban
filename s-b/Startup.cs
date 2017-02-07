// using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
// using Microsoft.AspNetCore.Cors;
// using Microsoft.AspNetCore.Mvc.Cors.Internal;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNet.Cors;

namespace SVD
{
    public class Startup
    {

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        public IConfigurationRoot Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            // services.AddCors();
          
            services.AddCors(options => options.AddPolicy("AllowAll",
            p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            services.AddSingleton<IConfiguration>(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("AllowAll");
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            // app.UseCors(_ => _.AllowAnyOrigin().AllowAnyHeader().WithMethods("POST", "PUT"));
            app.UseMvc();
            // app.UseCors(builder =>
                
            //        {  builder.AllowAnyOrigin(); //.WithOrigins("http://some.origin.com")
            //             // .WithMethods("GET", "POST", "PUT")
            //             // .AllowAnyHeader()
            //        }
            //     );

        }
    }
}