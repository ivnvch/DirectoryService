import * as React from "react"

const MOBILE_BREAKPOINT = 768

export function useIsMobile() {
  const getIsMobile = React.useCallback(
    () =>
      typeof window !== "undefined" && window.innerWidth < MOBILE_BREAKPOINT,
    []
  )
  const [isMobile, setIsMobile] = React.useState<boolean>(getIsMobile)

  React.useEffect(() => {
    const mediaQuery = window.matchMedia(
      `(max-width: ${MOBILE_BREAKPOINT - 1}px)`
    )

    const updateIsMobile = () => {
      // Using matchMedia keeps the JS state aligned with CSS breakpoints.
      setIsMobile(mediaQuery.matches)
    }

    updateIsMobile()
    mediaQuery.addEventListener("change", updateIsMobile)
    window.addEventListener("resize", updateIsMobile)

    return () => {
      mediaQuery.removeEventListener("change", updateIsMobile)
      window.removeEventListener("resize", updateIsMobile)
    }
  }, [getIsMobile])

  return isMobile
}
