namespace BlackjackApi.Models
{
    public class Hand
    {
        public List<Card> Cards { get; set; } = new List<Card>();
        public bool IsFinished { get; set; }
        public bool IsSurrendered { get; set; }
        
        public bool InsuranceTaken { get; set; }
    }
}