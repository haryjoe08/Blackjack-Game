export const inputClass =
  'px-2.5 py-2 rounded-md border border-neutral-700 bg-neutral-900 text-white'
export const panelClass =
  'bg-black/30 border border-amber-300/25 rounded-xl px-10 py-8 w-full max-w-xl animate-popIn'
export const primaryButtonClass =
  'w-full mt-2 px-4 py-2.5 rounded-lg bg-amber-300 text-neutral-900 font-semibold cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed hover:bg-amber-200 hover:scale-[1.02] active:scale-95 transition-all'

export function messageBannerClass(message) {
  if (!message) return 'bg-black/35'
  if (message.includes('Blackjack!')) return 'bg-amber-400/25 ring-1 ring-amber-300/60'
  if (message.includes('menang')) return 'bg-emerald-500/20 ring-1 ring-emerald-400/50'
  if (message.includes('kalah') || message.includes('menyerah')) return 'bg-red-500/20 ring-1 ring-red-400/50'
  if (message.includes('seri')) return 'bg-neutral-400/15 ring-1 ring-neutral-300/40'
  return 'bg-black/35'
}
