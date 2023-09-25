import type {RemoteProfileWithHash} from 'shared/profiles/types'
import {requestProfile} from './requestProfile'

export const cachedRequests = new Map<string, Promise<RemoteProfileWithHash | null>>()

export function requestCacheKey(userId: string, version?: number) {
  if (userId.startsWith('default')) return userId
  if (version) return `${userId}:${version}`
  return null
}

export function cachedRequest(userId: string, version?: number): Promise<RemoteProfileWithHash | null> {
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
      return {profile: {avatars: [], timestamp: Date.now()}, hash: "", signedHash: ""}
    } else {
      return _
    }
  })
}
