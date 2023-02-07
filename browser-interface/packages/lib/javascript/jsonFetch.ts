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

  fetch(url).then(
    async ($) => {
      if (!$.ok) {
        // do not cache in case of error fetching
        requestCache.delete(url)
        futureCache.reject(new Error('Response not ok - ' + url))
      } else {
        try {
          futureCache.resolve(await $.json())
        } catch (e: any) {
          console['error']('Error parsing json: ' + url, $)
          futureCache.reject(e)
        }
      }
    },
    (e) => futureCache.reject(e)
  )

  return futureCache
}
