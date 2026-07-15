// Presentation metadata for topics: an emoji glyph, a category for grouping, and an
// accent colour used to tint the learning-path unit for that topic. Keyed by topic
// slug; falls back gracefully for unknown slugs.
interface Meta { emoji: string; category: string; color: string }

const META: Record<string, Meta> = {
  'csharp-language':     { emoji: '🔷', category: 'Language & Runtime', color: '#1cb0f6' },
  'async':               { emoji: '⚡', category: 'Language & Runtime', color: '#ffc800' },
  'multithreading':      { emoji: '🧵', category: 'Language & Runtime', color: '#ff9600' },
  'garbage-collection':  { emoji: '🗑️', category: 'Language & Runtime', color: '#58cc02' },

  'algorithms':          { emoji: '🧮', category: 'Problem Solving', color: '#8b5cf6' },
  'system-design':       { emoji: '🏗️', category: 'Problem Solving', color: '#ff4b4b' },

  'design-patterns':     { emoji: '🎭', category: 'Design & Architecture', color: '#ec4899' },
  'enterprise-patterns': { emoji: '🏢', category: 'Design & Architecture', color: '#14b8a6' },
  'solid':               { emoji: '🧱', category: 'Design & Architecture', color: '#f97316' },
  'architecture':        { emoji: '🌐', category: 'Design & Architecture', color: '#1cb0f6' },
  'microservices':       { emoji: '🕸️', category: 'Design & Architecture', color: '#8b5cf6' },

  'unit-testing':        { emoji: '🧪', category: 'Craft & Quality', color: '#58cc02' },
  'clean-code':          { emoji: '🧼', category: 'Craft & Quality', color: '#1cb0f6' },

  'aspnet-core':         { emoji: '🌍', category: 'Web & Data', color: '#0ea5e9' },
  'ef-core':             { emoji: '🗄️', category: 'Web & Data', color: '#f59e0b' },
  'sql':                 { emoji: '📇', category: 'Web & Data', color: '#14b8a6' },

  'devops':              { emoji: '🐳', category: 'Platform & DevOps', color: '#2563eb' },

  'ai-llm':              { emoji: '🤖', category: 'AI & Data Science', color: '#a855f7' },
}

export function topicMeta(slug: string): Meta {
  return META[slug] ?? { emoji: '📘', category: 'Other', color: '#8b5cf6' }
}

// Stable category order for the dashboard.
export const CATEGORY_ORDER = [
  'Language & Runtime',
  'Problem Solving',
  'Design & Architecture',
  'Craft & Quality',
  'Web & Data',
  'Platform & DevOps',
  'AI & Data Science',
  'Other',
]
