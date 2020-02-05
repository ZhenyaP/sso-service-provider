using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace ServiceProvider.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseSerilog((context, config) =>
                        {
                            config
                                .MinimumLevel.Debug()
                                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                .MinimumLevel.Override("System", LogEventLevel.Warning)
                                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                                .Enrich.FromLogContext()
                                .Enrich.WithExceptionDetails()
                                .WriteTo.Console()
                                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day);
                        });
                });
    }
}
