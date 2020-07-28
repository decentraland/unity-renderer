import { future, IFuture } from 'fp-future'

import { setLoadingScreenVisible } from 'unity-interface/dcl'

import { disconnect, sendToMordor } from 'shared/comms'
import { removeUserProfile } from 'shared/comms/peers'
import { bringDownClientAndShowError } from 'shared/loading/ReportFatalError'
import { NEW_LOGIN } from 'shared/loading/types'
import { StoreContainer, RootState } from 'shared/store/rootTypes'

import { getCurrentIdentity, hasWallet as hasWalletSelector } from './selectors'
import { Store } from 'redux'

declare const globalThis: StoreContainer

export class Session {
  private static _instance: IFuture<Session> = future()

  static get current() {
    return Session._instance
  }

  async logout() {
    setLoadingScreenVisible(true)
    sendToMordor()
    disconnect()
    removeUserProfile()
    window.location.reload()
  }

  disable() {
    bringDownClientAndShowError(NEW_LOGIN)
    sendToMordor()
    disconnect()
  }
}

// tslint:disable-next-line
export const getIdentity = () => getCurrentIdentity(globalThis.globalStore.getState())!

export const hasWallet = () => hasWalletSelector(globalThis.globalStore.getState())

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
