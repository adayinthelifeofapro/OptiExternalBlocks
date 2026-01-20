using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using OptiExternalBlocks.Features.ExternalContent.Services.Abstractions;
using OptiExternalBlocks.Features.Templates.Models;
using OptiExternalBlocks.Features.Templates.Services.Abstractions;

namespace OptiExternalBlocks.Features.Templates;

public class EndpointManagementComponentBase : ComponentBase
{
    [Inject]
    protected IEndpointService EndpointService { get; set; } = null!;

    [Inject]
    protected IGraphApiClient GraphApiClient { get; set; } = null!;

    [Inject]
    protected AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

    protected List<EndpointModel> Endpoints { get; set; } = new();
    protected EndpointModel EditModel { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected bool IsEditing { get; set; }
    protected bool IsSaving { get; set; }
    protected bool IsTesting { get; set; }
    protected string? SuccessMessage { get; set; }
    protected string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadEndpointsAsync();
    }

    protected async Task LoadEndpointsAsync()
    {
        IsLoading = true;
        try
        {
            Endpoints = await EndpointService.GetAllEndpointsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading endpoints: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void CreateNew()
    {
        EditModel = new EndpointModel
        {
            IsActive = true,
            EndpointUrl = "https://cg.optimizely.com/content/v2"
        };
        IsEditing = true;
        ClearMessages();
    }

    protected void EditEndpoint(EndpointModel endpoint)
    {
        EditModel = new EndpointModel
        {
            Id = endpoint.Id,
            Name = endpoint.Name,
            EndpointUrl = endpoint.EndpointUrl,
            SingleKey = endpoint.SingleKey,
            AppKey = endpoint.AppKey,
            AppSecret = endpoint.AppSecret,
            IsDefault = endpoint.IsDefault,
            IsActive = endpoint.IsActive
        };
        IsEditing = true;
        ClearMessages();
    }

    protected void CancelEdit()
    {
        IsEditing = false;
        EditModel = new EndpointModel();
        ClearMessages();
    }

    protected async Task HandleSaveAsync()
    {
        IsSaving = true;
        ClearMessages();

        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var username = authState.User?.Identity?.Name ?? "Unknown";

            if (EditModel.Id == Guid.Empty)
            {
                await EndpointService.CreateEndpointAsync(EditModel, username);
                SuccessMessage = "Endpoint created successfully.";
            }
            else
            {
                await EndpointService.UpdateEndpointAsync(EditModel, username);
                SuccessMessage = "Endpoint updated successfully.";
            }

            await LoadEndpointsAsync();
            IsEditing = false;
            EditModel = new EndpointModel();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving endpoint: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected async Task DeleteEndpointAsync(Guid id)
    {
        try
        {
            await EndpointService.DeleteEndpointAsync(id);
            SuccessMessage = "Endpoint deleted successfully.";
            await LoadEndpointsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting endpoint: {ex.Message}";
        }
    }

    protected async Task SetDefaultAsync(Guid id)
    {
        try
        {
            await EndpointService.SetDefaultEndpointAsync(id);
            SuccessMessage = "Default endpoint updated.";
            await LoadEndpointsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error setting default: {ex.Message}";
        }
    }

    protected async Task TestConnectionAsync()
    {
        IsTesting = true;
        ClearMessages();

        try
        {
            // Simple introspection query to test connection
            var testQuery = "{ __schema { queryType { name } } }";
            var result = await GraphApiClient.ExecuteQueryAsync(testQuery);

            if (result != null)
            {
                SuccessMessage = "Connection successful!";
            }
            else
            {
                ErrorMessage = "Connection failed. Check the endpoint URL and credentials.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Connection error: {ex.Message}";
        }
        finally
        {
            IsTesting = false;
        }
    }

    protected void ClearMessages()
    {
        SuccessMessage = null;
        ErrorMessage = null;
    }
}
