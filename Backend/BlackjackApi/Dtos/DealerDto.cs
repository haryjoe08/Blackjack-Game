using BlackjackApi.Engine;
using BlackjackApi.Models;

namespace BlackjackApi.Dtos
{
    public record DealerDto(List<CardDto> Cards, int? Score, bool HoleCardHidden)
    {
        public static DealerDto From(GameEngine engine, Dealer dealer) => new(
            dealer.HoleCardHidden && dealer.Hand.Cards.Count > 0
                ? new List<CardDto> { CardDto.From(dealer.Hand.Cards[0]) } 
                : dealer.Hand.Cards.Select(CardDto.From).ToList(),
            dealer.HoleCardHidden ? null : engine.GetHandScore(dealer.Hand),
            dealer.HoleCardHidden);
    }
}
