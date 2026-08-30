using BlackjackApi.Models.Enums;
namespace BlackjackApi.Models
{
    public class Card
    {
        public Rank Rank { get; }
        public Suit Suit { get; }
        public int Value { get; }

        public Card(Rank rank, Suit suit)
        {
            Rank = rank;
            Suit = suit;
            Value = (int)rank;
        }

    }
}
