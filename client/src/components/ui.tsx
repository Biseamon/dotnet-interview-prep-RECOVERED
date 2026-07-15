// Reusable primitives for the Duolingo-style redesign. Authored with inline styles +
// CSS-var tokens (and the .card/.btn-3d recipe classes) to match the design handoff
// pixel-for-pixel. Screen-specific compositions live in the pages.
import type { ButtonHTMLAttributes, CSSProperties, ReactNode } from 'react'

// ---- Difficulty / category chips ----

type ChipTone = 'green' | 'gold' | 'pink' | 'blue' | 'purple' | 'muted'
const CHIP: Record<ChipTone, { bg: string; tx: string }> = {
  green:  { bg: 'var(--chip-green)',  tx: 'var(--chip-green-tx)' },
  gold:   { bg: 'var(--chip-gold)',   tx: 'var(--chip-gold-tx)' },
  pink:   { bg: 'var(--chip-pink)',   tx: 'var(--chip-pink-tx)' },
  blue:   { bg: 'var(--chip-blue)',   tx: 'var(--nav-tx)' },
  purple: { bg: 'var(--chip-purple)', tx: 'var(--purple-deep)' },
  muted:  { bg: 'var(--track)',       tx: 'var(--muted)' },
}

export function Chip({ tone, children, style }: { tone: ChipTone; children: ReactNode; style?: CSSProperties }) {
  const c = CHIP[tone]
  return (
    <span style={{ fontSize: 11, fontWeight: 800, padding: '3px 9px', borderRadius: 999, color: c.tx, background: c.bg, whiteSpace: 'nowrap', ...style }}>
      {children}
    </span>
  )
}

export function difficultyTone(d: string): ChipTone {
  if (d === 'Easy') return 'green'
  if (d === 'Medium') return 'gold'
  return 'pink' // Hard
}

export function DifficultyChip({ difficulty }: { difficulty: string }) {
  return <Chip tone={difficultyTone(difficulty)} style={{ textTransform: 'uppercase' }}>{difficulty}</Chip>
}

// ---- Card ----

export function Card({ children, style, className = '' }: { children: ReactNode; style?: CSSProperties; className?: string }) {
  return <div className={`card ${className}`} style={style}>{children}</div>
}

// ---- 3D button ----

type Variant = 'green' | 'purple' | 'blue' | 'ghost'
export function Button3D({
  variant = 'green',
  children,
  className = '',
  style,
  ...rest
}: { variant?: Variant } & ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <button className={`btn-3d btn-${variant} ${className}`} style={style} {...rest}>
      {children}
    </button>
  )
}

// ---- Progress bar (with the optional inner highlight stripe) ----

export function ProgressBar({
  pct,
  height = 12,
  color = 'var(--purple-lt)',
  track = 'var(--track)',
  stripe = false,
}: { pct: number; height?: number; color?: string; track?: string; stripe?: boolean }) {
  return (
    <div style={{ flex: 1, height, borderRadius: 999, background: track, overflow: 'hidden' }}>
      <div style={{ width: `${Math.max(0, Math.min(100, pct))}%`, height: '100%', background: color, borderRadius: 999, position: 'relative', transition: 'width .5s ease' }}>
        {stripe && <div style={{ position: 'absolute', top: 3, left: 8, right: 8, height: 4, borderRadius: 999, background: 'rgba(255,255,255,0.35)' }} />}
      </div>
    </div>
  )
}

// ---- Header stat strip: 🔥 streak · ⚡ XP · ❤️ hearts ----

export function StatStrip({ streak, xp, hearts, size = 15 }: { streak: number; xp: number; hearts: number; size?: number }) {
  const item: CSSProperties = { display: 'flex', alignItems: 'center', gap: 5 }
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 16, fontWeight: 800, fontSize: size }}>
      <span style={{ ...item, color: 'var(--streak)' }}><span style={{ fontSize: size + 4 }}>🔥</span>{streak}</span>
      <span style={{ ...item, color: 'var(--xp)' }}><span style={{ fontSize: size + 2 }}>⚡</span>{xp.toLocaleString()}</span>
      <span style={{ ...item, color: 'var(--heart)' }}><span style={{ fontSize: size + 2 }}>❤️</span>{hearts}</span>
    </div>
  )
}

// ---- Learning-path node ----

export type NodeState = 'done' | 'current' | 'locked' | 'checkpoint'
export function PathNode({ state, icon, onClick }: { state: NodeState; icon?: string; onClick?: () => void }) {
  if (state === 'locked') {
    return (
      <div style={{ width: 74, height: 74, borderRadius: 999, background: 'var(--locked)', boxShadow: '0 7px 0 var(--locked-sh)', display: 'grid', placeItems: 'center', fontSize: 26, color: 'var(--locked-tx)' }}>🔒</div>
    )
  }
  if (state === 'checkpoint') {
    return (
      <div onClick={onClick} className="path-node" style={{ width: 80, borderRadius: 22, background: 'var(--gold)', ['--nd-sh' as string]: 'var(--gold-sh)' }}>🏆</div>
    )
  }
  const gold = state === 'done'
  return (
    <div style={{ position: 'relative', width: 74, height: 74 }}>
      {state === 'current' && (
        <div style={{ position: 'absolute', inset: 0, borderRadius: 999, background: 'var(--purple)', opacity: 0.35, animation: 'pulse-ring 1.6s ease-out infinite' }} />
      )}
      <div
        onClick={onClick}
        className="path-node"
        style={{
          position: 'relative',
          background: gold ? 'var(--gold)' : 'var(--purple)',
          ['--nd-sh' as string]: gold ? 'var(--gold-sh)' : 'var(--purple-sh)',
        }}
      >
        {gold ? (icon ?? '👑') : (icon ?? '⭐')}
      </div>
    </div>
  )
}

// ---- Unsolved / solved circle used in exercise lists ----

export function SolvedCircle({ solved, upNext }: { solved: boolean; upNext?: boolean }) {
  if (solved) {
    return <span style={{ width: 30, height: 30, borderRadius: 999, background: 'var(--green)', boxShadow: '0 2px 0 var(--green-sh)', display: 'grid', placeItems: 'center', color: '#fff', fontWeight: 900, fontSize: 14, flex: 'none' }}>✓</span>
  }
  if (upNext) {
    return <span style={{ width: 30, height: 30, borderRadius: 999, border: '3px solid var(--purple-lt)', boxSizing: 'border-box', display: 'grid', placeItems: 'center', color: 'var(--purple-lt)', fontWeight: 900, fontSize: 12, flex: 'none' }}>▶</span>
  }
  return <span style={{ width: 30, height: 30, borderRadius: 999, border: '3px solid var(--ring)', boxSizing: 'border-box', flex: 'none' }} />
}
