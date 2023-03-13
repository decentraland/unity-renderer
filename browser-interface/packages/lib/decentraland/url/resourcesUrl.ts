/**
 * Utility function used in-browser to detect the absolute URL given a relative path
 *
 * TODO: Evaluate if this is a horrible hack or not. If not, find a better name for this.
 */
export function getResourcesURL(path: string) {
  const base = new URL('.', document.location.toString()).toString()
  return new URL(path, base).toString()
}
