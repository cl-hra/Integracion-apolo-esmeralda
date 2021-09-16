using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace WebService
{
    public static class MySqlDbConfigurationExtension
    {
        public static DbContextOptionsBuilder AddMysqlDb(
            this DbContextOptionsBuilder options,
            IConfiguration configuration
        )
        {
            var mysqlDb = new MySqlConnectionStringBuilder();
            mysqlDb.Server = configuration["DB_SERVER"] ?? "localhost";
            mysqlDb.Database = configuration["DB"] ?? "BD_MONITOR";
            mysqlDb.UserID = configuration["DB_USER"] ?? "root";
            mysqlDb.Password = configuration["DB_PASSWORD"] ?? "secret";
            mysqlDb.Port = uint.Parse(configuration["DB_PORT"] ?? "3306");

            mysqlDb.ConnectionProtocol =
                Enum.TryParse(configuration["BD_PROTOCOL"], out MySqlConnectionProtocol protocol)
                    ? protocol
                    : MySqlConnectionProtocol.Socket;

            options.UseMySql(
                mysqlDb.ToString(),
                config =>
                {
                    config.ServerVersion(Version.Parse("5.7.30"), ServerType.MySql)
                          .EnableRetryOnFailure();
                }
            );
            options.EnableSensitiveDataLogging();
            return options;
        }
    }
}
