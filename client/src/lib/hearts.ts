import { useCallback, useEffect, useState } from 'react'

// Client-side "hearts" (lives), Duolingo-style. Kept local (localStorage) rather than
// on the server: a wrong run is a low-stakes learning moment, so we don't want a DB
// write per mistake — and hearts refill over time so a learner is never hard-blocked.
const MAX_HEARTS = 5
const REFILL_MS = 30 * 60 * 1000 // one heart every 30 minutes
const KEY = 'dojo-hearts-v1'

interface HeartState {
  hearts: number
  updatedAt: number // epoch ms of the last change, for time-based refills
}

function load(): HeartState {
  try {
    const raw = localStorage.getItem(KEY)
    if (raw) return JSON.parse(raw) as HeartState
  } catch {
    /* ignore corrupt/absent storage */
  }
  return { hearts: MAX_HEARTS, updatedAt: Date.now() }
}

// Apply any hearts earned back since the last update (lazy, on read).
function withRefill(s: HeartState): HeartState {
  if (s.hearts >= MAX_HEARTS) return { ...s, updatedAt: Date.now() }
  const earned = Math.floor((Date.now() - s.updatedAt) / REFILL_MS)
  if (earned <= 0) return s
  const hearts = Math.min(MAX_HEARTS, s.hearts + earned)
  return { hearts, updatedAt: hearts >= MAX_HEARTS ? Date.now() : s.updatedAt + earned * REFILL_MS }
}

export function useHearts() {
  const [state, setState] = useState<HeartState>(() => withRefill(load()))

  // Persist + re-check refills on an interval so the count climbs while idle.
  useEffect(() => {
    localStorage.setItem(KEY, JSON.stringify(state))
    const id = setInterval(() => setState((s) => withRefill(s)), 60 * 1000)
    return () => clearInterval(id)
  }, [state])

  const loseHeart = useCallback(() => {
    setState((s) => {
      const cur = withRefill(s)
      return { hearts: Math.max(0, cur.hearts - 1), updatedAt: Date.now() }
    })
  }, [])

  const refillAll = useCallback(() => setState({ hearts: MAX_HEARTS, updatedAt: Date.now() }), [])

  // Minutes until the next heart returns (for a tooltip); null when full.
  const minutesToNext =
    state.hearts >= MAX_HEARTS ? null : Math.ceil((REFILL_MS - (Date.now() - state.updatedAt)) / 60000)

  return { hearts: state.hearts, max: MAX_HEARTS, loseHeart, refillAll, minutesToNext }
}
