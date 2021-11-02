using System;
using BaGet.Core;
using Dropbox.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BaGet.DropBox
{
    public static class DropBoxApplicationExtensions
    {
        public static BaGetApplication AddDropBoxStorage(this BaGetApplication app)
        {
            app.Services.AddBaGetOptions<DropBoxOptions>(nameof(BaGetOptions.Storage));

            app.Services.AddTransient<DropBoxService>();
            app.Services.TryAddTransient<IStorageService>(provider => provider.GetRequiredService<DropBoxService>());

            app.Services.AddProvider<IStorageService>((provider, config) =>
            {
                if (!config.HasStorageType("DropBox")) return null;

                return provider.GetRequiredService<DropBoxService>();
            });

            app.Services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<DropBoxOptions>>().Value;

                
                return new DropboxClient(options.AccessToken);
            });

            return app;
        }

        public static BaGetApplication AddDropBoxStorage(this BaGetApplication app, Action<DropBoxOptions> configure)
        {
            app.AddDropBoxStorage();
            app.Services.Configure(configure);
            return app;
        }

    }
    
}