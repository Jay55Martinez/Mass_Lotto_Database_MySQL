using src.Services;
using Microsoft.Extensions.Configuration;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Get connection string from environment variable or appsettings.json
string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
    ?? configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not found. Please set DB_CONNECTION_STRING environment variable.");

HttpClient client = new HttpClient();
LotteryApiService apiService = new LotteryApiService(client);
DatabaseService dbService = new DatabaseService(connectionString);

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