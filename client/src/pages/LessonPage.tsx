import { useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import ReactMarkdown from 'react-markdown'
import { api } from '../api/client'

// A lesson: renders the teaching Markdown, then links to each exercise.
export function LessonPage() {
  const { slug } = useParams<{ slug: string }>()
  const { data: lesson, isLoading, isError } = useQuery({
    queryKey: ['lesson', slug],
    queryFn: () => api.getLesson(slug!),
    enabled: !!slug,
  })
  // Solved-exercise slugs, so we can show a checkmark next to completed exercises.
  const { data: progress } = useQuery({ queryKey: ['progress'], queryFn: api.getProgress })
  const solved = new Set(progress?.solved ?? [])

  if (isLoading) return <p className="text-muted p-6">Loading lesson…</p>
  if (isError || !lesson) return <p className="text-blush p-6">Lesson not found.</p>

  return (
    <div className="max-w-3xl mx-auto p-6 space-y-6">
      <Link to="/" className="text-sm text-muted hover:text-ink">← All topics</Link>

      <article className="bg-card rounded-[var(--radius-soft)] p-6 shadow-sm">
        <h1 className="text-2xl font-bold mb-4">{lesson.title}</h1>
        {/* Prose styling kept simple; react-markdown emits semantic HTML. */}
        <div className="prose-sm space-y-3 leading-relaxed [&_code]:bg-cream [&_code]:px-1 [&_code]:rounded [&_h2]:font-semibold [&_h2]:text-lg [&_h2]:mt-4">
          <ReactMarkdown>{lesson.markdownContent}</ReactMarkdown>
        </div>
      </article>

      <section>
        <h2 className="text-lg font-semibold mb-2">Exercises</h2>
        <ul className="space-y-2">
          {lesson.exercises.map((ex) => (
            <li key={ex.slug}>
              <Link
                to={`/exercises/${ex.slug}`}
                className="flex items-center gap-3 bg-card rounded-[var(--radius-soft)] px-4 py-3 shadow-sm hover:shadow transition"
              >
                <span aria-label={solved.has(ex.slug) ? 'solved' : 'not solved'}>
                  {solved.has(ex.slug) ? '✅' : '⬜'}
                </span>
                <span className="font-medium">{ex.title}</span>
                <span className="ml-auto text-xs px-2 py-0.5 rounded-full"
                      style={{ backgroundColor: difficultyColor(ex.difficulty) }}>
                  {ex.difficulty}
                </span>
              </Link>
            </li>
          ))}
        </ul>
      </section>
    </div>
  )
}

function difficultyColor(d: string): string {
  if (d === 'Easy') return 'var(--color-sage)'
  if (d === 'Medium') return 'var(--color-butter)'
  return 'var(--color-blush)' // Hard
}
