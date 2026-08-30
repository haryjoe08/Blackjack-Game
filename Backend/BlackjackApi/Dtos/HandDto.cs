using BlackjackApi.Engine;
using BlackjackApi.Models;

namespace BlackjackApi.Dtos
{
    public record HandDto(List<CardDto> Cards, int Score, bool IsBusted, bool IsBlackjack, bool IsSoft)
    {
        public static HandDto From(GameEngine engine, Hand hand) => new(
            hand.Cards
                .Select(card => CardDto.From(card))
                .ToList(),
            engine.GetHandScore(hand),
            engine.IsHandBusted(hand),
            engine.IsHandBlackjack(hand),
            engine.IsHandSoft(hand));
    }
}