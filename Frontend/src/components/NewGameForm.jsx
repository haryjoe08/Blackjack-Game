import { useState } from 'react'
import { inputClass, panelClass, primaryButtonClass } from '../uiClasses.js'

export default function NewGameForm({ onStart, busy }) {
  const [name, setName] = useState('Player 1')
  const [balance, setBalance] = useState(1000)
  const [minBet, setMinBet] = useState(10)
  const [maxBet, setMaxBet] = useState(500)

  return (
    <div className={panelClass}>
      <div className="text-center mb-2">
     
        <div className="flex justify-center gap-2 text-2xl text-amber-300/60 mb-1">
          <span>&#9824;</span>
          <span>&#9829;</span>
          <span>&#9830;</span>
          <span>&#9827;</span>
        </div>
        <h1 className="text-3xl sm:text-4xl font-black tracking-widest uppercase text-amber-300 drop-shadow-[0_2px_12px_rgba(245,214,123,0.35)]">
          Blackjack
        </h1>
      </div>

      <h2 className="text-lg font-semibold mb-3 text-center">New Table</h2>

      <div className="flex flex-col">
        <label className="flex flex-col gap-1 mb-3 text-sm">
          Player Name
          <input className={inputClass} value={name} onChange={(e) => setName(e.target.value)} />
        </label>
        <label className="flex flex-col gap-1 mb-3 text-sm">
          Starting Balance
          <input
            className={inputClass}
            type="number"
            value={balance}
            onChange={(e) => setBalance(Number(e.target.value))}
          />
        </label>
        <div className="flex gap-3">
          <label className="flex flex-col gap-1 mb-3 text-sm flex-1">
            Min Bet
            <input
              className={inputClass}
              type="number"
              value={minBet}
              onChange={(e) => setMinBet(Number(e.target.value))}
            />
          </label>
          <label className="flex flex-col gap-1 mb-3 text-sm flex-1">
            Max Bet
            <input
              className={inputClass}
              type="number"
              value={maxBet}
              onChange={(e) => setMaxBet(Number(e.target.value))}
            />
          </label>
        </div>
        <button
          className={primaryButtonClass}
          disabled={busy || !name || balance <= 0 || minBet <= 0 || maxBet < minBet}
          onClick={() => onStart(name, balance, minBet, maxBet)}
        >
          Start Playing
        </button>
      </div>
    </div>
  )
}
