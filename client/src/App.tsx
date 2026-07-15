import { useEffect, useState } from 'react'

// Step-1 placeholder App: proves the frontend boots with the pastel theme AND
// can reach the backend through the Vite proxy. Real routing/pages arrive in
// Step 5. Kept intentionally tiny.
function App() {
  // Track the backend health-check result: 'checking' until the fetch resolves.
  const [health, setHealth] = useState<'checking' | 'ok' | 'down'>('checking')

  useEffect(() => {
    // Calls /api/health — Vite forwards this to the .NET API on :5246 (see vite.config.ts).
    fetch('/api/health')
      .then((r) => (r.ok ? setHealth('ok') : setHealth('down')))
      .catch(() => setHealth('down')) // network error / API not running
  }, [])

  return (
    <div className="min-h-screen flex items-center justify-center p-6">
      <div className="bg-card rounded-[var(--radius-soft)] shadow-sm p-10 max-w-md w-full text-center">
        <h1 className="text-3xl font-bold text-ink mb-2">DotNetDojo 🥋</h1>
        <p className="text-muted mb-6">Interactive .NET interview practice</p>

        {/* Backend connectivity badge — a live proof the full stack is wired up. */}
        <div className="inline-flex items-center gap-2 rounded-full px-4 py-2 text-sm font-medium"
             style={{ backgroundColor: health === 'ok' ? 'var(--color-sage)' : 'var(--color-butter)' }}>
          <span className="w-2 h-2 rounded-full bg-ink/60" />
          API: {health === 'checking' ? 'checking…' : health === 'ok' ? 'connected' : 'unreachable'}
        </div>
      </div>
    </div>
  )
}

export default App
