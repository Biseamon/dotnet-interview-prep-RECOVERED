import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { useProgress } from '../lib/progress'
import { useGamification } from '../lib/gamification'
import { usePlayer } from '../lib/player'
import { nextUnsolvedGlobal } from '../lib/nav'
import { topicMeta } from '../lib/topicMeta'
import { xpForDifficulty } from '../lib/gamification'
import { Sidebar } from '../components/Sidebar'
import { PathNode, ProgressBar, StatStrip, type NodeState } from '../components/ui'

// Staggered horizontal offsets that give the trail its winding "wiggle".
const OFFSETS = [-160, -40, 100, 180, 60, -120, 0, 140, -80, 40]

export function Dashboard() {
  const { data: topics } = useQuery({ queryKey: ['topics'], queryFn: api.getTopics })
  const { solved } = useProgress()
  const stats = useGamification()
  const { hearts } = usePlayer()

  // The "current" section = the topic holding the next unsolved exercise.
  const nextSlug = topics ? nextUnsolvedGlobal(topics, solved) : null
  const currentTopic = topics?.find((t) => t.exerciseSlugs.includes(nextSlug ?? '')) ?? topics?.[0]
  const { data: detail } = useQuery({
    queryKey: ['topic', currentTopic?.slug],
    queryFn: () => api.getTopic(currentTopic!.slug),
    enabled: !!currentTopic,
  })

  const meta = currentTopic ? topicMeta(currentTopic.slug) : null

  return (
    <div style={{ display: 'grid', gridTemplateColumns: '230px 1fr 320px', minHeight: '100vh', background: 'var(--bg)' }}>
      <Sidebar />

      {/* Center — learning path */}
      <main style={{ padding: '24px 40px 64px', display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
        {!topics && <p style={{ color: 'var(--muted)' }}>Loading…</p>}

        {detail && meta && detail.lessons.map((lesson, li) => {
          const current = li === detail.lessons.findIndex((l) => l.exercises.some((e) => !solved.has(e.slug)))
          const bannerBg = current ? 'var(--purple)' : 'var(--green)'
          const bannerSh = current ? 'var(--purple-sh)' : 'var(--green-sh)'
          return (
            <div key={lesson.slug} style={{ width: '100%', maxWidth: 560, marginTop: li === 0 ? 0 : 36 }}>
              {/* Unit banner */}
              <Link to={`/topics/${detail.slug}`} style={{ display: 'flex', alignItems: 'center', gap: 14, background: bannerBg, borderRadius: 18, boxShadow: `0 4px 0 ${bannerSh}`, padding: '18px 22px', color: '#fff', boxSizing: 'border-box' }}>
                <div>
                  <div style={{ fontSize: 12, fontWeight: 800, letterSpacing: '0.08em', opacity: 0.85 }}>{meta.category.toUpperCase()} · UNIT {li + 1}</div>
                  <div style={{ fontSize: 19, fontWeight: 900 }}>{lesson.title}</div>
                </div>
                <span style={{ marginLeft: 'auto', border: '2px solid rgba(255,255,255,0.4)', color: '#fff', fontWeight: 800, fontSize: 13, borderRadius: 12, padding: '9px 14px' }}>📖 GUIDEBOOK</span>
              </Link>

              {/* Nodes for this unit */}
              <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', paddingTop: 28 }}>
                {lesson.exercises.map((ex, ni) => {
                  const isSolved = solved.has(ex.slug)
                  const isCurrent = ex.slug === nextSlug
                  const state: NodeState = isSolved ? 'done' : isCurrent ? 'current' : 'locked'
                  const clickable = isSolved || isCurrent
                  const node = (
                    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', marginLeft: OFFSETS[ni % OFFSETS.length], marginTop: ni === 0 ? 0 : 22, position: 'relative' }}>
                      {isCurrent && (
                        <div style={{ position: 'absolute', top: -14, background: 'var(--card)', border: '2px solid var(--border)', borderRadius: 10, padding: '4px 12px', fontWeight: 900, fontSize: 12, color: 'var(--purple)', letterSpacing: '0.06em', boxShadow: '0 2px 0 var(--border)', zIndex: 2 }}>START</div>
                      )}
                      <PathNode state={state} />
                      <div style={{ fontSize: 12, fontWeight: 800, marginTop: 8, color: state === 'locked' ? 'var(--locked-tx)' : isCurrent ? 'var(--text)' : 'var(--muted)', maxWidth: 150, textAlign: 'center' }}>
                        {ex.title}{isCurrent ? ` · +${xpForDifficulty(ex.difficulty)} XP` : ''}
                      </div>
                    </div>
                  )
                  return clickable
                    ? <Link key={ex.slug} to={`/exercises/${ex.slug}`}>{node}</Link>
                    : <div key={ex.slug}>{node}</div>
                })}
              </div>
            </div>
          )
        })}
      </main>

      {/* Right rail */}
      <aside style={{ borderLeft: '2px solid var(--border)', padding: '24px 20px', display: 'flex', flexDirection: 'column', gap: 16, position: 'sticky', top: 0, height: '100vh', boxSizing: 'border-box' }}>
        <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
          <StatStrip streak={stats.streakDays} xp={stats.xp} hearts={hearts} />
        </div>

        {/* Streak card */}
        <div className="card" style={{ borderRadius: 18, padding: 18, boxShadow: 'none' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontWeight: 900, fontSize: 15, marginBottom: 12 }}><span style={{ fontSize: 20 }}>🔥</span>{stats.streakDays}-day streak</div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7,1fr)', gap: 4, textAlign: 'center' }}>
            {['M', 'T', 'W', 'T', 'F', 'S', 'S'].map((d, i) => <div key={i} style={{ fontSize: 11, fontWeight: 800, color: 'var(--muted)' }}>{d}</div>)}
            {stats.weekActivity.map((active, i) => {
              const todayIdx = (new Date().getUTCDay() + 6) % 7
              const isToday = i === todayIdx
              return (
                <div key={i} style={{ width: 24, height: 24, margin: '0 auto', borderRadius: 999, boxSizing: 'border-box', display: 'grid', placeItems: 'center', fontSize: 12, color: '#fff', fontWeight: 900,
                  background: active ? 'var(--gold)' : 'var(--track)',
                  border: !active && isToday ? '2px dashed var(--xp)' : undefined }}>
                  {active ? '✓' : ''}
                </div>
              )
            })}
          </div>
          <p style={{ margin: '12px 0 0', fontSize: 12.5, fontWeight: 600, color: 'var(--muted)' }}>Solve one exercise today to grow your streak!</p>
        </div>

        {/* Daily quests */}
        <div className="card" style={{ borderRadius: 18, padding: 18, boxShadow: 'none' }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
            <span style={{ fontWeight: 900, fontSize: 15 }}>Daily quests</span>
          </div>
          {stats.quests.map((q, i) => (
            <div key={q.id} style={{ display: 'flex', alignItems: 'center', gap: 12, marginTop: i === 0 ? 0 : 14 }}>
              <span style={{ fontSize: 24 }}>{q.emoji}</span>
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 800, fontSize: 13.5 }}>{q.label}</div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 4 }}>
                  <ProgressBar pct={(q.current / q.target) * 100} color="var(--gold)" />
                  <span style={{ fontSize: 11, fontWeight: 800, color: 'var(--muted)' }}>{q.current}/{q.target}</span>
                </div>
              </div>
              <span style={{ fontSize: 22 }}>🎁</span>
            </div>
          ))}
        </div>

        {/* Interview drill */}
        <div className="card" style={{ borderRadius: 18, padding: 18, boxShadow: 'none' }}>
          <div style={{ fontWeight: 900, fontSize: 15, marginBottom: 6 }}>🧠 Interview drill</div>
          <p style={{ margin: '0 0 12px', fontSize: 13, fontWeight: 600, color: 'var(--muted)' }}>Quick multiple-choice quiz on the concepts interviewers love. Keep them fresh.</p>
          <Link to="/drill" className="btn-3d btn-blue" style={{ width: '100%', boxSizing: 'border-box' }}>START DRILL</Link>
        </div>
      </aside>
    </div>
  )
}
