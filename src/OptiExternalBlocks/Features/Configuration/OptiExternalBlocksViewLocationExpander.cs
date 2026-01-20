using Microsoft.AspNetCore.Mvc.Razor;
using OptiExternalBlocks.Common;

namespace OptiExternalBlocks.Features.Configuration;

/// <summary>
/// View location expander that adds the OptiExternalBlocks module's view paths
/// to the Razor view engine search locations.
/// </summary>
public class OptiExternalBlocksViewLocationExpander : IViewLocationExpander
{
    private static readonly string[] AdditionalViewLocations = new[]
    {
        $"/modules/_protected/{OptiExternalBlocksConstants.ModuleName}/Views/{{1}}/{{0}}.cshtml",
        $"/modules/_protected/{OptiExternalBlocksConstants.ModuleName}/Views/Shared/{{0}}.cshtml",
        $"/modules/_protected/{OptiExternalBlocksConstants.ModuleName}/Views/Shared/DisplayTemplates/{{0}}.cshtml",
        $"/modules/_protected/{OptiExternalBlocksConstants.ModuleName}/Views/Shared/Blocks/{{0}}.cshtml"
    };

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        // No values to populate - we're just adding static locations
    }

    public IEnumerable<string> ExpandViewLocations(
        ViewLocationExpanderContext context,
        IEnumerable<string> viewLocations)
    {
        // Return original locations first, then add module locations
        return viewLocations.Concat(AdditionalViewLocations);
    }
}
