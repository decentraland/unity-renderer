import { RootState } from 'shared/store/rootTypes'
import { Store } from 'redux'
import { isMetaConfigurationInitiazed } from './selectors'

export async function ensureMetaConfigurationInitialized(): Promise<void> {
  const store: Store<RootState> = (window as any)['globalStore']

  const initialized = isMetaConfigurationInitiazed(store.getState())
  if (initialized) {
    return Promise.resolve()
  }

  return new Promise((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const initialized = isMetaConfigurationInitiazed(store.getState())
      if (initialized) {
        unsubscribe()
        return resolve()
      }
    })
  })
}
