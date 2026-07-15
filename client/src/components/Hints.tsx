import { useState } from 'react'
import { api, type Hint } from '../api/client'

// Progressive hint reveal: uncover one hint at a time so a stuck learner gets just
// enough of a nudge without seeing the whole approach.
interface Props {
  slug: string
  hintCount: number
}

export function Hints({ slug, hintCount }: Props) {
  const [revealed, setRevealed] = useState<Hint[]>([])
  const [loading, setLoading] = useState(false)

  if (hintCount === 0) return null

  const nextHintNumber = revealed.length + 1
  const allRevealed = revealed.length >= hintCount

  const revealNext = async () => {
    setLoading(true)
    try {
      const hint = await api.getHint(slug, nextHintNumber)
      setRevealed((prev) => [...prev, hint])
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-2">
      {revealed.map((h) => (
        <div key={h.order} className="rounded-[var(--radius-soft)] px-4 py-2 text-sm text-accent-ink"
             style={{ backgroundColor: 'var(--warning)' }}>
          <span className="font-semibold">Hint {h.order}:</span> {h.text}
        </div>
      ))}

      {!allRevealed && (
        <button
          onClick={revealNext}
          disabled={loading}
          className="text-sm text-muted hover:text-ink underline underline-offset-2 disabled:opacity-50"
        >
          {loading ? 'Revealing…' : `💡 Reveal hint ${nextHintNumber} of ${hintCount}`}
        </button>
      )}
    </div>
  )
}
