let base = new URL('.', document.location.toString()).toString()

export function setResourcesURL(baseUrl: string) {
  base = new URL(baseUrl, document.location.toString()).toString()
}

export function getResourcesURL(path: string) {
  return new URL(path, base).toString()
}
