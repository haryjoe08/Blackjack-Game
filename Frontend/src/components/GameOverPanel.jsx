import { panelClass, primaryButtonClass } from '../uiClasses.js'

export default function GameOverPanel({ finalBalance, onRestart }) {
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
