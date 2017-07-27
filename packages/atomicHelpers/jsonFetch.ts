import { future } from 'fp-future'

const requestCache: Map<string, any> = new Map()

/**
 * Fetches a resource and catches the response
 * @param url
 */
export async function jsonFetch(url: string): Promise<any> {
  if (requestCache.has(url)) {
    return requestCache.get(url)
  }

  const futureCache = future()

  requestCache.set(url, futureCache)

  fetch(url)
    .then($ => $.json(), e => futureCache.reject(e))
    .then($ => futureCache.resolve($), e => futureCache.reject(e))

  return futureCache
}
