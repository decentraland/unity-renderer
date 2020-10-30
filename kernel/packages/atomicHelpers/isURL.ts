export function isURL(maybeUrl: string): boolean {
  try {
    // tslint:disable-next-line
    new URL(maybeUrl)
    return true
  } catch (e) {
    return false
  }
}
