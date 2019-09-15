using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using CityInfo.API.Services;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AutoMapper;

namespace CityInfo.API
{
    public class Startup
    {

        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
              .AddMvcOptions(o => o.OutputFormatters.Add(
                  new XmlDataContractSerializerOutputFormatter())); // will add xml as option

            // If you want different serialization in response
            //    .AddJsonOptions(o => {
            //        if(o.SerializerSettings.ContractResolver != null)
            //        {
            //            var castedResolver = o.SerializerSettings.ContractResolver as DefaultContractResolver;
            //            castedResolver.NamingStrategy = null;
            //        }
            //});

            var connectionString = Startup.Configuration["connectionStrings:cityInfoDBConnectionString"];
            services.AddDbContext<CityInfoContext>(o => o.UseSqlServer(connectionString));

            services.AddScoped<ICityInfoRepository, CityInfoRepository>();

            services.AddAutoMapper(typeof(Startup));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, CityInfoContext cityInfoContext)
        {
            loggerFactory.AddConsole();

            loggerFactory.AddDebug();

            loggerFactory.AddNLog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            cityInfoContext.EnsureSeedDataForContext();

            app.UseStatusCodePages();



            app.UseMvc();

            //app.Run((context) => 
            //{
            //    throw new Exception("Example exception");
            //});

            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});
        }
    }
}
