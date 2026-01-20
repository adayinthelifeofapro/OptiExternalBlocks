using Microsoft.AspNetCore.Mvc;
using OptiExternalBlocks.Features.ExternalContent.Services.Abstractions;
using OptiExternalBlocks.Features.Templates.Services.Abstractions;

namespace OptiExternalBlocks.Blocks;

/// <summary>
/// View component that renders the external content block.
/// </summary>
[ViewComponent(Name = "ExternalContentBlock")]
public class ExternalContentBlockComponent : ViewComponent
{
    private readonly IGraphApiClient _graphApiClient;
    private readonly ITemplateRenderingService _templateRenderingService;

    public ExternalContentBlockComponent(
        IGraphApiClient graphApiClient,
        ITemplateRenderingService templateRenderingService)
    {
        _graphApiClient = graphApiClient;
        _templateRenderingService = templateRenderingService;
    }

    public async Task<IViewComponentResult> InvokeAsync(ExternalContentBlock block, bool isEditMode = false)
    {
        var model = new ExternalContentBlockViewModel
        {
            CssClass = block.CssClass,
            IsEditMode = isEditMode
        };

        if (string.IsNullOrEmpty(block.ExternalContentId) || block.TemplateId == Guid.Empty)
        {
            model.HasContent = false;
            model.RenderedHtml = isEditMode
                ? "<div class=\"external-content-placeholder\">Select external content...</div>"
                : string.Empty;
            return View("/modules/_protected/OptiExternalBlocks/Views/Shared/Blocks/ExternalContentBlock.cshtml", model);
        }

        try
        {
            var content = await _graphApiClient.GetContentByIdAsync(
                block.TemplateId,
                block.ExternalContentId);

            if (content != null)
            {
                model.HasContent = true;
                model.Title = content.Title;

                if (isEditMode)
                {
                    model.RenderedHtml = await _templateRenderingService.RenderEditModeAsync(
                        block.TemplateId,
                        content);
                }
                else
                {
                    model.RenderedHtml = await _templateRenderingService.RenderPublicAsync(
                        block.TemplateId,
                        content);
                }
            }
            else
            {
                model.HasContent = false;
                model.RenderedHtml = isEditMode
                    ? "<div class=\"external-content-error\">Content not found</div>"
                    : string.Empty;
            }
        }
        catch (Exception ex)
        {
            model.HasContent = false;
            model.RenderedHtml = isEditMode
                ? $"<div class=\"external-content-error\">Error: {ex.Message}</div>"
                : string.Empty;
        }

        return View("/modules/_protected/OptiExternalBlocks/Views/Shared/Blocks/ExternalContentBlock.cshtml", model);
    }
}

public class ExternalContentBlockViewModel
{
    public string? CssClass { get; set; }
    public bool IsEditMode { get; set; }
    public bool HasContent { get; set; }
    public string? Title { get; set; }
    public string RenderedHtml { get; set; } = string.Empty;
}
