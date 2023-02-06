export function isURL(maybeUrl: string): boolean {
  try {
    new URL(maybeUrl)
    return true
  } catch (e) {
    return false
  }
}
