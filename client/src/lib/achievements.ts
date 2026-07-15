import { useQuery } from '@tanstack/react-query'
import { api } from '../api/client'

// Achievement definitions + earned state, evaluated server-side from the event log.
export function useAchievements() {
  const { data } = useQuery({ queryKey: ['achievements'], queryFn: api.getAchievements })
  return data ?? []
}
