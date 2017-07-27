const URL = require('url')

export function resolveUrl(baseUrl: string, url: string): string
export function resolveUrl(baseUrl: string, url: URL): URL
export function resolveUrl(baseUrl: string, url: string | URL) {
  if (typeof url === 'string') {
    if (url.match(/^data:/)) {
      return url
    } else if (url.match(/^blob:/)) {
      return url
    } else if (url.match(/^https?:/)) {
      return url
    } else if (url.match(/^ipns:/)) {
      const ipns = url.replace(/^ipns:/, '')

      return `https://gateway.ipfs.io/ipns/${ipns}/`
    } else if (url.match(/^ipfs:/)) {
      const ipfs = url.replace(/^ipfs:/, '')

      return `https://gateway.ipfs.io/ipfs/${ipfs}/`
    } else if (url.match(/^test-local:/)) {
      const folder = url.replace('test-local:', '')

      return URL.resolve(baseUrl, `/test-parcels/${folder}/`)
    } else {
      return URL.resolve(baseUrl, url)
    }
  } else if (url instanceof URL) {
    return url
  }
}
