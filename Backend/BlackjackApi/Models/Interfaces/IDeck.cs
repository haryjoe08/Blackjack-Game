namespace BlackjackApi.Models.Interfaces
{
    public interface IDeck
    {
        void ShuffleDeck(Deck deck);
        int RemainingCard(Deck deck);
    }
}