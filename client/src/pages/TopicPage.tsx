import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import ReactMarkdown from 'react-markdown'
import { api } from '../api/client'
import { useProgress } from '../lib/progress'
import { topicMeta } from '../lib/topicMeta'
import { xpForDifficulty } from '../lib/gamification'
import { nextUnsolvedInTopic } from '../lib/nav'
import { TopBar } from '../components/TopBar'
import { Card, Chip, DifficultyChip, ProgressBar, SolvedCircle } from '../components/ui'

export function TopicPage() {
  const { slug } = useParams<{ slug: string }>()
  const { data: topic, isLoading, isError } = useQuery({
    queryKey: ['topic', slug],
    queryFn: () => api.getTopic(slug!),
    enabled: !!slug,
  })
  const { isSolved, solved: solvedSet } = useProgress()

  if (isLoading) return <><TopBar /><p style={{ padding: 24, color: 'var(--muted)' }}>Loading topic…</p></>
  if (isError || !topic) return <><TopBar /><p style={{ padding: 24, color: 'var(--heart)' }}>Topic not found.</p></>

  const meta = topicMeta(topic.slug)
  const allExercises = topic.lessons.flatMap((l) => l.exercises)
  const solvedCount = allExercises.filter((e) => isSolved(e.slug)).length
  const xpEarned = allExercises.filter((e) => isSolved(e.slug)).reduce((s, e) => s + xpForDifficulty(e.difficulty), 0)
  const upNext = nextUnsolvedInTopic(topic, solvedSet)
  const pct = allExercises.length ? (solvedCount / allExercises.length) * 100 : 0

  return (
    <div style={{ minHeight: '100vh' }}>
      <TopBar />
      <main style={{ maxWidth: 900, margin: '0 auto', padding: '28px 24px 56px', display: 'flex', flexDirection: 'column', gap: 20, boxSizing: 'border-box' }}>
        <Link to="/" style={{ fontSize: 14, fontWeight: 800, color: 'var(--muted)', alignSelf: 'flex-start' }}>← Back to learn</Link>

        {/* Hero */}
        <div style={{ background: 'var(--purple-lt)', borderRadius: 22, boxShadow: '0 5px 0 var(--purple-lt-sh)', padding: '24px 28px', color: '#fff', display: 'flex', alignItems: 'center', gap: 20 }}>
          <div style={{ width: 72, height: 72, borderRadius: 20, background: 'rgba(255,255,255,0.18)', display: 'grid', placeItems: 'center', fontSize: 38, flex: 'none' }}>{meta.emoji}</div>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: 12, fontWeight: 800, letterSpacing: '0.08em', opacity: 0.85 }}>{meta.category.toUpperCase()}</div>
            <h1 style={{ margin: '2px 0 0', fontSize: 24, fontWeight: 900 }}>{topic.name}</h1>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginTop: 12 }}>
              <div style={{ flex: 1, maxWidth: 320 }}><ProgressBar pct={pct} height={14} color="var(--card)" track="rgba(255,255,255,0.25)" /></div>
              <span style={{ fontSize: 13, fontWeight: 800 }}>{solvedCount}/{allExercises.length} · {xpEarned} XP earned</span>
            </div>
          </div>
          {upNext && (
            <Link to={`/exercises/${upNext}`} style={{ background: 'var(--card)', color: 'var(--purple-deep)', fontWeight: 900, fontSize: 14, borderRadius: 14, padding: '12px 20px', boxShadow: '0 4px 0 rgba(0,0,0,0.18)', letterSpacing: '0.03em' }}>CONTINUE ▶</Link>
          )}
        </div>

        {/* Lessons */}
        {topic.lessons.map((lesson, i) => (
          <LessonCard key={lesson.slug} lesson={lesson} index={i} upNext={upNext} isSolved={isSolved} />
        ))}
      </main>
    </div>
  )
}

function LessonCard({ lesson, index, upNext, isSolved }: {
  lesson: { slug: string; title: string; markdownContent: string; exercises: { slug: string; title: string; difficulty: string }[] }
  index: number
  upNext: string | null
  isSolved: (s: string) => boolean
}) {
  const [open, setOpen] = useState(false)
  const solved = lesson.exercises.filter((e) => isSolved(e.slug)).length
  const total = lesson.exercises.length
  const chipTone = solved === total && total > 0 ? 'green' : solved > 0 ? 'gold' : 'muted'

  return (
    <Card style={{ padding: '22px 24px' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 4 }}>
        <h2 style={{ margin: 0, fontSize: 17, fontWeight: 900 }}>Lesson {index + 1} · {lesson.title}</h2>
        <Chip tone={chipTone}>{solved} OF {total} SOLVED</Chip>
        <button
          onClick={() => setOpen((o) => !o)}
          style={{ marginLeft: 'auto', border: '2px solid var(--border)', background: 'var(--card)', cursor: 'pointer', fontFamily: 'inherit', fontWeight: 800, fontSize: 12.5, color: 'var(--purple-deep)', borderRadius: 12, padding: '7px 12px', boxShadow: '0 3px 0 var(--border)' }}
        >
          📖 {open ? 'HIDE LESSON' : 'READ LESSON'}
        </button>
      </div>

      {open && (
        <div className="prose-dojo" style={{ fontSize: 14, margin: '12px 0', borderRadius: 16, padding: 16, background: 'var(--track)' }}>
          <ReactMarkdown>{lesson.markdownContent}</ReactMarkdown>
        </div>
      )}

      <div style={{ display: 'flex', flexDirection: 'column' }}>
        {lesson.exercises.map((ex) => {
          const done = isSolved(ex.slug)
          const isUpNext = ex.slug === upNext
          const xp = xpForDifficulty(ex.difficulty)
          return (
            <Link
              key={ex.slug}
              to={`/exercises/${ex.slug}`}
              style={{
                display: 'flex', alignItems: 'center', gap: 14, padding: '13px 10px', margin: '0 -10px', borderRadius: 14,
                background: isUpNext ? 'var(--chip-purple)' : undefined,
                border: isUpNext ? '2px solid #c9bdf2' : undefined,
              }}
            >
              <SolvedCircle solved={done} upNext={isUpNext} />
              <span style={{ fontWeight: 800, fontSize: 15, color: isUpNext ? 'var(--purple-deep)' : 'var(--text)' }}>{ex.title}</span>
              <DifficultyChip difficulty={ex.difficulty} />
              <span style={{ marginLeft: 'auto', fontSize: 12, fontWeight: isUpNext ? 900 : 800, color: isUpNext ? 'var(--purple-lt)' : done ? 'var(--faint)' : 'var(--muted)' }}>
                {isUpNext ? `UP NEXT · +${xp} XP` : done ? `⚡ ${xp} XP earned` : `+${xp} XP`}
              </span>
            </Link>
          )
        })}
      </div>
    </Card>
  )
}
