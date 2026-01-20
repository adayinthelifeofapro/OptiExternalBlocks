using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiExternalBlocks.Blocks;
using OptiExternalBlocks.Common;
using OptiExternalBlocks.Features.ExternalContent.Models;
using OptiExternalBlocks.Features.ExternalContent.Services.Abstractions;
using OptiExternalBlocks.Features.Templates.Services.Abstractions;

namespace OptiExternalBlocks.Controllers;

/// <summary>
/// API controller for external content operations used by the editor interface.
/// </summary>
[ApiController]
[Route("api/optiexternalblocks")]
[Authorize(Policy = OptiExternalBlocksConstants.AuthorizationPolicy)]
public class ExternalContentApiController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IGraphApiClient _graphApiClient;
    private readonly ITemplateRenderingService _renderingService;
    private readonly IContentRepository _contentRepository;

    public ExternalContentApiController(
        ITemplateService templateService,
        IGraphApiClient graphApiClient,
        ITemplateRenderingService renderingService,
        IContentRepository contentRepository)
    {
        _templateService = templateService;
        _graphApiClient = graphApiClient;
        _renderingService = renderingService;
        _contentRepository = contentRepository;
    }

    /// <summary>
    /// Get all active templates for the content type selector.
    /// </summary>
    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _templateService.GetAllTemplatesAsync();
        var activeTemplates = templates
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.DisplayName)
            .Select(t => new
            {
                t.Id,
                t.ContentTypeName,
                t.DisplayName,
                t.Description,
                t.IconClass
            });

        return Ok(activeTemplates);
    }

    /// <summary>
    /// Search for external content items.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] Guid templateId,
        [FromQuery] string? query = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? language = null)
    {
        var request = new ExternalContentSearchRequest
        {
            TemplateId = templateId,
            SearchQuery = query,
            Page = page,
            PageSize = pageSize,
            Language = language
        };

        var response = await _graphApiClient.SearchContentAsync(request);

        return Ok(new
        {
            items = response.Items.Select(i => new
            {
                i.Id,
                i.ContentType,
                i.Title,
                i.ThumbnailUrl
            }),
            response.TotalCount,
            response.Page,
            response.PageSize,
            response.HasMorePages
        });
    }

    /// <summary>
    /// Get a specific content item by ID.
    /// </summary>
    [HttpGet("content/{templateId}/{externalId}")]
    public async Task<IActionResult> GetContent(Guid templateId, string externalId)
    {
        var content = await _graphApiClient.GetContentByIdAsync(templateId, externalId);

        if (content == null)
        {
            return NotFound(new { message = "Content not found" });
        }

        return Ok(new
        {
            content.Id,
            content.ContentType,
            content.Title,
            content.ThumbnailUrl
        });
    }

    /// <summary>
    /// Render a preview of external content for the editor.
    /// </summary>
    [HttpGet("preview/{templateId}/{externalId}")]
    public async Task<IActionResult> GetPreview(Guid templateId, string externalId)
    {
        var content = await _graphApiClient.GetContentByIdAsync(templateId, externalId);

        if (content == null)
        {
            return NotFound(new { message = "Content not found" });
        }

        var html = await _renderingService.RenderEditModeAsync(templateId, content);

        return Ok(new
        {
            content.Id,
            content.Title,
            html
        });
    }

    /// <summary>
    /// Create an ExternalContentBlock for drag and drop operations.
    /// </summary>
    [HttpPost("createblock")]
    public async Task<IActionResult> CreateBlock([FromBody] CreateBlockRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ExternalId) || request.TemplateId == Guid.Empty)
            {
                return BadRequest(new { message = "ExternalId and TemplateId are required" });
            }

            // Verify the external content exists
            var content = await _graphApiClient.GetContentByIdAsync(request.TemplateId, request.ExternalId);
            if (content == null)
            {
                return NotFound(new { message = "External content not found" });
            }

            // Create a new ExternalContentBlock
            var block = _contentRepository.GetDefault<ExternalContentBlock>(ContentReference.GlobalBlockFolder);

            // Set the block properties
            block.ExternalContentId = request.ExternalId;
            block.TemplateId = request.TemplateId;
            block.CachedTitle = content.Title;

            // Cast to IContent to set the Name property (required for saving)
            var blockContent = block as IContent;
            if (blockContent == null)
            {
                return StatusCode(500, new { message = "Failed to cast block to IContent" });
            }

            // Generate a unique name for the block - ensure we have a valid name
            var title = !string.IsNullOrWhiteSpace(content.Title) ? content.Title : "External Content";
            var blockName = $"{title} - {DateTime.UtcNow:yyyyMMddHHmmss}";
            blockContent.Name = blockName;

            // Save the block
            var contentReference = _contentRepository.Save(
                blockContent,
                SaveAction.Publish,
                AccessLevel.NoAccess);

            return Ok(new
            {
                contentLink = contentReference.ToString(),
                name = content.Title,
                typeIdentifier = "episerver.core.blockdata"
            });
        }
        catch (Exception ex)
        {
            // Log the error and return a proper error response
            return StatusCode(500, new { message = "Failed to create block", error = ex.Message });
        }
    }
}

public class CreateBlockRequest
{
    public string ExternalId { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }
}
