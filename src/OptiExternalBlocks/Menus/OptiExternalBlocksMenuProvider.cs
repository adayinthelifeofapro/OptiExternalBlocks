using EPiServer.Shell.Navigation;
using OptiExternalBlocks.Common;

namespace OptiExternalBlocks.Menus;

[MenuProvider]
public sealed class OptiExternalBlocksMenuProvider : IMenuProvider
{
    public IEnumerable<MenuItem> GetMenuItems()
    {
        // Parent menu item
        yield return CreateMenuItem(
            "External Content Blocks",
            "/global/cms/externalcontentblocks",
            "/optimizely-externalblocks/administration/templates",
            SortIndex.Last + 40);

        // Child menu items
        yield return CreateMenuItem(
            "Templates",
            "/global/cms/externalcontentblocks/templates",
            "/optimizely-externalblocks/administration/templates",
            SortIndex.Last + 41);

        yield return CreateMenuItem(
            "Endpoints",
            "/global/cms/externalcontentblocks/endpoints",
            "/optimizely-externalblocks/administration/endpoints",
            SortIndex.Last + 42);

        yield return CreateMenuItem(
            "About",
            "/global/cms/externalcontentblocks/about",
            "/optimizely-externalblocks/administration/about",
            SortIndex.Last + 43);
    }

    private static UrlMenuItem CreateMenuItem(string name, string path, string url, int index)
    {
        return new UrlMenuItem(name, path, url)
        {
            IsAvailable = context => true,
            SortIndex = index,
            AuthorizationPolicy = OptiExternalBlocksConstants.AuthorizationPolicy
        };
    }
}
