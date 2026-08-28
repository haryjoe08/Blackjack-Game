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
        int MinBet,
        int MaxBet,
        bool IsGameOver,
        bool CanOfferInsurance,
        string? LastMessage);
}
