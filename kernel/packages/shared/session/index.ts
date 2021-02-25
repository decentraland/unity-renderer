import { setLoadingScreenVisible } from 'unity-interface/dcl'

import { disconnect, sendToMordor } from 'shared/comms'
import { RootState, StoreContainer } from 'shared/store/rootTypes'

import { getCurrentIdentity, hasWallet as hasWalletSelector } from './selectors'
import { Store } from 'redux'
import {
  getFromLocalStorage,
  getKeysFromLocalStorage,
  removeFromLocalStorage,
  saveToLocalStorage
} from 'atomicHelpers/localStorage'
import { StoredSession } from './types'
import { ProviderType } from '../ethereum/ProviderType'

declare const globalThis: StoreContainer

const SESSION_KEY_PREFIX = 'dcl-session'
const LAST_SESSION_KEY = 'dcl-last-session-id'

function sessionKey(userId: string) {
  return `${SESSION_KEY_PREFIX}-${userId}`
}

export const setStoredSession: (session: StoredSession) => void = (session) => {
  saveToLocalStorage(LAST_SESSION_KEY, session.userId)
  saveToLocalStorage(sessionKey(session.userId), session)
}

export const getStoredSession: (userId: string) => StoredSession | null = (userId) => {
  const existingSession: StoredSession | null = getFromLocalStorage(sessionKey(userId))

  if (existingSession) {
    return existingSession
  } else {
    // If not existing session was found, we check the old session storage
    const oldSession: StoredSession | null = getFromLocalStorage('dcl-profile') || {}
    if (oldSession && oldSession.userId === userId) {
      setStoredSession(oldSession)
      return oldSession
    }
  }

  return null
}

export const removeStoredSession = (userId?: string) => {
  if (userId) removeFromLocalStorage(sessionKey(userId))
}
export const getLastSessionWithoutWallet: () => StoredSession | null = () => {
  const lastSessionId = getFromLocalStorage(LAST_SESSION_KEY)
  if (lastSessionId) {
    const lastSession = getStoredSession(lastSessionId)
    return lastSession ? lastSession : null
  } else {
    return getFromLocalStorage('dcl-profile')
  }
}

export const getLastSessionByProvider = (provider: ProviderType): StoredSession | null => {
  const sessions: StoredSession[] = getKeysFromLocalStorage()
    .filter((k) => k.indexOf(SESSION_KEY_PREFIX) === 0)
    .map((id) => getFromLocalStorage(id))
    .filter(({ identity }) => identity.provider === provider)
    .sort((a, b) => {
      const da = new Date(a.identity.expiration)
      const db = new Date(b.identity.expiration)
      if (da > db) return -1
      if (da < db) return 1
      return 0
    }) // last expiration is first

  return sessions.length > 0 ? sessions[0] : null
}

export const getIdentity = () => getCurrentIdentity(globalThis.globalStore.getState())

export const hasWallet = () => hasWalletSelector(globalThis.globalStore.getState())

export class Session {
  private static _instance: Session = new Session()

  static get current() {
    return Session._instance
  }

  async logout() {
    setLoadingScreenVisible(true)
    sendToMordor()
    disconnect()
    removeStoredSession(getIdentity()?.address)
    removeUrlParam('position')
    removeUrlParam('show_wallet')
    window.location.reload()
  }

  async redirectToSignUp() {
    window.location.search += '&show_wallet=1'
  }
}

export async function userAuthentified(): Promise<void> {
  const store: Store<RootState> = globalThis.globalStore

  const initialized = store.getState().session.initialized
  if (initialized) {
    return Promise.resolve()
  }

  return new Promise((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const initialized = store.getState().session.initialized
      if (initialized) {
        unsubscribe()
        return resolve()
      }
    })
  })
}

function removeUrlParam(paramToRemove: string) {
  let params = new URLSearchParams(window.location.search)
  params.delete(paramToRemove)
  window.history.replaceState({}, document.title, '?' + params.toString())
}
