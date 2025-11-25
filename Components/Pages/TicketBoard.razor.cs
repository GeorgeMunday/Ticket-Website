using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using tutorial.Services;
using Microsoft.Data.Sqlite;

namespace tutorial.Components.Pages
{
    public partial class TicketBoard : ComponentBase, IDisposable
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



        private const string connectionString =
            @"Data Source=c:\Users\geoge\OneDrive\Desktop\dbs\tutorial.db";

        protected override void OnInitialized()
        {
            if (HeaderService != null)
                HeaderService.Heading = "Ticket Board";

            authState = AuthService?.AuthState ?? false;

            RoomService!.OnChange += StateHasChanged;
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

        public void Dispose()
        {
            RoomService!.OnChange -= StateHasChanged;
        }

        private async Task CheckExistingRoomAssignment()
        {
            if (AuthService!.UserId == 0)
                return;
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT RoomId FROM Users WHERE Id = @uid";
            cmd.Parameters.AddWithValue("@uid", AuthService.UserId);
            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                int roomId = Convert.ToInt32(result);
                if (roomId != 0)
                {
                    AuthService.RoomId = roomId;
                }
            }
        }

        // -----------------------------------------------------------
        // ENTER EXISTING ROOM
        // -----------------------------------------------------------
        public async Task EnterRoom()
        {
            try
            {
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT Id 
                    FROM Rooms 
                    WHERE Code = @code AND Password = @password";

                cmd.Parameters.AddWithValue("@code", newRoom);
                cmd.Parameters.AddWithValue("@password", newRoomPassword);

                int roomId = 0;

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    roomId = reader.GetInt32(0);
                }

                if (roomId == 0)
                {
                    Console.WriteLine("Room not found.");
                    return;
                }
                using var updateCmd = connection.CreateCommand();
                updateCmd.CommandText =
                    "UPDATE Users SET RoomId = @rid WHERE Id = @uid";

                updateCmd.Parameters.AddWithValue("@rid", roomId);
                updateCmd.Parameters.AddWithValue("@uid", AuthService!.UserId);

                await updateCmd.ExecuteNonQueryAsync();
                AuthService.RoomId = roomId;

                Console.WriteLine("Room entered successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // -----------------------------------------------------------
        // CREATE A NEW ROOM
        // -----------------------------------------------------------
        private async Task CreateRoom()
        {
            try
            {
                int roomId = rng.Next(10000, 100000);

                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Rooms (Id, Code, Password)
                    VALUES (@id, @code, @password)";

                cmd.Parameters.AddWithValue("@id", roomId);
                cmd.Parameters.AddWithValue("@code", newRoom);
                cmd.Parameters.AddWithValue("@password", newRoomPassword);

                await cmd.ExecuteNonQueryAsync();
                using var updateCmd = connection.CreateCommand();
                updateCmd.CommandText =
                    "UPDATE Users SET RoomId = @rid WHERE Id = @uid";

                updateCmd.Parameters.AddWithValue("@rid", roomId);
                updateCmd.Parameters.AddWithValue("@uid", AuthService!.UserId);

                await updateCmd.ExecuteNonQueryAsync();
                newRoom = "";
                newRoomPassword = "";

                AuthService.RoomId = roomId;

                Console.WriteLine("Room created and assigned.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    public List<Ticket> RoomTickets { get; set; } = new();

    public async Task FetchRoomTickets()
    {
        if (AuthService!.RoomId == 0)
            return;

        RoomTickets.Clear();

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, TicketName, Description, Priority, EstimatedTime, RoomId, UserId, CreatedAt
            FROM Tickets
            WHERE RoomId = @rid
            ORDER BY CreatedAt DESC;
        ";

        cmd.Parameters.AddWithValue("@rid", AuthService.RoomId);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
    {
        var ticket = new Ticket();

        ticket.Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
        ticket.TicketName = reader.IsDBNull(1) ? "" : reader.GetString(1);
        ticket.Description = reader.IsDBNull(2) ? "" : reader.GetString(2);
        ticket.Priority = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
        ticket.EstimatedTime = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
        ticket.RoomId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
        ticket.UserId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);

        ticket.CreatedAt = reader.IsDBNull(7)
            ? DateTime.MinValue
            : reader.GetDateTime(7);

        RoomTickets.Add(ticket);
    }

        StateHasChanged();
    }
    public class Ticket
    {
        public int Id { get; set; }
        public string TicketName { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public int EstimatedTime { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

        void TicketClick(int ticketId)
        {
            NavManager!.NavigateTo($"/modifyticket/{ticketId}");
        }
    }

}
