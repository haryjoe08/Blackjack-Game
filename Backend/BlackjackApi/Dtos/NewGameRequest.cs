namespace BlackjackApi.Dtos
{
    public record NewGameRequest(string Name, int StartingBalance, int MinBet, int MaxBet);
}
