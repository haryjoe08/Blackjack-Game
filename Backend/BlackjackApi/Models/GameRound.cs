namespace BlackjackApi.Models
{

    public class GameRound
    {
        public int RoundNumber { get; set; }
        public List<ActionType> ActionsTaken { get; set; } = new();
        public Dictionary<int, HandResult> ResultsByPlayerId { get; set; } = new();
    }
}