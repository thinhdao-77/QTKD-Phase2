using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QTKD.Api.Data;

namespace QTKD.Api.Tests.Integration;

/// <summary>
/// WebApplicationFactory dùng cơ sở dữ liệu kiểm thử riêng biệt 'qtkd_test_db' trên PostgreSQL (Bước 16),
/// đảm bảo tách biệt hoàn toàn với cơ sở dữ liệu chính 'qtkd_db'.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string DefaultTestConnectionString =
        "Host=localhost;Port=5432;Database=qtkd_test_db;Username=postgres;Password=123456";

    public string TestConnectionString =>
        Environment.GetEnvironmentVariable("QTKD_TEST_CONNECTION_STRING") ?? DefaultTestConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<QtkdDbContext>>();
            services.RemoveAll<QtkdDbContext>();

            services.AddDbContext<QtkdDbContext>(options =>
                options.UseNpgsql(TestConnectionString));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();

        // Làm sạch và chạy lại toàn bộ Migration trên database kiểm thử riêng 'qtkd_test_db'
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
