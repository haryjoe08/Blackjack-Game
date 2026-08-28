const RANK_LABEL = {
  Two: '2', Three: '3', Four: '4', Five: '5', Six: '6', Seven: '7',
  Eight: '8', Nine: '9', Ten: '10', Jack: 'J', Queen: 'Q', King: 'K', Ace: 'A',
}

const SUIT_SYMBOL = { Heart: '\u2665', Diamond: '\u2666', Clubs: '\u2663', Spades: '\u2660' }
const RED_SUITS = new Set(['Heart', 'Diamond'])

export default function CardView({ card, faceDown = false }) {
  const base =
    'w-16 h-[92px] -ml-4 first:ml-0 rounded-lg shadow-lg relative flex items-center justify-center select-none'

  if (faceDown) {
    return (
      <div
        className={`${base} border-2 border-[#0d2545] bg-[repeating-linear-gradient(45deg,#1d4f8a,#1d4f8a_6px,#163d6b_6px,#163d6b_12px)]`}
        aria-label="Kartu tertutup"
      />
    )
  }

  const label = RANK_LABEL[card.rank] ?? card.rank
  const symbol = SUIT_SYMBOL[card.suit] ?? '?'
  const isRed = RED_SUITS.has(card.suit)
  const colorClass = isRed ? 'text-red-700' : 'text-neutral-900'

  return (
    <div className={`${base} bg-neutral-50 ${colorClass}`}>
      <span className="absolute left-1.5 top-1.5 text-xs font-bold leading-none">
        {label}
        {symbol}
      </span>
      <span className="text-2xl">{symbol}</span>
      <span className="absolute left-1.5 bottom-1.5 text-xs font-bold leading-none rotate-180">
        {label}
        {symbol}
      </span>
    </div>
  )
}
