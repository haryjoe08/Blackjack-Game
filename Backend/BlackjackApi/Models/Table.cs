namespace BlackjackApi.Models
{

    public class Table
    {
        public int TableId { get; set; }
        public int MinBet { get; set; }
        public int MaxBet { get; set; }

        public List<GameRound> Rounds { get; set; } = new();
        public Table(int tableId, int minBet, int maxBet)
        {
            TableId = tableId;
            MinBet = minBet;
            MaxBet = maxBet;
        }
    }
}