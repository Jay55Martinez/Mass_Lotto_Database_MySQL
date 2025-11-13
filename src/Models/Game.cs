namespace src.Models;

    public class Game
    {
        public int MassGameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string GameIdentifier { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int TicketCost { get; set; }
        public string Odds { get; set; } = string.Empty;
        public int? AmountPrinted { get; set; }
        public List<PrizeTier> PrizeTiers { get; set; } = new List<PrizeTier>();
        
        public override string ToString()
        {
            return $"{GameName} ({GameIdentifier}) - Starts: {StartDate.ToShortDateString()}, Cost: {TicketCost}, Odds: {Odds}, Printed: {AmountPrinted}, Prize Tiers: {PrizeTiers[1]}";
        }
    }