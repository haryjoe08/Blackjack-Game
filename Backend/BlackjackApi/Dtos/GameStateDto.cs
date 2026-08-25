namespace BlackjackApi.Dtos
{
    public record GameStateDto(
        int PlayerId,
        string Name,
        int Balance,
        int CurrentBet,
        List<HandDto> Hands,
        int ActiveHandIndex,
        DealerDto Dealer,
        int RemainingCards,
        int RoundNumber,
        int MinBet,
        int MaxBet,
        bool IsGameOver,
        string? LastMessage);
}
