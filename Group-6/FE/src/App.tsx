import type { ReactNode } from "react"


// Add global providers here (e.g. QueryClientProvider, ThemeProvider, etc.)
function App({ children }: { children: ReactNode }) {
  return (
    <>
      {children}
    </>
  )
}

export default App