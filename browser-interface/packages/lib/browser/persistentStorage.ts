import type { PersistentAsyncStorage } from '@dcl/kernel-interface'

declare let window: any

class PersistentLocalStorage implements PersistentAsyncStorage {
  storage: any
  constructor(storage: any) {
    if (storage) {
      this.storage = storage
    } else {
      throw new Error('Cannot create PersistentLocalStorage without localStorage object')
    }
  }

  async clear(): Promise<void> {
    this.storage.clear()
  }

  async getItem(key: string): Promise<string | null> {
    return this.storage.getItem(key)
  }

  async keys(): Promise<string[]> {
    const keys: string[] = []
    for (let i = 0; i < this.storage.length; i++) {
      keys.push(this.storage.key(i) as string)
    }
    return keys
  }

  async removeItem(key: string): Promise<void> {
    this.storage.removeItem(key)
  }

  async setItem(key: string, value: string): Promise<void> {
    this.storage.setItem(key, value)
  }
}

let persistentStorage: PersistentAsyncStorage | null = null
if (window && window.localStorage) {
  persistentStorage = new PersistentLocalStorage(window.localStorage)
}

export function setPersistentStorage(storage: PersistentAsyncStorage) {
  persistentStorage = storage
}

export default persistentStorage

export async function saveToPersistentStorage(key: string, data: any) {
  if (!persistentStorage) {
    throw new Error('Storage not supported')
  }
  return persistentStorage.setItem(key, JSON.stringify(data))
}

export async function getFromPersistentStorage(key: string) {
  if (!persistentStorage) {
    throw new Error('Storage not supported')
  }
  const data = await persistentStorage.getItem(key)
  return (data && JSON.parse(data)) || null
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
