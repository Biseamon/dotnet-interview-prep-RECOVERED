import { useQuery } from '@tanstack/react-query'
import { api, type GamificationStats } from '../api/client'

// Server-computed player stats (XP, level, belt, streak, weekly XP, quests). Cached
// under the 'gamification' key; the workspace/drill invalidate it on a pass/finish.
export function useGamification() {
  const { data } = useQuery({ queryKey: ['gamification'], queryFn: api.getGamification })
  const stats: GamificationStats = data ?? {
    xp: 0, level: 1, belt: 'White belt', xpIntoLevel: 0, xpForNextLevel: 100,
    solvedCount: 0, streakDays: 0, longestStreak: 0, dailyXp: 0, correctInARow: 0,
    weeklyXp: [0, 0, 0, 0, 0, 0, 0], weekActivity: [false, false, false, false, false, false, false],
    quests: [], badges: [],
  }
  return stats
}

// XP awarded per difficulty — mirrors the server (Easy 10 / Medium 15 / Hard 20).
export function xpForDifficulty(difficulty: string): number {
  if (difficulty === 'Hard') return 20
  if (difficulty === 'Medium') return 15
  return 10
}

// Belt name for a level — mirrors the server's BeltFor (Lv7 Purple, Lv8 Brown).
export function beltName(level: number): string {
  const belts = ['White belt', 'Yellow belt', 'Orange belt', 'Green belt', 'Blue belt', 'Purple belt', 'Purple belt', 'Brown belt', 'Red belt']
  return belts[level - 1] ?? 'Black belt'
}

// The coloured belt emoji for a belt name (used on Progress + level cards).
export function beltEmoji(belt: string): string {
  const map: Record<string, string> = {
    'White belt': '⚪', 'Yellow belt': '🟡', 'Orange belt': '🟠', 'Green belt': '🟢',
    'Blue belt': '🔵', 'Purple belt': '🟣', 'Brown belt': '🟤', 'Red belt': '🔴', 'Black belt': '⚫',
  }
  return map[belt] ?? '🟣'
}
