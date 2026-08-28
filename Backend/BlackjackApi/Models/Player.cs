using BlackjackApi.Models;

namespace BlackjackApi.Models
{
    public class Player
    {
        private static int _nextId = 1;
        public int PlayerId { get; set; }
        public int CurrentBet { get; set; }
        public string Name { get; set; }
        public Dictionary<ChipType, int> Balance { get; set; } = new();
        public List<Hand> Hands { get; set; } = new() { new Hand() };

        public Player(string name, int balance)
        {
            Name = name;
            PlayerId = _nextId++;
            // Balance is set to White because White = 1;
            // White * balance = balance
            Balance[ChipType.White] = balance;
        }
    }
}