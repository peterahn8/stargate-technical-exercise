using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StargateAPI.Business;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests
{
    /// <summary>
    /// One isolated in-memory SQLite database per test, built with the real migrations and served
    /// through the same MediatR pipeline as the API (D11).
    /// </summary>
    public sealed class TestHost : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _services;

        public TestHost()
        {
            // An in-memory SQLite database only lives while its connection is open.
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _services = new ServiceCollection()
                .AddDbContext<StargateContext>(options => options.UseSqlite(_connection))
                .AddStargateMediatR()
                .BuildServiceProvider();

            using var scope = _services.CreateScope();
            scope.ServiceProvider.GetRequiredService<StargateContext>().Database.Migrate();
        }

        /// <summary>Sends a request in its own scope, the way each HTTP request gets its own.</summary>
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
        {
            using var scope = _services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(request);
        }

        /// <summary>Reads or arranges data directly, in its own scope.</summary>
        public T Db<T>(Func<StargateContext, T> action)
        {
            using var scope = _services.CreateScope();
            return action(scope.ServiceProvider.GetRequiredService<StargateContext>());
        }

        public void Dispose()
        {
            _services.Dispose();
            _connection.Dispose();
        }
    }
}
