export default function ActionPanel({ hand, onAction, disabled, canOfferInsurance }) {
  const canDouble = hand.cards.length === 2
  const canSplit = hand.cards.length === 2 && hand.cards[0].value === hand.cards[1].value
  const canSurrender = hand.cards.length === 2

  const btn =
    'px-4 py-2.5 rounded-lg border-none bg-emerald-700 text-white font-semibold cursor-pointer hover:bg-emerald-600 disabled:opacity-40 disabled:cursor-not-allowed transition-colors'
  const insuranceBtn =
    'px-4 py-2.5 rounded-lg border-none bg-amber-600 text-white font-semibold cursor-pointer hover:bg-amber-500 disabled:opacity-40 disabled:cursor-not-allowed transition-colors'

  return (
    <div className="flex gap-2.5 flex-wrap justify-center max-w-xl">
      <button className={btn} disabled={disabled} onClick={() => onAction('Hit')}>
        Hit
      </button>
      <button className={btn} disabled={disabled} onClick={() => onAction('Stand')}>
        Stand
      </button>
      <button className={btn} disabled={disabled || !canDouble} onClick={() => onAction('DoubleDown')}>
        Double Down
      </button>
      <button className={btn} disabled={disabled || !canSplit} onClick={() => onAction('Split')}>
        Split
      </button>
      <button className={btn} disabled={disabled || !canSurrender} onClick={() => onAction('Surrender')}>
        Surrender
      </button>
      
      {/* Insurance Button */}
      {canOfferInsurance && (
        <button className={insuranceBtn} disabled={disabled} onClick={() => onAction('Insurance')}>
          Insurance
        </button>
      )}
    </div>
  )
}
