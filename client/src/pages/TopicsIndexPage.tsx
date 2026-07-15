import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { useProgress } from '../lib/progress'
import { CATEGORY_ORDER, topicMeta } from '../lib/topicMeta'
import { SidebarLayout } from '../components/Sidebar'
import { Card, ProgressBar } from '../components/ui'

// A simple catalogue of every topic, grouped by category — the TOPICS nav destination.
export function TopicsIndexPage() {
  const { data: topics } = useQuery({ queryKey: ['topics'], queryFn: api.getTopics })
  const { countSolved } = useProgress()

  const byCategory = new Map<string, typeof topics>()
  for (const t of topics ?? []) {
    const cat = topicMeta(t.slug).category
    if (!byCategory.has(cat)) byCategory.set(cat, [])
    byCategory.get(cat)!.push(t)
  }
  const categories = CATEGORY_ORDER.filter((c) => byCategory.has(c))

  return (
    <SidebarLayout>
      <main style={{ padding: '28px 40px 64px', maxWidth: 920, boxSizing: 'border-box' }}>
        <h1 style={{ margin: '0 0 4px', fontSize: 26, fontWeight: 900 }}>🧮 All topics</h1>
        <p style={{ margin: '0 0 24px', color: 'var(--muted)', fontWeight: 600 }}>Pick a topic to open its learning path.</p>

        {!topics && <p style={{ color: 'var(--muted)' }}>Loading…</p>}

        {categories.map((cat) => (
          <section key={cat} style={{ marginBottom: 28 }}>
            <h2 style={{ fontSize: 13, fontWeight: 900, textTransform: 'uppercase', letterSpacing: '0.06em', color: 'var(--muted)', marginBottom: 12 }}>{cat}</h2>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2,1fr)', gap: 14 }}>
              {byCategory.get(cat)!.map((t) => {
                const meta = topicMeta(t.slug)
                const total = t.exerciseSlugs.length
                const done = countSolved(t.exerciseSlugs)
                return (
                  <Link key={t.slug} to={`/topics/${t.slug}`}>
                    <Card style={{ padding: 18, display: 'flex', flexDirection: 'column', gap: 12, height: '100%' }}>
                      <div style={{ display: 'flex', alignItems: 'flex-start', gap: 12 }}>
                        <span style={{ width: 44, height: 44, borderRadius: 14, display: 'grid', placeItems: 'center', fontSize: 24, flex: 'none', background: `color-mix(in srgb, ${meta.color} 18%, transparent)` }}>{meta.emoji}</span>
                        <div style={{ minWidth: 0 }}>
                          <div style={{ fontWeight: 900, fontSize: 15 }}>{t.name}</div>
                          <div style={{ fontSize: 12.5, fontWeight: 600, color: 'var(--muted)', overflow: 'hidden', textOverflow: 'ellipsis', display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical' }}>{t.description}</div>
                        </div>
                      </div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 'auto' }}>
                        <ProgressBar pct={total ? (done / total) * 100 : 0} color={meta.color} />
                        <span style={{ fontSize: 12, fontWeight: 800, color: 'var(--muted)', width: 44, textAlign: 'right' }}>{done}/{total}</span>
                      </div>
                    </Card>
                  </Link>
                )
              })}
            </div>
          </section>
        ))}
      </main>
    </SidebarLayout>
  )
}
