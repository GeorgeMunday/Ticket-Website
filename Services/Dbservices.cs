using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace tutorial.Services
{
    public class DbService
    {
        private readonly string _connectionString;
        private readonly Random _rng = new Random();

        public DbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool TestConnection()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error: " + ex);
                return false;
            }
        }

        public async Task<(bool Success, int UserId, int RoomId)> SignInAsync(string email, string password)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
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
                    return (false, 0, 0);

                int userId = reader.GetInt32(0);
                int roomId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                return (true, userId, roomId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sign-in error: " + ex);
                return (false, 0, 0);
            }
        }

        public async Task<(bool Success, int UserId)> SignUpAsync(string username, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                Console.WriteLine("Passwords do not match.");
                return (false, 0);
            }

            int id = _rng.Next(10000, 100000);

            try
            {
                using var connection = new SqliteConnection(_connectionString);
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
                return (true, id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sign-up error: " + ex);
                return (false, 0);
            }
        }
    }
}
