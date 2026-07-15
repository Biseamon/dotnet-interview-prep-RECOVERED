import { useEffect, useRef, useState } from 'react'
import { SidebarLayout } from '../components/Sidebar'
import { Card } from '../components/ui'
import { RESUME_TEMPLATES } from '../lib/templateMeta'
import { api } from '../api/client'
import type {
  AtsCheck,
  ResumeAnalysis,
  ResumeConfig,
  ResumeExperience,
  ResumeExportFormat,
  ResumeModel,
} from '../api/client'

// ---- Local-draft persistence (no DB: resume PII stays in the browser) ----
const LS_MODEL = 'resume:model'
const LS_JD = 'resume:jd'
const LS_TEMPLATE = 'resume:template'

function loadModel(): ResumeModel | null {
  try {
    const raw = localStorage.getItem(LS_MODEL)
    return raw ? (JSON.parse(raw) as ResumeModel) : null
  } catch {
    return null
  }
}

function emptyResume(): ResumeModel {
  return {
    contact: { fullName: '', title: '', email: '', phone: '', location: '', website: '' },
    summary: '',
    experience: [{ company: '', role: '', startDate: '', endDate: '', location: '', bullets: [''] }],
    education: [{ school: '', degree: '', startDate: '', endDate: '', details: '' }],
    skills: [],
  }
}

export function ResumeBuilderPage() {
  const [resume, setResume] = useState<ResumeModel | null>(loadModel)
  const [jd, setJd] = useState(() => localStorage.getItem(LS_JD) ?? '')
  const [template, setTemplate] = useState(() => localStorage.getItem(LS_TEMPLATE) ?? 'classic')
  const [analysis, setAnalysis] = useState<ResumeAnalysis | null>(null)
  const [applied, setApplied] = useState<Set<string>>(new Set())
  const [config, setConfig] = useState<ResumeConfig | null>(null)

  const [parsing, setParsing] = useState(false)
  const [analyzing, setAnalyzing] = useState(false)
  const [exporting, setExporting] = useState<ResumeExportFormat | null>(null)
  const [error, setError] = useState<string | null>(null)

  // Persist the draft as it changes.
  useEffect(() => {
    if (resume) localStorage.setItem(LS_MODEL, JSON.stringify(resume))
  }, [resume])
  useEffect(() => { localStorage.setItem(LS_JD, jd) }, [jd])
  useEffect(() => { localStorage.setItem(LS_TEMPLATE, template) }, [template])

  // Which local model is configured — shown in the "AI unavailable" hint.
  useEffect(() => { api.getResumeConfig().then(setConfig).catch(() => {}) }, [])

  // ---- Actions ----

  const parseProg = useProgress(parsing, 35) // first parse loads the model, so estimate longer
  const analyzeProg = useProgress(analyzing, 30)

  async function handleFile(file: File) {
    setError(null)
    setParsing(true)
    const t0 = performance.now()
    console.log(`[resume] ⬆ uploading "${file.name}" (${(file.size / 1024).toFixed(0)} KB) — the local model parses it; first call can take 30–60s while the model loads.`)
    try {
      const model = await api.parseResume(file)
      console.log(`[resume] ✓ parsed in ${((performance.now() - t0) / 1000).toFixed(1)}s`, model)
      setResume(model)
      setAnalysis(null)
      setApplied(new Set())
    } catch (e) {
      console.error('[resume] ✗ parse failed:', e)
      setError(e instanceof Error ? e.message : 'Failed to read that file.')
    } finally {
      setParsing(false)
    }
  }

  async function runAnalyze() {
    if (!resume || !jd.trim()) return
    setError(null)
    setAnalyzing(true)
    const t0 = performance.now()
    console.log('[resume] ⬆ analyzing against job description — waiting for the local model…')
    try {
      const result = await api.analyzeResume(resume, jd)
      console.log(`[resume] ✓ analyzed in ${((performance.now() - t0) / 1000).toFixed(1)}s — match ${result.matchScore}`, result)
      setAnalysis(result)
    } catch (e) {
      console.error('[resume] ✗ analyze failed:', e)
      setError(e instanceof Error ? e.message : 'Analysis failed.')
    } finally {
      setAnalyzing(false)
    }
  }

  async function runExport(format: ResumeExportFormat) {
    if (!resume) return
    setError(null)
    setExporting(format)
    const t0 = performance.now()
    console.log(`[resume] ⬇ exporting ${format} (template: ${template})…`)
    try {
      const blob = await api.exportResume(resume, template, format)
      const name = (resume.contact.fullName || 'resume').replace(/[^a-z0-9]+/gi, '_')
      downloadBlob(blob, `${name}_Resume.${format.toLowerCase()}`)
      console.log(`[resume] ✓ ${format} downloaded in ${((performance.now() - t0) / 1000).toFixed(1)}s (${(blob.size / 1024).toFixed(0)} KB)`)
    } catch (e) {
      console.error('[resume] ✗ export failed:', e)
      setError(e instanceof Error ? e.message : 'Export failed.')
    } finally {
      setExporting(null)
    }
  }

  function applySuggestion(ei: number, bi: number, text: string, key: string) {
    if (!text.trim()) return
    setResume((prev) => {
      if (!prev) return prev
      const experience = prev.experience.map((exp, i) =>
        i === ei ? { ...exp, bullets: exp.bullets.map((b, j) => (j === bi ? text : b)) } : exp,
      )
      return { ...prev, experience }
    })
    setApplied((prev) => new Set(prev).add(key))
  }

  function applySummary(text: string) {
    setResume((prev) => (prev ? { ...prev, summary: text } : prev))
    setApplied((prev) => new Set(prev).add('summary'))
  }

  function addSkill(kw: string) {
    setResume((prev) => {
      if (!prev) return prev
      if (prev.skills.some((s) => s.toLowerCase() === kw.toLowerCase())) return prev
      return { ...prev, skills: [...prev.skills, kw] }
    })
  }

  function addAllSkills(kws: string[]) {
    setResume((prev) => {
      if (!prev) return prev
      const have = new Set(prev.skills.map((s) => s.toLowerCase()))
      const toAdd = kws.filter((k) => !have.has(k.toLowerCase()))
      return toAdd.length ? { ...prev, skills: [...prev.skills, ...toAdd] } : prev
    })
  }

  // Rewrite every experience/education date to a consistent "Mon YYYY" — the one-click fix
  // for the "Consistent date formatting" ATS check. Best-effort parse of common formats.
  function normalizeAllDates() {
    setResume((prev) => {
      if (!prev) return prev
      return {
        ...prev,
        experience: prev.experience.map((e) => ({ ...e, startDate: normalizeDate(e.startDate), endDate: normalizeDate(e.endDate) })),
        education: prev.education.map((e) => ({ ...e, startDate: normalizeDate(e.startDate), endDate: normalizeDate(e.endDate) })),
      }
    })
  }

  // ---- Render ----

  return (
    <SidebarLayout>
      <main style={{ padding: '28px 40px 64px', boxSizing: 'border-box', width: '100%', minWidth: 0 }}>
        <header style={{ marginBottom: 20 }}>
          <h1 style={{ fontSize: 28, fontWeight: 900, margin: 0 }}>📄 Resume Builder</h1>
          <p style={{ color: 'var(--muted)', fontWeight: 700, marginTop: 6, fontSize: 14 }}>
            Upload your resume, tailor it to a job with local AI, and export an ATS-safe PDF or DOCX.
            {config && <> {' '}<span style={{ color: 'var(--muted)' }}>AI model: <code>{config.model}</code> (local).</span></>}
          </p>
        </header>

        {error && (
          <div style={{ marginBottom: 16, borderRadius: 14, padding: '12px 16px', fontWeight: 800, fontSize: 13.5, color: '#fff', background: 'var(--heart)' }}>
            {error}
          </div>
        )}

        <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0,1fr) minmax(0,1fr)', gap: 20, alignItems: 'start' }}>
          {/* ---------------- LEFT: input + editing ---------------- */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 16, minWidth: 0 }}>
            <Dropzone onFile={handleFile} parsing={parsing} secs={parseProg.secs} pct={parseProg.pct} hasResume={!!resume} onStartBlank={() => setResume(emptyResume())} />

            {resume && (
              <>
                <TemplatePicker value={template} onChange={setTemplate} />

                <Card style={{ padding: 16 }}>
                  <SectionLabel>Target job description</SectionLabel>
                  <textarea
                    value={jd}
                    onChange={(e) => setJd(e.target.value)}
                    placeholder="Paste the job posting here — the AI tailors your resume and scores keyword match against it."
                    style={{ ...textareaStyle, minHeight: 120 }}
                  />
                  <button
                    className="btn-3d btn-purple"
                    disabled={analyzing || !jd.trim()}
                    onClick={runAnalyze}
                    style={{ marginTop: 12, opacity: analyzing || !jd.trim() ? 0.6 : 1 }}
                  >
                    {analyzing ? 'Analyzing…' : '✨ Analyze & tailor'}
                  </button>
                </Card>

                <ResumeEditor resume={resume} setResume={setResume} />
              </>
            )}
          </div>

          {/* ---------------- RIGHT: analysis + preview + export ---------------- */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 16, minWidth: 0, position: 'sticky', top: 16 }}>
            {resume ? (
              <>
                <AnalysisPanel
                  analysis={analysis}
                  analyzing={analyzing}
                  secs={analyzeProg.secs}
                  pct={analyzeProg.pct}
                  applied={applied}
                  skills={resume.skills}
                  onApplyBullet={applySuggestion}
                  onApplySummary={applySummary}
                  onAddSkill={addSkill}
                  onAddAllSkills={addAllSkills}
                  onNormalizeDates={normalizeAllDates}
                />

                <Card style={{ padding: 16 }}>
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12, gap: 8, flexWrap: 'wrap' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      <SectionLabel style={{ margin: 0 }}>Preview & export</SectionLabel>
                      <PageBadge resume={resume} />
                    </div>
                    <div style={{ display: 'flex', gap: 8 }}>
                      <button className="btn-3d btn-green" disabled={exporting !== null} onClick={() => runExport('Pdf')} style={{ padding: '8px 14px', fontSize: 13 }}>
                        {exporting === 'Pdf' ? '…' : '⬇ PDF'}
                      </button>
                      <button className="btn-3d btn-blue" disabled={exporting !== null} onClick={() => runExport('Docx')} style={{ padding: '8px 14px', fontSize: 13 }}>
                        {exporting === 'Docx' ? '…' : '⬇ DOCX'}
                      </button>
                    </div>
                  </div>
                  <ResumePreview resume={resume} template={template} />
                </Card>
              </>
            ) : (
              <Card style={{ padding: 24, textAlign: 'center', color: 'var(--muted)', fontWeight: 700 }}>
                Upload a resume (or start blank) to see the tailored analysis and live preview here.
              </Card>
            )}
          </div>
        </div>
      </main>
    </SidebarLayout>
  )
}

// =============================== Left-side pieces ===============================

function Dropzone({ onFile, parsing, secs, pct, hasResume, onStartBlank }: { onFile: (f: File) => void; parsing: boolean; secs: number; pct: number; hasResume: boolean; onStartBlank: () => void }) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [drag, setDrag] = useState(false)
  return (
    <Card style={{ padding: 16 }}>
      <div
        onClick={() => !parsing && inputRef.current?.click()}
        onDragOver={(e) => { e.preventDefault(); setDrag(true) }}
        onDragLeave={() => setDrag(false)}
        onDrop={(e) => { e.preventDefault(); setDrag(false); if (parsing) return; const f = e.dataTransfer.files?.[0]; if (f) onFile(f) }}
        style={{
          border: `2px dashed ${drag ? 'var(--purple-lt)' : 'var(--border)'}`,
          background: drag ? 'var(--nav-bg)' : 'transparent',
          borderRadius: 16, padding: '28px 16px', textAlign: 'center', cursor: parsing ? 'default' : 'pointer',
        }}
      >
        <div style={{ fontSize: 32 }}>{parsing ? '⏳' : '📤'}</div>
        <div style={{ fontWeight: 800, marginTop: 8, fontSize: 15 }}>
          {parsing ? `Reading your resume… ${secs}s` : hasResume ? 'Upload a different resume' : 'Drop your resume or click to upload'}
        </div>
        <div style={{ fontSize: 12.5, color: 'var(--muted)', fontWeight: 700, marginTop: 4 }}>
          {parsing ? 'The local AI is parsing it — the first run loads the model (30–60s).' : 'PDF, DOCX, or plain text · parsed on-device'}
        </div>
        {parsing && <div style={{ marginTop: 12 }}><FillBar pct={pct} /></div>}
        <input ref={inputRef} type="file" accept=".pdf,.docx,.txt,.md" hidden onChange={(e) => { const f = e.target.files?.[0]; if (f) onFile(f); e.currentTarget.value = '' }} />
      </div>
      {!hasResume && (
        <button className="btn-3d btn-ghost" onClick={onStartBlank} style={{ marginTop: 10, width: '100%', fontSize: 13 }}>
          …or start from a blank resume
        </button>
      )}
    </Card>
  )
}

function TemplatePicker({ value, onChange }: { value: string; onChange: (slug: string) => void }) {
  return (
    <Card style={{ padding: 16 }}>
      <SectionLabel>ATS template</SectionLabel>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 8 }}>
        {RESUME_TEMPLATES.map((t) => {
          const active = t.slug === value
          return (
            <button
              key={t.slug}
              onClick={() => onChange(t.slug)}
              title={t.description}
              style={{
                border: `2px solid ${active ? 'var(--nav-bd)' : 'var(--border)'}`,
                background: active ? 'var(--nav-bg)' : 'var(--card)',
                color: active ? 'var(--nav-tx)' : 'var(--text)',
                borderRadius: 14, padding: '12px 8px', cursor: 'pointer', fontFamily: 'inherit',
                fontWeight: 800, fontSize: 13, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4,
              }}
            >
              <span style={{ fontSize: 22 }}>{t.emoji}</span>
              {t.name}
            </button>
          )
        })}
      </div>
      <p style={{ fontSize: 12, color: 'var(--muted)', fontWeight: 700, marginTop: 8, marginBottom: 0 }}>
        All templates are single-column, standard fonts, no tables or graphics — the format ATS parsers read cleanly.
      </p>
    </Card>
  )
}

// The structured-field editor. Kept compact; every field is bound so applied
// suggestions and manual edits both flow into the same model.
function ResumeEditor({ resume, setResume }: { resume: ResumeModel; setResume: React.Dispatch<React.SetStateAction<ResumeModel | null>> }) {
  const patch = (p: Partial<ResumeModel>) => setResume((prev) => (prev ? { ...prev, ...p } : prev))
  const patchContact = (k: keyof ResumeModel['contact'], v: string) =>
    setResume((prev) => (prev ? { ...prev, contact: { ...prev.contact, [k]: v } } : prev))
  const patchExp = (i: number, p: Partial<ResumeExperience>) =>
    setResume((prev) => (prev ? { ...prev, experience: prev.experience.map((e, j) => (j === i ? { ...e, ...p } : e)) } : prev))

  return (
    <Card style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 14 }}>
      <SectionLabel style={{ margin: 0 }}>Resume content</SectionLabel>

      {/* Contact */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
        <Field label="Full name" value={resume.contact.fullName} onChange={(v) => patchContact('fullName', v)} />
        <Field label="Headline / title" value={resume.contact.title} onChange={(v) => patchContact('title', v)} />
        <Field label="Email" value={resume.contact.email} onChange={(v) => patchContact('email', v)} />
        <Field label="Phone" value={resume.contact.phone} onChange={(v) => patchContact('phone', v)} />
        <Field label="Location" value={resume.contact.location} onChange={(v) => patchContact('location', v)} />
        <Field label="Website / LinkedIn" value={resume.contact.website} onChange={(v) => patchContact('website', v)} />
      </div>

      <div>
        <FieldLabel>Professional summary</FieldLabel>
        <textarea value={resume.summary} onChange={(e) => patch({ summary: e.target.value })} style={{ ...textareaStyle, minHeight: 72 }} />
      </div>

      {/* Experience */}
      <div>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <FieldLabel>Experience</FieldLabel>
          <SmallButton onClick={() => patch({ experience: [...resume.experience, { company: '', role: '', startDate: '', endDate: '', location: '', bullets: [''] }] })}>+ job</SmallButton>
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          {resume.experience.map((exp, i) => (
            <div key={i} style={{ border: '2px solid var(--border)', borderRadius: 12, padding: 10 }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
                <Field label="Role" value={exp.role} onChange={(v) => patchExp(i, { role: v })} />
                <Field label="Company" value={exp.company} onChange={(v) => patchExp(i, { company: v })} />
                <Field label="Start (e.g. Jan 2022)" value={exp.startDate} onChange={(v) => patchExp(i, { startDate: v })} />
                <Field label="End (or Present)" value={exp.endDate} onChange={(v) => patchExp(i, { endDate: v })} />
              </div>
              <FieldLabel style={{ marginTop: 8 }}>Bullets</FieldLabel>
              {exp.bullets.map((b, j) => (
                <div key={j} style={{ display: 'flex', gap: 6, marginBottom: 6 }}>
                  <textarea
                    value={b}
                    onChange={(e) => patchExp(i, { bullets: exp.bullets.map((x, k) => (k === j ? e.target.value : x)) })}
                    style={{ ...textareaStyle, minHeight: 40, flex: 1 }}
                  />
                  <SmallButton onClick={() => patchExp(i, { bullets: exp.bullets.filter((_, k) => k !== j) })}>✕</SmallButton>
                </div>
              ))}
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <SmallButton onClick={() => patchExp(i, { bullets: [...exp.bullets, ''] })}>+ bullet</SmallButton>
                <SmallButton onClick={() => patch({ experience: resume.experience.filter((_, k) => k !== i) })}>remove job</SmallButton>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Skills */}
      <div>
        <FieldLabel>Skills (comma-separated)</FieldLabel>
        <textarea
          value={resume.skills.join(', ')}
          onChange={(e) => patch({ skills: e.target.value.split(',').map((s) => s.trim()).filter(Boolean) })}
          style={{ ...textareaStyle, minHeight: 48 }}
        />
      </div>
    </Card>
  )
}

// =============================== Right-side pieces ===============================

function AnalysisPanel({
  analysis, analyzing, secs, pct, applied, skills, onApplyBullet, onApplySummary, onAddSkill, onAddAllSkills, onNormalizeDates,
}: {
  analysis: ResumeAnalysis | null
  analyzing: boolean
  secs: number
  pct: number
  applied: Set<string>
  skills: string[]
  onApplyBullet: (ei: number, bi: number, text: string, key: string) => void
  onApplySummary: (text: string) => void
  onAddSkill: (kw: string) => void
  onAddAllSkills: (kws: string[]) => void
  onNormalizeDates: () => void
}) {
  if (analyzing)
    return (
      <Card style={{ padding: 20, display: 'flex', flexDirection: 'column', gap: 12 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, color: 'var(--muted)', fontWeight: 800, fontSize: 14 }}>
          <span style={{ width: 16, height: 16, borderRadius: 999, border: '2px solid var(--purple-lt)', borderTopColor: 'transparent', display: 'inline-block', animation: 'spin 0.8s linear infinite' }} />
          The local AI is tailoring your resume… {secs}s
        </div>
        <FillBar pct={pct} />
      </Card>
    )

  if (!analysis)
    return (
      <Card style={{ padding: 20, color: 'var(--muted)', fontWeight: 700, fontSize: 13.5 }}>
        Paste a job description and press <b>Analyze &amp; tailor</b> to get an ATS match score, keyword gaps, and bullet rewrites.
      </Card>
    )

  const score = analysis.matchScore
  const scoreColor = score >= 80 ? 'var(--green)' : score >= 60 ? 'var(--gold)' : 'var(--heart)'

  return (
    <Card style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 14 }}>
      {/* Match score */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
        <div style={{ fontSize: 40, fontWeight: 900, color: scoreColor, lineHeight: 1 }}>{score}</div>
        <div style={{ flex: 1 }}>
          <div style={{ fontWeight: 900, fontSize: 13, letterSpacing: '0.04em' }}>JOB MATCH SCORE</div>
          <div style={{ height: 10, borderRadius: 999, background: 'var(--track)', overflow: 'hidden', marginTop: 4 }}>
            <div style={{ width: `${Math.max(0, Math.min(100, score))}%`, height: '100%', background: scoreColor, borderRadius: 999, transition: 'width .5s ease' }} />
          </div>
          <div style={{ fontSize: 12, color: 'var(--muted)', fontWeight: 700, marginTop: 4 }}>Aim for ~75–85 — competitive, but still natural to read.</div>
        </div>
      </div>

      {analysis.summary && <p style={{ margin: 0, fontSize: 13.5, fontWeight: 600 }}>{analysis.summary}</p>}

      {/* ATS checklist — each Warn/Fail with a safe auto-fix gets a Fix button */}
      {analysis.atsChecks.length > 0 && (
        <div>
          <SectionLabel>ATS checklist</SectionLabel>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
            {analysis.atsChecks.map((c) => {
              let action: { label: string; onClick: () => void } | undefined
              if (c.status !== 'Pass') {
                if (c.id === 'dates') action = { label: 'Normalize to Month YYYY', onClick: onNormalizeDates }
                else if (c.id === 'sections' && analysis.missingKeywords.length > 0)
                  action = { label: 'Add missing skills', onClick: () => onAddAllSkills(analysis.missingKeywords) }
              }
              return <AtsRow key={c.id} check={c} action={action} />
            })}
          </div>
        </div>
      )}

      {/* Missing keywords — click a chip to add it to the Skills section */}
      {analysis.missingKeywords.length > 0 && (
        <div>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 8 }}>
            <SectionLabel style={{ margin: 0 }}>Missing keywords</SectionLabel>
            <SmallButton onClick={() => onAddAllSkills(analysis.missingKeywords)}>＋ Add all to Skills</SmallButton>
          </div>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6 }}>
            {analysis.missingKeywords.map((k, i) => {
              const added = skills.some((s) => s.toLowerCase() === k.toLowerCase())
              return (
                <button
                  key={i}
                  onClick={() => onAddSkill(k)}
                  disabled={added}
                  title={added ? 'Already in your Skills' : 'Add to your Skills section'}
                  style={{
                    fontSize: 11, fontWeight: 800, padding: '3px 9px', borderRadius: 999, border: 'none', fontFamily: 'inherit',
                    color: added ? 'var(--muted)' : 'var(--chip-pink-tx)', background: added ? 'var(--track)' : 'var(--chip-pink)',
                    cursor: added ? 'default' : 'pointer', whiteSpace: 'nowrap',
                  }}
                >
                  {added ? '✓ ' : '＋ '}{k}
                </button>
              )
            })}
          </div>
          <p style={{ fontSize: 11, color: 'var(--muted)', fontWeight: 600, marginTop: 6, marginBottom: 0 }}>
            Click a keyword to add it to Skills — only ones you genuinely have.
          </p>
        </div>
      )}

      {/* Strengths */}
      {analysis.strengths.length > 0 && (
        <div>
          <SectionLabel>Strengths</SectionLabel>
          <ul style={{ margin: 0, paddingLeft: 18, fontSize: 13, fontWeight: 600, display: 'flex', flexDirection: 'column', gap: 3 }}>
            {analysis.strengths.map((s, i) => <li key={i}>{s}</li>)}
          </ul>
        </div>
      )}

      {/* Summary rewrite */}
      {analysis.summarySuggestion && (
        <div>
          <SectionLabel>Suggested summary</SectionLabel>
          <SuggestionCard suggested={analysis.summarySuggestion} reason="Tailored to the job description." applied={applied.has('summary')} onApply={() => onApplySummary(analysis.summarySuggestion!)} />
        </div>
      )}

      {/* Bullet rewrites (skip any the model returned without real text) */}
      {(() => {
        const rewrites = analysis.bulletSuggestions.filter((s) => s.suggested.trim())
        return (
          <div>
            <SectionLabel>Bullet rewrites</SectionLabel>
            {rewrites.length === 0 ? (
              <p style={{ fontSize: 13, color: 'var(--muted)', fontWeight: 600, margin: 0 }}>
                No specific bullet rewrites this round — your bullets already read well, or re-run Analyze (a larger local model gives more).
              </p>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
                {rewrites.map((s, i) => {
                  const key = `${s.experienceIndex}:${s.bulletIndex}:${i}`
                  return (
                    <SuggestionCard
                      key={key}
                      original={s.original}
                      suggested={s.suggested}
                      reason={s.reason}
                      applied={applied.has(key)}
                      onApply={() => onApplyBullet(s.experienceIndex, s.bulletIndex, s.suggested, key)}
                    />
                  )
                })}
              </div>
            )}
          </div>
        )
      })()}
    </Card>
  )
}

function AtsRow({ check, action }: { check: AtsCheck; action?: { label: string; onClick: () => void } }) {
  const map = {
    Pass: { icon: '✅', color: 'var(--green)' },
    Warn: { icon: '⚠️', color: 'var(--gold)' },
    Fail: { icon: '❌', color: 'var(--heart)' },
  } as const
  const m = map[check.status] ?? map.Warn
  return (
    <div style={{ display: 'flex', gap: 8, fontSize: 13 }}>
      <span>{m.icon}</span>
      <div style={{ flex: 1, minWidth: 0 }}>
        <span style={{ fontWeight: 800, color: m.color }}>{check.label}</span>
        <span style={{ fontWeight: 600, color: 'var(--muted)' }}> — {check.detail}</span>
        {action && (
          <div style={{ marginTop: 5 }}>
            <button className="btn-3d btn-green" onClick={action.onClick} style={{ padding: '4px 11px', fontSize: 11.5 }}>
              🛠 {action.label}
            </button>
          </div>
        )}
      </div>
    </div>
  )
}

function SuggestionCard({ original, suggested, reason, applied, onApply }: { original?: string; suggested: string; reason: string; applied: boolean; onApply: () => void }) {
  // A rewrite containing a [bracketed] placeholder is a metric example — the candidate
  // applies it and then swaps the placeholder for their real figure.
  const isExample = /\[[^\]]+\]/.test(suggested)
  return (
    <div style={{ border: '2px solid var(--border)', borderRadius: 12, padding: 10, fontSize: 13 }}>
      {original && <div style={{ color: 'var(--muted)', textDecoration: 'line-through', marginBottom: 4 }}>{original}</div>}
      <div style={{ fontWeight: 700 }}>{suggested}</div>
      {reason && <div style={{ color: 'var(--muted)', fontSize: 12, fontWeight: 600, marginTop: 4 }}>💡 {reason}</div>}
      {isExample && (
        <div style={{ marginTop: 6 }}>
          <span style={{ fontSize: 11, fontWeight: 800, padding: '2px 8px', borderRadius: 999, background: 'var(--chip-gold)', color: 'var(--chip-gold-tx)' }}>
            ✏️ Example — replace the [bracketed] number with your real figure
          </span>
        </div>
      )}
      <div style={{ marginTop: 8 }}>
        <button
          className={`btn-3d ${applied ? 'btn-ghost' : 'btn-green'}`}
          onClick={onApply}
          disabled={applied}
          style={{ padding: '5px 12px', fontSize: 12 }}
        >
          {applied ? '✓ Applied' : isExample ? 'Use this template' : 'Apply'}
        </button>
      </div>
    </div>
  )
}

// A light HTML approximation of the selected template for the live preview. The real
// PDF/DOCX come from the server renderer; this just gives the user a visual read.
function ResumePreview({ resume, template }: { resume: ResumeModel; template: string }) {
  const s = previewStyle(template)
  const contactBits = [resume.contact.email, resume.contact.phone, resume.contact.location, resume.contact.website].filter(Boolean)
  return (
    <div style={{ border: '1px solid var(--border)', borderRadius: 10, background: '#fff', color: '#1a1a1a', padding: '22px 26px', fontFamily: s.font, fontSize: 12, lineHeight: 1.4, maxHeight: 460, overflowY: 'auto' }}>
      <div style={{ fontSize: s.nameSize, fontWeight: 800, color: s.accent, fontFamily: s.headingFont }}>{resume.contact.fullName || 'Your Name'}</div>
      {resume.contact.title && <div style={{ color: '#444', fontSize: 13 }}>{resume.contact.title}</div>}
      {contactBits.length > 0 && <div style={{ color: '#666', fontSize: 11, marginTop: 2 }}>{contactBits.join('  •  ')}</div>}

      {resume.summary && <PreviewSection s={s} title="Summary"><p style={{ margin: 0 }}>{resume.summary}</p></PreviewSection>}

      {resume.experience.some((e) => e.role || e.company) && (
        <PreviewSection s={s} title="Experience">
          {resume.experience.map((e, i) => (
            <div key={i} style={{ marginBottom: 8 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ fontWeight: 700 }}>{e.role}{e.company ? `  —  ${e.company}` : ''}</span>
                <span style={{ color: '#666' }}>{[e.startDate, e.endDate].filter(Boolean).join(' – ')}</span>
              </div>
              <div style={{ marginTop: 3, display: 'flex', flexDirection: 'column', gap: 2 }}>
                {e.bullets.filter(Boolean).map((b, j) => (
                  <div key={j} style={{ display: 'flex', gap: 6 }}>
                    <span aria-hidden>•</span><span>{b}</span>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </PreviewSection>
      )}

      {resume.education.some((e) => e.school || e.degree) && (
        <PreviewSection s={s} title="Education">
          {resume.education.map((e, i) => (
            <div key={i} style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span style={{ fontWeight: 700 }}>{e.degree}{e.school ? `  —  ${e.school}` : ''}</span>
              <span style={{ color: '#666' }}>{[e.startDate, e.endDate].filter(Boolean).join(' – ')}</span>
            </div>
          ))}
        </PreviewSection>
      )}

      {resume.skills.length > 0 && <PreviewSection s={s} title="Skills"><div>{resume.skills.join(', ')}</div></PreviewSection>}
    </div>
  )
}

function PreviewSection({ s, title, children }: { s: PreviewStyle; title: string; children: React.ReactNode }) {
  return (
    <div style={{ marginTop: 12 }}>
      <div style={{ fontWeight: 800, color: s.accent, fontFamily: s.headingFont, textTransform: s.upper ? 'uppercase' : 'none', letterSpacing: s.upper ? '0.06em' : 0, borderBottom: s.rule ? '1px solid #bbb' : 'none', paddingBottom: s.rule ? 2 : 0, fontSize: 12 }}>
        {title}
      </div>
      <div style={{ marginTop: 4 }}>{children}</div>
    </div>
  )
}

interface PreviewStyle { font: string; headingFont: string; nameSize: number; accent: string; upper: boolean; rule: boolean }
function previewStyle(template: string): PreviewStyle {
  switch (template) {
    case 'compact': return { font: 'Arial, sans-serif', headingFont: 'Arial, sans-serif', nameSize: 18, accent: '#222', upper: true, rule: false }
    case 'modern': return { font: 'Helvetica, Arial, sans-serif', headingFont: 'Helvetica, Arial, sans-serif', nameSize: 22, accent: '#0f766e', upper: false, rule: false }
    default: return { font: '"Times New Roman", Georgia, serif', headingFont: 'Georgia, serif', nameSize: 21, accent: '#222', upper: true, rule: true }
  }
}

// =============================== Small shared bits ===============================

const textareaStyle: React.CSSProperties = {
  width: '100%', boxSizing: 'border-box', border: '2px solid var(--border)', borderRadius: 10,
  padding: '8px 10px', fontFamily: 'inherit', fontSize: 13, fontWeight: 600, background: 'var(--card)',
  color: 'var(--text)', resize: 'vertical',
}

function SectionLabel({ children, style }: { children: React.ReactNode; style?: React.CSSProperties }) {
  return <div style={{ fontWeight: 900, fontSize: 12, letterSpacing: '0.05em', textTransform: 'uppercase', color: 'var(--muted)', marginBottom: 8, ...style }}>{children}</div>
}

function FieldLabel({ children, style }: { children: React.ReactNode; style?: React.CSSProperties }) {
  return <div style={{ fontWeight: 800, fontSize: 11.5, color: 'var(--muted)', marginBottom: 3, ...style }}>{children}</div>
}

function Field({ label, value, onChange }: { label: string; value: string; onChange: (v: string) => void }) {
  return (
    <label style={{ display: 'block' }}>
      <FieldLabel>{label}</FieldLabel>
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        style={{ width: '100%', boxSizing: 'border-box', border: '2px solid var(--border)', borderRadius: 10, padding: '7px 10px', fontFamily: 'inherit', fontSize: 13, fontWeight: 600, background: 'var(--card)', color: 'var(--text)' }}
      />
    </label>
  )
}

function SmallButton({ children, onClick }: { children: React.ReactNode; onClick: () => void }) {
  return (
    <button onClick={onClick} style={{ border: '2px solid var(--border)', background: 'var(--card)', color: 'var(--muted)', borderRadius: 9, padding: '4px 10px', fontFamily: 'inherit', fontWeight: 800, fontSize: 12, cursor: 'pointer' }}>
      {children}
    </button>
  )
}

// Elapsed seconds + an eased progress percentage since `active` became true. Ollama
// returns nothing until the whole response is done, so there's no real percentage to
// report — instead the bar eases toward ~95% on a curve (fast at first, slowing down),
// which always climbs and never falsely claims "done" until the response actually lands.
function useProgress(active: boolean, estSeconds = 25): { secs: number; pct: number } {
  const [state, setState] = useState({ secs: 0, pct: 0 })
  useEffect(() => {
    if (!active) { setState({ secs: 0, pct: 0 }); return }
    const start = Date.now()
    const id = setInterval(() => {
      const t = (Date.now() - start) / 1000
      setState({ secs: Math.round(t), pct: 95 * (1 - Math.exp(-t / estSeconds)) })
    }, 120)
    return () => clearInterval(id)
  }, [active, estSeconds])
  return state
}

// A determinate progress bar that fills to `pct` (0–100).
function FillBar({ pct }: { pct: number }) {
  return (
    <div style={{ height: 8, borderRadius: 999, background: 'var(--track)', overflow: 'hidden' }}>
      <div style={{ width: `${Math.max(0, Math.min(100, pct))}%`, height: '100%', background: 'var(--purple-lt)', borderRadius: 999, transition: 'width .18s linear' }} />
    </div>
  )
}

// Rough page-count estimate from content length (a one-column A4 resume holds ~46 lines /
// ~95 chars per line at 10pt). Approximate — helps the user keep it to one page.
function estimatePages(r: ResumeModel): number {
  const CPL = 95
  let lines = 3 // name / title / contact line
  if (r.summary.trim()) lines += 1 + Math.ceil(r.summary.length / CPL)
  r.experience.forEach((e) => {
    if (e.role || e.company) {
      lines += 2 // heading + location
      e.bullets.filter(Boolean).forEach((b) => { lines += Math.max(1, Math.ceil(b.length / CPL)) })
    }
  })
  r.education.forEach((e) => { if (e.degree || e.school) lines += 1 })
  if (r.skills.length) lines += 1 + Math.ceil(r.skills.join(', ').length / CPL)
  lines += 4 // section headings + spacing
  return Math.max(1, Math.ceil(lines / 46))
}

function PageBadge({ resume }: { resume: ResumeModel }) {
  const pages = estimatePages(resume)
  const ok = pages <= 1
  return (
    <span
      title="Rough estimate based on content length — the actual PDF may differ slightly by template."
      style={{ fontSize: 11, fontWeight: 800, padding: '2px 9px', borderRadius: 999, whiteSpace: 'nowrap', background: ok ? 'var(--chip-green)' : 'var(--chip-gold)', color: ok ? 'var(--chip-green-tx)' : 'var(--chip-gold-tx)' }}
    >
      ≈ {pages} page{pages > 1 ? 's' : ''}{ok ? '' : ' · trim to fit 1'}
    </span>
  )
}

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

// Best-effort normalise a free-form date string to "Mon YYYY" (ATS-preferred). Handles
// "Present", "Mon YYYY", DD/MM/YYYY, MM/YYYY, and leaves a bare year as-is (can't invent a
// month). Numeric day-first is assumed for ambiguous d/m pairs (both ≤ 12).
function normalizeDate(raw: string): string {
  const s = (raw || '').trim()
  if (!s) return s
  if (/^(present|current|now|ongoing|to date)$/i.test(s)) return 'Present'

  // Already "Mon[th] YYYY"
  const monYear = s.match(/^([A-Za-z]{3,9})\.?\s+(\d{4})$/)
  if (monYear) {
    const mi = MONTHS.findIndex((m) => monYear[1].toLowerCase().startsWith(m.toLowerCase()))
    return mi >= 0 ? `${MONTHS[mi]} ${monYear[2]}` : s
  }

  const parts = s.split(/[/\-.]/).map((p) => p.trim()).filter(Boolean)
  const toMon = (m: number, y: number) => (m >= 1 && m <= 12 && y > 1900 ? `${MONTHS[m - 1]} ${y}` : s)

  if (parts.length === 3 && parts.every((p) => /^\d+$/.test(p))) {
    const [a, b, c] = parts.map(Number)
    if (a > 31) return toMon(b, a)          // YYYY/MM/DD
    const month = a > 12 ? b : b > 12 ? a : b // DD/MM/YYYY (day-first when ambiguous)
    return toMon(month, c)
  }
  if (parts.length === 2 && parts.every((p) => /^\d+$/.test(p))) {
    const [a, b] = parts.map(Number)
    if (b > 31) return toMon(a, b)          // MM/YYYY
    if (a > 31) return toMon(b, a)          // YYYY/MM
  }
  return s // bare year or unrecognised — leave untouched
}

function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}
