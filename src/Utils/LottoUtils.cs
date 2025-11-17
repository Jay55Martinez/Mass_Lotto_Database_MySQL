using System.Text.RegularExpressions;

namespace src.Utils;

public static class LottoUtils
{
    // Extracts the number of printed tickets from the Massachusetts Lottery API description.
    // Searches for patterns like "approximately X tickets" or "approximately X,XXX,XXX tickets"
    // Also handles standalone numbers like "10,340,000" or "3,000,000"
    // The prize structure description string from the API. The number of tickets as an integer,
    // or null if not found or invalid
    /*
    Example Inputs:
    "Prize structure is based on the sale of approximately 18,144,000 tickets. All winners..."
    Output: 18144000

    "Prize structure is based on the sale of approximately 15,120,000 tickets. All winners..."
    Output: 15120000
    
    "Prize structure is based on the sale of approximately 10,080,000 tickets. All winners..."
    Output: 10080000
    
    "10,340,000"
    Output: 10340000
    
    "3,000,000"
    Output: 3000000
    */
    public static int? ExtractPrintedTickets(string description)
    {
        // Return null for null or empty input
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        try
        {
            // First try the full pattern with "approximately" and "tickets"
            string pattern = @"approximately\s+([\d,]+)\s+tickets";
            Match match = Regex.Match(description, pattern, RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                // Extract the captured number and remove commas
                string numberString = match.Groups[1].Value.Replace(",", "");
                
                // Try to parse the cleaned number
                if (int.TryParse(numberString, out int result))
                {
                    // Sanity check: reasonable range for lottery tickets (between 1,000 and 1 billion)
                    if (result >= 1000 && result <= 1_000_000_000)
                    {
                        return result;
                    }
                }
            }
            
            // If the full pattern didn't match, try to extract just a number with commas
            // This handles standalone numbers like "10,340,000"
            pattern = @"^[\s]*([\d,]+)[\s]*$";
            match = Regex.Match(description, pattern);
            
            if (match.Success)
            {
                string numberString = match.Groups[1].Value.Replace(",", "");
                
                if (int.TryParse(numberString, out int result))
                {
                    // Same sanity check
                    if (result >= 1000 && result <= 1_000_000_000)
                    {
                        return result;
                    }
                }
            }
            
            // If no pattern matched, return null
            return null;
        }
        catch (Exception ex)
        {
            // Log the exception if you have logging configured
            // For now, just return null on any error
            Console.WriteLine($"Error extracting printed tickets: {ex.Message}");
            return null;
        }
    }
    
    // Alternative implementation using multiple patterns for even more robustness
    public static int? ExtractPrintedTicketsAdvanced(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        try
        {
            // Try multiple patterns in order of specificity
            string[] patterns = new[]
            {
                @"approximately\s+([\d,]+)\s+tickets",           
                @"sale\s+of\s+approximately\s+([\d,]+)\s+tickets", 
                @"based\s+on\s+(?:the\s+)?sale\s+of\s+approximately\s+([\d,]+)", 
                @"([\d,]+)\s+tickets",
                @"^[\s]*([\d,]+)[\s]*$"  // Standalone number pattern
            };

            foreach (string pattern in patterns)
            {
                Match match = Regex.Match(description, pattern, RegexOptions.IgnoreCase);
                
                if (match.Success)
                {
                    string numberString = match.Groups[1].Value.Replace(",", "");
                    
                    if (int.TryParse(numberString, out int result))
                    {
                        // Validate the number is in a reasonable range
                        if (result >= 1000 && result <= 1_000_000_000)
                        {
                            return result;
                        }
                    }
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting printed tickets: {ex.Message}");
            return null;
        }
    }
}