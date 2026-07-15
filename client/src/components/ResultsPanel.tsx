import type { GradeResult } from '../api/client'

// Renders the outcome of a grading run in the redesign's chunky style: a status banner
// plus per-test cards. The Exercise page shows its own green celebration banner on a
// full pass, so here we focus on the failing/compile-error detail and the test list.
interface Props {
  result: GradeResult | null
  isGrading: boolean
}

export function ResultsPanel({ result, isGrading }: Props) {
  if (isGrading)
    return (
      <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 14, fontWeight: 700, color: 'var(--muted)' }}>
        <span style={{ width: 16, height: 16, borderRadius: 999, border: '2px solid var(--purple-lt)', borderTopColor: 'transparent', display: 'inline-block', animation: 'spin 0.8s linear infinite' }} />
        Compiling &amp; running your code…
      </div>
    )

  if (!result)
    return <p style={{ fontSize: 13, fontWeight: 700, color: 'var(--muted)' }}>Press RUN (or ⌘/Ctrl + Enter) to check your solution.</p>

  const passed = result.status === 'Passed'
  const banner = bannerFor(result)

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
      {!passed && (
        <div style={{ borderRadius: 14, padding: '10px 16px', fontWeight: 900, fontSize: 14, color: '#fff', background: banner.color }}>
          {banner.text}
        </div>
      )}

      {result.compileErrors.length > 0 && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
          {result.compileErrors.map((e, i) => (
            <div key={i} style={{ fontSize: 13, background: 'var(--card)', borderRadius: 12, padding: '10px 14px', border: '2px solid var(--heart)' }}>
              <span style={{ fontFamily: 'ui-monospace,Menlo,monospace', fontSize: 12, color: 'var(--muted)' }}>{e.id}</span> · line {e.line}:{e.column}
              <div style={{ fontWeight: 700 }}>{e.message}</div>
            </div>
          ))}
        </div>
      )}

      {result.testResults.length > 0 && (
        <div className="card" style={{ borderColor: passed ? 'var(--pass-bd)' : 'var(--border)', boxShadow: 'none', borderRadius: 16, padding: '12px 16px', display: 'flex', flexDirection: 'column', gap: 8 }}>
          {result.testResults.map((t, i) => (
            <div key={i}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 13.5, fontWeight: 700 }}>
                <span>{t.passed ? '✅' : '❌'}</span>
                <span>{t.name}</span>
                <span style={{ marginLeft: 'auto', fontSize: 12, color: 'var(--muted)' }}>{t.elapsedMs}ms</span>
              </div>
              {!t.passed && (t.expected !== null || t.actual !== null) && (
                <div style={{ marginTop: 6, fontSize: 13, fontFamily: 'ui-monospace,Menlo,monospace', display: 'grid', gridTemplateColumns: 'auto 1fr', columnGap: 12, rowGap: 2 }}>
                  <span style={{ color: 'var(--muted)' }}>expected</span><span>{t.expected ?? '—'}</span>
                  <span style={{ color: 'var(--muted)' }}>actual</span><span>{t.actual ?? '—'}</span>
                </div>
              )}
              {t.exceptionType && (
                <div style={{ marginTop: 6, fontSize: 13 }}>threw <span style={{ fontFamily: 'ui-monospace,Menlo,monospace' }}>{t.exceptionType}</span>: {t.exceptionMessage}</div>
              )}
              {t.stdout && <pre style={{ marginTop: 6, fontSize: 12, background: 'var(--track)', borderRadius: 8, padding: 8, overflowX: 'auto' }}>{t.stdout}</pre>}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

function bannerFor(r: GradeResult): { text: string; color: string } {
  switch (r.status) {
    case 'Failed': return { text: `${r.passedCount} / ${r.totalCount} tests passed — keep going!`, color: 'var(--gold)' }
    case 'CompileError': return { text: "Doesn't compile — see the errors below", color: 'var(--heart)' }
    case 'Timeout': return { text: '⏱ Timed out — an infinite loop, perhaps?', color: 'var(--heart)' }
    case 'RuntimeError': return { text: 'Runtime error while running your code', color: 'var(--heart)' }
    default: return { text: '', color: 'var(--green)' }
  }
}
