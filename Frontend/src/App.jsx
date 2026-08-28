import { useState, useEffect } from 'react'
import { api, sessionStore } from './api.js'
import HandView from './components/HandView.jsx'
import DealerView from './components/DealerView.jsx'
import BettingPanel from './components/BettingPanel.jsx'
import ActionPanel from './components/ActionPanel.jsx'

const inputClass = 'px-2.5 py-2 rounded-md border border-neutral-700 bg-neutral-900 text-white'
const panelClass = 'bg-black/30 border border-amber-300/25 rounded-xl px-5 py-4 w-full max-w-md'
const primaryButtonClass =
  'w-full mt-2 px-4 py-2.5 rounded-lg bg-amber-300 text-neutral-900 font-semibold cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed hover:bg-amber-200 transition-colors'

function NewGameForm({ onStart, busy }) {
  const [name, setName] = useState('Player 1')
  const [balance, setBalance] = useState(1000)
  const [minBet, setMinBet] = useState(10)
  const [maxBet, setMaxBet] = useState(500)

  return (
    <div className={panelClass}>
      <h2 className="text-lg font-semibold mb-3">Meja Blackjack Baru</h2>
      <label className="flex flex-col gap-1 mb-3 text-sm">
        Nama Pemain
        <input className={inputClass} value={name} onChange={(e) => setName(e.target.value)} />
      </label>
      <label className="flex flex-col gap-1 mb-3 text-sm">
        Saldo Awal
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
        Mulai Bermain
      </button>
    </div>
  )
}

function GameOverPanel({ finalBalance, onRestart }) {
  return (
    <div className={`${panelClass} text-center`}>
      <h2 className="text-lg font-semibold text-amber-300 mt-0">Game Over</h2>
      <p>Saldo kamu ({finalBalance}) sudah di bawah taruhan minimum meja ini.</p>
      <button className={primaryButtonClass} onClick={onRestart}>
        Main Lagi
      </button>
    </div>
  )
}

export default function App() {
  const [state, setState] = useState(null)
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)
  const [tableConfig, setTableConfig] = useState(null)
  const [checkingResume, setCheckingResume] = useState(true)

  useEffect(() => {
    let cancelled = false
    api
      .resume()
      .then((resumed) => {
        if (cancelled) return
        if (resumed) {
          setState(resumed)
          setTableConfig({ minBet: resumed.minBet, maxBet: resumed.maxBet })
        }
      })
      .catch((err) => {
        if (!cancelled) setError(err.message)
      })
      .finally(() => {
        if (!cancelled) setCheckingResume(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  async function run(fn) {
    setBusy(true)
    setError(null)
    try {
      const result = await fn()
      setState(result)
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  const startGame = (name, balance, minBet, maxBet) => {
    setTableConfig({ minBet, maxBet })
    run(() => api.newGame(name, balance, minBet, maxBet))
  }

  const placeBetAndDeal = (amount) =>
    run(async () => {
      await api.placeBet(amount)
      return api.deal()
    })

  const performAction = (action) => run(() => api.action(action, state.activeHandIndex))

  const restart = () => {
    sessionStore.clearGameId()
    setState(null)
    setTableConfig(null)
    setError(null)
  }

  const feltClass =
    'min-h-screen p-6 w-full mx-auto flex flex-col items-center gap-5 text-neutral-100 bg-[radial-gradient(ellipse_at_center,_#1b5e3a_0%,_#0e3521_70%,_#082014_100%)]'

  if (checkingResume) {
    return (
      <div className={feltClass}>
        <h1 className="text-3xl tracking-widest uppercase text-amber-300 m-0">Blackjack</h1>
        <p className="text-neutral-300 text-sm">Memeriksa game yang sedang berjalan...</p>
      </div>
    )
  }

  if (!state) {
    return (
      <div className={feltClass}>
        <h1 className="text-3xl tracking-widest uppercase text-amber-300 m-0">Blackjack</h1>
        <NewGameForm onStart={startGame} busy={busy} />
        {error && <p className="text-red-400 font-semibold">{error}</p>}
      </div>
    )
  }

  const activeHand = state.hands[state.activeHandIndex] ?? state.hands[0]
  const roundInProgress = state.hands.some((h) => h.cards.length > 0)
  const roundOver = roundInProgress && !state.dealer.holeCardHidden

  return (
    <div className={feltClass}>
      <header className="w-full flex justify-between items-center flex-wrap gap-3">
        <h1 className="text-3xl tracking-widest uppercase text-amber-300 m-0">Blackjack</h1>
        <div className="flex items-center gap-4 text-sm bg-black/25 px-3.5 py-2 rounded-lg">
          <span>{state.name}</span>
          <span>Saldo: {state.balance}</span>
          <span>Taruhan: {state.currentBet}</span>
          <span>Sisa kartu: {state.remainingCards}</span>
          <button
            onClick={restart}
            className="ml-2 px-2.5 py-1 text-xs rounded bg-red-800/80 hover:bg-red-700 text-white transition-colors cursor-pointer"
            title="Keluar dari sesi game ini"
          >
            Sesi Baru
          </button>
        </div>
      </header>

      <DealerView dealer={state.dealer} />

      {roundInProgress && (
        <div className="flex gap-5 flex-wrap justify-center w-full">
          {state.hands.map((hand, i) => (
            <HandView
              key={i}
              hand={hand}
              index={i}
              isActive={roundInProgress && !roundOver && i === state.activeHandIndex}
            />
          ))}
        </div>
      )}

      {state.lastMessage && (
        <p className="bg-black/35 px-4 py-2.5 rounded-lg font-semibold text-center">{state.lastMessage}</p>
      )}
      {error && <p className="text-red-400 font-semibold">{error}</p>}

      {(!roundInProgress || roundOver) && (
        <>
          {state.isGameOver ? (
            <GameOverPanel finalBalance={state.balance} onRestart={restart} />
          ) : (
            <BettingPanel
              minBet={tableConfig?.minBet ?? 10}
              maxBet={tableConfig?.maxBet ?? 500}
              balance={state.balance}
              onBet={placeBetAndDeal}
              disabled={busy}
            />
          )}
        </>
      )}

      {roundInProgress && !roundOver && (
        <ActionPanel
          hand={activeHand}
          onAction={performAction}
          disabled={busy}
          canOfferInsurance={state.canOfferInsurance}
        />
      )}
    </div>
  )
}