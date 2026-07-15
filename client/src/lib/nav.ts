import type { Topic, TopicDetail } from '../api/client'

// Navigation/resume helpers shared by the dashboard, topic page, and workspace.

// The next exercise the learner hasn't solved, scanning topics in display order.
// Returns null when everything is solved. Used by the dashboard "Continue" button.
export function nextUnsolvedGlobal(topics: Topic[], solved: Set<string>): string | null {
  for (const t of topics)
    for (const slug of t.exerciseSlugs)
      if (!solved.has(slug)) return slug
  return null
}

// Flatten a topic's exercises (across lessons) into an ordered slug list.
export function topicExerciseSlugs(topic: TopicDetail): string[] {
  return topic.lessons.flatMap((l) => l.exercises.map((e) => e.slug))
}

// The next unsolved exercise within a single topic (or its first, if all solved).
export function nextUnsolvedInTopic(topic: TopicDetail, solved: Set<string>): string | null {
  const slugs = topicExerciseSlugs(topic)
  return slugs.find((s) => !solved.has(s)) ?? slugs[0] ?? null
}

// Prev/next neighbours + 1-based position of an exercise within its topic.
export function neighbours(slugs: string[], current: string) {
  const i = slugs.indexOf(current)
  return {
    index: i,
    total: slugs.length,
    prev: i > 0 ? slugs[i - 1] : null,
    next: i >= 0 && i < slugs.length - 1 ? slugs[i + 1] : null,
  }
}
