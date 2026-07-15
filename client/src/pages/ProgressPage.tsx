import { useState, type ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { useProgress } from '../lib/progress'
import { useGamification } from '../lib/gamification'
import { useAchievements } from '../lib/achievements'
import { topicMeta } from '../lib/topicMeta'
import { SidebarLayout } from '../components/Sidebar'
import { Card, ProgressBar } from '../components/ui'

export function ProgressPage() {
  const stats = useGamification()
  const achievements = useAchievements()
  const { data: topics } = useQuery({ queryKey: ['topics'], queryFn: api.getTopics })
  const { countSolved } = useProgress()
  const [showAllTopics, setShowAllTopics] = useState(false)

  const totalExercises = topics?.reduce((s, t) => s + t.exerciseSlugs.length, 0) ?? 0
  const maxWeekly = Math.max(1, ...stats.weeklyXp)
  const todayIdx = (new Date().getUTCDay() + 6) % 7
  const nextBelt = beltNameFor(stats.level + 1)
  const shownTopics = showAllTopics ? topics ?? [] : (topics ?? []).slice(0, 5)

  return (
    <SidebarLayout>
      <main style={{ padding: '28px 40px 64px', maxWidth: 920, boxSizing: 'border-box' }}>
        <h1 style={{ margin: '0 0 20px', fontSize: 26, fontWeight: 900 }}>📊 Your progress</h1>

        {/* Stat cards */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4,1fr)', gap: 14, marginBottom: 24 }}>
          <StatCard emoji="🔥" value={String(stats.streakDays)} label="DAY STREAK" color="var(--streak)" />
          <StatCard emoji="⚡" value={stats.xp.toLocaleString()} label="TOTAL XP" color="var(--xp)" />
          <StatCard emoji="🥋" value={`Lv ${stats.level}`} label={stats.belt.toUpperCase()} color="var(--purple-lt)" />
          <StatCard emoji="✅" value={<>{stats.solvedCount}<span style={{ color: 'var(--faint)' }}>/{totalExercises}</span></>} label="SOLVED" color="var(--green)" />
        </div>

        {/* Weekly XP + belt */}
        <div style={{ display: 'grid', gridTemplateColumns: '1.2fr 1fr', gap: 16, marginBottom: 24 }}>
          <Card style={{ padding: 22 }}>
            <div style={{ display: 'flex', alignItems: 'baseline', gap: 10, marginBottom: 18 }}>
              <h2 style={{ margin: 0, fontSize: 16, fontWeight: 900 }}>XP this week</h2>
              <span style={{ fontSize: 12, fontWeight: 800, color: 'var(--muted)' }}>{stats.weeklyXp.reduce((a, b) => a + b, 0)} XP</span>
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7,1fr)', gap: 10, alignItems: 'end', height: 120 }}>
              {['M', 'T', 'W', 'T', 'F', 'S', 'S'].map((d, i) => {
                const xp = stats.weeklyXp[i]
                const isToday = i === todayIdx
                const h = xp > 0 ? Math.max(10, Math.round((xp / maxWeekly) * 100)) : 0
                return (
                  <div key={i} style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 6, height: '100%', justifyContent: 'flex-end' }}>
                    {xp > 0 ? (
                      <div style={{ width: '100%', height: `${h}%`, background: isToday ? 'var(--gold)' : 'var(--xp)', opacity: isToday ? 1 : 0.55, borderRadius: '8px 8px 4px 4px', border: isToday ? '2px dashed var(--gold-sh)' : undefined, boxSizing: 'border-box' }} title={`${xp} XP`} />
                    ) : (
                      <div style={{ width: '100%', height: 4, background: 'var(--track)', borderRadius: 4 }} />
                    )}
                    <span style={{ fontSize: 11, fontWeight: isToday ? 900 : 800, color: isToday ? 'var(--text)' : 'var(--muted)' }}>{d}</span>
                  </div>
                )
              })}
            </div>
          </Card>

          <Card style={{ padding: 22 }}>
            <h2 style={{ margin: '0 0 14px', fontSize: 16, fontWeight: 900 }}>Level {stats.level} · {stats.belt}</h2>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <span style={{ fontSize: 26 }}>{beltEmojiFor(stats.belt)}</span>
              <ProgressBar pct={(stats.xpIntoLevel / stats.xpForNextLevel) * 100} height={14} color="var(--purple-lt)" />
              <span style={{ fontSize: 26, filter: 'grayscale(1)', opacity: 0.5 }}>{beltEmojiFor(nextBelt)}</span>
            </div>
            <p style={{ margin: '12px 0 0', fontSize: 13, fontWeight: 700, color: 'var(--muted)' }}>
              {stats.xpForNextLevel - stats.xpIntoLevel} XP to <strong style={{ color: 'var(--text)' }}>Level {stats.level + 1} — {nextBelt}</strong>.
            </p>
          </Card>
        </div>

        {/* Topics */}
        <Card style={{ padding: 22, marginBottom: 24 }}>
          <h2 style={{ margin: '0 0 16px', fontSize: 16, fontWeight: 900 }}>Topics</h2>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
            {shownTopics.map((t) => {
              const total = t.exerciseSlugs.length
              const done = countSolved(t.exerciseSlugs)
              return (
                <Link key={t.slug} to={`/topics/${t.slug}`} style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                  <span style={{ fontSize: 20, width: 28 }}>{topicMeta(t.slug).emoji}</span>
                  <span style={{ fontWeight: 800, fontSize: 14, width: 260, color: 'var(--text)' }}>{t.name}</span>
                  <ProgressBar pct={total ? (done / total) * 100 : 0} color="var(--purple-lt)" />
                  <span style={{ fontSize: 12, fontWeight: 800, color: 'var(--muted)', width: 44, textAlign: 'right' }}>{done}/{total}</span>
                </Link>
              )
            })}
            {(topics?.length ?? 0) > 5 && (
              <span onClick={() => setShowAllTopics((s) => !s)} style={{ fontSize: 12.5, fontWeight: 800, color: 'var(--purple-deep)', cursor: 'pointer' }}>
                {showAllTopics ? 'SHOW FEWER ▲' : `SHOW ALL ${topics?.length} TOPICS ▼`}
              </span>
            )}
          </div>
        </Card>

        {/* Achievements */}
        <Card style={{ padding: 22 }}>
          <h2 style={{ margin: '0 0 16px', fontSize: 16, fontWeight: 900 }}>Achievements</h2>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3,1fr)', gap: 14 }}>
            {achievements.map((a) => (
              <div key={a.code} style={{
                border: `2px solid ${a.earned ? 'var(--chip-gold-bd)' : 'var(--border)'}`,
                background: a.earned ? 'var(--chip-gold)' : 'var(--surface)',
                borderRadius: 16, padding: 16, display: 'flex', alignItems: 'center', gap: 12, opacity: a.earned ? 1 : 0.65,
              }}>
                <span style={{ fontSize: 30, filter: a.earned ? undefined : 'grayscale(1)' }}>{a.emoji}</span>
                <div>
                  <div style={{ fontWeight: 900, fontSize: 14, color: a.earned ? 'var(--text)' : 'var(--locked-tx)' }}>{a.title}</div>
                  <div style={{ fontSize: 12, fontWeight: 700, color: a.earned ? 'var(--muted)' : 'var(--locked-tx)' }}>{a.description}</div>
                </div>
              </div>
            ))}
          </div>
        </Card>
      </main>
    </SidebarLayout>
  )
}

function StatCard({ emoji, value, label, color }: { emoji: string; value: ReactNode; label: string; color: string }) {
  return (
    <Card style={{ padding: 18, display: 'flex', alignItems: 'center', gap: 12 }}>
      <span style={{ fontSize: 30 }}>{emoji}</span>
      <div>
        <div style={{ fontSize: 22, fontWeight: 900, color }}>{value}</div>
        <div style={{ fontSize: 12, fontWeight: 800, color: 'var(--muted)' }}>{label}</div>
      </div>
    </Card>
  )
}

// Local belt helpers (kept here to avoid an extra import cycle).
function beltNameFor(level: number): string {
  const belts = ['White belt', 'Yellow belt', 'Orange belt', 'Green belt', 'Blue belt', 'Purple belt', 'Purple belt', 'Brown belt', 'Red belt']
  return belts[level - 1] ?? 'Black belt'
}
function beltEmojiFor(belt: string): string {
  const map: Record<string, string> = { 'White belt': '⚪', 'Yellow belt': '🟡', 'Orange belt': '🟠', 'Green belt': '🟢', 'Blue belt': '🔵', 'Purple belt': '🟣', 'Brown belt': '🟤', 'Red belt': '🔴', 'Black belt': '⚫' }
  return map[belt] ?? '🟣'
}
