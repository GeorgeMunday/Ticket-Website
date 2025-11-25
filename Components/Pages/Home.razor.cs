using Microsoft.AspNetCore.Components;
using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;
using tutorial.Services;

namespace tutorial.Components.Pages
{
    public partial class Home : ComponentBase
    {
        private bool isSignUp = false;
        private bool dbAvailable = false;

        private string username = "";
        private string email = "";
        private string password = "";
        private string confirmPassword = "";

        private readonly Random rng = new Random();
        private const string ConnectionString = @"Data Source=c:\Users\geoge\OneDrive\Desktop\dbs\tutorial.db";

        [Inject] public HeaderService? HeaderService { get; set; }
        [Inject] public AuthService? AuthService { get; set; }
        [Inject] public NavigationManager? NavManager { get; set; }

        protected override void OnInitialized()
        {
            HeaderService!.Heading = "Auth";
            dbAvailable = TestConnection();
        }

        private void ToggleForm()
        {
            isSignUp = !isSignUp;
        }

        private bool TestConnection()
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error: " + ex);
                return false;
            }
        }

        private async Task HandleAuth()
        {
            if (!dbAvailable)
                return;

            if (isSignUp)
                await SignUpAsync();
            else
                await SignInAsync();
        }

        // --------------------------------------------------------------------
        // sign in
        // --------------------------------------------------------------------
        private async Task SignInAsync()
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT Id, RoomId 
                    FROM Users 
                    WHERE Email=@email AND Password=@pw 
                    LIMIT 1";

                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@pw", password);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!reader.Read())
                {
                    Console.WriteLine("Invalid email or password");
                    return;
                }

                int userId = reader.GetInt32(0);
                int roomId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                Console.WriteLine("user:" + userId + "Room id:" + roomId);
                ApplyAuth(userId, email, roomId);

                ClearInputs();
                NavManager!.NavigateTo("/ticketboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sign-in error: " + ex);
            }
        }


        // --------------------------------------------------------------------
        // sign up
        // --------------------------------------------------------------------
        private async Task SignUpAsync()
        {
            try
            {
                if (password != confirmPassword)
                {
                    Console.WriteLine("Passwords do not match.");
                    return;
                }

                int id = rng.Next(10000, 100000);

                using var connection = new SqliteConnection(ConnectionString);
                await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Users (Id, Username, Email, Password, RoomId) 
                    VALUES (@id, @username, @email, @pw, 0)";

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@pw", password);

                await cmd.ExecuteNonQueryAsync();
                ApplyAuth(id, email, 0);

                ClearInputs();
                Console.WriteLine("Signed up successfully");
                NavManager!.NavigateTo("/ticketboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sign-up error: " + ex);
            }
        }

        private void ApplyAuth(int userId, string userEmail, int roomId)
        {
            if (AuthService == null) return;

            AuthService.AuthState = true;
            AuthService.UserId = userId;
            AuthService.UserState = userEmail;
            AuthService.RoomId = roomId;
        }

        private void ClearInputs()
        {
            username = "";
            email = "";
            password = "";
            confirmPassword = "";
        }
    }
}
