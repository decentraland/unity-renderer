import { setLoadingScreenVisible } from 'unity-interface/dcl'

import { disconnect, sendToMordor } from 'shared/comms'
import { bringDownClientAndShowError } from 'shared/loading/ReportFatalError'
import { NEW_LOGIN } from 'shared/loading/types'
import { StoreContainer, RootState } from 'shared/store/rootTypes'

import { getCurrentIdentity, hasWallet as hasWalletSelector } from './selectors'
import { Store } from 'redux'
import { getFromLocalStorage, removeFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { StoredSession } from './types'

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
    return lastSession && !lastSession.identity.hasConnectedWeb3 ? lastSession : null
  } else {
    return getFromLocalStorage('dcl-profile')
  }
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
    window.location.reload()
  }

  disable() {
    bringDownClientAndShowError(NEW_LOGIN)
    sendToMordor()
    disconnect()
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
