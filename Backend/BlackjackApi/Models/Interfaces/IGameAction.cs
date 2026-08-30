using BlackjackApi.Models.Enums;
namespace BlackjackApi.Models.Interfaces
{
    public interface IGameAction
    {
        GameActionResult PerformAction(Player player, ActionType action, Deck deck, Hand h, Dealer dealer);
    }
}