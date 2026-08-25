using BlackjackApi.Engine;
using BlackjackApi.Models;

namespace BlackjackApi.Dtos
{
    public record DealerDto(List<CardDto> Cards, int? Score, bool HoleCardHidden)
    {
        public static DealerDto From(GameEngine engine, Dealer d) => new(
            d.HoleCardHidden && d.Hand.Cards.Count > 0
                ? new List<CardDto> { CardDto.From(d.Hand.Cards[0]) } // hide the hole card
                : d.Hand.Cards.Select(CardDto.From).ToList(),
            d.HoleCardHidden ? null : engine.GetHandScore(d.Hand),
            d.HoleCardHidden);
    }
}
