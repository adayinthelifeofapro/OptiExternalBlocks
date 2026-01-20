using System.Reflection;
using EPiServer.Shell.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OptiExternalBlocks.Common;
using OptiExternalBlocks.Controllers;
using OptiExternalBlocks.Entities;
using OptiExternalBlocks.Features.ExternalContent.Services;
using OptiExternalBlocks.Features.ExternalContent.Services.Abstractions;
using OptiExternalBlocks.Features.Templates.Services;
using OptiExternalBlocks.Features.Templates.Services.Abstractions;

namespace OptiExternalBlocks.Features.Configuration;

public static class OptiExternalBlocksServiceExtensions
{
    /// <summary>
    /// Adds the External Content Blocks module to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Configuration action for module options.</param>
    /// <param name="configureAuthorization">Configuration action for authorization options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOptiExternalBlocks(
        this IServiceCollection services,
        Action<OptiExternalBlocksSetupOptions>? configureOptions = null,
        Action<AuthorizationOptions>? configureAuthorization = null)
    {
        var options = new OptiExternalBlocksSetupOptions();
        configureOptions?.Invoke(options);

        // Get configuration
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString(options.ConnectionStringName);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{options.ConnectionStringName}' not found in configuration.");
        }

        // Register DbContext
        services.AddDbContext<OptiExternalBlocksDataContext>(dbOptions =>
        {
            dbOptions.UseSqlServer(connectionString);
        });

        services.AddScoped<IOptiExternalBlocksDataContext>(sp =>
            sp.GetRequiredService<OptiExternalBlocksDataContext>());

        // Register services
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IEndpointService, EndpointService>();
        services.AddScoped<ITemplateRenderingService, TemplateRenderingService>();
        services.AddScoped<IGraphApiClient, GraphApiClient>();

        // Register HttpClient for Graph API
        services.AddHttpClient("GraphApi");

        // Add memory cache if not already registered
        services.AddMemoryCache();

        // Register view location expander so Razor can find module views (DisplayTemplates, etc.)
        services.Configure<RazorViewEngineOptions>(razorOptions =>
        {
            razorOptions.ViewLocationExpanders.Add(new OptiExternalBlocksViewLocationExpander());
        });

        // Register this assembly as an application part so MVC discovers ViewComponents
        services.AddMvcCore().ConfigureApplicationPartManager(manager =>
        {
            var assembly = typeof(OptiExternalBlocksServiceExtensions).Assembly;
            if (!manager.ApplicationParts.Any(p => p.Name == assembly.GetName().Name))
            {
                manager.ApplicationParts.Add(new AssemblyPart(assembly));
            }
        });

        // Configure authorization - let the consumer configure the policy
        if (configureAuthorization != null)
        {
            services.AddAuthorization(configureAuthorization);
        }

        // Register the module with Optimizely's protected module system
        // This is required for CMS 12+ to discover and load the module's client resources
        services.Configure<ProtectedModuleOptions>(pm =>
        {
            if (!pm.Items.Any(i =>
                i.Name.Equals(OptiExternalBlocksConstants.ModuleName, StringComparison.OrdinalIgnoreCase)))
            {
                pm.Items.Add(new ModuleDetails
                {
                    Name = OptiExternalBlocksConstants.ModuleName
                });
            }
        });

        // Register options for IOptions<T> pattern
        services.Configure<OptiExternalBlocksSetupOptions>(opts =>
        {
            opts.ConnectionStringName = options.ConnectionStringName;
            opts.AutoMigrate = options.AutoMigrate;
            opts.DefaultGraphEndpoint = options.DefaultGraphEndpoint;
            opts.DefaultSingleKey = options.DefaultSingleKey;
            opts.CacheDurationMinutes = options.CacheDurationMinutes;
        });

        // Register hosted service for auto-migration
        services.AddHostedService<OptiExternalBlocksMigrationHostedService>();

        return services;
    }

    /// <summary>
    /// Ensures the database is created and migrated.
    /// Call this during application startup.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OptiExternalBlocksDataContext>();
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Synchronously ensures the database is created and migrated.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public static void EnsureDatabaseCreated(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OptiExternalBlocksDataContext>();
        context.Database.Migrate();
    }
}
