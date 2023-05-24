import { PersistentAsyncStorage } from '@dcl/kernel-interface'
import { globalObservable } from 'shared/observables'

declare let window: WindowLocalStorage

export const asyncLocalStorage: PersistentAsyncStorage = {
  clear: async function () {
    return Promise.resolve(window.localStorage.clear())
  },
  getItem: async function (key: string) {
    return Promise.resolve(window.localStorage.getItem(key))
  },
  keys: async function () {
    const keys: string[] = []
    for (let i = 0; i < window.localStorage.length; i++) {
      keys.push(window.localStorage.key(i) as string)
    }
    return Promise.resolve(keys)
  },
  removeItem: async function (key: string): Promise<void> {
    return Promise.resolve(window.localStorage.removeItem(key))
  },
  setItem(key: string, value: string): Promise<void> {
    return Promise.resolve(window.localStorage.setItem(key, value))
  }
}
let persistentStorage: PersistentAsyncStorage | null = null
if (window && window.localStorage) {
  persistentStorage = asyncLocalStorage
}

export function setPersistentStorage(storage: PersistentAsyncStorage) {
  persistentStorage = storage
}

export function getPersistentStorage() {
  return persistentStorage
}

export async function saveToPersistentStorage(key: string, data: any) {
  if (!persistentStorage) {
    throw new Error('Storage not supported')
  }
  return persistentStorage.setItem(key, JSON.stringify(data))
}

export async function getFromPersistentStorage(key: string): Promise<any | null> {
  if (!persistentStorage) {
    throw new Error('Storage not supported')
  }
  const data = await persistentStorage.getItem(key)
  try {
    return (data && JSON.parse(data)) || null
  } catch (e) {
    globalObservable.emit('error', {
      error: new Error(`Impossible to parse JSON: ${key} from localStorage's value is ${data} (not JSON)`),
      code: 'local',
      level: 'serious'
    })
    return null
  }
}

export async function removeFromPersistentStorage(key: string) {
  if (!persistentStorage) {
    throw new Error('Storage not supported')
  }
  return persistentStorage.removeItem(key)
}

export async function getKeysFromPersistentStorage(): Promise<string[]> {
  if (!persistentStorage) {
    throw new Error('Storage not supported')
  }
  return persistentStorage.keys()
}
