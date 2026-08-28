const BASE_URL = 'http://localhost:5080/api/game'
const STORAGE_KEY = 'blackjack_game_id'

export const sessionStore = {
  getGameId: () => localStorage.getItem(STORAGE_KEY),
  setGameId: (id) => localStorage.setItem(STORAGE_KEY, id),
  clearGameId: () => localStorage.removeItem(STORAGE_KEY),
}

async function extractErrorMessage(res) {
  try {
    const body = await res.json()
    if (body && typeof body.message === 'string') {
      return body.message
    }
  } catch {
 
  }
  return `Request gagal (${res.status})`
}

async function request(path, options) {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,sessionStore
  })

  if (!res.ok) {
    throw new Error(await extractErrorMessage(res))
  }

  return res.json()
}

export const api = {
  // Cek dan resume game dari gameId yang ada di localStorage
  resume: async () => {
    const gameId = sessionStore.getGameId()
    if (!gameId) return null

    const res = await fetch(`${BASE_URL}/${gameId}/resume`)
    if (res.status === 404) {
      sessionStore.clearGameId()
      return null
    }
    if (!res.ok) throw new Error(await extractErrorMessage(res))
    return res.json()
  },

  // Buat sesi game baru dan simpan gameId yang didapat dari backend
  newGame: async (name, startingBalance, minBet, maxBet) => {
    const data = await request('/new', {
      method: 'POST',
      body: JSON.stringify({ name, startingBalance, minBet, maxBet }),
    })

    // Tangkap gameId dari response body 
    const gameId = data.gameId 
    if (gameId) {
      sessionStore.setGameId(gameId)
    }

    return data.state 
  },

  placeBet: (amount) => {
    const gameId = sessionStore.getGameId()
    return request(`/${gameId}/bet`, {
      method: 'POST',
      body: JSON.stringify({ amount }),
    })
  },

  deal: () => {
    const gameId = sessionStore.getGameId()
    return request(`/${gameId}/deal`, { method: 'POST' })
  },

  action: (action, handIndex = 0) => {
    const gameId = sessionStore.getGameId()
    return request(`/${gameId}/action`, {
      method: 'POST',
      body: JSON.stringify({ action, handIndex }),
    })
  },
}