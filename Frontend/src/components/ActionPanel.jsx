import { playClick } from '../sound.js'

export default function ActionPanel({ hand, onAction, disabled, canOfferInsurance }) {
  const canDouble = hand.cards.length === 2
  const canSplit = hand.cards.length === 2 && hand.cards[0].value === hand.cards[1].value
  const canSurrender = hand.cards.length === 2

  const act = (action) => {
    playClick()
    onAction(action)
  }

  const btn =
    'px-4 py-2.5 rounded-lg border-none bg-emerald-700 text-white font-semibold cursor-pointer hover:bg-emerald-600 hover:scale-105 active:scale-95 disabled:opacity-40 disabled:hover:scale-100 disabled:cursor-not-allowed transition-all'
  const insuranceBtn =
    'px-4 py-2.5 rounded-lg border-none bg-amber-600 text-white font-semibold cursor-pointer hover:bg-amber-500 hover:scale-105 active:scale-95 disabled:opacity-40 disabled:hover:scale-100 disabled:cursor-not-allowed transition-all animate-pulseGlow'

  return (
    <div className="flex gap-2.5 flex-wrap justify-center max-w-xl">
      <button className={btn} disabled={disabled} onClick={() => act('Hit')}>
        Hit
      </button>
      <button className={btn} disabled={disabled} onClick={() => act('Stand')}>
        Stand
      </button>
      <button className={btn} disabled={disabled || !canDouble} onClick={() => act('DoubleDown')}>
        Double Down
      </button>
      <button className={btn} disabled={disabled || !canSplit} onClick={() => act('Split')}>
        Split
      </button>
      <button className={btn} disabled={disabled || !canSurrender} onClick={() => act('Surrender')}>
        Surrender
      </button>
  
      {canOfferInsurance && (
        <button className={insuranceBtn} disabled={disabled} onClick={() => act('Insurance')}>
          Insurance
        </button>
      )}
    </div>
  )
}
