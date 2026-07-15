import { useEffect, useState } from 'react'

// Theme state: 'light' | 'dark', persisted to localStorage and reflected as a
// `.dark` class on <html> (which flips the CSS variables in index.css). Defaults to
// the OS preference on first visit.
type Theme = 'light' | 'dark'

function initialTheme(): Theme {
  const saved = localStorage.getItem('dojo-theme') as Theme | null
  if (saved) return saved
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export function useTheme() {
  const [theme, setTheme] = useState<Theme>(initialTheme)

  useEffect(() => {
    document.documentElement.classList.toggle('dark', theme === 'dark')
    localStorage.setItem('dojo-theme', theme)
  }, [theme])

  const toggle = () => setTheme((t) => (t === 'dark' ? 'light' : 'dark'))
  return { theme, toggle }
}
