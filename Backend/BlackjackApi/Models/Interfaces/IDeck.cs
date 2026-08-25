using BlackjackApi.Models;

namespace BlackjackApi.Interfaces
{
    /// <summary>
    /// DEVIATION FROM DIAGRAM: RemainingCard gained an explicit Deck
    /// parameter, for the same reason as IBetting.PlaceBet above - there's no
    /// way to answer "how many cards remain" without knowing which deck.
    /// </summary>
    public interface IDeck
    {
        void ShuffleDeck(Deck deck);
        int RemainingCard(Deck deck);
    }
}