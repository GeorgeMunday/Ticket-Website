using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using tutorial.Services;

namespace tutorial.Components.Pages
{
    public partial class AddTicket : ComponentBase
    {
        protected bool authState;
        protected bool redirectStarted = false;

        [Inject] public HeaderService? HeaderService { get; set; }
        [Inject] public AuthService? AuthService { get; set; }
        [Inject] public NavigationManager? NavManager { get; set; }

        protected override void OnInitialized()
        {
            HeaderService.Heading = "Add Ticket";
            authState = AuthService.AuthState;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !authState && !redirectStarted)
            {
                redirectStarted = true;
                await Task.Delay(1000);
                NavManager.NavigateTo("/");
            }
        }
    }
}