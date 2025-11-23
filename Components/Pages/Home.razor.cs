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
        // Removed unused fields displayAuthState and displayUserState
        public static bool dbState = false;
        string username = "";
        string email = "";
        string password = "";
        string confirmPassword = "";

        Random rng = new Random();

        [Inject] public HeaderService? HeaderService { get; set; }
        [Inject] public AuthService? AuthService { get; set; }

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
                string connectionString = @"Data Source=C:\\Users\\geoge\\tutorial.db";
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
                if (dbState == false)
                {
                    return;
                }

                string connectionString = @"Data Source=C:\\Users\\geoge\\tutorial.db";
                SqliteConnection connection = new SqliteConnection(connectionString);
                connection.Open();
                using SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(1) FROM Users Where Email=@email AND Password=@password";
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);
                var result = await cmd.ExecuteScalarAsync();
                int count = Convert.ToInt32(result);

                if (count == 1)
                {
                    Console.WriteLine("logged in");
                    if (AuthService != null)
                    {
                        AuthService.AuthState = true;
                        AuthService.UserState = email;
                    }
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
                string connectionString = @"Data Source=C:\\Users\\geoge\\tutorial.db";
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

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void LogoutAuthState()
        {
            if (AuthService != null)
                AuthService.AuthState = false;
        }
    }
}
