namespace BlackjackApi.Models
{
    public class Deck
    {
        public Stack<Card> Cards { get; set; } = new Stack<Card>();

        
        public static Deck CreateStandardDeck()
        {
            var deck = new Deck();
            var ranks = Enum.GetValues<Rank>(); // returns all 13 named members even though some share a value
            var suits = Enum.GetValues<Suit>();

            var freshCards = new List<Card>();
            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    freshCards.Add(new Card(rank, suit));
                }
            }

            // Shuffle before pushing onto the stack so a fresh deck is already randomized.
            var rng = new Random();
            foreach (var card in freshCards.OrderBy(_ => rng.Next()))
            {
                deck.Cards.Push(card);
            }

            return deck;
        }

        public Card DrawCard()
        {
            if (Cards.Count == 0)
            {
                throw new InvalidOperationException("Deck kosong, tidak ada kartu tersisa untuk diambil.");
            }
            return Cards.Pop();
        }
    }
}
