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

        protected override void OnInitialized()
        {
            HeaderService.Heading = "Add Ticket";
            authState = AuthService.AuthState;
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
            string connectionString = @"Data Source=c:\Users\geoge\OneDrive\Desktop\dbs\tutorial.db";

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO Tickets 
        (TicketName, Description, Priority, EstimatedTime, RoomId, UserId, CreatedAt)
        VALUES 
        (@ticketname, @description, @priority, @estimated, @roomid, @userid, @createdat);
    ";

            cmd.Parameters.AddWithValue("@ticketname", TicketName);
            cmd.Parameters.AddWithValue("@description", Description);
            cmd.Parameters.AddWithValue("@priority", Priority);
            cmd.Parameters.AddWithValue("@estimated", EstimatedTime);
            cmd.Parameters.AddWithValue("@ticketid", TicketId);
            cmd.Parameters.AddWithValue("@roomid", AuthService?.RoomId);
            cmd.Parameters.AddWithValue("@userid", AuthService?.UserId);
            cmd.Parameters.AddWithValue("@createdat", DateTime.UtcNow);
            await cmd.ExecuteNonQueryAsync();
            NavManager?.NavigateTo("/ticketboard");
        }
    }
}
