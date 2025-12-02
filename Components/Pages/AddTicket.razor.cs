using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using tutorial.Services;
using Microsoft.Data.Sqlite;

namespace tutorial.Components.Pages
{
    public partial class AddTicket : ComponentBase
    {
        protected bool authState;
        protected bool redirectStarted = false;
        protected string TicketName = "";
        protected string TicketId = "";
        protected string Description = "";
        protected int Priority = 0;
        protected int EstimatedTime = 0;

        [Inject] public HeaderService? HeaderService { get; set; }
        [Inject] public AuthService? AuthService { get; set; }
        [Inject] public NavigationManager? NavManager { get; set; }
        [Inject] public DbService? DbService { get; set; }
        protected override void OnInitialized()
        {
            HeaderService!.Heading = "Add Ticket";            
            authState = AuthService!.AuthState;
            TicketId = new Random().Next(100000, 999999).ToString();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !authState && !redirectStarted)
            {
                redirectStarted = true;
                await Task.Delay(1000);
                NavManager?.NavigateTo("/");
            }
        }

        public async Task SubmitTicket()
        {

        var success = await DbService!.SubmitTicket(
            TicketName,
            Description,
            Priority,
            EstimatedTime,
            TicketId,
            AuthService?.RoomId,
            AuthService?.UserId
        );

        if (success)
        {
            NavManager?.NavigateTo("/ticketboard");
        }
        }
    }
}
