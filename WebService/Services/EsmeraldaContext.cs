using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Text;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace WebService.Services
{
    public class EsmeraldaContext : DbContext
    {

        public EsmeraldaContext(DbContextOptions<EsmeraldaContext> options) : base(options)
        {

        }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var env = Environment.GetEnvironmentVariables();
            var host = env["DBHOST"] ?? "localhost";
            var port = env["DBPORT"] ?? "3306";
            var user = env["DBUSER"] ?? "root";
            var db = env["DB"] ?? "testEsmeraldos";
            var password = env["DBPASSWORD"] ?? "";
            options.UseMySql(Environment.GetEnvironmentVariable($"server = {host} ; database = {db}; port = {port}; user id = {user}; password ={db}"));
        }

        public DbSet<users> users { get; set; }
        public DbSet<Patients> patients { get; set; }
        public DbSet<Communes> communes { get; set; }
        public DbSet<demographics> Demographics { get; set; }
        public DbSet<Sospecha> suspect_cases { get; set; }
    }
}
