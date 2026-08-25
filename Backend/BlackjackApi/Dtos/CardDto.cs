using BlackjackApi.Models;

namespace BlackjackApi.Dtos
{
    public record CardDto(string Rank, string Suit, int Value)
    {
        public static CardDto From(Card c) => new(c.Rank.ToString(), c.Suit.ToString(), c.Value);
    }

   
}
