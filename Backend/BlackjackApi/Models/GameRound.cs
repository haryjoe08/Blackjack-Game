namespace BlackjackApi.Models
{

    public class GameRound
    {
        public int RoundNumber { get; set; }
        public Dictionary<int, HandResult> ResultsByPlayerId { get; set; } = new();
    }
}