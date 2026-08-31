// Web Audio API sound effects, synthesized on the fly (no external audio
// files needed - everything here is generated tones/noise via oscillators).
// Muted state persists across reloads via localStorage.

let audioCtx = null
let muted = typeof localStorage !== 'undefined' && localStorage.getItem('blackjack_muted') === 'true'

function ctx() {
  if (!audioCtx) {
    const AudioContextClass = window.AudioContext || window.webkitAudioContext
    audioCtx = new AudioContextClass()
  }
  // Browsers suspend the context until a user gesture - resume defensively
  // on every call so the very first click's sound isn't silently dropped.
  if (audioCtx.state === 'suspended') audioCtx.resume()
  return audioCtx
}

function tone(freq, duration, { type = 'sine', volume = 0.15, delay = 0, slideTo = null } = {}) {
  if (muted) return
  const c = ctx()
  const osc = c.createOscillator()
  const gain = c.createGain()
  osc.type = type
  osc.frequency.setValueAtTime(freq, c.currentTime + delay)
  if (slideTo) {
    osc.frequency.exponentialRampToValueAtTime(slideTo, c.currentTime + delay + duration)
  }
  gain.gain.setValueAtTime(volume, c.currentTime + delay)
  gain.gain.exponentialRampToValueAtTime(0.0001, c.currentTime + delay + duration)
  osc.connect(gain)
  gain.connect(c.destination)
  osc.start(c.currentTime + delay)
  osc.stop(c.currentTime + delay + duration)
}

export function setMuted(value) {
  muted = value
  if (typeof localStorage !== 'undefined') {
    localStorage.setItem('blackjack_muted', String(value))
  }
}

export function isMuted() {
  return muted
}

// A card sliding onto the table - short, dry, percussive.
export function playCard() {
  tone(650, 0.06, { type: 'square', volume: 0.06, slideTo: 300 })
}

// A chip clinking onto the felt.
export function playChip() {
  tone(1100, 0.05, { type: 'triangle', volume: 0.1 })
  tone(1600, 0.03, { type: 'triangle', volume: 0.05, delay: 0.02 })
}

// Generic UI button press - subtle, unobtrusive.
export function playClick() {
  tone(500, 0.03, { type: 'sine', volume: 0.05 })
}

// Win - short rising major arpeggio.
export function playWin() {
  tone(523.25, 0.12, { type: 'sine', volume: 0.15 })
  tone(659.25, 0.12, { type: 'sine', volume: 0.15, delay: 0.1 })
  tone(783.99, 0.2, { type: 'sine', volume: 0.15, delay: 0.2 })
}

// Blackjack - a slightly bigger fanfare than a normal win.
export function playBlackjack() {
  tone(523.25, 0.1, { type: 'sine', volume: 0.16 })
  tone(659.25, 0.1, { type: 'sine', volume: 0.16, delay: 0.09 })
  tone(783.99, 0.1, { type: 'sine', volume: 0.16, delay: 0.18 })
  tone(1046.5, 0.28, { type: 'sine', volume: 0.18, delay: 0.27 })
}

// Lose / bust - descending, slightly harsh.
export function playLose() {
  tone(320, 0.18, { type: 'sawtooth', volume: 0.1, slideTo: 160 })
}

// Push - neutral, flat little blip.
export function playPush() {
  tone(440, 0.15, { type: 'sine', volume: 0.1 })
}
