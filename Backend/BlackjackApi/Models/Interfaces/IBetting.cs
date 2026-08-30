namespace BlackjackApi.Models.Interfaces
{
    public interface IBetting
    {
        bool PlaceBet(Player player, Table table, int amount);
        bool IsValidBet(Table table, int amount);
    }
}