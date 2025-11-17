using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using src.Models;
using src.Utils;

namespace src.Services;

public class LotteryApiService
{
    // public fields
    public List<Game> Games { get; set; } = new List<Game>();

    // private fields
    private readonly HttpClient _client;
    private const string Url = "https://www.masslottery.com/api/v1/instant-game-prizes";

    // constructor
    // Inject HttpClient
    public LotteryApiService(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    // Main class method responsible for fetching games from the API
    // makes an initial call to get all games, then calls GetGame for each
    // game to get detailed info
    public async Task GetGamesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine("Fetching data from Mass Lottery API...");

            using var response = await _client.GetAsync(Url);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var jsonResponse = JsonSerializer.Deserialize<List<Game>>(responseBody, options);

            if (jsonResponse is null || jsonResponse.Count == 0)
                throw new InvalidOperationException("No games found from API response.");

            foreach (var game in jsonResponse)
            {
                if (game is null)
                    continue;

                // Skip entries without a valid MassGameId
                if (game.MassGameId == 0)
                {
                    Console.WriteLine("Skipping game with missing or invalid MassGameId.");
                    continue;
                }

                Console.WriteLine($"Fetching details for game ID: {game.MassGameId}");

                try
                {   
                    await GetGame(game.MassGameId.ToString(), cancellationToken);
                }
                catch (KeyNotFoundException)
                {
                    // This can occur when a JSON property expected in GetGame is missing.
                    Console.WriteLine($"Expected JSON property missing for game ID {game.MassGameId}, skipping.");
                }
                catch (Exception ex)
                {
                    // Log and continue with other games instead of failing the entire operation
                    Console.WriteLine($"Error fetching details for game ID {game.MassGameId}: {ex.Message}");
                }
            }

            Console.WriteLine("Data fetching complete.");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"HTTP Request error: {httpEx.Message}");
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"JSON Parsing error: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            throw;
        }
    }
    
    private async Task GetGame(string massGameId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _client.GetAsync(Url + "?gameID=" + massGameId, cancellationToken);

            response.EnsureSuccessStatusCode();
            
            var jsonResponseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var game = JsonSerializer.Deserialize<Game>(jsonResponseBody, options);

            // JsonDocument is used here for more complex parsing needs (to get the amout of tickets printed)
            using JsonDocument doc = JsonDocument.Parse(jsonResponseBody);

            if (game is null || doc is null)
                throw new InvalidOperationException("No games found from API response.");

            // Try to extract printed tickets from either prizeTierInfo or secondChanceInfo
            string? prizeText = null;
            JsonElement? sourceElement = null;
            
            if (doc.RootElement.TryGetProperty("prizeTierInfo", out var prizeTierInfoElement))
            {
                sourceElement = prizeTierInfoElement;
            }
            else if (doc.RootElement.TryGetProperty("secondChanceInfo", out var secondChanceInfoElement))
            {
                sourceElement = secondChanceInfoElement;
            }

            if (sourceElement.HasValue)
            {
                var contentArray = sourceElement.Value
                    .GetProperty("text")
                    .GetProperty("content")[0]
                    .GetProperty("content");

                foreach (var value in contentArray.EnumerateArray())
                {
                    prizeText = value.GetProperty("value").GetString();
                    var printedTickets = LottoUtils.ExtractPrintedTickets(prizeText ?? string.Empty);
                    
                    if (printedTickets.HasValue)
                    {
                        game.AmountPrinted = printedTickets.Value;
                        break;
                    }
                }
            }

            // Extract the number of printed tickets using the utility method
            // game.AmountPrinted = LottoUtils.ExtractPrintedTickets(prizeText ?? string.Empty);

            // Ensure that all prizetiers are associated with a massGameId
            foreach (var prizeTier in game.PrizeTiers)
            {
                prizeTier.MassGameId = game.MassGameId;
            }


            // Validate that at least one prize tier exists
            if (game.PrizeTiers.Count == 0)
            {
                throw new InvalidOperationException($"No prize tiers found for game ID {game.MassGameId}");
            }

            Games.Add(game);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Request was canceled.");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error while fetching games: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
        }
    }
}