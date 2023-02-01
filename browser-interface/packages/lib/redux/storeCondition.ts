import { store } from 'shared/store/isolatedStore'
import { RootState } from 'shared/store/rootTypes'

export async function storeCondition(selector: (store: RootState) => boolean): Promise<void> {
  if (selector(store.getState())) {
    return
  }
  return new Promise<void>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      if (selector(store.getState())) {
        unsubscribe()
        return resolve()
      }
    })
  })
}
