import { RootState, StoreContainer } from 'shared/store/rootTypes'
import { Store } from 'redux'
import { isMetaConfigurationInitiazed } from './selectors'
import { MessageOfTheDayConfig } from './types'

declare const globalThis: StoreContainer

export async function ensureMetaConfigurationInitialized(): Promise<void> {
  const store: Store<RootState> = (window as any)['globalStore']

  const initialized = isMetaConfigurationInitiazed(store.getState())
  if (initialized) {
    return Promise.resolve()
  }

  return new Promise<void>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const initialized = isMetaConfigurationInitiazed(store.getState())
      if (initialized) {
        unsubscribe()
        return resolve()
      }
    })
  })
}

export async function waitForMessageOfTheDay(): Promise<MessageOfTheDayConfig | null> {
  const store: Store<RootState> = globalThis.globalStore
  const worldConfig = store.getState().meta.config.world
  if (worldConfig && worldConfig.messageOfTheDayInit) {
    return Promise.resolve(worldConfig.messageOfTheDay || null)
  }
  return new Promise<MessageOfTheDayConfig | null>((resolve) => {
    const unsubscribe = globalThis.globalStore.subscribe(() => {
      const worldConfig = globalThis.globalStore.getState().meta.config.world
      if (worldConfig && worldConfig.messageOfTheDayInit) {
        unsubscribe()
        return resolve(worldConfig.messageOfTheDay || null)
      }
    })
  })
}
