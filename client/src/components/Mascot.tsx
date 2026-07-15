// Dojo mascot — a friendly rounded ".NET" creature ("Dot"). One inline SVG with a
// few moods so it can react across the app (idle on the dashboard, cheering on a
// pass, thinking on a prompt, sad when out of hearts). Pure SVG = crisp at any size,
// themeable, and no asset pipeline.
export type Mood = 'happy' | 'cheer' | 'think' | 'sad'

export function Mascot({ mood = 'happy', size = 96, className = '' }: { mood?: Mood; size?: number; className?: string }) {
  // Eyes + mouth vary by mood; body stays constant.
  const mouth: Record<Mood, string> = {
    happy: 'M40 63 Q50 72 60 63',           // gentle smile
    cheer: 'M39 60 Q50 76 61 60 Q50 68 39 60', // big open grin
    think: 'M42 65 L58 65',                  // straight line
    sad:   'M40 70 Q50 62 60 70',            // frown
  }
  const browY = mood === 'think' ? 33 : 36

  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 100 100"
      className={className}
      role="img"
      aria-label="Dojo mascot"
    >
      {/* soft shadow */}
      <ellipse cx="50" cy="92" rx="26" ry="5" fill="rgba(0,0,0,0.10)" />
      {/* body — a rounded purple blob (the .NET brand hue) */}
      <path
        d="M50 12
           C74 12 84 30 84 52
           C84 76 70 88 50 88
           C30 88 16 76 16 52
           C16 30 26 12 50 12 Z"
        fill="#8b5cf6"
        stroke="#7c3aed"
        strokeWidth="2.5"
      />
      {/* belly highlight */}
      <ellipse cx="50" cy="58" rx="22" ry="20" fill="#a78bfa" opacity="0.55" />
      {/* little top antenna dot */}
      <circle cx="50" cy="9" r="4" fill="#ffc800" />
      <line x1="50" y1="12" x2="50" y2="16" stroke="#7c3aed" strokeWidth="2.5" />
      {/* eyes */}
      <g fill="#ffffff">
        <circle cx="40" cy="45" r="9" />
        <circle cx="60" cy="45" r="9" />
      </g>
      <g fill="#1f2937">
        <circle cx={mood === 'think' ? 42 : 41} cy="46" r="4" />
        <circle cx={mood === 'think' ? 62 : 59} cy="46" r="4" />
      </g>
      {/* brows */}
      <line x1="33" y1={browY} x2="45" y2={browY + (mood === 'sad' ? 3 : 0)} stroke="#7c3aed" strokeWidth="2.5" strokeLinecap="round" />
      <line x1="55" y1={browY + (mood === 'sad' ? 3 : 0)} x2="67" y2={browY} stroke="#7c3aed" strokeWidth="2.5" strokeLinecap="round" />
      {/* cheeks when happy/cheer */}
      {(mood === 'happy' || mood === 'cheer') && (
        <g fill="#ff8fb0" opacity="0.7">
          <circle cx="30" cy="58" r="4" />
          <circle cx="70" cy="58" r="4" />
        </g>
      )}
      {/* mouth */}
      <path d={mouth[mood]} fill="none" stroke="#1f2937" strokeWidth="2.6" strokeLinecap="round" />
    </svg>
  )
}
