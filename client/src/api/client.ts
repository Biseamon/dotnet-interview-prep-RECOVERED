// Typed API client. One place that knows the backend contract, so components stay
// clean and the DTO shapes are defined once. All calls go through /api (Vite proxies
// to the .NET server in dev; same-origin in prod).

// ---- Types mirroring the backend DTOs ----

export interface LessonSummary { slug: string; title: string }
export interface Topic {
  slug: string
  name: string
  description: string
  lessons: LessonSummary[]
  exerciseSlugs: string[]
}

export interface LessonWithExercises {
  slug: string
  title: string
  markdownContent: string
  exercises: ExerciseSummary[]
}
export interface TopicDetail {
  slug: string
  name: string
  description: string
  lessons: LessonWithExercises[]
}

export interface ExerciseSummary { slug: string; title: string; difficulty: string }
export interface Lesson {
  slug: string
  title: string
  markdownContent: string
  exercises: ExerciseSummary[]
}

export interface Exercise {
  slug: string
  title: string
  prompt: string
  explanation: string | null
  difficulty: string
  kind: string
  language: string // 'CSharp' | 'Sql' | 'Config' → editor syntax
  starterCode: string
  hintCount: number
  visibleTests: { name: string }[]
  topicSlug: string | null
}

// Grading result — mirrors Domain.Grading.GradeResult.
export type GradeStatus = 'CompileError' | 'Failed' | 'RuntimeError' | 'Timeout' | 'Passed'

export interface CompileError {
  severity: string
  id: string
  message: string
  line: number
  column: number
  endLine: number
  endColumn: number
}

export interface TestCaseResult {
  name: string
  passed: boolean
  expected: string | null
  actual: string | null
  exceptionType: string | null
  exceptionMessage: string | null
  stdout: string | null
  elapsedMs: number
}

export interface GradeResult {
  status: GradeStatus
  compileErrors: CompileError[]
  testResults: TestCaseResult[]
  passedCount: number
  totalCount: number
}

export interface Hint { order: number; text: string; total: number }

// Player stats derived from the event log (mirrors Application.GamificationStats).
export interface QuestProgress { id: string; emoji: string; label: string; current: number; target: number }
export interface GamificationStats {
  xp: number
  level: number
  belt: string
  xpIntoLevel: number
  xpForNextLevel: number
  solvedCount: number
  streakDays: number
  longestStreak: number
  dailyXp: number
  correctInARow: number
  weeklyXp: number[]      // Mon..Sun
  weekActivity: boolean[] // Mon..Sun
  quests: QuestProgress[]
  badges: string[]
}

// Server-persisted hearts (lives).
export interface Player { hearts: number; maxHearts: number; minutesToNext: number | null }

// A drill quiz question (low-stakes self-check → correctIndex is included).
export interface DrillQuestion {
  id: number
  tag: string
  text: string
  options: string[]
  correctIndex: number
  explanation: string
}
export interface DrillCompleteResult { xpEarned: number; stats: GamificationStats }

// An achievement definition + earned state.
export interface Achievement {
  code: string
  emoji: string
  title: string
  description: string
  earned: boolean
  unlockedAtUtc: string | null
}

// ---- Resume builder types (mirror Application.Resume records) ----

export interface ResumeContact {
  fullName: string
  title: string
  email: string
  phone: string
  location: string
  website: string
}
export interface ResumeExperience {
  company: string
  role: string
  startDate: string
  endDate: string
  location: string
  bullets: string[]
}
export interface ResumeEducation {
  school: string
  degree: string
  startDate: string
  endDate: string
  details: string
}
export interface ResumeModel {
  contact: ResumeContact
  summary: string
  experience: ResumeExperience[]
  education: ResumeEducation[]
  skills: string[]
}

// Enums serialize as their string names (JsonStringEnumConverter).
export type AtsCheckStatus = 'Pass' | 'Warn' | 'Fail'
export interface AtsCheck { id: string; label: string; status: AtsCheckStatus; detail: string }
export interface BulletSuggestion {
  experienceIndex: number
  bulletIndex: number
  original: string
  suggested: string
  reason: string
}
export interface ResumeAnalysis {
  matchScore: number
  summary: string
  missingKeywords: string[]
  strengths: string[]
  atsChecks: AtsCheck[]
  bulletSuggestions: BulletSuggestion[]
  summarySuggestion: string | null
}

export interface ResumeConfig { model: string; baseUrl: string }
export type ResumeExportFormat = 'Pdf' | 'Docx'

// ---- Fetch helpers ----

async function getJson<T>(url: string): Promise<T> {
  const res = await fetch(url)
  if (!res.ok) throw new Error(`GET ${url} failed: ${res.status}`)
  return res.json() as Promise<T>
}

async function post<T>(url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: body === undefined ? undefined : JSON.stringify(body),
  })
  if (!res.ok) throw new Error(`POST ${url} failed: ${res.status}`)
  return res.json() as Promise<T>
}

// The resume endpoints return a JSON `{ error }` body on 4xx/503 (e.g. "Ollama is
// unreachable — run `ollama serve`"). These helpers surface that message so the UI can
// show it verbatim, rather than a bare status code.
async function errorFrom(res: Response): Promise<Error> {
  let msg = `Request failed (${res.status})`
  try {
    const body = await res.json()
    if (body?.error) msg = body.error
  } catch { /* non-JSON body — keep the status message */ }
  return new Error(msg)
}

async function postJson<T>(url: string, body: unknown): Promise<T> {
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) throw await errorFrom(res)
  return res.json() as Promise<T>
}

// Multipart upload — deliberately does NOT set Content-Type so the browser adds the
// multipart boundary for us.
async function postForm<T>(url: string, form: FormData): Promise<T> {
  const res = await fetch(url, { method: 'POST', body: form })
  if (!res.ok) throw await errorFrom(res)
  return res.json() as Promise<T>
}

// Binary download — returns the raw blob for the caller to save.
async function postBlob(url: string, body: unknown): Promise<Blob> {
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) throw await errorFrom(res)
  return res.blob()
}

export const api = {
  getTopics: () => getJson<Topic[]>('/api/topics'),
  getTopic: (slug: string) => getJson<TopicDetail>(`/api/topics/${slug}`),
  getLesson: (slug: string) => getJson<Lesson>(`/api/lessons/${slug}`),
  getExercise: (slug: string) => getJson<Exercise>(`/api/exercises/${slug}`),
  getHint: (slug: string, n: number) => getJson<Hint>(`/api/exercises/${slug}/hints/${n}`),
  getSolution: (slug: string) => getJson<{ solution: string }>(`/api/exercises/${slug}/solution`),

  // Slugs of exercises the learner has solved (for checkmarks / progress).
  getProgress: () => getJson<{ solved: string[] }>('/api/progress'),

  // XP / level / belt / streak / weekly XP / quests for the gamified UI.
  getGamification: () => getJson<GamificationStats>('/api/gamification'),

  // Hearts (server-persisted).
  getPlayer: () => getJson<Player>('/api/player'),
  loseHeart: () => post<Player>('/api/player/lose-heart'),
  refillHearts: () => post<Player>('/api/player/refill'),

  // Interview drill (MCQ quiz).
  getDrillQuestions: (count = 5) => getJson<DrillQuestion[]>(`/api/drill/questions?count=${count}`),
  completeDrill: (correctCount: number, total: number) =>
    post<DrillCompleteResult>('/api/drill/complete', { correctCount, total }),

  // Achievements.
  getAchievements: () => getJson<Achievement[]>('/api/achievements'),

  grade: async (slug: string, source: string): Promise<GradeResult> => {
    const res = await fetch(`/api/exercises/${slug}/grade`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ source }),
    })
    if (!res.ok) throw new Error(`Grade failed: ${res.status}`)
    return res.json()
  },

  // "Practice again": clear this exercise's attempt history on the server.
  clearAttempts: (slug: string) =>
    fetch(`/api/exercises/${slug}/attempts`, { method: 'DELETE' }),

  // ---- Resume builder ----

  // Which local Ollama model is configured (shown in the "AI unavailable" hint).
  getResumeConfig: () => getJson<ResumeConfig>('/api/resume/config'),

  // Upload a resume file → structured model (local AI parses the extracted text).
  parseResume: (file: File): Promise<ResumeModel> => {
    const form = new FormData()
    form.append('file', file)
    return postForm<ResumeModel>('/api/resume/parse', form)
  },

  // Score + tailor the resume against a job description.
  analyzeResume: (resume: ResumeModel, jobDescription: string): Promise<ResumeAnalysis> =>
    postJson<ResumeAnalysis>('/api/resume/analyze', { resume, jobDescription }),

  // Render to a downloadable PDF/DOCX blob in the chosen ATS template.
  exportResume: (resume: ResumeModel, template: string, format: ResumeExportFormat): Promise<Blob> =>
    postBlob('/api/resume/export', { resume, template, format }),
}
