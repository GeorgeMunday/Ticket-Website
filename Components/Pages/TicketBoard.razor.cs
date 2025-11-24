using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using tutorial.Services;
using Microsoft.Data.Sqlite;
namespace tutorial.Components.Pages
{
    public partial class TicketBoard : ComponentBase
    {
        protected bool authState;
        protected bool redirectStarted = false;
        protected string newRoom = "";
        protected string newRoomPassword = "";

        Random rng = new Random();
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

        public void EnterRoom(string roomName, string roomPassword)
        {
            
        }

        private async Task CreateRoom()
        {
            try
            {
                int roomId = rng.Next(10000, 100000);

                string connectionString = @"Data Source=c:\Users\geoge\OneDrive\Desktop\dbs\tutorial.db";
                SqliteConnection connection = new SqliteConnection(connectionString);
                connection.Open();

                using SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO Rooms (Id, Code, Password) VALUES (@id, @code, @password)";
                cmd.Parameters.AddWithValue("@id", roomId);
                cmd.Parameters.AddWithValue("@code", newRoom);  
                cmd.Parameters.AddWithValue("@password", newRoomPassword); 

                await cmd.ExecuteNonQueryAsync();

                newRoom = "";
                newRoomPassword = "";

                Console.WriteLine("Room created");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}