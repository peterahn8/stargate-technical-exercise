using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests
{
    /// <summary>
    /// The whole API in memory, for HTTP-level tests of status codes (D11). Uses its own in-memory
    /// SQLite database built by the real migrations, never starbase.db.
    /// </summary>
    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");
        private readonly string _environment;

        /// <param name="environment">
        /// "Testing" by default, so Program skips its Development-only migrate-and-seed step. Either
        /// way the database is this factory's in-memory one, never starbase.db.
        /// </param>
        public ApiFactory(string environment = "Testing")
        {
            _environment = environment;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(_environment);

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<StargateContext>>();
                services.AddDbContext<StargateContext>(options => options.UseSqlite(_connection));
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            _connection.Open();
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<StargateContext>().Database.Migrate();

            return host;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) _connection.Dispose();
        }
    }
}
