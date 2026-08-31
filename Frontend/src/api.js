const BASE_URL = 'http://localhost:5080/api/game'
const GAME_ID_STORAGE_KEY = 'blackjack_game_id'


function getGameId() {
  return localStorage.getItem(GAME_ID_STORAGE_KEY)
}
function setGameId(id) {
  localStorage.setItem(GAME_ID_STORAGE_KEY, id)
}
function clearGameId() {
  localStorage.removeItem(GAME_ID_STORAGE_KEY)
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
    ...options,
  })

  if (!res.ok) {
    throw new Error(await extractErrorMessage(res))
  }

  return res.json()
}

function gameRequest(path, options) {
  const gameId = getGameId()
  if (!gameId) {
    throw new Error('Belum ada game aktif - mulai game baru dulu.')
  }
  return request(`/${gameId}${path}`, options)
}

export const api = {

  resume: async () => {
    const gameId = getGameId()
    if (!gameId) return null

    const res = await fetch(`${BASE_URL}/${gameId}/resume`)
    if (res.status === 404) {
      clearGameId()
      return null
    }
    if (!res.ok) throw new Error(await extractErrorMessage(res))
    return res.json()
  },

  newGame: async (name, startingBalance, minBet, maxBet) => {
    const { gameId, state } = await request('/new', {
      method: 'POST',
      body: JSON.stringify({ name, startingBalance, minBet, maxBet }),
    })
    setGameId(gameId)
    return state
  },

  forgetGame: () => clearGameId(),

  placeBet: (amount) =>
    gameRequest('/bet', {
      method: 'POST',
      body: JSON.stringify({ amount }),
    }),

  deal: () => gameRequest('/deal', { method: 'POST' }),

  action: (action, handIndex = 0) =>
    gameRequest('/action', {
      method: 'POST',
      body: JSON.stringify({ action, handIndex }),
    }),
}
