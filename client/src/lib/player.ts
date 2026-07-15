import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../api/client'

// Server-persisted hearts (lives). Replaces the old client-side localStorage model:
// the backend owns hearts now (with time-based refill), so they survive reloads and
// are the single source of truth. Wrong exercise runs / drill answers call loseHeart.
export function usePlayer() {
  const qc = useQueryClient()
  const { data } = useQuery({ queryKey: ['player'], queryFn: api.getPlayer })

  const lose = useMutation({
    mutationFn: api.loseHeart,
    onSuccess: (p) => qc.setQueryData(['player'], p),
  })
  const refill = useMutation({
    mutationFn: api.refillHearts,
    onSuccess: (p) => qc.setQueryData(['player'], p),
  })

  return {
    hearts: data?.hearts ?? 5,
    max: data?.maxHearts ?? 5,
    minutesToNext: data?.minutesToNext ?? null,
    loseHeart: () => lose.mutate(),
    refillHearts: () => refill.mutate(),
  }
}
