using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace WebService
{
    public class Program
    {
        public static int Main(string[] args)
        {

            try
            {
                using IHost host = CreateHostBuilder(args)
                   .Build();
                host.Run();
                return 0;
            }
            catch (Exception e)
            {
                if (Log.Logger == null ||
                    Log.Logger.GetType().Name == "SilentLogger")
                {
                    Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                                          .WriteTo.Console()
                                                          .CreateLogger();
                }
                Log.Fatal(e,"Integración Esmeralda Terminó Inesperadamente");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .UseSerilog(
                            (context, logConfig) =>
                            {
                                logConfig.ReadFrom.Configuration(context.Configuration)
                                         .Enrich.WithCorrelationId()
                                         .Enrich.WithCorrelationIdHeader()
                                         .Enrich.WithProperty("ApplicationName", "API Integración Esmeralda")
                                         .Enrich.WithProperty(
                                              "Environment",
                                              context.HostingEnvironment.EnvironmentName
                                          );
                            } )
                       .ConfigureWebHostDefaults(
                            webBuilder =>
                            {
                                webBuilder.UseStartup<Startup>()
                                          .CaptureStartupErrors(true);
                            }
                        );
        }
    }
}
