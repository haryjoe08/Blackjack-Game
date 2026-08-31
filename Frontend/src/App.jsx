import { useState, useEffect, useRef } from 'react'
import { api } from './api.js'
import BettingPanel from './components/BettingPanel.jsx'
import ActionPanel from './components/ActionPanel.jsx'
import LandingPage from './components/LandingPage.jsx'
import NewGameForm from './components/NewGameForm.jsx'
import GameOverPanel from './components/GameOverPanel.jsx'
import GameHeader from './components/GameHeader.jsx'
import GameTable from './components/GameTable.jsx'
import { messageBannerClass } from './uiClasses.js'
import { isMuted, setMuted, playWin, playLose, playBlackjack, playPush, playClick } from './sound.js'

const feltClass =
  'min-h-screen p-4 w-full mx-auto flex flex-col items-center gap-5 text-neutral-100 bg-[radial-gradient(ellipse_at_center,_#1b5e3a_0%,_#0e3521_70%,_#082014_100%)]'

export default function App() {
  const [state, setState] = useState(null)
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)
  const [tableConfig, setTableConfig] = useState(null)
  const [muted, setMutedState] = useState(() => isMuted())

  const [showLanding, setShowLanding] = useState(true)

  const [checkingResume, setCheckingResume] = useState(true)

  const playedMessageRef = useRef(null)

  useEffect(() => {
    let cancelled = false
    api
      .resume()
      .then((resumed) => {
        if (cancelled) return
        if (resumed) {
          setState(resumed)
          setTableConfig({ minBet: resumed.minBet, maxBet: resumed.maxBet })
          setShowLanding(false)
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

  useEffect(() => {
    if (!state) return
    const inProgress = state.hands.some((h) => h.cards.length > 0)
    const over = inProgress && !state.dealer.holeCardHidden
    if (!over || !state.lastMessage) return
    if (playedMessageRef.current === state.lastMessage) return
    playedMessageRef.current = state.lastMessage

    const msg = state.lastMessage
    if (msg.includes('Blackjack!')) playBlackjack()
    else if (msg.includes('menang')) playWin()
    else if (msg.includes('kalah') || msg.includes('menyerah')) playLose()
    else if (msg.includes('seri')) playPush()
  }, [state])

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
    api.forgetGame()
    setState(null)
    setTableConfig(null)
    setError(null)
    setShowLanding(false) 
  }

  const toggleMute = () => {
    playClick()
    const next = !muted
    setMuted(next)
    setMutedState(next)
  }

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
        {showLanding ? (
          <LandingPage onPlay={() => setShowLanding(false)} />
        ) : (
          <NewGameForm onStart={startGame} busy={busy} />
        )}
        {error && <p className="text-red-400 font-semibold">{error}</p>}
      </div>
    )
  }

  const activeHand = state.hands[state.activeHandIndex] ?? state.hands[0]
  const roundInProgress = state.hands.some((h) => h.cards.length > 0)
  const roundOver = roundInProgress && !state.dealer.holeCardHidden

  const handleNewSession = () => {
    if (roundInProgress && !roundOver) {
      const confirmed = window.confirm(
        'Ronde masih berjalan. Yakin mau mulai sesi baru? Progress ronde ini akan hilang.'
      )
      if (!confirmed) return
    }
    playClick()
    restart()
  }

  return (
    <div className={feltClass}>
      <GameHeader state={state} muted={muted} onToggleMute={toggleMute} onNewSession={handleNewSession} />

      <GameTable
        dealer={state.dealer}
        hands={state.hands}
        roundInProgress={roundInProgress}
        roundOver={roundOver}
        activeHandIndex={state.activeHandIndex}
      />

      {state.lastMessage && (
        <p
          className={`px-4 py-2.5 rounded-lg font-semibold text-center transition-colors ${messageBannerClass(
            state.lastMessage
          )}`}
        >
          {state.lastMessage}
        </p>
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
