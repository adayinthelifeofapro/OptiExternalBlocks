namespace OptiExternalBlocks.Features.Configuration;

/// <summary>
/// Configuration options for the External Content Blocks module.
/// </summary>
public class OptiExternalBlocksSetupOptions
{
    /// <summary>
    /// The name of the connection string to use for the database.
    /// Defaults to "EPiServerDB".
    /// </summary>
    public string ConnectionStringName { get; set; } = "EPiServerDB";

    /// <summary>
    /// Whether to automatically run database migrations on startup.
    /// Defaults to true.
    /// </summary>
    public bool AutoMigrate { get; set; } = true;

    /// <summary>
    /// Default Graph API endpoint URL.
    /// </summary>
    public string? DefaultGraphEndpoint { get; set; }

    /// <summary>
    /// Default single key for authentication.
    /// </summary>
    public string? DefaultSingleKey { get; set; }

    /// <summary>
    /// Cache duration in minutes for external content.
    /// Defaults to 5 minutes.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 5;
}
