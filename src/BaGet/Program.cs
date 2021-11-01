using System;
using System.Linq;
using System.Threading.Tasks;
using BaGet.Core;
using BaGet.Web;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BaGet
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if(args.Length != 0)
            {
                string[] rmArgs = new string[] {"/bin/sh", "-c", @"bundle\", @"exec\", @"puma\", @"-C\", @"source\", @"/app/setup_heroku_env.sh","heroku", "start", "run", "stop"};
                for(int i = 0; i< rmArgs.Length; i++)
                {
                    if(args.Contains(rmArgs[i]))
                    {
                        int index = Array.IndexOf(args,rmArgs[i]);
                        args[index] = string.Empty;
                    }

                    args = args.Where(a => !string.IsNullOrEmpty(a)).ToArray();
                }

            }
            
            var host = CreateHostBuilder(args).Build();
            if (!host.ValidateStartupOptions())
            {
                return;
            }

            var app = new CommandLineApplication
            {
                Name = "baget",
                Description = "A light-weight NuGet service",
            };

            app.HelpOption(inherited: true);

            app.Command("import", import =>
            {
                import.Command("downloads", downloads =>
                {
                    downloads.OnExecuteAsync(async cancellationToken =>
                    {
                        using (var scope = host.Services.CreateScope())
                        {
                            var importer = scope.ServiceProvider.GetRequiredService<DownloadsImporter>();

                            await importer.ImportAsync(cancellationToken);
                        }
                    });
                });
            });

            app.Option("--urls", "The URLs that BaGet should bind to.", CommandOptionType.SingleValue);

            app.OnExecuteAsync(async cancellationToken =>
            {
                await host.RunMigrationsAsync(cancellationToken);
                await host.RunAsync(cancellationToken);
            });

            await app.ExecuteAsync(args);
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    var root = Environment.GetEnvironmentVariable("BAGET_CONFIG_ROOT");

                    if (!string.IsNullOrEmpty(root))
                    {
                        config.SetBasePath(root);
                    }
                })
                .ConfigureWebHostDefaults(web =>
                {
                    web.ConfigureKestrel(options =>
                    {
                        // Remove the upload limit from Kestrel. If needed, an upload limit can
                        // be enforced by a reverse proxy server, like IIS.
                        options.Limits.MaxRequestBodySize = null;
                    });

                    web.UseStartup<Startup>();
                });
        }
    }
}
