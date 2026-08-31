import CardView from './CardView.jsx'

export default function DealerView({ dealer }) {
  return (
    <div className="flex flex-col items-center gap-2 p-2.5 rounded-lg mb-1">
      <div className="flex">
        {dealer.cards.map((card, i) => (
          <CardView key={i} card={card} />
        ))}
        {dealer.holeCardHidden && <CardView faceDown />}
      </div>
      <div className="flex items-center gap-2.5 text-sm">
        <span>Dealer</span>
        <span key={dealer.holeCardHidden ? 'hidden' : dealer.score} className="font-bold text-amber-300 animate-popIn">
          {dealer.holeCardHidden ? '?' : dealer.score}
        </span>
      </div>
    </div>
  )
}
