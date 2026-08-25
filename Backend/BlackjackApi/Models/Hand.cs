namespace BlackjackApi.Models
{
    public class Hand
    {
        public List<Card> Cards { get; set; } = new List<Card>();

        // True when the hand is finished and can no longer take actions.
        public bool IsFinished { get; set; }

        // True when the hand is surrendered.
        public bool IsSurrendered { get; set; }
    }
}