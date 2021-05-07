using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Billing.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        // TODO: read our conventional configuration values

        //// It is if you want to override the configuration in your
        //// local environment, `*.Secret.*` files will not be
        //// included in git.
        //configurationBuilder.AddJsonFile("appsettings.Secret.json", true);
        //// It is to override configuration using Docker's services.
        //// Probably this will be the way of overriding the
        //// configuration in our Swarm stack.
        //configurationBuilder.AddJsonFile("/run/secrets/appsettings.Secret.json", true);
        //// It is to override configuration using a different file
        //// for each configuration entry. For example, you can create
        //// the file `/run/secrets/Logging__LogLevel__Default` with
        //// the content `Trace`. See:
        //// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1#key-per-file-configuration-provider
        //configurationBuilder.AddKeyPerFile("/run/secrets", true);

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.Secret.json", true);
                    config.AddJsonFile("C:\\ProgramData\\Docker\\secrets\\appsettings.Secret.json", true);
                    config.AddJsonFile("/run/secrets/appsettings.Secret.json", true);
                    config.AddKeyPerFile("C:\\ProgramData\\Docker\\secrets", true);
                    config.AddKeyPerFile("/run/secrets", true);
                })
                .UseStartup<Startup>();
    }
}
