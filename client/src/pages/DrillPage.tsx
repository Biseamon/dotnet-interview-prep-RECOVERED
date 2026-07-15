import { useEffect, useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { usePlayer } from '../lib/player'

// The interview drill: a timed-feeling MCQ quiz. Questions come from the seeded bank;
// finishing records the result server-side (awards XP, keeps the streak). Ports the
// design prototype's state machine (q / selected / checked / correctCount / done).
export function DrillPage() {
  const qc = useQueryClient()
  const { hearts, loseHeart } = usePlayer()
  const { data: questions, isLoading, refetch } = useQuery({
    queryKey: ['drill-questions'],
    queryFn: () => api.getDrillQuestions(5),
    staleTime: 0,
    refetchOnMount: 'always',
  })

  const [q, setQ] = useState(0)
  const [selected, setSelected] = useState<number | null>(null)
  const [checked, setChecked] = useState(false)
  const [correctCount, setCorrectCount] = useState(0)
  const [done, setDone] = useState(false)
  const [recorded, setRecorded] = useState<{ xp: number } | null>(null)

  const total = questions?.length ?? 0

  // Record the result once, when the quiz completes.
  useEffect(() => {
    if (done && questions && !recorded) {
      api.completeDrill(correctCount, total).then((res) => {
        setRecorded({ xp: res.xpEarned })
        qc.invalidateQueries({ queryKey: ['gamification'] })
        qc.invalidateQueries({ queryKey: ['achievements'] })
        qc.invalidateQueries({ queryKey: ['player'] })
      })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [done])

  if (isLoading || !questions) return <div style={{ minHeight: '100vh', display: 'grid', placeItems: 'center', color: 'var(--muted)', fontWeight: 700 }}>Loading drill…</div>

  const cur = questions[Math.min(q, total - 1)]
  const isCorrect = selected === cur.correctIndex

  const restart = () => {
    setQ(0); setSelected(null); setChecked(false); setCorrectCount(0); setDone(false); setRecorded(null)
    refetch()
  }
  const next = () => {
    if (q + 1 >= total) setDone(true)
    else { setQ(q + 1); setSelected(null); setChecked(false) }
  }
  const mainAction = () => {
    if (!checked) {
      if (selected === null) return
      setChecked(true)
      if (isCorrect) setCorrectCount((c) => c + 1)
      else loseHeart()
    } else next()
  }

  const pct = done ? 100 : Math.round((q / total) * 100)

  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', background: 'var(--bg)' }}>
      {/* Top bar */}
      <div style={{ maxWidth: 1080, width: '100%', margin: '0 auto', padding: '22px 24px 0', boxSizing: 'border-box', display: 'flex', alignItems: 'center', gap: 18 }}>
        <Link to="/" style={{ fontSize: 22, color: 'var(--faint)', fontWeight: 800 }}>✕</Link>
        <div style={{ flex: 1, height: 16, borderRadius: 999, background: 'var(--track)', overflow: 'hidden' }}>
          <div style={{ width: `${pct}%`, height: '100%', background: 'var(--blue)', borderRadius: 999, position: 'relative', transition: 'width 0.4s ease' }}>
            <div style={{ position: 'absolute', top: 3, left: 8, right: 8, height: 4, borderRadius: 999, background: 'rgba(255,255,255,0.35)' }} />
          </div>
        </div>
        <span style={{ fontSize: 13, fontWeight: 800, color: 'var(--muted)' }}>{done ? `${total} / ${total}` : `${q + 1} / ${total}`}</span>
        <span style={{ display: 'flex', alignItems: 'center', gap: 5, fontWeight: 800, color: 'var(--heart)', fontSize: 16 }}><span style={{ fontSize: 19 }}>❤️</span>{hearts}</span>
      </div>

      {!done ? (
        <>
          <div style={{ maxWidth: 760, width: '100%', margin: '0 auto', padding: '36px 24px 24px', boxSizing: 'border-box', flex: 1, display: 'flex', flexDirection: 'column', gap: 20 }}>
            <span style={{ fontSize: 12, fontWeight: 800, letterSpacing: '0.08em', color: 'var(--blue)' }}>🧠 INTERVIEW DRILL · {cur.tag}</span>
            <h1 style={{ margin: 0, fontSize: 24, fontWeight: 900, lineHeight: 1.35 }}>{cur.text}</h1>

            <div style={{ display: 'flex', flexDirection: 'column', gap: 12, marginTop: 8 }}>
              {cur.options.map((label, i) => {
                const sel = selected === i
                let bd = 'var(--border)', bg = 'var(--card)', sh = 'var(--border)', tx = 'var(--text)', mark = ''
                if (!checked && sel) { bd = 'var(--blue)'; bg = 'rgba(77,163,255,0.08)'; sh = 'var(--blue-sh)'; tx = 'var(--blue-sh)' }
                if (checked && i === cur.correctIndex) { bd = 'var(--green)'; bg = 'var(--chip-green)'; sh = 'var(--green-sh)'; tx = 'var(--chip-green-tx)'; mark = '✅' }
                if (checked && sel && !isCorrect) { bd = 'var(--heart)'; bg = 'var(--chip-pink)'; sh = '#c25064'; tx = 'var(--chip-pink-tx)'; mark = '❌' }
                return (
                  <button key={i} onClick={() => { if (!checked) setSelected(i) }}
                    style={{ display: 'flex', alignItems: 'center', gap: 14, textAlign: 'left', background: bg, border: `3px solid ${bd}`, borderRadius: 16, padding: '15px 18px', cursor: 'pointer', fontFamily: 'inherit', boxShadow: `0 3px 0 ${sh}` }}>
                    <span style={{ width: 30, height: 30, borderRadius: 9, border: `2px solid ${bd}`, display: 'grid', placeItems: 'center', fontWeight: 900, fontSize: 13, color: tx, flex: 'none' }}>{i + 1}</span>
                    <span style={{ fontWeight: 800, fontSize: 15.5, color: tx }}>{label}</span>
                    <span style={{ marginLeft: 'auto', fontSize: 18 }}>{mark}</span>
                  </button>
                )
              })}
            </div>

            {checked && (
              <div style={{ background: isCorrect ? 'var(--green)' : 'var(--heart)', borderRadius: 18, padding: '16px 20px', display: 'flex', alignItems: 'center', gap: 14, color: '#fff' }}>
                <span style={{ fontSize: 26 }}>{isCorrect ? '🎉' : '💔'}</span>
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: 900, fontSize: 16 }}>{isCorrect ? 'Correct!' : 'Not quite — you lose a heart.'}</div>
                  <div style={{ fontSize: 13.5, fontWeight: 700, opacity: 0.92 }}>{cur.explanation}</div>
                </div>
              </div>
            )}
          </div>

          <div style={{ borderTop: '2px solid var(--border)', background: 'var(--card)' }}>
            <div style={{ maxWidth: 760, margin: '0 auto', padding: '18px 24px', display: 'flex', alignItems: 'center', gap: 14, boxSizing: 'border-box' }}>
              <button onClick={next} className="btn-3d btn-ghost" style={{ letterSpacing: '0.05em' }}>SKIP</button>
              <button onClick={mainAction} className={`btn-3d ${checked ? 'btn-blue' : 'btn-green'}`} style={{ marginLeft: 'auto', fontSize: 16, padding: '14px 44px', letterSpacing: '0.06em', opacity: !checked && selected === null ? 0.45 : 1 }}>
                {checked ? (q + 1 >= total ? 'FINISH' : 'NEXT') : 'CHECK'}
              </button>
            </div>
          </div>
        </>
      ) : (
        <div style={{ maxWidth: 560, width: '100%', margin: '0 auto', padding: '60px 24px', boxSizing: 'border-box', flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 18, textAlign: 'center' }}>
          <span style={{ fontSize: 64 }}>🏆</span>
          <h1 style={{ margin: 0, fontSize: 28, fontWeight: 900 }}>Drill complete!</h1>
          <p style={{ margin: 0, fontSize: 15, fontWeight: 700, color: 'var(--muted)' }}>{correctCount} of {total} correct — your knowledge is staying sharp.</p>
          <div style={{ display: 'flex', gap: 16, marginTop: 8 }}>
            <div className="card" style={{ padding: '14px 22px' }}><div style={{ fontSize: 20, fontWeight: 900, color: 'var(--xp)' }}>+{recorded?.xp ?? correctCount * 5 + 5} XP</div><div style={{ fontSize: 11.5, fontWeight: 800, color: 'var(--muted)' }}>EARNED</div></div>
            <div className="card" style={{ padding: '14px 22px' }}><div style={{ fontSize: 20, fontWeight: 900, color: 'var(--streak)' }}>🔥 saved</div><div style={{ fontSize: 11.5, fontWeight: 800, color: 'var(--muted)' }}>STREAK</div></div>
          </div>
          <div style={{ display: 'flex', gap: 12, marginTop: 12 }}>
            <button onClick={restart} className="btn-3d btn-ghost">↺ AGAIN</button>
            <Link to="/" className="btn-3d btn-green">BACK TO LEARN</Link>
          </div>
        </div>
      )}
    </div>
  )
}
