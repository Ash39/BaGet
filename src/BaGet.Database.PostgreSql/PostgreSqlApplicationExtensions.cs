using System;
using BaGet.Core;
using BaGet.Database.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BaGet
{
    public static class PostgreSqlApplicationExtensions
    {
        public static BaGetApplication AddPostgreSqlDatabase(this BaGetApplication app)
        {
            app.Services.AddBaGetDbContextProvider<PostgreSqlContext>("PostgreSql", (provider, options) =>
            {
                
                var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

                if(Environment.GetEnvironmentVariable("DATABASE_URL") != null)
                {
                    var databaseUri = new Uri(Environment.GetEnvironmentVariable("DATABASE_URL"));
                    var userInfo = databaseUri.UserInfo.Split(':');

                    var builder = new NpgsqlConnectionStringBuilder
                    {
                        Host = databaseUri.Host,
                        Port = databaseUri.Port,
                        Username = userInfo[0],
                        Password = userInfo[1],
                        Database = databaseUri.LocalPath.TrimStart('/'),
                        SslMode = SslMode.Require, 
                        TrustServerCertificate = true
                    };

                    databaseOptions.Value.ConnectionString = builder.ToString();
                }
                
                options.UseNpgsql(databaseOptions.Value.ConnectionString);
            });

            return app;
        }

        public static BaGetApplication AddPostgreSqlDatabase(
            this BaGetApplication app,
            Action<DatabaseOptions> configure)
        {
            app.AddPostgreSqlDatabase();
            app.Services.Configure(configure);
            return app;
        }
    }
}
