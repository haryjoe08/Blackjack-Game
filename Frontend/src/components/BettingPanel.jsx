import { useState } from 'react'
import { CHIP_TYPES, formatChipLabel } from '../chipTypes.js'
import { playChip, playClick } from '../sound.js'

export default function BettingPanel({ minBet, maxBet, balance, onBet, disabled }) {
  const [amount, setAmount] = useState(0)

 
  const availableChips = CHIP_TYPES.filter((c) => c.value <= maxBet)

  const addChip = (value) => {
    playChip()
    setAmount((prev) => Math.min(prev + value, maxBet, balance))
  }

  const clear = () => {
    playClick()
    setAmount(0)
  }

  const canDeal = amount >= minBet && amount <= maxBet && amount <= balance

  return (
    <div className="bg-black/30 border border-amber-300/25 rounded-xl px-5 py-2 w-full max-w-5xl ">
     

      <div className="flex flex-col items-center justify-center w-24 h-24 rounded-full border-4 border-dashed border-amber-300/50 mx-auto my-3 bg-black/20">
        <span className="text-[11px] uppercase tracking-wide text-neutral-300">Bet Amount</span>
        <span key={amount} className="text-2xl font-bold text-amber-300 animate-popIn">
          {amount}
        </span>
      </div>

      <div className="flex gap-2.5 flex-wrap justify-center my-3.5">
        {availableChips.map((chip) => (
          <button
            key={chip.name}
            title={`Chip ${chip.name} (${chip.value})`}
            disabled={disabled || chip.value > balance - amount}
            onClick={() => addChip(chip.value)}
            className="w-[58px] h-[58px] rounded-full font-bold text-[13px] transition-transform duration-75 hover:-translate-y-1 active:translate-y-0 disabled:opacity-35 disabled:cursor-not-allowed"
            style={{
              color: chip.fg,
              background: `radial-gradient(circle at center, ${chip.bg} 62%, transparent 63%), repeating-conic-gradient(${chip.ring} 0deg 15deg, ${chip.bg} 15deg 30deg)`,
              boxShadow: '0 3px 6px rgba(0,0,0,0.5), inset 0 0 0 2px rgba(255,255,255,0.15)',
            }}
          >
            {formatChipLabel(chip.value)}
          </button>
        ))}
      </div>

      <div className="flex gap-2.5 justify-center">
        <button
          className="px-4 py-2.5 rounded-lg border border-amber-300/40 bg-transparent text-amber-300 font-semibold cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed hover:bg-amber-300/10 transition-colors"
          onClick={clear}
          disabled={disabled || amount === 0}
        >
          Clear
        </button>
        <button
          className="px-6 py-2.5 rounded-lg bg-amber-300 text-neutral-900 font-semibold cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed hover:bg-amber-200 hover:scale-105 active:scale-95 transition-all"
          onClick={() => {
            playClick()
            onBet(amount)
          }}
          disabled={disabled || !canDeal}
        >
          Deal
        </button>
      </div>
    </div>
  )
}
