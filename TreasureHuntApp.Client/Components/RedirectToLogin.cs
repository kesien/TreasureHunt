using Microsoft.AspNetCore.Components;

namespace TreasureHuntApp.Client.Components;

public class RedirectToLogin : ComponentBase
{
    [Inject] NavigationManager Navigation { get; set; } = null!;

    protected override void OnInitialized()
    {
        Navigation.NavigateTo("/admin/login");
    }
}
