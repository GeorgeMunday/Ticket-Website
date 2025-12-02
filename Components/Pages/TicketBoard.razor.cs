using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tutorial.Services;

namespace tutorial.Components.Pages
{
    public partial class TicketBoard : ComponentBase
    {
        [Inject] public HeaderService? HeaderService { get; set; }
        [Inject] public AuthService? AuthService { get; set; }
        [Inject] public NavigationManager? NavManager { get; set; }
        [Inject] public DbService? DbService { get; set; }

        protected bool authState;
        protected bool redirectStarted = false;
        protected string newRoom = "";
        protected string newRoomPassword = "";

        // FIXED: Use DbService.Ticket
        public List<DbService.Ticket> RoomTickets { get; set; } = new();

        protected override void OnInitialized()
        {
            if (HeaderService != null)
                HeaderService.Heading = "Ticket Board";

            authState = AuthService?.AuthState ?? false;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !authState && !redirectStarted)
            {
                redirectStarted = true;
                await Task.Delay(1000);
                NavManager!.NavigateTo("/");
                return;
            }

            if (firstRender && authState)
            {
                await CheckExistingRoomAssignment();
                await FetchRoomTickets();
            }
        }

        private async Task CheckExistingRoomAssignment()
        {
            if (AuthService!.UserId == 0)
                return;

            int roomId = await DbService!.GetUserRoomIdAsync(AuthService.UserId);
            if (roomId != 0)
                AuthService.RoomId = roomId;
        }

        public async Task EnterRoom()
        {
            int roomId = await DbService!.EnterRoomAsync(AuthService!.UserId, newRoom, newRoomPassword);

            if (roomId != 0)
            {
                AuthService.RoomId = roomId;
                Console.WriteLine("Room entered successfully.");
            }
            else
            {
                Console.WriteLine("Room not found.");
            }
        }

        private async Task CreateRoom()
        {
            int roomId = await DbService!.CreateRoomAsync(AuthService!.UserId, newRoom, newRoomPassword);
            AuthService.RoomId = roomId;
            newRoom = "";
            newRoomPassword = "";
            Console.WriteLine("Room created and assigned.");
        }

        public async Task FetchRoomTickets()
        {
            RoomTickets = await DbService!.FetchRoomTicketsAsync(AuthService!.RoomId);
            StateHasChanged();
        }

        void TicketClick(int ticketId)
        {
            NavManager!.NavigateTo($"/modifyticket/{ticketId}");
        }
    }
}
