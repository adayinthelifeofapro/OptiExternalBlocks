using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OptiExternalBlocks.Entities;
using OptiExternalBlocks.Features.ExternalContent.Models;
using OptiExternalBlocks.Features.Templates.Services.Abstractions;
using Stubble.Core.Builders;
using Stubble.Core.Interfaces;

namespace OptiExternalBlocks.Features.Templates.Services;

public class TemplateRenderingService : ITemplateRenderingService
{
    private readonly IOptiExternalBlocksDataContext _dataContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TemplateRenderingService> _logger;
    private readonly IStubbleRenderer _stubble;

    public TemplateRenderingService(
        IOptiExternalBlocksDataContext dataContext,
        IMemoryCache cache,
        ILogger<TemplateRenderingService> logger)
    {
        _dataContext = dataContext;
        _cache = cache;
        _logger = logger;
        _stubble = new StubbleBuilder()
            .Configure(settings =>
            {
                settings.SetIgnoreCaseOnKeyLookup(true);
                settings.SetMaxRecursionDepth(10);
            })
            .Build();
    }

    public async Task<string> RenderEditModeAsync(
        Guid templateId,
        ExternalContentItem content,
        CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateAsync(templateId, cancellationToken);
        if (template == null)
        {
            return $"<div class=\"epi-error\">Template not found: {templateId}</div>";
        }

        return RenderWithTemplate(template.EditModeTemplate, content);
    }

    public async Task<string> RenderPublicAsync(
        Guid templateId,
        ExternalContentItem content,
        CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateAsync(templateId, cancellationToken);
        if (template == null)
        {
            return string.Empty;
        }

        return RenderWithTemplate(template.RenderTemplate, content);
    }

    public TemplateValidationResult ValidateTemplate(string template)
    {
        var result = new TemplateValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(template))
        {
            result.IsValid = false;
            result.Errors.Add("Template cannot be empty.");
            return result;
        }

        try
        {
            // Try to parse the template
            var testData = new Dictionary<string, object>
            {
                ["test"] = "value",
                ["items"] = new List<object>()
            };

            _stubble.Render(template, testData);

            // Check for common issues
            var openTags = CountOccurrences(template, "{{");
            var closeTags = CountOccurrences(template, "}}");

            if (openTags != closeTags)
            {
                result.Warnings.Add($"Unmatched mustache tags: {openTags} opening, {closeTags} closing.");
            }

            // Check for unclosed sections
            var sectionStarts = CountOccurrences(template, "{{#");
            var sectionEnds = CountOccurrences(template, "{{/");

            if (sectionStarts != sectionEnds)
            {
                result.Warnings.Add($"Unmatched section tags: {sectionStarts} opening, {sectionEnds} closing.");
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Template parsing error: {ex.Message}");
        }

        return result;
    }

    public string PreviewTemplate(string template, string sampleDataJson)
    {
        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(sampleDataJson);
            if (data == null)
            {
                return "<div class=\"epi-error\">Invalid sample data JSON.</div>";
            }

            // Convert JsonElements to proper types
            var convertedData = ConvertJsonElements(data);

            return _stubble.Render(template, convertedData);
        }
        catch (JsonException ex)
        {
            return $"<div class=\"epi-error\">JSON parsing error: {ex.Message}</div>";
        }
        catch (Exception ex)
        {
            return $"<div class=\"epi-error\">Rendering error: {ex.Message}</div>";
        }
    }

    private string RenderWithTemplate(string template, ExternalContentItem content)
    {
        try
        {
            // Combine content data with some standard helpers
            var data = new Dictionary<string, object>(content.Data)
            {
                ["_id"] = content.Id,
                ["_title"] = content.Title,
                ["_thumbnail"] = content.ThumbnailUrl ?? string.Empty,
                ["_contentType"] = content.ContentType
            };

            return _stubble.Render(template, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template for content {ContentId}", content.Id);
            return $"<div class=\"epi-error\">Rendering error: {ex.Message}</div>";
        }
    }

    private async Task<ExternalContentTemplate?> GetTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"template:{templateId}";

        if (_cache.TryGetValue(cacheKey, out ExternalContentTemplate? template))
        {
            return template;
        }

        template = await _dataContext.Templates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template != null)
        {
            _cache.Set(cacheKey, template, TimeSpan.FromMinutes(10));
        }

        return template;
    }

    private static int CountOccurrences(string source, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }

    private Dictionary<string, object> ConvertJsonElements(Dictionary<string, object> data)
    {
        var result = new Dictionary<string, object>();

        foreach (var kvp in data)
        {
            result[kvp.Key] = ConvertValue(kvp.Value);
        }

        return result;
    }

    private object ConvertValue(object value)
    {
        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => string.Empty,
                JsonValueKind.Array => element.EnumerateArray()
                    .Select(e => ConvertValue(e))
                    .ToList(),
                JsonValueKind.Object => element.EnumerateObject()
                    .ToDictionary(p => p.Name, p => ConvertValue(p.Value)),
                _ => element.GetRawText()
            };
        }

        return value;
    }
}
