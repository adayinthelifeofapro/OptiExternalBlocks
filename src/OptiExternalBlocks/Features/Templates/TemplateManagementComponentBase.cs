using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using OptiExternalBlocks.Features.Templates.Models;
using OptiExternalBlocks.Features.Templates.Services.Abstractions;

namespace OptiExternalBlocks.Features.Templates;

public class TemplateManagementComponentBase : ComponentBase
{
    [Inject]
    protected ITemplateService TemplateService { get; set; } = null!;

    [Inject]
    protected ITemplateRenderingService RenderingService { get; set; } = null!;

    [Inject]
    protected AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

    protected List<TemplateModel> Templates { get; set; } = new();
    protected TemplateModel EditModel { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected bool IsEditing { get; set; }
    protected bool IsSaving { get; set; }
    protected string? SuccessMessage { get; set; }
    protected string? ErrorMessage { get; set; }
    protected bool ShowPreview { get; set; }
    protected string PreviewData { get; set; } = @"{""title"": ""Sample Title"", ""description"": ""Sample description"", ""imageUrl"": ""https://example.com/image.jpg""}";
    protected string PreviewHtml { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadTemplatesAsync();
    }

    protected async Task LoadTemplatesAsync()
    {
        IsLoading = true;
        try
        {
            Templates = await TemplateService.GetAllTemplatesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading templates: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void CreateNew()
    {
        EditModel = new TemplateModel
        {
            IconClass = "epi-iconDocument",
            IsActive = true,
            EditModeTemplate = @"<div class=""external-content-preview"">
    <h3>{{title}}</h3>
    {{#thumbnail}}
    <img src=""{{thumbnail}}"" alt=""{{title}}"" />
    {{/thumbnail}}
    <p>{{description}}</p>
</div>",
            RenderTemplate = @"<article class=""external-content"">
    <h2>{{title}}</h2>
    {{#imageUrl}}
    <img src=""{{imageUrl}}"" alt=""{{title}}"" />
    {{/imageUrl}}
    <div class=""content"">{{description}}</div>
</article>",
            GraphQLQuery = @"query GetContent($searchText: String, $limit: Int = 20, $skip: Int = 0) {
    ContentType(
        where: { _fulltext: { contains: $searchText } }
        limit: $limit
        skip: $skip
    ) {
        items {
            id: _metadata { key }
            name: Name
            description: Description
        }
        total
    }
}"
        };
        IsEditing = true;
        ClearMessages();
    }

    protected void EditTemplate(TemplateModel template)
    {
        EditModel = new TemplateModel
        {
            Id = template.Id,
            ContentTypeName = template.ContentTypeName,
            DisplayName = template.DisplayName,
            Description = template.Description,
            EditModeTemplate = template.EditModeTemplate,
            RenderTemplate = template.RenderTemplate,
            GraphQLQuery = template.GraphQLQuery,
            GraphQLVariables = template.GraphQLVariables,
            IconClass = template.IconClass,
            IsActive = template.IsActive,
            SortOrder = template.SortOrder,
            TitleFieldName = template.TitleFieldName,
            ThumbnailFieldName = template.ThumbnailFieldName
        };
        IsEditing = true;
        ClearMessages();
    }

    protected void CancelEdit()
    {
        IsEditing = false;
        ShowPreview = false;
        EditModel = new TemplateModel();
        ClearMessages();
    }

    protected async Task HandleSaveAsync()
    {
        IsSaving = true;
        ClearMessages();

        try
        {
            // Validate templates
            var editValidation = RenderingService.ValidateTemplate(EditModel.EditModeTemplate);
            var renderValidation = RenderingService.ValidateTemplate(EditModel.RenderTemplate);

            if (!editValidation.IsValid || !renderValidation.IsValid)
            {
                var errors = editValidation.Errors.Concat(renderValidation.Errors);
                ErrorMessage = $"Template validation errors: {string.Join(", ", errors)}";
                return;
            }

            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var username = authState.User?.Identity?.Name ?? "Unknown";

            if (EditModel.Id == Guid.Empty)
            {
                await TemplateService.CreateTemplateAsync(EditModel, username);
                SuccessMessage = "Template created successfully.";
            }
            else
            {
                await TemplateService.UpdateTemplateAsync(EditModel, username);
                SuccessMessage = "Template updated successfully.";
            }

            await LoadTemplatesAsync();
            IsEditing = false;
            ShowPreview = false;
            EditModel = new TemplateModel();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving template: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected async Task DeleteTemplateAsync(Guid id)
    {
        try
        {
            await TemplateService.DeleteTemplateAsync(id);
            SuccessMessage = "Template deleted successfully.";
            await LoadTemplatesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting template: {ex.Message}";
        }
    }

    protected Task ShowPreviewAsync()
    {
        ShowPreview = true;
        return RefreshPreviewAsync();
    }

    protected Task RefreshPreviewAsync()
    {
        PreviewHtml = RenderingService.PreviewTemplate(EditModel.EditModeTemplate, PreviewData);
        return Task.CompletedTask;
    }

    protected void ClearMessages()
    {
        SuccessMessage = null;
        ErrorMessage = null;
    }
}
