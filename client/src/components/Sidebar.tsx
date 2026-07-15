import type { ReactNode } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { useTheme } from '../lib/theme'

// The 230px left navigation used by the sidebar screens (Learn, Topics, Glossary,
// Q&A, Progress). Active item gets the blue "nav" pill; theme toggle pinned bottom.
const NAV = [
  { to: '/', icon: '🏠', label: 'LEARN', match: (p: string) => p === '/' },
  { to: '/topics', icon: '🧮', label: 'TOPICS', match: (p: string) => p.startsWith('/topics') },
  { to: '/glossary', icon: '📖', label: 'GLOSSARY', match: (p: string) => p.startsWith('/glossary') },
  { to: '/qa', icon: '❓', label: 'Q&A', match: (p: string) => p.startsWith('/qa') },
  { to: '/resume', icon: '📄', label: 'RESUME', match: (p: string) => p.startsWith('/resume') },
  { to: '/progress', icon: '📊', label: 'PROGRESS', match: (p: string) => p.startsWith('/progress') },
]

const itemBase = {
  display: 'flex', alignItems: 'center', gap: 12, padding: '11px 14px', borderRadius: 14,
  fontWeight: 800, fontSize: 14, letterSpacing: '0.04em',
} as const

export function Sidebar() {
  const { pathname } = useLocation()
  const { toggle } = useTheme()
  return (
    <nav style={{ borderRight: '2px solid var(--border)', padding: '20px 14px', display: 'flex', flexDirection: 'column', gap: 6, position: 'sticky', top: 0, height: '100vh', boxSizing: 'border-box' }}>
      <Link to="/" style={{ display: 'flex', alignItems: 'center', gap: 8, fontWeight: 900, fontSize: 20, padding: '6px 10px 20px', color: 'var(--text)' }}>
        <span style={{ fontSize: 26 }}>🥋</span><span>DotNetDojo</span>
      </Link>
      {NAV.map((n) => {
        const active = n.match(pathname)
        return (
          <Link
            key={n.to}
            to={n.to}
            style={{
              ...itemBase,
              border: active ? '2px solid var(--nav-bd)' : '2px solid transparent',
              background: active ? 'var(--nav-bg)' : 'transparent',
              color: active ? 'var(--nav-tx)' : 'var(--muted)',
            }}
          >
            {n.icon} {n.label}
          </Link>
        )
      })}
      <button
        onClick={toggle}
        style={{ ...itemBase, marginTop: 'auto', border: '2px solid transparent', color: 'var(--muted)', cursor: 'pointer', background: 'none', fontFamily: 'inherit', textAlign: 'left' }}
      >
        <span className="tl">🌙</span><span className="td">☀️</span> THEME
      </button>
    </nav>
  )
}

// Wraps a sidebar page: 230px nav + fluid content, matching the design grid.
export function SidebarLayout({ children }: { children: ReactNode }) {
  return (
    <div style={{ display: 'grid', gridTemplateColumns: '230px 1fr', minHeight: '100vh', background: 'var(--bg)' }}>
      <Sidebar />
      {children}
    </div>
  )
}
