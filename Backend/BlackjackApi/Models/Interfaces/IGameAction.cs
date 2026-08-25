using BlackjackApi.Models;

namespace BlackjackApi.Interfaces
{
    /// <summary>
    /// DEVIATION FROM DIAGRAM: added a Dealer parameter. The diagram's
    /// original signature (Player, ActionType, Deck, Hand only) can't support
    /// the Insurance action, which genuinely needs to see the dealer's hand
    /// to decide the payout. Rather than keep a diagram-exact signature that
    /// silently can't do everything PerformAction needs to do (and fake it
    /// with a throwing stub), the interface itself was corrected to carry
    /// what every action actually needs.
    /// </summary>
    public interface IGameAction
    {
        ActionResult PerformAction(Player p, ActionType action, Deck deck, Hand h, Dealer dealer);
    }
}