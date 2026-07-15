import Editor, { type OnMount } from '@monaco-editor/react'
import { useRef } from 'react'
import type { CompileError } from '../api/client'
import { useTheme } from '../lib/theme'

// Monaco editor wrapper for C#. Responsibilities:
//  1. Two-way bind the text (value/onChange).
//  2. Render backend compile errors as inline markers (red squiggles).
//  3. Match the app light/dark theme.
//  4. Submit on Cmd/Ctrl+Enter via onSubmit.
interface Props {
  value: string
  onChange: (value: string) => void
  compileErrors: CompileError[]
  onSubmit?: () => void
  height?: string
  language?: string // exercise language: 'CSharp' | 'Sql' | 'Config'
}

// Map our exercise language to a Monaco syntax mode.
function monacoLang(language?: string): string {
  if (language === 'Sql') return 'sql'
  if (language === 'Config') return 'yaml'
  return 'csharp'
}

export function CodeEditor({ value, onChange, compileErrors, onSubmit, height = '62vh', language }: Props) {
  const { theme } = useTheme()
  const editorRef = useRef<Parameters<OnMount>[0] | null>(null)
  const monacoRef = useRef<Parameters<OnMount>[1] | null>(null)
  // Keep the latest onSubmit so the Monaco command (bound once) calls the current one.
  const submitRef = useRef<(() => void) | undefined>(onSubmit)
  submitRef.current = onSubmit

  const handleMount: OnMount = (editor, monaco) => {
    editorRef.current = editor
    monacoRef.current = monaco
    // Cmd/Ctrl+Enter → run.
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.Enter, () => submitRef.current?.())
    applyMarkers()
  }

  const applyMarkers = () => {
    const editor = editorRef.current
    const monaco = monacoRef.current
    if (!editor || !monaco) return
    const model = editor.getModel()
    if (!model) return
    const markers = compileErrors.map((e) => ({
      startLineNumber: e.line,
      startColumn: e.column,
      endLineNumber: e.endLine,
      endColumn: e.endColumn,
      message: `${e.id}: ${e.message}`,
      severity: monaco.MarkerSeverity.Error,
    }))
    monaco.editor.setModelMarkers(model, 'grader', markers)
  }

  applyMarkers()

  return (
    <div style={{ overflow: 'hidden' }}>
      <Editor
        height={height}
        language={monacoLang(language)}
        theme={theme === 'dark' ? 'vs-dark' : 'vs'}
        value={value}
        onChange={(v) => onChange(v ?? '')}
        onMount={handleMount}
        options={{
          fontSize: 14,
          fontLigatures: true,
          minimap: { enabled: false },
          scrollBeyondLastLine: false,
          padding: { top: 14, bottom: 14 },
          tabSize: 4,
          smoothScrolling: true,
          cursorBlinking: 'smooth',
          renderLineHighlight: 'all',
          roundedSelection: true,
        }}
      />
    </div>
  )
}
