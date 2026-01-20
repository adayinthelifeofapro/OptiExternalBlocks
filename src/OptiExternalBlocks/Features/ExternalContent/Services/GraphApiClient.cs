using System.Text.Json;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OptiExternalBlocks.Entities;
using OptiExternalBlocks.Features.ExternalContent.Models;
using OptiExternalBlocks.Features.ExternalContent.Services.Abstractions;

namespace OptiExternalBlocks.Features.ExternalContent.Services;

public class GraphApiClient : IGraphApiClient
{
    private readonly IOptiExternalBlocksDataContext _dataContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GraphApiClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public GraphApiClient(
        IOptiExternalBlocksDataContext dataContext,
        IMemoryCache cache,
        ILogger<GraphApiClient> logger,
        IHttpClientFactory httpClientFactory)
    {
        _dataContext = dataContext;
        _cache = cache;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Dictionary<string, object>?> ExecuteQueryAsync(
        string query,
        string? variables = null,
        Guid? endpointId = null,
        CancellationToken cancellationToken = default)
    {
        var endpoint = await GetEndpointAsync(endpointId, cancellationToken);
        if (endpoint == null)
        {
            _logger.LogWarning("No Graph endpoint configured");
            return null;
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient("GraphApi");

            // Add authentication headers if configured
            if (!string.IsNullOrEmpty(endpoint.SingleKey))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"epi-single {endpoint.SingleKey}");
            }
            else if (!string.IsNullOrEmpty(endpoint.AppKey) && !string.IsNullOrEmpty(endpoint.AppSecret))
            {
                var credentials = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($"{endpoint.AppKey}:{endpoint.AppSecret}"));
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
            }

            using var graphQLClient = new GraphQLHttpClient(
                new GraphQLHttpClientOptions { EndPoint = new Uri(endpoint.EndpointUrl) },
                new SystemTextJsonSerializer(),
                httpClient);

            var request = new GraphQLRequest
            {
                Query = query
            };

            if (!string.IsNullOrEmpty(variables))
            {
                request.Variables = JsonSerializer.Deserialize<Dictionary<string, object>>(variables);
            }

            var response = await graphQLClient.SendQueryAsync<JsonElement>(request, cancellationToken);

            if (response.Errors?.Any() == true)
            {
                _logger.LogError("GraphQL errors: {Errors}",
                    string.Join(", ", response.Errors.Select(e => e.Message)));
                return null;
            }

            return JsonSerializer.Deserialize<Dictionary<string, object>>(response.Data.GetRawText());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing GraphQL query");
            return null;
        }
    }

    public async Task<ExternalContentSearchResponse> SearchContentAsync(
        ExternalContentSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new ExternalContentSearchResponse
        {
            Page = request.Page,
            PageSize = request.PageSize
        };

        var template = await _dataContext.Templates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.IsActive, cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("Template {TemplateId} not found or inactive", request.TemplateId);
            return response;
        }

        // Build the query with search and pagination
        var query = template.GraphQLQuery;
        var variables = template.GraphQLVariables ?? "{}";

        // Add search and pagination to variables
        var variablesDict = JsonSerializer.Deserialize<Dictionary<string, object>>(variables)
            ?? new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(request.SearchQuery))
        {
            variablesDict["searchText"] = request.SearchQuery;
        }
        variablesDict["skip"] = (request.Page - 1) * request.PageSize;
        variablesDict["limit"] = request.PageSize;

        if (!string.IsNullOrEmpty(request.Language))
        {
            variablesDict["locale"] = request.Language;
        }

        var result = await ExecuteQueryAsync(
            query,
            JsonSerializer.Serialize(variablesDict),
            null,
            cancellationToken);

        if (result == null)
        {
            return response;
        }

        // Parse the response and extract items
        response.Items = ParseContentItems(result, template.ContentTypeName, template.TitleFieldName, template.ThumbnailFieldName);

        // Try to get total count from response
        if (result.TryGetValue("total", out var totalObj) && totalObj is JsonElement totalElement)
        {
            response.TotalCount = totalElement.GetInt32();
        }
        else
        {
            response.TotalCount = response.Items.Count;
        }

        return response;
    }

    public async Task<ExternalContentItem?> GetContentByIdAsync(
        Guid templateId,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"external-content:{templateId}:{externalId}";

        if (_cache.TryGetValue(cacheKey, out ExternalContentItem? cachedItem))
        {
            return cachedItem;
        }

        var template = await _dataContext.Templates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive, cancellationToken);

        if (template == null)
        {
            return null;
        }

        // Modify query to fetch single item by ID
        var query = template.GraphQLQuery;
        var variables = new Dictionary<string, object>
        {
            ["id"] = externalId
        };

        var result = await ExecuteQueryAsync(
            query,
            JsonSerializer.Serialize(variables),
            null,
            cancellationToken);

        if (result == null)
        {
            return null;
        }

        var items = ParseContentItems(result, template.ContentTypeName, template.TitleFieldName, template.ThumbnailFieldName);

        // Find the specific item by ID (the query may return multiple items)
        var item = items.FirstOrDefault(i => i.Id == externalId);

        if (item != null)
        {
            _cache.Set(cacheKey, item, TimeSpan.FromMinutes(5));
        }

        return item;
    }

    private async Task<GraphEndpointConfiguration?> GetEndpointAsync(
        Guid? endpointId,
        CancellationToken cancellationToken)
    {
        if (endpointId.HasValue)
        {
            return await _dataContext.Endpoints
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == endpointId && e.IsActive, cancellationToken);
        }

        return await _dataContext.Endpoints
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IsDefault && e.IsActive, cancellationToken);
    }

    private List<ExternalContentItem> ParseContentItems(
        Dictionary<string, object> result,
        string contentTypeName,
        string? titleFieldName = null,
        string? thumbnailFieldName = null)
    {
        var items = new List<ExternalContentItem>();

        try
        {
            // Try to find items array in the response
            foreach (var kvp in result)
            {
                if (kvp.Value is JsonElement element)
                {
                    if (element.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var itemElement in element.EnumerateArray())
                        {
                            var item = ParseSingleItem(itemElement, contentTypeName, titleFieldName, thumbnailFieldName);
                            if (item != null)
                            {
                                items.Add(item);
                            }
                        }
                        break;
                    }
                    else if (element.ValueKind == JsonValueKind.Object)
                    {
                        // Check if this object has an items array
                        if (element.TryGetProperty("items", out var itemsElement) &&
                            itemsElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var itemElement in itemsElement.EnumerateArray())
                            {
                                var item = ParseSingleItem(itemElement, contentTypeName, titleFieldName, thumbnailFieldName);
                                if (item != null)
                                {
                                    items.Add(item);
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing content items");
        }

        return items;
    }

    private ExternalContentItem? ParseSingleItem(
        JsonElement element,
        string contentTypeName,
        string? titleFieldName = null,
        string? thumbnailFieldName = null)
    {
        try
        {
            var item = new ExternalContentItem
            {
                ContentType = contentTypeName,
                RawData = JsonDocument.Parse(element.GetRawText())
            };

            // Try to extract common fields
            if (element.TryGetProperty("id", out var idProp))
            {
                item.Id = idProp.GetString() ?? Guid.NewGuid().ToString();
            }
            else if (element.TryGetProperty("_id", out var _idProp))
            {
                item.Id = _idProp.GetString() ?? Guid.NewGuid().ToString();
            }
            else if (element.TryGetProperty("contentLink", out var contentLinkProp) &&
                     contentLinkProp.TryGetProperty("id", out var clIdProp))
            {
                item.Id = clIdProp.GetString() ?? Guid.NewGuid().ToString();
            }

            // Try to get title - use configured field name first, then fall back to common names
            var titleProps = !string.IsNullOrEmpty(titleFieldName)
                ? new[] { titleFieldName }
                : new[] { "name", "title", "Name", "Title", "displayName", "DisplayName" };

            foreach (var prop in titleProps)
            {
                if (TryGetNestedProperty(element, prop, out var titleValue) &&
                    titleValue.ValueKind == JsonValueKind.String)
                {
                    item.Title = titleValue.GetString() ?? string.Empty;
                    break;
                }
            }

            // Try to get thumbnail - use configured field name first, then fall back to common names
            var thumbnailProps = !string.IsNullOrEmpty(thumbnailFieldName)
                ? new[] { thumbnailFieldName }
                : new[] { "thumbnail", "image", "thumbnailUrl", "imageUrl", "Thumbnail", "Image" };

            foreach (var prop in thumbnailProps)
            {
                if (TryGetNestedProperty(element, prop, out var thumbProp))
                {
                    if (thumbProp.ValueKind == JsonValueKind.String)
                    {
                        item.ThumbnailUrl = thumbProp.GetString();
                        break;
                    }
                    else if (thumbProp.ValueKind == JsonValueKind.Object &&
                             thumbProp.TryGetProperty("url", out var urlProp))
                    {
                        item.ThumbnailUrl = urlProp.GetString();
                        break;
                    }
                }
            }

            // Convert to dictionary for Mustache
            item.Data = JsonElementToDictionary(element);

            return item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing single content item");
            return null;
        }
    }

    /// <summary>
    /// Tries to get a property value, supporting nested paths like "metadata.title".
    /// </summary>
    private static bool TryGetNestedProperty(JsonElement element, string path, out JsonElement value)
    {
        value = default;
        var parts = path.Split('.');
        var current = element;

        foreach (var part in parts)
        {
            if (!current.TryGetProperty(part, out current))
            {
                return false;
            }
        }

        value = current;
        return true;
    }

    private Dictionary<string, object> JsonElementToDictionary(JsonElement element)
    {
        var dict = new Dictionary<string, object>();

        if (element.ValueKind != JsonValueKind.Object)
        {
            return dict;
        }

        foreach (var property in element.EnumerateObject())
        {
            dict[property.Name] = ConvertJsonElement(property.Value);
        }

        return dict;
    }

    private object ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => string.Empty,
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToList(),
            JsonValueKind.Object => JsonElementToDictionary(element),
            _ => element.GetRawText()
        };
    }
}
