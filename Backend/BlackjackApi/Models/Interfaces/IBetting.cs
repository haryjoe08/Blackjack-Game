using BlackjackApi.Models;

namespace BlackjackApi.Interfaces
{
    /// <summary>
    /// DEVIATION FROM DIAGRAM: PlaceBet gained explicit Player/Table
    /// parameters. The diagram's original PlaceBet(int amount) has no way to
    /// know which player or table it's betting against - it only ever
    /// "worked" in an earlier design where GameEngine secretly remembered a
    /// hidden "current player/table". A stateless engine has nowhere to hide
    /// that, so the interface now says explicitly what it always actually
    /// needed instead of pretending int amount was ever enough.
    /// </summary>
    public interface IBetting
    {
        bool PlaceBet(Player p, Table table, int amount);
        bool IsValidBet(Table table, int amount);
    }
}