// Presentation metadata for the ATS-friendly resume templates. Keyed by the same slug
// the backend renderer understands (ResumeDocumentService.TemplateStyle.Resolve).
// Every template is single-column with standard fonts and no tables/graphics — the
// differences are purely typographic, so all of them parse cleanly. Mirrors topicMeta.
interface TemplateMeta {
  slug: string
  name: string
  emoji: string
  description: string
}

export const RESUME_TEMPLATES: TemplateMeta[] = [
  {
    slug: 'classic',
    name: 'Classic',
    emoji: '📃',
    description: 'Serif, uppercase headings with a rule. Traditional and recruiter-safe.',
  },
  {
    slug: 'compact',
    name: 'Compact',
    emoji: '🗜️',
    description: 'Tight sans-serif that fits more on one page. Great for long histories.',
  },
  {
    slug: 'modern',
    name: 'Modern',
    emoji: '🎯',
    description: 'Clean sans-serif with a subtle teal accent on section headings.',
  },
]

export function templateMeta(slug: string): TemplateMeta {
  return RESUME_TEMPLATES.find((t) => t.slug === slug) ?? RESUME_TEMPLATES[0]
}
