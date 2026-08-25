namespace BlackjackApi.Models
{
    /// <summary>
    /// DEVIATION FROM DIAGRAM: The diagram uses "ActionResult" as the return type of
    /// IGameAction.PerformAction / GameEngine.PerformAction, but never defines what
    /// ActionResult actually is. It was originally given 7 fields (Success, Message,
    /// Action, HandScore, IsBusted, IsBlackjack, Result) to mirror everything a
    /// caller might want to know about the action just taken.
    ///
    /// Trimmed down to just Success + Message after checking: nothing in the
    /// codebase ever read .Action, .HandScore, .IsBusted, .IsBlackjack, or
    /// .Result - GameSessionService only ever used .Message. Worse, HandScore/
    /// IsBusted/IsBlackjack were being computed here via GameEngine calls and
    /// then immediately discarded, only for BuildState() to compute the exact
    /// same values again moments later when building HandDto. Keeping only
    /// what's actually consumed removes that duplicate work entirely.
    /// </summary>
    public class ActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}