import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import ReactMarkdown from 'react-markdown'
import { api, type GradeResult } from '../api/client'
import { CodeEditor } from '../components/CodeEditor'
import { ResultsPanel } from '../components/ResultsPanel'
import { useProgress } from '../lib/progress'
import { usePlayer } from '../lib/player'
import { xpForDifficulty } from '../lib/gamification'
import { neighbours, topicExerciseSlugs } from '../lib/nav'
import { DifficultyChip, ProgressBar } from '../components/ui'

export function ExercisePage() {
  const { slug } = useParams<{ slug: string }>()
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const { isSolved } = useProgress()
  const { hearts, loseHeart } = usePlayer()

  const { data: exercise, isLoading } = useQuery({
    queryKey: ['exercise', slug],
    queryFn: () => api.getExercise(slug!),
    enabled: !!slug,
  })
  const { data: topic } = useQuery({
    queryKey: ['topic', exercise?.topicSlug],
    queryFn: () => api.getTopic(exercise!.topicSlug!),
    enabled: !!exercise?.topicSlug,
  })

  const [code, setCode] = useState('')
  const [result, setResult] = useState<GradeResult | null>(null)
  const [isGrading, setIsGrading] = useState(false)
  const [solution, setSolution] = useState<string | null>(null)
  const [hints, setHints] = useState<{ order: number; text: string }[]>([])
  const [focus, setFocus] = useState(false)
  const [confetti, setConfetti] = useState(false)
  const wasSolvedOnLoad = useRef(false)

  useEffect(() => {
    if (exercise) {
      setCode(exercise.starterCode)
      setHints([])
      setSolution(null)
      setResult(null)
      wasSolvedOnLoad.current = isSolved(exercise.slug)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [exercise?.slug])

  const nav = neighbours(topic ? topicExerciseSlugs(topic) : [], slug ?? '')

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (!e.altKey) return
      if (e.key === 'ArrowRight' && nav.next) navigate(`/exercises/${nav.next}`)
      if (e.key === 'ArrowLeft' && nav.prev) navigate(`/exercises/${nav.prev}`)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [nav.prev, nav.next, navigate])

  if (isLoading || !exercise) return <p style={{ padding: 24, color: 'var(--muted)' }}>Loading exercise…</p>

  const justSolved = result?.status === 'Passed'
  const firstTimeSolve = justSolved && !wasSolvedOnLoad.current
  const xpGain = xpForDifficulty(exercise.difficulty)
  const maxHints = Math.min(3, exercise.hintCount)
  const solutionUnlocked = hints.length >= 2 || justSolved || wasSolvedOnLoad.current

  const submit = async () => {
    setIsGrading(true); setResult(null)
    try {
      const graded = await api.grade(exercise.slug, code)
      setResult(graded)
      if (graded.status === 'Passed') {
        queryClient.invalidateQueries({ queryKey: ['progress'] })
        queryClient.invalidateQueries({ queryKey: ['gamification'] })
        queryClient.invalidateQueries({ queryKey: ['achievements'] })
        setConfetti(true); setTimeout(() => setConfetti(false), 2600)
      } else {
        loseHeart()
      }
    } finally { setIsGrading(false) }
  }

  const reset = async () => {
    setCode(exercise.starterCode); setResult(null); setSolution(null); setHints([])
    wasSolvedOnLoad.current = false
    await api.clearAttempts(exercise.slug)
    queryClient.invalidateQueries({ queryKey: ['progress'] })
    queryClient.invalidateQueries({ queryKey: ['gamification'] })
  }

  const revealHint = async () => {
    if (hints.length >= maxHints) return
    const next = hints.length + 1
    const h = await api.getHint(exercise.slug, next)
    setHints((hs) => [...hs, { order: h.order, text: h.text }])
  }

  const revealSolution = async () => {
    const { solution } = await api.getSolution(exercise.slug)
    setSolution(solution)
  }

  const codePanel = (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
      <div style={{ border: '2px solid var(--border)', borderRadius: 16, overflow: 'hidden', background: 'var(--code-bg)', boxShadow: '0 4px 0 var(--border)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '8px 14px', borderBottom: '2px solid var(--border)', background: 'var(--surface)' }}>
          <span style={{ fontSize: 12, fontWeight: 800, color: 'var(--muted)', fontFamily: 'ui-monospace,Menlo,monospace' }}>
            {exercise.language === 'Sql' ? 'query.sql' : exercise.language === 'Config' ? 'config.yml' : 'Solution.cs'}
          </span>
          <button onClick={() => setFocus((f) => !f)} style={{ marginLeft: 'auto', fontSize: 11.5, fontWeight: 800, padding: '4px 10px', borderRadius: 9, border: '2px solid var(--border)', background: 'var(--card)', cursor: 'pointer', color: 'var(--muted)', fontFamily: 'inherit' }}>
            {focus ? '↹ SPLIT' : '⤢ FOCUS'}
          </button>
        </div>
        <CodeEditor value={code} onChange={setCode} compileErrors={result?.compileErrors ?? []} onSubmit={submit} height={focus ? 'calc(100vh - 260px)' : '300px'} language={exercise.language} />
      </div>

      <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
        <button onClick={submit} disabled={isGrading} className="btn-3d btn-green" style={{ fontSize: 16, padding: '13px 32px' }}>{isGrading ? 'RUNNING…' : 'RUN ▶'}</button>
        <button onClick={reset} className="btn-3d btn-ghost">↺ RESET</button>
        <span style={{ marginLeft: 'auto', fontSize: 12, fontWeight: 700, color: 'var(--faint)' }}>⌘ + Enter</span>
      </div>

      {justSolved && (
        <div className="animate-pop" style={{ background: 'var(--green)', borderRadius: 18, boxShadow: '0 4px 0 var(--green-sh)', padding: '16px 20px', color: '#fff', display: 'flex', alignItems: 'center', gap: 14 }}>
          <span style={{ fontSize: 28 }}>🎉</span>
          <div style={{ flex: 1 }}>
            <div style={{ fontWeight: 900, fontSize: 16 }}>Nailed it! All {result!.totalCount} tests passed.</div>
            {firstTimeSolve && <div style={{ fontSize: 13, fontWeight: 700, opacity: 0.9 }}>+{xpGain} XP · streak kept alive 🔥</div>}
          </div>
          {firstTimeSolve && <span style={{ background: 'rgba(255,255,255,0.2)', borderRadius: 12, padding: '6px 12px', fontWeight: 900, fontSize: 14 }}>+{xpGain} XP</span>}
          {nav.next && <button onClick={() => navigate(`/exercises/${nav.next}`)} style={{ background: 'var(--card)', color: 'var(--green-sh)', fontWeight: 900, fontSize: 13, borderRadius: 12, padding: '10px 16px', border: 'none', cursor: 'pointer' }}>NEXT →</button>}
        </div>
      )}

      {(result || isGrading) && !justSolved && <ResultsPanel result={result} isGrading={isGrading} />}
    </div>
  )

  return (
    <div style={{ minHeight: '100vh' }}>
      {confetti && <Confetti />}
      <main style={{ maxWidth: 1152, margin: '0 auto', padding: '20px 24px 48px', display: 'flex', flexDirection: 'column', gap: 16, boxSizing: 'border-box' }}>
        {/* Top bar */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
          <Link to={topic ? `/topics/${topic.slug}` : '/'} style={{ fontSize: 14, fontWeight: 800, color: 'var(--muted)' }}>← {topic?.name ?? 'Back'}</Link>
          <div style={{ flex: 1 }}><ProgressBar pct={nav.total ? ((nav.index + 1) / nav.total) * 100 : 0} height={16} color="var(--green)" track="var(--border)" stripe /></div>
          <span style={{ fontSize: 13, fontWeight: 800, color: 'var(--muted)' }}>{nav.total ? `${nav.index + 1} / ${nav.total}` : ''}</span>
          <span style={{ display: 'flex', alignItems: 'center', gap: 5, fontWeight: 800, color: 'var(--heart)', fontSize: 15 }}><span style={{ fontSize: 18 }}>❤️</span>{hearts}</span>
        </div>

        {focus ? codePanel : (
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 20 }}>
            {/* Left: problem + hints */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div className="card" style={{ padding: '20px 22px' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 12 }}>
                  <h1 style={{ margin: 0, fontSize: 20, fontWeight: 900 }}>{exercise.title}</h1>
                  <DifficultyChip difficulty={exercise.difficulty} />
                  <span style={{ marginLeft: 'auto', fontSize: 12.5, fontWeight: 900, color: 'var(--xp)' }}>⚡ +{xpGain} XP</span>
                </div>

                {exercise.explanation && (
                  <div style={{ display: 'flex', gap: 12, alignItems: 'flex-start', marginBottom: 14 }}>
                    <div style={{ width: 44, height: 44, borderRadius: 999, background: 'var(--chip-purple)', display: 'grid', placeItems: 'center', fontSize: 24, flex: 'none' }}>🥋</div>
                    <div style={{ position: 'relative', background: 'var(--chip-blue)', borderRadius: 16, padding: '12px 16px', fontSize: 14, fontWeight: 600, lineHeight: 1.6 }}>
                      <div style={{ position: 'absolute', left: -6, top: 16, width: 12, height: 12, background: 'var(--chip-blue)', transform: 'rotate(45deg)' }} />
                      {exercise.explanation}
                    </div>
                  </div>
                )}

                <div className="prose-dojo" style={{ fontSize: 14 }}><ReactMarkdown>{exercise.prompt}</ReactMarkdown></div>

                {exercise.visibleTests.length > 0 && (
                  <div style={{ marginTop: 14 }}>
                    <p style={{ margin: '0 0 4px', fontSize: 11, fontWeight: 800, textTransform: 'uppercase', letterSpacing: '0.06em', color: 'var(--muted)' }}>Example checks</p>
                    <ul style={{ listStyle: 'none', margin: 0, padding: 0, fontSize: 12, fontFamily: 'ui-monospace,Menlo,monospace', color: 'var(--muted)', display: 'flex', flexDirection: 'column', gap: 2 }}>
                      {exercise.visibleTests.map((t, i) => <li key={i}>• {t.name}</li>)}
                    </ul>
                  </div>
                )}
              </div>

              {/* Revealed hints */}
              {hints.map((h) => (
                <div key={h.order} style={{ background: 'var(--chip-gold)', border: '2px solid var(--chip-gold-bd)', borderRadius: 16, padding: '14px 18px', fontSize: 14, fontWeight: 600, lineHeight: 1.6, color: 'var(--chip-gold-tx)' }}>
                  <strong>Hint {h.order}:</strong> {h.text}
                </div>
              ))}

              {maxHints > 0 && hints.length < maxHints && (
                <button onClick={revealHint} style={{ alignSelf: 'flex-start', border: '2px solid var(--chip-gold-bd)', cursor: 'pointer', fontFamily: 'inherit', background: 'var(--chip-gold)', color: 'var(--chip-gold-tx)', fontWeight: 800, fontSize: 14, borderRadius: 14, padding: '10px 18px', boxShadow: '0 3px 0 var(--chip-gold-bd)' }}>
                  💡 GET A HINT · {maxHints - hints.length} LEFT
                </button>
              )}

              {/* Solution (gated) */}
              {solution ? (
                <pre style={{ fontSize: 12, background: 'var(--track)', borderRadius: 12, padding: 14, overflowX: 'auto', whiteSpace: 'pre-wrap' }}>{solution}</pre>
              ) : solutionUnlocked ? (
                <button onClick={revealSolution} style={{ alignSelf: 'flex-start', fontSize: 13, fontWeight: 800, color: 'var(--purple-deep)', background: 'none', border: 'none', cursor: 'pointer', textDecoration: 'underline' }}>📖 View the reference solution</button>
              ) : (
                <div style={{ background: 'var(--surface)', border: '2px dashed var(--ring)', borderRadius: 16, padding: '14px 18px', fontSize: 13, fontWeight: 700, color: 'var(--muted)' }}>🙈 Stuck? The reference solution unlocks after 2 hints or a pass.</div>
              )}
            </div>

            {/* Right: editor */}
            {codePanel}
          </div>
        )}
      </main>
    </div>
  )
}

// Lightweight one-shot confetti overlay (deterministic spread; self-removed by caller).
function Confetti() {
  const colors = ['#58bd6d', '#4da3ff', '#f2b13d', '#e06a7c', '#7c5ce0', '#f28c3d']
  return (
    <div style={{ pointerEvents: 'none', position: 'fixed', inset: 0, zIndex: 50, overflow: 'hidden' }} aria-hidden>
      {Array.from({ length: 80 }).map((_, i) => (
        <span key={i} style={{
          position: 'absolute', left: `${(i * 137.5) % 100}%`, top: '-5vh',
          width: 6 + (i % 4) * 3, height: (6 + (i % 4) * 3) * 1.6, background: colors[i % colors.length], borderRadius: 2,
          animation: `confetti-fall ${1.6 + (i % 7) * 0.18}s linear ${(i % 10) * 0.06}s forwards`,
        }} />
      ))}
    </div>
  )
}
