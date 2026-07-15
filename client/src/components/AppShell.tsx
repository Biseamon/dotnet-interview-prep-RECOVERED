import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useTheme } from '../lib/theme'
import { useGamification } from '../lib/gamification'
import { useHearts } from '../lib/hearts'
import { StreakFlame, XpPill, HeartBar } from './ui'

// The persistent app frame: a sticky header with the brand, a Duolingo-style status
// bar (streak · XP/level · hearts), nav links, and the theme toggle. Wraps every page.
export function AppShell({ children }: { children: ReactNode }) {
  const { theme, toggle } = useTheme()
  const stats = useGamification()
  const { hearts, max, minutesToNext } = useHearts()

  return (
    <div className="min-h-screen">
      <header
        className="sticky top-0 z-20 backdrop-blur-md border-b border-border"
        style={{ background: 'color-mix(in srgb, var(--bg) 82%, transparent)' }}
      >
        <div className="max-w-6xl mx-auto px-5 h-16 flex items-center gap-3">
          <Link to="/" className="flex items-center gap-2 font-extrabold text-lg font-display">
            <span className="text-2xl">🥋</span>
            <span className="hidden sm:inline">DotNetDojo</span>
          </Link>

          {/* Gamification status bar */}
          <div className="ml-auto flex items-center gap-1 sm:gap-2">
            <StreakFlame days={stats.streakDays} />
            <XpPill level={stats.level} xp={stats.xp} />
            <div className="hidden sm:block"><HeartBar hearts={hearts} max={max} minutesToNext={minutesToNext} /></div>

            <span className="w-px h-6 bg-border mx-1 hidden sm:block" />

            <Link to="/questions" className="text-sm text-muted hover:text-ink hidden md:block" title="Interview Q&A">❓</Link>
            <Link to="/glossary" className="text-sm text-muted hover:text-ink hidden md:block" title="Glossary">📖</Link>

            {/* Theme toggle */}
            <button
              onClick={toggle}
              aria-label="Toggle theme"
              className="w-9 h-9 grid place-items-center rounded-full border border-border hover:bg-surface-2 transition"
            >
              {theme === 'dark' ? '☀️' : '🌙'}
            </button>
          </div>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-5 py-8">{children}</main>
    </div>
  )
}
