import { useQuery } from '@tanstack/react-query'
import { api } from '../api/client'

// Central progress hook: fetches the set of solved exercise slugs once (cached by
// React Query under the 'progress' key, which the workspace invalidates on a pass).
export function useProgress() {
  const { data } = useQuery({ queryKey: ['progress'], queryFn: api.getProgress })
  const solved = new Set(data?.solved ?? [])
  return {
    solved,
    isSolved: (slug: string) => solved.has(slug),
    countSolved: (slugs: string[]) => slugs.filter((s) => solved.has(s)).length,
  }
}
