namespace BlackjackApi.Models
{
    public class Dealer
    {
        public Hand Hand { get; set; } = new Hand();
        public bool HoleCardHidden { get; set; } = true;
    }
}
