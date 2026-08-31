const SUITS = ['\u2660', '\u2665', '\u2666', '\u2663']

const FEATURE_BADGES = [
  'Split & Double Down',
  'Insurance',
  'Chip kasino asli',
  'Sound effect',
]

export default function LandingPage({ onPlay }) {
  return (
   <div className="flex flex-col items-center gap-8 text-center animate-popIn w-full">
      <div className="flex gap-3 text-3xl text-amber-300/70">
        {SUITS.map((s, i) => (
          <span key={i} className="animate-pulseGlow" style={{ animationDelay: `${i * 0.2}s` }}>
            {s}
          </span>
        ))}
      </div>

      <div>
        <h1 className="text-3xl sm:text-6xl font-black tracking-widest uppercase text-orange-300 drop-shadow-[0_2px_12px_rgba(245,214,123,0.35)]">
          <span className="text-orange-300 text-5xl">Lightning</span>  <br /> Blackjack
        </h1>
        <img src="https://images.prismic.io/fanduel-casino/Z9FQkBsAHJWomarh_lightningblackjacklivedealer_logo.png?auto=format,compress" alt="dealr-logo" className="mx-auto w-180 h-80" />
        <p className="text-  font-black tracking-widest uppercase text-neutral-300 drop-shadow-[0_2px_12px_rgba(245,214,123,0.35)]">
            Place your chips, compete against the dealer,
            aim for 21.
        </p>

      </div>

      <button
        onClick={onPlay}
        className="px-10 py-4 rounded-full bg-amber-300 text-neutral-900 font-bold text-lg uppercase tracking-wide shadow-[0_8px_24px_rgba(245,214,123,0.35)] hover:bg-amber-200 hover:scale-105 active:scale-95 transition-all"
      >
        Play Now
      </button>
    </div>
  )
}
