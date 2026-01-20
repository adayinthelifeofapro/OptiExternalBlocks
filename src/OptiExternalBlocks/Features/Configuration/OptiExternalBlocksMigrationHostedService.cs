using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OptiExternalBlocks.Entities;

namespace OptiExternalBlocks.Features.Configuration;

/// <summary>
/// Hosted service that runs database migrations on application startup
/// when AutoMigrate is enabled.
/// </summary>
public class OptiExternalBlocksMigrationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OptiExternalBlocksMigrationHostedService> _logger;
    private readonly OptiExternalBlocksSetupOptions _options;

    public OptiExternalBlocksMigrationHostedService(
        IServiceProvider serviceProvider,
        ILogger<OptiExternalBlocksMigrationHostedService> logger,
        IOptions<OptiExternalBlocksSetupOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.AutoMigrate)
        {
            _logger.LogInformation("OptiExternalBlocks: AutoMigrate is disabled, skipping database migrations");
            return;
        }

        _logger.LogInformation("OptiExternalBlocks: Running database migrations...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OptiExternalBlocksDataContext>();
            await context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("OptiExternalBlocks: Database migrations completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OptiExternalBlocks: Error running database migrations");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
