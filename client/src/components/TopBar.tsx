import { Link } from 'react-router-dom'
import { useTheme } from '../lib/theme'
import { useGamification } from '../lib/gamification'
import { usePlayer } from '../lib/player'
import { StatStrip } from './ui'

// Sticky header used by the Topic and Exercise screens: logo + live stat strip +
// theme toggle. (Sidebar screens use Sidebar instead.)
export function TopBar() {
  const { toggle } = useTheme()
  const stats = useGamification()
  const { hearts } = usePlayer()
  return (
    <header style={{ position: 'sticky', top: 0, zIndex: 20, borderBottom: '2px solid var(--border)', background: 'var(--surface)' }}>
      <div style={{ maxWidth: 900, margin: '0 auto', padding: '0 24px', height: 64, display: 'flex', alignItems: 'center', gap: 16 }}>
        <Link to="/" style={{ display: 'flex', alignItems: 'center', gap: 8, fontWeight: 900, fontSize: 19, color: 'var(--text)' }}>
          <span style={{ fontSize: 24 }}>🥋</span><span>DotNetDojo</span>
        </Link>
        <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', gap: 16 }}>
          <StatStrip streak={stats.streakDays} xp={stats.xp} hearts={hearts} />
          <button
            onClick={toggle}
            aria-label="Toggle theme"
            style={{ width: 36, height: 36, display: 'grid', placeItems: 'center', borderRadius: 999, border: '2px solid var(--border)', background: 'var(--card)', cursor: 'pointer', fontSize: 14 }}
          >
            <span className="tl">🌙</span><span className="td">☀️</span>
          </button>
        </div>
      </div>
    </header>
  )
}
