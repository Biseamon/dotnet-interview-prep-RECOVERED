import { useMemo, useState } from 'react'
import { GLOSSARY, type Term } from '../lib/glossary'
import { GLOSSARY_SQL } from '../lib/glossarySql'
import { GLOSSARY_DEVOPS } from '../lib/glossaryDevops'
import { GLOSSARY_AI } from '../lib/glossaryAi'
import { SidebarLayout } from '../components/Sidebar'
import { Card, Chip } from '../components/ui'

// Each glossary domain maps to a chip colour and a label shown on the term cards.
type Tone = 'blue' | 'green' | 'gold' | 'purple'
const DOMAINS: { key: string; label: string; chip: string; tone: Tone; terms: Term[] }[] = [
  { key: 'csharp', label: 'C# & RUNTIME', chip: 'C# & RUNTIME', tone: 'blue', terms: GLOSSARY },
  { key: 'sql', label: 'SQL', chip: 'SQL', tone: 'green', terms: GLOSSARY_SQL },
  { key: 'devops', label: 'DEVOPS', chip: 'DEVOPS', tone: 'gold', terms: GLOSSARY_DEVOPS },
  { key: 'ai', label: 'AI / LLM', chip: 'AI / LLM', tone: 'purple', terms: GLOSSARY_AI },
]

export function GlossaryPage() {
  const [domainKey, setDomainKey] = useState<string>('all')
  const [query, setQuery] = useState('')

  const rows = useMemo(() => {
    const pick = domainKey === 'all' ? DOMAINS : DOMAINS.filter((d) => d.key === domainKey)
    const q = query.trim().toLowerCase()
    return pick.flatMap((d) =>
      d.terms
        .filter((t) => !q || t.term.toLowerCase().includes(q) || t.definition.toLowerCase().includes(q))
        .map((t) => ({ t, domain: d })),
    )
  }, [domainKey, query])

  return (
    <SidebarLayout>
      <main style={{ padding: '28px 40px 64px', maxWidth: 920, boxSizing: 'border-box' }}>
        <h1 style={{ margin: 0, fontSize: 26, fontWeight: 900 }}>📖 Glossary</h1>
        <p style={{ margin: '4px 0 20px', color: 'var(--muted)', fontWeight: 600 }}>Every term you'll meet in lessons and interviews — in plain words.</p>

        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search terms… e.g. boxing, deadlock, LINQ"
          style={{ width: '100%', maxWidth: 520, boxSizing: 'border-box', border: '2px solid var(--border)', borderRadius: 16, padding: '13px 18px', fontSize: 15, fontWeight: 700, fontFamily: 'inherit', background: 'var(--card)', color: 'var(--text)', outline: 'none', boxShadow: '0 3px 0 var(--border)' }}
        />

        <div style={{ display: 'flex', gap: 8, margin: '16px 0 24px', flexWrap: 'wrap' }}>
          {[{ key: 'all', label: 'ALL' }, ...DOMAINS].map((d) => {
            const active = domainKey === d.key
            return (
              <span key={d.key} onClick={() => setDomainKey(d.key)}
                style={{ fontSize: 12.5, fontWeight: 800, padding: '6px 14px', borderRadius: 999, cursor: 'pointer', background: active ? 'var(--purple-lt)' : 'var(--track)', color: active ? '#fff' : 'var(--muted)' }}>
                {'label' in d ? d.label : ''}
              </span>
            )
          })}
        </div>

        {rows.length === 0 && <p style={{ color: 'var(--muted)', fontWeight: 700 }}>No terms match “{query}”.</p>}

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
          {rows.slice(0, 300).map(({ t, domain }) => (
            <Card key={domain.key + t.term} style={{ padding: '20px 22px' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 8 }}>
                <h3 style={{ margin: 0, fontSize: 16, fontWeight: 900 }}>{t.term}</h3>
                <Chip tone={domain.tone}>{t.category || domain.chip}</Chip>
              </div>
              <p style={{ margin: 0, fontSize: 14, fontWeight: 600, lineHeight: 1.6, color: 'var(--muted)' }}>{t.definition}</p>
              {t.example && (
                <pre style={{ fontSize: 12, marginTop: 10, background: 'var(--track)', borderRadius: 8, padding: 8, overflowX: 'auto', whiteSpace: 'pre-wrap' }}>{t.example}</pre>
              )}
            </Card>
          ))}
        </div>
        {rows.length > 300 && <p style={{ color: 'var(--muted)', fontWeight: 700, marginTop: 16 }}>Showing first 300 of {rows.length}. Refine your search to see more.</p>}
      </main>
    </SidebarLayout>
  )
}
