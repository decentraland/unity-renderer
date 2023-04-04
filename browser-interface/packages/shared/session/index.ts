import {
  getFromPersistentStorage,
  getKeysFromPersistentStorage,
  removeFromPersistentStorage,
  saveToPersistentStorage
} from 'lib/browser/persistentStorage'
import { StoredSession } from './types'
import { getProvider as getProviderSelector } from './selectors'
import { now } from 'lib/javascript/now'
import { store } from 'shared/store/isolatedStore'

/**
 * Store a session into the current PersistentStorage
 */
export const storeSession: (session: StoredSession) => Promise<void> = async (session) => {
  await saveToPersistentStorage(LAST_SESSION_KEY, session.identity.address)
  await saveToPersistentStorage(sessionKey(session.identity.address), session)
}

/**
 * Forget a session existing in the current PersistentStorage
 */
export const deleteSession = async (userId?: string) => {
  if (userId) {
    await removeFromPersistentStorage(sessionKey(userId))
  }
}

/**
 * Retrieve the last session stored by address
 */
export const retrieveLastSessionByAddress: (address: string) => Promise<StoredSession | null> = async (
  address: string
) => {
  const sessions = (await retrieveAllCurrentSessions()).filter(withAddress(address))

  return sessions.length > 0 ? sessions[0] : null
}

/**
 * Retrieve the last session stored that is a guest session
 */
export const retrieveLastGuestSession: () => Promise<StoredSession | null> = async () => {
  const sessions = (await retrieveAllCurrentSessions()).filter(isGuest).sort(byExpiration)

  return sessions.length > 0 ? sessions[0] : null
}

/**
 * Get the current provider (utility function that calls store.getState())
 */
export const getProvider = () => getProviderSelector(store.getState())

const SESSION_KEY_PREFIX = 'dcl-session'
const LAST_SESSION_KEY = 'dcl-last-session-id'

function sessionKey(userId: string) {
  return `${SESSION_KEY_PREFIX}-${userId.toLocaleLowerCase()}`
}

async function retrieveAllCurrentSessions(): Promise<StoredSession[]> {
  const allKeys = await getKeysFromPersistentStorage()
  const sessionKeys = allKeys.filter((_) => _.startsWith(SESSION_KEY_PREFIX))
  const sessions = await Promise.all(sessionKeys.map(getFromPersistentStorage))
  return sessions.filter(notNull).filter(notExpired)
}

function isGuest(session: StoredSession) {
  return !!session.isGuest
}

function notNull(session: StoredSession | null): session is StoredSession {
  return !!session
}

function withAddress(address: string) {
  return function withAddressFilter(session: StoredSession) {
    return ('' + session.identity.address).toLowerCase() === address.toLowerCase()
  }
}

function notExpired(session: StoredSession) {
  return new Date(session.identity.expiration).getTime() > now()
}

function byExpiration(a: StoredSession, b: StoredSession) {
  const da = new Date(a.identity.expiration)
  const db = new Date(b.identity.expiration)
  if (da > db) return -1
  if (da < db) return 1
  return 0
}
