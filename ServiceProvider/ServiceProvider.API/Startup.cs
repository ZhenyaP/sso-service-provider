using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using ServiceProvider.API.Entities;
using ServiceProvider.API.Filters;
using ServiceProvider.API.Middlewares;

namespace ServiceProvider.API
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(
                p =>
                {
                    var configSettings = p.GetService<IOptions<ConfigSettings>>().Value;
                    var httpClient = new HttpClient(new SocketsHttpHandler
                    {
                        MaxConnectionsPerServer = 100,
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    })
                    {
                        Timeout = TimeSpan.FromSeconds(configSettings.RequestTimespanSeconds)
                    };

                    return httpClient;
                });
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add(new ServiceFilterAttribute(typeof(JwtValidateFilter)));

                //we don't need LogErrorFilter for now since we are using 
                //RequestLoggerMiddleware middleware which catches and logs all Exceptions
                //during the ASP.NET Core request pipeline execution.

                //options.Filters.Add(typeof(LogErrorFilter));     
            });
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Service Provider API",
                    Description = @"The Service Provider (SP) with API that is protected by the
                    certificate-bound JWT access tokens 
                    (according to  https://tools.ietf.org/html/draft-ietf-oauth-mtls-17#section-3.1).",
                    Contact = new OpenApiContact
                    {
                        Name = "Eugene Petrovich",
                        Email = "zhpetrovich@yahoo.com",
                        Url = new Uri("https://www.linkedin.com/in/yauheniy-piatrovich/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://example.com/license")
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ErrorContextInitializationMiddleware>();
            app.UseMiddleware<RequestLoggerMiddleware>();
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve Swagger-UI (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Service Provider API V1");
            });
            app.UseMvc();
        }
    }
}
