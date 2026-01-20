using EPiServer.Shell;
using EPiServer.Shell.ViewComposition;

namespace OptiExternalBlocks.Features.Configuration;

/// <summary>
/// Registers the External Content tab in the Optimizely CMS navigation panel
/// alongside Blocks and Media tabs.
/// </summary>
/// <remarks>
/// Registers the External Content tab in the assets panel (alongside Blocks and Media).
/// The dojo module is loaded from the OptiExternalBlocks.zip module package.
/// </remarks>
[Component]
public class ExternalContentNavigationComponent : ComponentDefinitionBase
{
    public ExternalContentNavigationComponent()
        : base("opti-external-blocks/ExternalContentComponent")
    {
        LanguagePath = "/optiexternalblocks/components/externalcontent";
        Title = "External Content";
        Description = "Browse and select external content from the Graph API";
        SortOrder = 400; // After Media (usually around 300)
        PlugInAreas = new[] { PlugInArea.AssetsDefaultGroup };
        Categories = new[] { "content" };
        Settings.Add(new Setting("apiUrl", "/api/optiexternalblocks"));
    }
}
