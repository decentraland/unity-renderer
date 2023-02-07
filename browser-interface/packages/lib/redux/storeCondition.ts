import { store } from 'shared/store/isolatedStore'

export async function storeCondition<T>(selector: (state: any) => T | false): Promise<T> {
  const value: T | false = selector(store.getState())
  if (!!value) {
    return value
  }
  return new Promise<T>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const value: T | false = selector(store.getState())
      if (!!value) {
        unsubscribe()
        return resolve(value)
      }
    })
  })
}
