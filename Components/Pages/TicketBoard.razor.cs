using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using tutorial.Services;

namespace tutorial.Components.Pages
{
    public partial class TicketBoard : ComponentBase
    {
        protected bool authState;
        protected bool redirectStarted = false;
        protected string newRoom = "";

        [Inject] public HeaderService? HeaderService { get; set; }
        [Inject] public AuthService? AuthService { get; set; }
        [Inject] public NavigationManager? NavManager { get; set; }
        [Inject] public RoomService? RoomService { get; set; }

        protected override void OnInitialized()
        {
            if (HeaderService != null)
                HeaderService.Heading = "Ticket Board";
            authState = AuthService?.AuthState ?? false;
            if (RoomService != null)
                RoomService.OnChange += StateHasChanged;
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

        public void Dispose()
        {
            if (RoomService != null)
                RoomService.OnChange -= StateHasChanged;
        }

        private void SetRoom()
        {
            if (RoomService != null && !string.IsNullOrWhiteSpace(newRoom))
            {
                RoomService.SetRoom(newRoom.Trim());
                newRoom = string.Empty;
            }
        }
    }
}