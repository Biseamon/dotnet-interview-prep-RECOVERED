import type { ReactNode } from 'react'

// Simple, theme-aware SVG diagrams for the concepts that are much clearer as a
// picture than as words. Keyed by exercise slug; the workspace shows one as a
// "Picture it" card (with a kid-friendly caption) when the current exercise has one.
//
// Colours use the app's CSS design tokens so they adapt to light/dark automatically.

// --- shared little building blocks ----------------------------------------
const T = 'var(--text)'
const MUT = 'var(--muted)'
const ACC = 'var(--accent)'
const OK = 'var(--success)'
const ERR = 'var(--error)'
const WARN = 'var(--warning)'
const INFO = 'var(--info)'
const SURF = 'var(--surface-2)'
const BORD = 'var(--border-strong)'

function Svg({ children, w = 340, h = 150 }: { children: ReactNode; w?: number; h?: number }) {
  return (
    <svg viewBox={`0 0 ${w} ${h}`} className="w-full max-w-md" style={{ fontSize: 13 }}>
      <defs>
        <marker id="arw" markerWidth="9" markerHeight="9" refX="7" refY="3" orient="auto">
          <path d="M0,0 L7,3 L0,6 Z" fill={MUT} />
        </marker>
      </defs>
      {children}
    </svg>
  )
}

// A labelled box.
function Box({ x, y, w = 40, h = 30, label, fill = SURF }: {
  x: number; y: number; w?: number; h?: number; label: string; fill?: string
}) {
  return (
    <g>
      <rect x={x} y={y} width={w} height={h} rx={7} fill={fill} stroke={BORD} />
      <text x={x + w / 2} y={y + h / 2 + 4} textAnchor="middle" fill={T} fontFamily="ui-monospace, monospace">
        {label}
      </text>
    </g>
  )
}

function Line({ x1, y1, x2, y2, dashed = false }: { x1: number; y1: number; x2: number; y2: number; dashed?: boolean }) {
  return <line x1={x1} y1={y1} x2={x2} y2={y2} stroke={MUT} strokeWidth={1.6}
               markerEnd="url(#arw)" strokeDasharray={dashed ? '4 3' : undefined} />
}

function Cap({ x, y, children, fill = MUT }: { x: number; y: number; children: ReactNode; fill?: string }) {
  return <text x={x} y={y} fill={fill} fontSize={11}>{children}</text>
}

// ---------------------------------------------------------------------------
export const DIAGRAMS: Record<string, { svg: ReactNode; caption: string }> = {
  // Stack (valid parentheses / RPN / min-stack)
  'valid-parentheses': {
    caption: 'A stack is a pile of plates — you only add or take from the top.',
    svg: (
      <Svg h={160}>
        <Box x={130} y={20} w={70} label="{" fill={ACC} />
        <Box x={130} y={55} w={70} label="[" fill={SURF} />
        <Box x={130} y={90} w={70} label="(" fill={SURF} />
        <line x1={130} y1={128} x2={200} y2={128} stroke={BORD} strokeWidth={2} />
        <Line x1={250} y1={35} x2={205} y2={35} />
        <Cap x={255} y={39}>top — push &amp; pop here</Cap>
        <Cap x={40} y={39}>last in,</Cap>
        <Cap x={40} y={54}>first out</Cap>
      </Svg>
    ),
  },

  // Two Sum / hashing
  'two-sum': {
    caption: "Remember every price you've seen. For a new one, check if its partner is already remembered.",
    svg: (
      <Svg h={150}>
        <Cap x={10} y={24}>looking at</Cap>
        <Box x={90} y={12} w={34} label="7" fill={ACC} />
        <Cap x={135} y={31}>need 10 − 7 = 3</Cap>
        <rect x={10} y={60} width={320} height={70} rx={8} fill={SURF} stroke={BORD} />
        <Cap x={20} y={78} fill={T}>seen so far (value → where):</Cap>
        <Box x={20} y={90} w={54} h={28} label="2 → 0" />
        <Box x={84} y={90} w={54} h={28} label="3 → 1" fill={OK} />
        <Line x1={152} y1={26} x2={116} y2={90} dashed />
        <Cap x={150} y={112} fill={T}>found 3! → answer is those two</Cap>
      </Svg>
    ),
  },

  // Two pointers (palindrome)
  'valid-palindrome': {
    caption: 'Put one finger on each end and walk them inward, checking the letters match.',
    svg: (
      <Svg h={120}>
        {['r', 'a', 'c', 'e', 'c', 'a', 'r'].map((c, i) => (
          <Box key={i} x={20 + i * 42} y={30} w={34} label={c} fill={i === 0 || i === 6 ? ACC : SURF} />
        ))}
        <Line x1={37} y1={90} x2={37} y2={64} />
        <Line x1={289} y1={90} x2={289} y2={64} />
        <Cap x={18} y={104}>left →</Cap>
        <Cap x={262} y={104}>← right</Cap>
      </Svg>
    ),
  },

  // Sliding window
  'longest-substring-no-repeat': {
    caption: 'Slide a window of all-different letters; when a repeat sneaks in, shrink from the left.',
    svg: (
      <Svg h={120}>
        {['a', 'b', 'c', 'a', 'b'].map((c, i) => (
          <Box key={i} x={40 + i * 44} y={40} w={36} label={c} fill={i < 3 ? INFO : SURF} />
        ))}
        <rect x={36} y={36} width={136} height={38} rx={8} fill="none" stroke={ACC} strokeWidth={2.5} />
        <Cap x={64} y={100} fill={T}>window = "abc" (all unique)</Cap>
      </Svg>
    ),
  },

  // Binary search
  'binary-search': {
    caption: 'Look at the middle, then throw away the half that can’t contain your number.',
    svg: (
      <Svg h={130}>
        {[1, 2, 3, 4, 5, 6, 7].map((n, i) => (
          <Box key={i} x={16 + i * 44} y={40} w={36} label={String(n)}
               fill={i === 3 ? ACC : i > 3 ? SURF : 'var(--surface)'} />
        ))}
        {/* strike out the discarded left half */}
        <line x1={16} y1={58} x2={148} y2={58} stroke={WARN} strokeWidth={3} />
        <Line x1={38} y1={95} x2={38} y2={72} />
        <Cap x={18} y={110}>low</Cap>
        <Line x1={182} y1={95} x2={182} y2={72} />
        <Cap x={168} y={110}>mid</Cap>
        <Line x1={314} y1={95} x2={314} y2={72} />
        <Cap x={300} y={110}>high</Cap>
      </Svg>
    ),
  },

  // Reverse linked list
  'reverse-linked-list': {
    caption: 'Walk the train and flip every arrow to point backward.',
    svg: (
      <Svg h={150}>
        <Cap x={10} y={20}>before</Cap>
        <Box x={40} y={28} w={34} label="1" />
        <Box x={120} y={28} w={34} label="2" />
        <Box x={200} y={28} w={34} label="3" />
        <Line x1={76} y1={45} x2={118} y2={45} />
        <Line x1={156} y1={45} x2={198} y2={45} />
        <Cap x={10} y={100}>after</Cap>
        <Box x={40} y={100} w={34} label="1" fill={ACC} />
        <Box x={120} y={100} w={34} label="2" fill={ACC} />
        <Box x={200} y={100} w={34} label="3" fill={ACC} />
        <Line x1={118} y1={117} x2={76} y2={117} />
        <Line x1={198} y1={117} x2={156} y2={117} />
      </Svg>
    ),
  },

  // Linked list cycle (fast/slow)
  'linked-list-cycle': {
    caption: 'A fast runner (2 steps) and a slow runner (1 step) will meet if the track loops.',
    svg: (
      <Svg h={150}>
        <Box x={30} y={60} w={30} label="1" />
        <Box x={100} y={60} w={30} label="2" />
        <Box x={170} y={60} w={30} label="3" fill={OK} />
        <Line x1={62} y1={75} x2={98} y2={75} />
        <Line x1={132} y1={75} x2={168} y2={75} />
        {/* loop back to node 2 */}
        <path d="M185,60 C185,20 115,20 115,58" fill="none" stroke={MUT} strokeWidth={1.6} markerEnd="url(#arw)" />
        <Cap x={220} y={64} fill={T}>3 points back → loop!</Cap>
        <Cap x={30} y={120}>🐢 slow +1   🐇 fast +2 → they meet</Cap>
      </Svg>
    ),
  },

  // Tree depth
  'max-depth-binary-tree': {
    caption: 'A branch’s height is 1 + the taller of its two children.',
    svg: (
      <Svg h={160}>
        <line x1={170} y1={40} x2={110} y2={85} stroke={BORD} strokeWidth={1.6} />
        <line x1={170} y1={40} x2={230} y2={85} stroke={BORD} strokeWidth={1.6} />
        <line x1={230} y1={100} x2={200} y2={130} stroke={BORD} strokeWidth={1.6} />
        <line x1={230} y1={100} x2={260} y2={130} stroke={BORD} strokeWidth={1.6} />
        <circle cx={170} cy={35} r={16} fill={ACC} stroke={BORD} />
        <circle cx={110} cy={95} r={16} fill={SURF} stroke={BORD} />
        <circle cx={230} cy={95} r={16} fill={SURF} stroke={BORD} />
        <circle cx={200} cy={140} r={14} fill={SURF} stroke={BORD} />
        <circle cx={260} cy={140} r={14} fill={SURF} stroke={BORD} />
        <Cap x={10} y={40}>level 1</Cap>
        <Cap x={10} y={100}>level 2</Cap>
        <Cap x={10} y={145}>level 3 → depth = 3</Cap>
      </Svg>
    ),
  },

  // BFS level order
  'binary-tree-level-order': {
    caption: 'Read the tree floor by floor, left to right — a queue keeps the order.',
    svg: (
      <Svg h={150}>
        <line x1={170} y1={30} x2={110} y2={75} stroke={BORD} strokeWidth={1.6} />
        <line x1={170} y1={30} x2={230} y2={75} stroke={BORD} strokeWidth={1.6} />
        <circle cx={170} cy={28} r={15} fill={ACC} stroke={BORD} />
        <circle cx={110} cy={85} r={15} fill={INFO} stroke={BORD} />
        <circle cx={230} cy={85} r={15} fill={INFO} stroke={BORD} />
        <rect x={40} y={118} width={260} height={26} rx={7} fill={SURF} stroke={BORD} />
        <text x={170} y={135} textAnchor="middle" fill={T} fontFamily="ui-monospace, monospace">3, 9, 20, …</text>
        <Cap x={40} y={112}>output, floor by floor:</Cap>
      </Svg>
    ),
  },

  // LRU cache
  'lru-cache': {
    caption: 'Newest used goes to the front; when the shelf is full, drop the oldest-used from the back.',
    svg: (
      <Svg h={130}>
        <Cap x={20} y={30} fill={T}>most recent</Cap>
        <Cap x={250} y={30} fill={T}>least recent</Cap>
        <Box x={20} y={45} w={50} label="C" fill={OK} />
        <Box x={90} y={45} w={50} label="B" />
        <Box x={160} y={45} w={50} label="A" />
        <Box x={250} y={45} w={60} h={30} label="evict" fill={WARN} />
        <Line x1={215} y1={60} x2={248} y2={60} />
        <Cap x={20} y={104}>use something → it jumps to the front</Cap>
      </Svg>
    ),
  },

  // Min heap
  'min-heap': {
    caption: 'The smallest value always sits on top, so you can grab it instantly.',
    svg: (
      <Svg h={150}>
        <line x1={170} y1={35} x2={120} y2={80} stroke={BORD} strokeWidth={1.6} />
        <line x1={170} y1={35} x2={220} y2={80} stroke={BORD} strokeWidth={1.6} />
        <circle cx={170} cy={32} r={17} fill={ACC} stroke={BORD} />
        <text x={170} y={37} textAnchor="middle" fill={T} fontFamily="ui-monospace, monospace">1</text>
        <circle cx={120} cy={88} r={16} fill={SURF} stroke={BORD} />
        <text x={120} y={93} textAnchor="middle" fill={T} fontFamily="ui-monospace, monospace">3</text>
        <circle cx={220} cy={88} r={16} fill={SURF} stroke={BORD} />
        <text x={220} y={93} textAnchor="middle" fill={T} fontFamily="ui-monospace, monospace">8</text>
        <Cap x={10} y={140}>parent is always ≤ its children</Cap>
      </Svg>
    ),
  },

  // Union-Find
  'union-find': {
    caption: 'Dots joined into groups — you can ask “same group?” super fast.',
    svg: (
      <Svg h={130}>
        <circle cx={60} cy={60} r={14} fill={ACC} stroke={BORD} />
        <circle cx={110} cy={45} r={14} fill={ACC} stroke={BORD} />
        <circle cx={110} cy={85} r={14} fill={ACC} stroke={BORD} />
        <line x1={74} y1={57} x2={96} y2={48} stroke={MUT} strokeWidth={1.6} />
        <line x1={74} y1={63} x2={96} y2={82} stroke={MUT} strokeWidth={1.6} />
        <circle cx={250} cy={60} r={14} fill={INFO} stroke={BORD} />
        <circle cx={300} cy={60} r={14} fill={INFO} stroke={BORD} />
        <line x1={264} y1={60} x2={286} y2={60} stroke={MUT} strokeWidth={1.6} />
        <Cap x={40} y={112}>group A</Cap>
        <Cap x={250} y={112}>group B</Cap>
      </Svg>
    ),
  },

  // Merge intervals
  'merge-intervals': {
    caption: 'Overlapping time blocks get squished into one bigger block.',
    svg: (
      <Svg h={130}>
        <line x1={20} y1={100} x2={320} y2={100} stroke={BORD} strokeWidth={1.5} />
        <rect x={40} y={35} width={110} height={20} rx={5} fill={ACC} opacity={0.85} />
        <rect x={110} y={60} width={120} height={20} rx={5} fill={INFO} opacity={0.85} />
        <Cap x={40} y={30}>[1,4]</Cap>
        <Cap x={190} y={55}>[3,7]</Cap>
        <rect x={40} y={100} width={190} height={0} />
        <text x={130} y={122} textAnchor="middle" fill={T} fontSize={12}>they overlap → merge into [1,7]</text>
      </Svg>
    ),
  },

  // Middleware onion
  'middleware-pipeline': {
    caption: 'Each layer does a bit on the way IN, calls the next, then a bit on the way OUT — like an onion.',
    svg: (
      <Svg h={160}>
        <rect x={40} y={20} width={260} height={120} rx={14} fill="none" stroke={ACC} strokeWidth={2} />
        <rect x={80} y={45} width={180} height={70} rx={12} fill="none" stroke={INFO} strokeWidth={2} />
        <rect x={130} y={65} width={80} height={30} rx={9} fill={OK} stroke={BORD} />
        <text x={170} y={85} textAnchor="middle" fill={T} fontSize={12}>your code</text>
        <Cap x={46} y={38} fill={T}>Middleware A</Cap>
        <Cap x={86} y={62} fill={T}>Middleware B</Cap>
        <Cap x={40} y={155}>request goes in → core → response comes back out</Cap>
      </Svg>
    ),
  },

  // DI lifetimes
  'di-lifetimes': {
    caption: 'Singleton = everyone shares ONE. Transient = a brand-new one each time you ask.',
    svg: (
      <Svg h={140}>
        <Cap x={20} y={24} fill={T}>singleton (shared)</Cap>
        <circle cx={60} cy={60} r={14} fill={ACC} stroke={BORD} />
        <Line x1={95} y1={45} x2={70} y2={55} />
        <Line x1={95} y1={75} x2={70} y2={65} />
        <Cap x={100} y={48}>asker 1</Cap>
        <Cap x={100} y={82}>asker 2</Cap>
        <Cap x={210} y={24} fill={T}>transient (new each)</Cap>
        <circle cx={240} cy={50} r={12} fill={INFO} stroke={BORD} />
        <circle cx={290} cy={50} r={12} fill={INFO} stroke={BORD} />
        <circle cx={265} cy={90} r={12} fill={INFO} stroke={BORD} />
        <Cap x={210} y={124}>3 asks → 3 objects</Cap>
      </Svg>
    ),
  },

  // Cosine similarity — angle between two vectors
  'cosine-similarity': {
    caption: 'Two arrows from the same start: pointing the same way = 1, at a right angle = 0.',
    svg: (
      <Svg h={150}>
        <line x1={40} y1={125} x2={210} y2={45} stroke={ACC} strokeWidth={2.4} markerEnd="url(#arw)" />
        <line x1={40} y1={125} x2={230} y2={110} stroke={INFO} strokeWidth={2.4} markerEnd="url(#arw)" />
        <path d="M78,113 A40,40 0 0 1 92,120" fill="none" stroke={MUT} strokeWidth={1.4} />
        <Cap x={92} y={112}>θ</Cap>
        <Cap x={150} y={40} fill={T}>vector A</Cap>
        <Cap x={175} y={125} fill={T}>vector B</Cap>
        <Cap x={20} y={140}>small angle → high similarity</Cap>
      </Svg>
    ),
  },

  // Softmax — scores become probabilities
  'softmax': {
    caption: 'Turn raw scores into slices of a pie that add up to 100%.',
    svg: (
      <Svg h={140}>
        <Cap x={20} y={20}>logits</Cap>
        {[30, 55, 80].map((h, i) => (
          <rect key={i} x={30 + i * 26} y={110 - h} width={18} height={h} rx={3} fill={SURF} stroke={BORD} />
        ))}
        <Line x1={140} y1={70} x2={175} y2={70} />
        <Cap x={135} y={60}>softmax</Cap>
        <Cap x={200} y={20}>probabilities</Cap>
        {[18, 33, 55].map((h, i) => (
          <rect key={i} x={205 + i * 26} y={110 - h} width={18} height={h} rx={3} fill={ACC} stroke={BORD} />
        ))}
        <Cap x={205} y={128}>sum = 1.0</Cap>
      </Svg>
    ),
  },

  // Top-K — keep the tallest few
  'top-k-logits': {
    caption: 'Keep only the few highest-scoring options to choose from — a shortlist.',
    svg: (
      <Svg h={130}>
        {[40, 90, 55, 75, 25].map((h, i) => (
          <rect key={i} x={30 + i * 40} y={105 - h} width={26} height={h} rx={4}
                fill={i === 1 || i === 3 ? ACC : SURF} stroke={BORD} />
        ))}
        <Cap x={30} y={122}>the 2 tallest (k=2) are picked</Cap>
      </Svg>
    ),
  },

  // Text chunking — overlapping windows
  'text-chunking': {
    caption: 'Cut text into overlapping pieces so ideas aren’t sliced in half.',
    svg: (
      <Svg h={120}>
        {['a', 'b', 'c', 'd', 'e'].map((c, i) => (
          <Box key={i} x={30 + i * 46} y={45} w={38} label={c} />
        ))}
        <rect x={26} y={38} width={140} height={40} rx={8} fill="none" stroke={ACC} strokeWidth={2} />
        <rect x={118} y={44} width={140} height={40} rx={8} fill="none" stroke={INFO} strokeWidth={2} />
        <Cap x={30} y={104} fill={T}>chunk 1 and chunk 2 share "c"</Cap>
      </Svg>
    ),
  },

  // Jaccard — Venn diagram
  'jaccard-similarity': {
    caption: 'Shared words ÷ all words. Big overlap = similar.',
    svg: (
      <Svg h={140}>
        <circle cx={135} cy={75} r={48} fill={ACC} opacity={0.35} stroke={BORD} />
        <circle cx={205} cy={75} r={48} fill={INFO} opacity={0.35} stroke={BORD} />
        <Cap x={95} y={78} fill={T}>a</Cap>
        <text x={170} y={80} textAnchor="middle" fill={T} fontSize={12}>shared</text>
        <Cap x={240} y={78} fill={T}>b</Cap>
        <Cap x={110} y={132}>overlap ÷ total</Cap>
      </Svg>
    ),
  },

  // Bloom filter — bit array
  'bloom-filter': {
    caption: 'One key flips a few switches ON. If any of them is OFF later, the key was definitely never added.',
    svg: (
      <Svg h={140}>
        <Box x={20} y={20} w={44} h={26} label="key" fill={ACC} />
        {[0, 1, 2, 3, 4, 5, 6, 7].map((i) => (
          <rect key={i} x={30 + i * 34} y={80} width={28} height={28} rx={5}
                fill={i === 1 || i === 4 || i === 6 ? OK : SURF} stroke={BORD} />
        ))}
        <Line x1={44} y1={48} x2={64} y2={78} />
        <Line x1={54} y1={48} x2={166} y2={78} />
        <Line x1={64} y1={48} x2={234} y2={78} />
        <Cap x={30} y={124}>3 hashes → 3 bits set</Cap>
      </Svg>
    ),
  },

  // Consistent hashing — the ring
  'consistent-hashing': {
    caption: 'Servers sit around a ring; a key walks clockwise to the next server. Adding one only moves a few keys.',
    svg: (
      <Svg h={165}>
        <circle cx={170} cy={82} r={58} fill="none" stroke={BORD} strokeWidth={2} />
        <circle cx={170} cy={24} r={9} fill={ACC} stroke={BORD} /><Cap x={162} y={16}>A</Cap>
        <circle cx={222} cy={112} r={9} fill={ACC} stroke={BORD} /><Cap x={232} y={116}>B</Cap>
        <circle cx={118} cy={112} r={9} fill={ACC} stroke={BORD} /><Cap x={100} y={116}>C</Cap>
        <circle cx={210} cy={45} r={6} fill={WARN} stroke={BORD} />
        <path d="M214,52 A30,30 0 0 1 224,100" fill="none" stroke={MUT} strokeWidth={1.4} markerEnd="url(#arw)" />
        <Cap x={228} y={44}>key → B</Cap>
      </Svg>
    ),
  },

  // Token bucket
  'token-bucket': {
    caption: 'Coins drip into a bucket over time; each request spends one. Empty bucket = wait.',
    svg: (
      <Svg h={150}>
        <path d="M120,40 L220,40 L205,120 L135,120 Z" fill={SURF} stroke={BORD} strokeWidth={1.6} />
        <circle cx={155} cy={95} r={8} fill={WARN} stroke={BORD} />
        <circle cx={180} cy={100} r={8} fill={WARN} stroke={BORD} />
        <circle cx={172} cy={78} r={8} fill={WARN} stroke={BORD} />
        <Line x1={170} y1={12} x2={170} y2={38} />
        <Cap x={185} y={22}>refill</Cap>
        <Line x1={210} y1={110} x2={250} y2={110} />
        <Cap x={228} y={102}>spend</Cap>
      </Svg>
    ),
  },

  // Sliding window rate limiter
  'sliding-window-rate-limiter': {
    caption: 'Count only the requests inside the moving time window; old ones slide out.',
    svg: (
      <Svg h={120}>
        <line x1={20} y1={80} x2={320} y2={80} stroke={BORD} strokeWidth={1.5} />
        {[40, 70, 110, 200, 240, 275].map((x, i) => (
          <circle key={i} cx={x} cy={80} r={6} fill={x >= 180 ? ACC : MUT} stroke={BORD} />
        ))}
        <rect x={180} y={58} width={120} height={44} rx={8} fill="none" stroke={ACC} strokeWidth={2} />
        <Cap x={190} y={52} fill={T}>last window → 3 requests</Cap>
        <Cap x={20} y={108}>older requests don’t count</Cap>
      </Svg>
    ),
  },

  // Ring buffer
  'ring-buffer': {
    caption: 'A loop of slots: when full, the new value writes over the oldest.',
    svg: (
      <Svg h={150}>
        <circle cx={170} cy={78} r={52} fill="none" stroke={BORD} strokeWidth={2} />
        {[[170, 26], [214, 60], [198, 112], [142, 112], [126, 60]].map(([x, y], i) => (
          <circle key={i} cx={x} cy={y} r={13} fill={i === 0 ? ACC : SURF} stroke={BORD} />
        ))}
        <Line x1={205} y1={20} x2={182} y2={22} />
        <Cap x={210} y={20}>write here (wraps around)</Cap>
      </Svg>
    ),
  },

  // Exponential backoff
  'exponential-backoff': {
    caption: 'Wait longer after each failure — 100, 200, 400… — up to a cap.',
    svg: (
      <Svg h={130}>
        {[20, 40, 70, 95].map((h, i) => (
          <rect key={i} x={30 + i * 48} y={105 - h} width={32} height={h} rx={4} fill={ACC} stroke={BORD} />
        ))}
        <line x1={20} y1={20} x2={230} y2={20} stroke={WARN} strokeWidth={2} strokeDasharray="4 3" />
        <Cap x={232} y={24}>max cap</Cap>
        <Cap x={30} y={122}>retry 1 · 2 · 3 · 4 →</Cap>
      </Svg>
    ),
  },

  // Weighted round-robin
  'weighted-round-robin': {
    caption: 'Deal work in turns, but bigger servers get more turns each round.',
    svg: (
      <Svg h={110}>
        {['A', 'A', 'B', 'A', 'A', 'B'].map((c, i) => (
          <Box key={i} x={20 + i * 48} y={40} w={38} label={c} fill={c === 'A' ? ACC : INFO} />
        ))}
        <Cap x={20} y={92}>weights A=2, B=1 → A A B repeating</Cap>
      </Svg>
    ),
  },

  // Event sourcing fold
  'event-sourcing-fold': {
    caption: 'Replay every change in order to get the total — like adding up a receipt.',
    svg: (
      <Svg h={120}>
        <Box x={20} y={40} w={70} h={30} label="+100" fill={OK} />
        <Box x={100} y={40} w={70} h={30} label="−30" fill={ERR} />
        <Box x={180} y={40} w={70} h={30} label="+10" fill={OK} />
        <Line x1={255} y1={55} x2={285} y2={55} />
        <Box x={288} y={40} w={44} h={30} label="80" fill={ACC} />
        <Cap x={20} y={92}>fold the events → balance</Cap>
      </Svg>
    ),
  },
}
