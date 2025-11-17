using MySql.Data.MySqlClient;
using src.Models;
using System.Data;

namespace src.Services;
public class DatabaseService
{
    // private fields
    private readonly string _connectionString;

    // constructor
    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Tests the database connection
    public bool TestConnection()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return true;
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Database connection error: {ex.Message}");
            return false;
        }
    }

    // Insert a game into the database
    public void InsertGame(Game game)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        string query = @"
            INSERT INTO games 
            (massGameId, gameName, gameIdentifier, startDate, ticketCost, odds, amountPrinted)
            VALUES (@massGameId, @gameName, @gameIdentifier, @startDate, @ticketCost, @odds, @amountPrinted);
            SELECT LAST_INSERT_ID();";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@massGameId", game.MassGameId);
        command.Parameters.AddWithValue("@gameName", game.GameName);
        command.Parameters.AddWithValue("@gameIdentifier", game.GameIdentifier);
        command.Parameters.AddWithValue("@startDate", game.StartDate);
        command.Parameters.AddWithValue("@ticketCost", game.TicketCost);
        command.Parameters.AddWithValue("@odds", game.Odds ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@amountPrinted", game.AmountPrinted ?? (object)DBNull.Value);
        command.ExecuteScalar();
    }

    // Insert a prize tier into the database
    public void InsertPrizeTier(PrizeTier prizeTier)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        string query = @"
                INSERT INTO prizeTiers 
                (massGameId, tierNumber, prizeAmount, totalPrizes, paidPrizes, 
                 prizesRemaining, prizeDescription, typeOfWin)
                VALUES (@massGameId, @tierNumber, @prizeAmount, @totalPrizes, 
                        @paidPrizes, @prizesRemaining, @prizeDescription, @typeOfWin)";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@massGameId", prizeTier.MassGameId);
        command.Parameters.AddWithValue("@tierNumber", prizeTier.TierNumber);
        command.Parameters.AddWithValue("@prizeAmount", prizeTier.PrizeAmount);
        command.Parameters.AddWithValue("@totalPrizes", prizeTier.TotalPrizes);
        command.Parameters.AddWithValue("@paidPrizes", prizeTier.PaidPrizes);
        command.Parameters.AddWithValue("@prizesRemaining", prizeTier.PrizesRemaining);
        command.Parameters.AddWithValue("@prizeDescription", prizeTier.PrizeDescription);
        command.Parameters.AddWithValue("@typeOfWin", prizeTier.Type);

        command.ExecuteNonQuery();
    }

    // Insert game with all prize tiers (transaction)
    public void InsertGameWithPrizes(Game game)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Insert game with specified massGameId
            string gameQuery = @"
                INSERT INTO games 
                (massGameId, gameName, gameIdentifier, startDate, ticketCost, odds, amountPrinted)
                VALUES (@massGameId, @gameName, @gameIdentifier, @startDate, @ticketCost, @odds, @amountPrinted)";

            using (var command = new MySqlCommand(gameQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@massGameId", game.MassGameId);
                command.Parameters.AddWithValue("@gameName", game.GameName);
                command.Parameters.AddWithValue("@gameIdentifier", game.GameIdentifier);
                command.Parameters.AddWithValue("@startDate", game.StartDate);
                command.Parameters.AddWithValue("@ticketCost", game.TicketCost);
                command.Parameters.AddWithValue("@odds", game.Odds ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@amountPrinted", game.AmountPrinted ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
            }

            // Insert prize tiers
            string prizeQuery = @"
                INSERT INTO prizeTiers 
                (massGameId, tierNumber, prizeAmount, totalPrizes, paidPrizes, 
                    prizesRemaining, prizeDescription, typeOfWin)
                VALUES (@massGameId, @tierNumber, @prizeAmount, @totalPrizes, 
                        @paidPrizes, @prizesRemaining, @prizeDescription, @typeOfWin)";

            foreach (var prize in game.PrizeTiers)
            {
                using var command = new MySqlCommand(prizeQuery, connection, transaction);
                command.Parameters.AddWithValue("@massGameId", game.MassGameId);
                command.Parameters.AddWithValue("@tierNumber", prize.TierNumber);
                command.Parameters.AddWithValue("@prizeAmount", prize.PrizeAmount);
                command.Parameters.AddWithValue("@totalPrizes", prize.TotalPrizes);
                command.Parameters.AddWithValue("@paidPrizes", prize.PaidPrizes);
                command.Parameters.AddWithValue("@prizesRemaining", prize.PrizesRemaining);
                command.Parameters.AddWithValue("@prizeDescription", prize.PrizeDescription);
                command.Parameters.AddWithValue("@typeOfWin", prize.Type);

                command.ExecuteNonQuery();
            }

            transaction.Commit();
            Console.WriteLine($"Successfully inserted game '{game.GameName}' (ID: {game.MassGameId}) with {game.PrizeTiers.Count} prize tiers");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inserting game: {ex.Message}");
            try
            {
                transaction.Rollback();
                Console.WriteLine($"Transaction rolled back due to error: {ex.Message}");
            }
            catch (Exception rollbackEx)
            {
                Console.WriteLine($"Error during rollback: {rollbackEx.Message}");
            }
            throw;
        }
    }
    
    // Check if a game already exists in the database
    public bool CheckIfGameExists(int massGameId)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        string query = "SELECT COUNT(*) FROM games WHERE massGameId = @massGameId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@massGameId", massGameId);

        long count = (long)command.ExecuteScalar();
        return count > 0;
    }
}