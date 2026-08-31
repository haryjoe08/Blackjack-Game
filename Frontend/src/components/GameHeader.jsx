export default function GameHeader({ state, muted, onToggleMute, onNewSession }) {
  return (
    <header className="w-full flex justify-between items-center flex-wrap gap-3">
      <div className="flex items-center gap-3">
        <h1 className="text-3xl tracking-widest uppercase text-amber-300 m-0">Blackjack</h1>
        <button
          onClick={onToggleMute}
          title={muted ? 'Nyalakan suara' : 'Matikan suara'}
          className="w-9 h-9 flex items-center justify-center rounded-full bg-black/25 hover:bg-black/40 text-amber-300 transition-colors"
        >
          {muted ? '\u{1F507}' : '\u{1F50A}'}
        </button>
      </div>

      <div className="flex items-center gap-4 text-sm bg-black/25 px-3.5 py-2 rounded-lg">
        <span>{state.name}</span>
        <span>Balance: {state.balance}</span>
        <span>Bet: {state.currentBet}</span>
        <span>Remaining Cards: {state.remainingCards}</span>
        <button
          onClick={onNewSession}
          className="px-3 py-1.5 rounded-full bg-red-700 hover:bg-red-600 text-white text-xs font-semibold transition-colors"
        >
          New Session
        </button>
      </div>
    </header>
  )
}
