import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Dev proxy: any request the frontend makes to /api/* is forwarded to the
    // ASP.NET Core API on port 5246. This lets the browser call "/api/health"
    // (same-origin) instead of a cross-origin URL, sidestepping most CORS pain
    // and mirroring how it works in production (API serves the built SPA).
    proxy: {
      '/api': 'http://localhost:5246',
    },
  },
})
