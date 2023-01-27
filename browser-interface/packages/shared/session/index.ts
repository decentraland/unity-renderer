import { getProvider as getProviderSelector } from './selectors'
import {
  getFromPersistentStorage,
  getKeysFromPersistentStorage,
  removeFromPersistentStorage,
  saveToPersistentStorage
} from 'atomicHelpers/persistentStorage'
import { StoredSession } from './types'
import { store } from 'shared/store/isolatedStore'

const SESSION_KEY_PREFIX = 'dcl-session'
const LAST_SESSION_KEY = 'dcl-last-session-id'

function sessionKey(userId: string) {
  return `${SESSION_KEY_PREFIX}-${userId.toLocaleLowerCase()}`
}

export const setStoredSession: (session: StoredSession) => Promise<void> = async (session) => {
  await saveToPersistentStorage(LAST_SESSION_KEY, session.identity.address)
  await saveToPersistentStorage(sessionKey(session.identity.address), session)
}

export const getStoredSession: (userId: string) => Promise<StoredSession | null> = async (userId) => {
  const existingSession: StoredSession | null = (await getFromPersistentStorage(sessionKey(userId))) as StoredSession

  if (existingSession) {
    return existingSession
  } else {
    // If not existing session was found, we check the old session storage
    const oldSession: StoredSession | null = (await getFromPersistentStorage('dcl-profile')) || {}
    if (oldSession && oldSession.identity && oldSession.identity.address === userId) {
      await setStoredSession(oldSession)
      return oldSession
    }
  }

  return null
}

export const removeStoredSession = async (userId?: string) => {
  if (userId) await removeFromPersistentStorage(sessionKey(userId))
}
export const getLastSessionWithoutWallet: () => Promise<StoredSession | null> = async () => {
  const lastSessionId = await getFromPersistentStorage(LAST_SESSION_KEY)
  if (lastSessionId) {
    const lastSession = await getStoredSession(lastSessionId)
    return lastSession ? lastSession : null
  } else {
    return getFromPersistentStorage('dcl-profile')
  }
}

export const getLastSessionByAddress: (address: string) => Promise<StoredSession | null> = async (address: string) => {
  const sessionsKey = (await getKeysFromPersistentStorage()).filter((k) => k.indexOf(SESSION_KEY_PREFIX) === 0)
  const sessions: StoredSession[] = await Promise.all(sessionsKey.map((id) => getFromPersistentStorage(id)))
  const filteredSessions = sessions.filter(
    ({ identity }) => ('' + identity.address).toLowerCase() === address.toLowerCase()
  )
  return filteredSessions.length > 0 ? filteredSessions[0] : null
}

export const getLastGuestSession: () => Promise<StoredSession | null> = async () => {
  const sessionsKey = (await getKeysFromPersistentStorage()).filter((k) => k.indexOf(SESSION_KEY_PREFIX) === 0)
  const sessions: StoredSession[] = await Promise.all(sessionsKey.map((id) => getFromPersistentStorage(id)))

  const filteredSessions: StoredSession[] = sessions
    .filter(({ isGuest }) => isGuest)
    .sort((a, b) => {
      const da = new Date(a.identity.expiration)
      const db = new Date(b.identity.expiration)
      if (da > db) return -1
      if (da < db) return 1
      return 0
    }) // last expiration is first

  return filteredSessions.length > 0 ? filteredSessions[0] : null
}

export const getProvider = () => getProviderSelector(store.getState())
