using src.Services;

HttpClient client = new HttpClient();
LotteryApiService apiService = new LotteryApiService(client);
DatabaseService dbService = new DatabaseService("server=localhost;user=root;password=jayjay55;database=mass_loto_db");

// Ensure database connection
if (!dbService.TestConnection())
{
    Console.WriteLine("Failed to connect to the database.");
    return;
}
Console.WriteLine("Database connection successful...");

// Fetch games from the API
await apiService.GetGamesAsync();

// Insert games and their prize tiers into the database
foreach (var game in apiService.Games)
{
    if (!dbService.CheckIfGameExists(game.MassGameId))
    {
        Console.WriteLine($"Inserting {game.GameName} into the database...");
        dbService.InsertGameWithPrizes(game);
    }
}

Console.WriteLine("Script completed successfully!");