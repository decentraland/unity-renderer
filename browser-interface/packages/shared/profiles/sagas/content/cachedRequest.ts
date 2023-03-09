import type { RemoteProfile } from 'shared/profiles/types'
import { requestProfile } from './requestProfile'

export const cachedRequests = new Map<string, Promise<RemoteProfile | null>>()

export function requestCacheKey(userId: string, version?: number) {
  if (userId.startsWith('default')) return userId
  if (version) return `${userId}:${version}`
  return null
}

export function cachedRequest(userId: string, version?: number): Promise<RemoteProfile | null> {
  const key = requestCacheKey(userId, version)

  if (key !== null && cachedRequests.has(key)) {
    return cachedRequests.get(key)!
  }

  const req = requestProfile(userId, version)

  if (key) {
    cachedRequests.set(key, req)
  }

  return req.then((_) => {
    if (_ === null) {
      if (key) {
        cachedRequests.delete(key)
      }
      // TODO: Document why are we returning an empty response instead of null
      return { avatars: [], timestamp: Date.now() }
    } else {
      return _
    }
  })
}
