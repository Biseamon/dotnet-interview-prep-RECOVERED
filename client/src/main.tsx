import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import './index.css'
import { Dashboard } from './pages/Dashboard'
import { TopicPage } from './pages/TopicPage'
import { TopicsIndexPage } from './pages/TopicsIndexPage'
import { ExercisePage } from './pages/ExercisePage'
import { GlossaryPage } from './pages/GlossaryPage'
import { QuestionsPage } from './pages/QuestionsPage'
import { ProgressPage } from './pages/ProgressPage'
import { DrillPage } from './pages/DrillPage'
import { ResumeBuilderPage } from './pages/ResumeBuilderPage'

// React Query manages server-state caching/loading for all API calls.
const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000, refetchOnWindowFocus: false } },
})

// Each page renders its own chrome (Sidebar / TopBar / full-screen), matching the
// three layout types in the design handoff — so there's no single global shell.
createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/topics" element={<TopicsIndexPage />} />
          <Route path="/topics/:slug" element={<TopicPage />} />
          <Route path="/exercises/:slug" element={<ExercisePage />} />
          <Route path="/glossary" element={<GlossaryPage />} />
          <Route path="/qa" element={<QuestionsPage />} />
          <Route path="/questions" element={<Navigate to="/qa" replace />} />
          <Route path="/progress" element={<ProgressPage />} />
          <Route path="/drill" element={<DrillPage />} />
          <Route path="/resume" element={<ResumeBuilderPage />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>,
)
