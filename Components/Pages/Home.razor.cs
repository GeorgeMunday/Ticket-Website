using Microsoft.AspNetCore.Components;

using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;
using tutorial.Services;

namespace tutorial.Components.Pages
{
    public partial class Home : ComponentBase
    {
        bool formState = false;
        public static bool dbState = false;
        string username = "";
        string email = "";
        string password = "";
        string confirmPassword = "";

        Random rng = new Random();

        [Inject] public HeaderService? HeaderService { get; set; }
        [Inject] public AuthService? AuthService { get; set; }
        [Inject] public NavigationManager? NavManager { get; set; }

        protected override void OnInitialized()
        {
            if (HeaderService != null)
                HeaderService.Heading = "Auth";
            TestConnection();
        }

        void ToggleForm()
        {
            formState = !formState;
        }

        private static SqliteConnection TestConnection()
        {
            try
            {
                string connectionString = @"Data Source=c:\Users\geoge\OneDrive\Desktop\dbs\tutorial.db";
                using var connection = new SqliteConnection(connectionString);
                connection.Open();
                dbState = true;
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error" + ex);
                dbState = false;
            }

            return null;
        }

        private async Task AuthHelp()
        {
            if (formState)
            {
                await SignUpService();
            }
            else
            {
                await SignInService();
            }
        }

        private async Task SignInService()
        {
            try
            {
                if (!dbState)
                    return;

                string connectionString = @"Data Source=c:\Users\geoge\OneDrive\Desktop\dbs\tutorial.db";
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT Id, RoomId 
                    FROM Users 
                    WHERE Email=@email AND Password=@password 
                    LIMIT 1";

                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);

                using var reader = await cmd.ExecuteReaderAsync();

                if (reader.Read())
                {
                    int userId = reader.GetInt32(0);
                    int roomId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                    Console.WriteLine($"Logged in. UserId={userId}, RoomId={roomId}");

                    if (AuthService != null)
                    {
                        AuthService.AuthState = true;
                        AuthService.UserState = email;  
                        AuthService.UserId = userId;      
                        AuthService.RoomId = roomId;       
                    }

                    NavManager.NavigateTo("/ticketboard");

                    email = "";
                    password = "";
                }
                else
                {
                    Console.WriteLine("Invalid email or password");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private async Task SignUpService()
        {
            try
            {
                if (password != confirmPassword)
                {
                    return;
                }

                if (dbState == false)
                {
                    return;
                }

                int rand1 = rng.Next(10000, 100000);
                string connectionString = @"Data Source=c:\Users\geoge\OneDrive\Desktop\dbs\tutorial.db";
                SqliteConnection connection = new SqliteConnection(connectionString);
                connection.Open();
                using SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO Users (Id, Username, Email, Password) VAlUES (@id,@username,@email,@password)";
                cmd.Parameters.AddWithValue("@id", rand1);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);
                await cmd.ExecuteNonQueryAsync();
                email = "";
                password = "";
                username = "";
                confirmPassword = "";
                Console.WriteLine("signed up");
                NavManager.NavigateTo("/ticketboard");


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
