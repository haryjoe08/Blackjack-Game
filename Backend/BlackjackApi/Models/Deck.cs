namespace BlackjackApi.Models
{
    public class Deck
    {
        public Stack<Card> Cards { get; set; } = new Stack<Card>();
    }
}
