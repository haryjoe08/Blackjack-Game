import CardView from './CardView.jsx'

export default function HandView({ hand, index, isActive }) {
  return (
    <div
      className={`flex flex-col items-center gap-2 p-2.5 rounded-lg transition-all duration-300 ${
        isActive ? 'bg-amber-300/10 ring-1 ring-amber-300/30 animate-pulseGlow' : ''
      }`}
    >
      <div className="flex">
        {hand.cards.map((card, i) => (
          <CardView key={i} card={card} />
        ))}
      </div>
      <div className="flex items-center gap-2.5 text-sm">
        <span>Tangan {index + 1}</span>
        <span className="font-bold text-amber-300">
          {hand.score}
          {hand.isSoft && !hand.isBusted ? ' (soft)' : ''}
        </span>
        {hand.isBlackjack && (
          <span className="px-2 py-0.5 rounded text-xs font-bold bg-amber-300 text-neutral-900 animate-popIn">
            Blackjack!
          </span>
        )}
        {hand.isBusted && (
          <span className="px-2 py-0.5 rounded text-xs font-bold bg-red-800 text-white animate-popIn">Bust</span>
        )}
      </div>
    </div>
  )
}
