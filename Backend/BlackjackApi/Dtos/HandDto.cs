using BlackjackApi.Engine;
using BlackjackApi.Models;

namespace BlackjackApi.Dtos
{
    public record HandDto(List<CardDto> Cards, int Score, bool IsBusted, bool IsBlackjack, bool IsSoft)
    {
        // Needs the engine now: Hand itself no longer knows how to compute
        // its own score/bust/blackjack/soft state (see GameEngine's
        // "anemic model" note) - those calculations moved there.
        public static HandDto From(GameEngine engine, Hand h) => new(
            h.Cards
                .Select(card => CardDto.From(card))
                .ToList(),
            engine.GetHandScore(h),
            engine.IsHandBusted(h),
            engine.IsHandBlackjack(h),
            engine.IsHandSoft(h));
    }
}