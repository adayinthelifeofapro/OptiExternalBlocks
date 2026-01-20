# Opti External Blocks

An Optimizely CMS 12+ plugin that exposes Graph External Content in the Optimizely CMS Interface, allowing editors to use external content natively inside Content Areas.

## Features

- **External Content Tab**: A new "External Content Blocks" tab appears alongside Blocks and Media in the CMS editor interface
- **Admin Template Configuration**: Administrators can configure Mustache templates per external content type
- **Mustache Templating**: Flexible rendering using Mustache templates for both edit mode preview and public rendering
- **Graph API Integration**: Connects to Optimizely Content Graph API to fetch external content
- **Multiple Endpoints**: Support for configuring multiple Graph API endpoints
- **Blazor Admin UI**: Modern admin interface built with Blazor Server
- **Content Area Support**: External content blocks can be dragged and dropped into Content Areas like any other block

## Installation

### NuGet Package

```bash
dotnet add package OptiExternalBlocks
```

### Service Registration

In your `Program.cs` or `Startup.cs`:

```csharp
using OptiExternalBlocks.Common;
using OptiExternalBlocks.Features.Configuration;

// Add External Content Blocks
services.AddOptiExternalBlocks(options =>
{
    options.ConnectionStringName = "EPiServerDB";
    options.AutoMigrate = true;
}, authOptions =>
{
    authOptions.AddPolicy(OptiExternalBlocksConstants.AuthorizationPolicy, policy =>
    {
        policy.RequireRole("WebAdmins", "CmsAdmins", "Administrators");
    });
});
```

After building, ensure the database is migrated:

```csharp
app.Services.EnsureDatabaseCreated();
```

## Configuration

### 1. Configure Graph API Endpoint

1. Log in to Optimizely CMS as an administrator
2. Navigate to **External Content Blocks** in the admin menu
3. Go to **Endpoints** tab
4. Click **Create Endpoint**
5. Enter your Graph API URL and authentication credentials
6. Set as default if this is your primary endpoint

### 2. Configure Content Templates

1. Go to **Templates** tab
2. Click **Create Template**
3. Configure:
   - **Content Type Name**: The type name from your Graph schema (e.g., "Article", "Product")
   - **Display Name**: How it appears to editors
   - **GraphQL Query**: The query to fetch content of this type
   - **Edit Mode Template**: Mustache template for editor preview
   - **Render Template**: Mustache template for public site rendering

### Example Template Configuration

**GraphQL Query:**
```graphql
query GetArticles($searchText: String, $limit: Int = 20, $skip: Int = 0) {
    Article(
        where: { _fulltext: { contains: $searchText } }
        limit: $limit
        skip: $skip
    ) {
        items {
            id: _metadata { key }
            title: Name
            description: Description
            imageUrl: MainImage { url }
        }
        total
    }
}
```

**Edit Mode Template:**
```mustache
<div class="external-content-preview">
    <h3>{{title}}</h3>
    {{#imageUrl}}
    <img src="{{imageUrl}}" alt="{{title}}" style="max-width: 200px;" />
    {{/imageUrl}}
    <p>{{description}}</p>
</div>
```

**Render Template:**
```mustache
<article class="article-block">
    <h2>{{title}}</h2>
    {{#imageUrl}}
    <img src="{{imageUrl}}" alt="{{title}}" class="article-image" />
    {{/imageUrl}}
    <div class="article-content">
        {{description}}
    </div>
</article>
```

## Usage for Editors

1. Open a page in the Optimizely CMS editor
2. Look for the **External Content** tab in the assets panel (alongside Blocks and Media)
3. Select a content type from the dropdown
4. Search or browse for external content
5. Drag content into a Content Area, or double-click to insert
6. The content will render using the configured Mustache template

## Mustache Template Reference

### Available Variables

Standard variables available in all templates:

- `{{_id}}` - The external content ID
- `{{_title}}` - The content title
- `{{_thumbnail}}` - Thumbnail URL (if available)
- `{{_contentType}}` - The content type name

Plus all properties returned by your GraphQL query.

### Sections

```mustache
{{#hasItems}}
    <ul>
    {{#items}}
        <li>{{name}}</li>
    {{/items}}
    </ul>
{{/hasItems}}
```

### Inverted Sections (show if empty)

```mustache
{{^items}}
    <p>No items found.</p>
{{/items}}
```

### Comments

```mustache
{{! This is a comment and won't be rendered }}
```

## API Endpoints

The plugin exposes these API endpoints for the editor interface:

- `GET /api/optiexternalblocks/templates` - List all active templates
- `GET /api/optiexternalblocks/search` - Search external content
- `GET /api/optiexternalblocks/content/{templateId}/{externalId}` - Get specific content
- `GET /api/optiexternalblocks/preview/{templateId}/{externalId}` - Get rendered preview

## Database Tables

The plugin creates these tables:

- `tbl_OptiExternalBlocks_Templates` - Template configurations
- `tbl_OptiExternalBlocks_Endpoints` - Graph API endpoint configurations
- `tbl_OptiExternalBlocks_References` - Cached external content references

## Requirements

- Optimizely CMS 12.23.0+
- .NET 6.0 or .NET 8.0
- SQL Server database
- Optimizely Content Graph subscription (for external content source)

## License

MIT License - See LICENSE file for details.
