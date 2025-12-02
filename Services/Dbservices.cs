using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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

        // -----------------------------
        //  INTERNAL TICKET MODEL HERE
        // -----------------------------
        public class Ticket
        {
            public int Id { get; set; }
            public string TicketName { get; set; } = "";
            public string Description { get; set; } = "";
            public int Priority { get; set; }
            public int EstimatedTime { get; set; }
            public int RoomId { get; set; }
            public int UserId { get; set; }
            public DateTime CreatedAt { get; set; }
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
                return (false, 0);

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

        public async Task<bool> SubmitTicket(
            string TicketName,
            string Description,
            int Priority,
            int EstimatedTime,
            string TicketId,
            int? RoomId,
            int? UserId
        )
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Tickets 
                    (TicketName, Description, Priority, EstimatedTime, RoomId, UserId, CreatedAt, Id)
                    VALUES 
                    (@ticketname, @description, @priority, @estimated, @roomid, @userid, @createdat, @id);
                ";

                cmd.Parameters.AddWithValue("@ticketname", TicketName);
                cmd.Parameters.AddWithValue("@description", Description);
                cmd.Parameters.AddWithValue("@priority", Priority);
                cmd.Parameters.AddWithValue("@estimated", EstimatedTime);
                cmd.Parameters.AddWithValue("@id", TicketId);
                cmd.Parameters.AddWithValue("@roomid", RoomId);
                cmd.Parameters.AddWithValue("@userid", UserId);
                cmd.Parameters.AddWithValue("@createdat", DateTime.UtcNow);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<int> GetUserRoomIdAsync(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT RoomId FROM Users WHERE Id = @uid";
            cmd.Parameters.AddWithValue("@uid", userId);

            var result = await cmd.ExecuteScalarAsync();
            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
        }

        public async Task<int> EnterRoomAsync(int userId, string roomCode, string password)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT Id 
                FROM Rooms 
                WHERE Code = @code AND Password = @password";

            cmd.Parameters.AddWithValue("@code", roomCode);
            cmd.Parameters.AddWithValue("@password", password);

            int roomId = 0;

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                roomId = reader.GetInt32(0);

            if (roomId != 0)
            {
                using var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = "UPDATE Users SET RoomId = @rid WHERE Id = @uid";
                updateCmd.Parameters.AddWithValue("@rid", roomId);
                updateCmd.Parameters.AddWithValue("@uid", userId);
                await updateCmd.ExecuteNonQueryAsync();
            }

            return roomId;
        }

        public async Task<List<Ticket>> FetchRoomTicketsAsync(int roomId)
        {
            var tickets = new List<Ticket>();

            if (roomId == 0)
                return tickets;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, TicketName, Description, Priority, EstimatedTime, RoomId, UserId, CreatedAt
                FROM Tickets
                WHERE RoomId = @rid
                ORDER BY CreatedAt DESC;";
            cmd.Parameters.AddWithValue("@rid", roomId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var ticket = new Ticket
                {
                    Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    TicketName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Priority = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    EstimatedTime = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    RoomId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    UserId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                    CreatedAt = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7)
                };

                tickets.Add(ticket);
            }

            return tickets;
        }

        public async Task<int> CreateRoomAsync(int userId, string roomCode, string password)
{
    int roomId = _rng.Next(10000, 100000);

    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    using var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO Rooms (Id, Code, Password)
        VALUES (@id, @code, @password)";
    cmd.Parameters.AddWithValue("@id", roomId);
    cmd.Parameters.AddWithValue("@code", roomCode);
    cmd.Parameters.AddWithValue("@password", password);

    await cmd.ExecuteNonQueryAsync();

    using var updateCmd = connection.CreateCommand();
    updateCmd.CommandText = "UPDATE Users SET RoomId = @rid WHERE Id = @uid";
    updateCmd.Parameters.AddWithValue("@rid", roomId);
    updateCmd.Parameters.AddWithValue("@uid", userId);
    await updateCmd.ExecuteNonQueryAsync();

    return roomId;
}

    }
}
