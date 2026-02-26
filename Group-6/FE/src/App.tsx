import type { ReactNode } from "react"
import { Toaster } from "sonner"

// Add global providers here (e.g. QueryClientProvider, ThemeProvider, etc.)
function App({ children }: { children: ReactNode }) {
  return (
    <>
      {children}
      <Toaster richColors position="top-right" />
    </>
  )
}

export default App