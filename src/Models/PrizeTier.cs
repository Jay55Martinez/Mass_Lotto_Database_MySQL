namespace src.Models;
    public class PrizeTier
    {
        public int PrizeTierId { get; set; }
        public int MassGameId { get; set; }
        public int TierNumber { get; set; }
        public int PrizeAmount { get; set; }
        public int TotalPrizes { get; set; }
        public int PaidPrizes { get; set; }
        public int PrizesRemaining { get; set; }
        public string PrizeDescription { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        
        public override string ToString()
        {
            return $"Tier {TierNumber}: {PrizeDescription} - Amount: {PrizeAmount}, Total: {TotalPrizes}, Paid: {PaidPrizes}, Remaining: {PrizesRemaining}, Type: {Type}";
        }
    }