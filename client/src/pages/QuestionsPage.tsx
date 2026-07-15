import { useMemo, useState } from 'react'
import { QUESTIONS } from '../lib/interviewQuestions'
import { SidebarLayout } from '../components/Sidebar'

// An emoji for each Q&A category (falls back to a generic marker).
function emojiFor(category: string): string {
  const c = category.toLowerCase()
  if (c.includes('async') || c.includes('concurren')) return '⚡'
  if (c.includes('memory') || c.includes('gc')) return '🗑️'
  if (c.includes('oop')) return '🧩'
  if (c.includes('linq')) return '🔗'
  if (c.includes('collection')) return '📚'
  if (c.includes('test')) return '🧪'
  if (c.includes('sql') || c.includes('data')) return '🗄️'
  if (c.includes('asp') || c.includes('web')) return '🌍'
  if (c.includes('cloud') || c.includes('devops')) return '🐳'
  if (c.includes('ai') || c.includes('llm')) return '🤖'
  if (c.includes('micro')) return '🕸️'
  if (c.includes('design') || c.includes('architect') || c.includes('system')) return '🏛️'
  if (c.includes('clean')) return '🧼'
  if (c.includes('exception')) return '💥'
  if (c.includes('runtime') || c.includes('.net')) return '⚙️'
  if (c.includes('generic') || c.includes('delegate')) return '🧬'
  return '🆚'
}

export function QuestionsPage() {
  const [tag, setTag] = useState('ALL')
  const [query, setQuery] = useState('')
  const [open, setOpen] = useState<number>(-1)

  const categories = useMemo(() => [...new Set(QUESTIONS.map((q) => q.category))], [])

  const rows = useMemo(() => {
    const q = query.trim().toLowerCase()
    return QUESTIONS.map((item, i) => ({ item, i })).filter(({ item }) => {
      if (tag !== 'ALL' && item.category !== tag) return false
      if (!q) return true
      return item.q.toLowerCase().includes(q) || item.a.toLowerCase().includes(q)
    })
  }, [tag, query])

  return (
    <SidebarLayout>
      <main style={{ padding: '28px 40px 64px', maxWidth: 820, boxSizing: 'border-box' }}>
        <h1 style={{ margin: 0, fontSize: 26, fontWeight: 900 }}>❓ Interview Q&amp;A</h1>
        <p style={{ margin: '4px 0 20px', color: 'var(--muted)', fontWeight: 600 }}>Classic questions, straight answers. Try answering out loud before you reveal — that's the real drill.</p>

        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search questions…"
          style={{ width: '100%', maxWidth: 520, boxSizing: 'border-box', border: '2px solid var(--border)', borderRadius: 16, padding: '12px 18px', fontSize: 15, fontWeight: 700, fontFamily: 'inherit', background: 'var(--card)', color: 'var(--text)', outline: 'none', boxShadow: '0 3px 0 var(--border)', marginBottom: 16 }}
        />

        <div style={{ display: 'flex', gap: 8, margin: '0 0 24px', flexWrap: 'wrap' }}>
          {['ALL', ...categories].map((c) => {
            const active = tag === c
            return (
              <span key={c} onClick={() => setTag(c)}
                style={{ fontSize: 12.5, fontWeight: 800, padding: '6px 14px', borderRadius: 999, cursor: 'pointer', background: active ? 'var(--purple-lt)' : 'var(--track)', color: active ? '#fff' : 'var(--muted)' }}>
                {c.toUpperCase()}
              </span>
            )
          })}
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          {rows.map(({ item, i }) => {
            const isOpen = open === i
            return (
              <div key={i} className="card" style={{ overflow: 'hidden' }}>
                <button
                  onClick={() => setOpen(isOpen ? -1 : i)}
                  style={{ width: '100%', display: 'flex', alignItems: 'center', gap: 14, padding: '18px 22px', background: 'none', border: 'none', cursor: 'pointer', fontFamily: 'inherit', textAlign: 'left', color: 'var(--text)' }}
                >
                  <span style={{ width: 34, height: 34, borderRadius: 999, background: 'var(--chip-purple)', display: 'grid', placeItems: 'center', fontSize: 16, flex: 'none' }}>{emojiFor(item.category)}</span>
                  <span style={{ flex: 1, fontWeight: 800, fontSize: 15.5 }}>{item.q}</span>
                  <span style={{ fontSize: 11, fontWeight: 800, padding: '3px 9px', borderRadius: 999, color: 'var(--purple-deep)', background: 'var(--chip-purple)', whiteSpace: 'nowrap' }}>{item.category}</span>
                  <span style={{ fontSize: 14, color: 'var(--faint)', fontWeight: 900 }}>{isOpen ? '▲' : '▼'}</span>
                </button>
                {isOpen && (
                  <div style={{ padding: '0 22px 20px 70px', fontSize: 14.5, fontWeight: 600, lineHeight: 1.7, color: 'var(--muted)' }}>
                    {item.a}
                    {item.code && <pre style={{ marginTop: 12, fontSize: 12, background: 'var(--track)', borderRadius: 10, padding: 12, overflowX: 'auto' }}>{item.code}</pre>}
                  </div>
                )}
              </div>
            )
          })}
        </div>
      </main>
    </SidebarLayout>
  )
}
