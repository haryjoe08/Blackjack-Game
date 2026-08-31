import DealerView from './DealerView.jsx'
import HandView from './HandView.jsx'

export default function GameTable({ dealer, hands, roundInProgress, roundOver, activeHandIndex }) {
  return (
    <div
      className="relative w-full max-w-3xl rounded-[3rem] border-4 border-amber-300/40 px-6 py-8 sm:px-10 sm:py-6 overflow-hidden"
      style={{
        background: 'radial-gradient(ellipse at top, #1f6b43 0%, #124026 60%, #0a2617 100%)',
        boxShadow: '0 0 0 6px rgba(0,0,0,0.35), 0 20px 50px rgba(0,0,0,0.5), inset 0 0 60px rgba(0,0,0,0.35)',
      }}
    >

      <div
        className="absolute inset-0 opacity-[0.06] pointer-events-none"
        style={{ backgroundImage: 'radial-gradient(circle, #ffffff 1px, transparent 1px)', backgroundSize: '10px 10px' }}
      />

      <div className="relative flex flex-col items-center">
        <DealerView dealer={dealer} />
      </div>

      {roundInProgress && (
        <>
          <div className="relative border-t border-amber-300/15 my-5 w-2/3 mx-auto" />
          <div className="relative flex flex-col items-center gap-2">
            <span className="text-[10px] tracking-[0.35em] uppercase text-amber-300/50">Pemain</span>
            <div className="flex gap-5 flex-wrap justify-center w-full">
              {hands.map((hand, i) => (
                <HandView
                  key={i}
                  hand={hand}
                  index={i}
                  isActive={roundInProgress && !roundOver && i === activeHandIndex}
                />
              ))}
            </div>
          </div>
        </>
      )}
    </div>
  )
}
