import future from 'fp-future'
import { createLogger } from 'lib/logger'

const logger = createLogger('cache: ')
const DEBUG = false

const OBJECT_STORE_KEY = 'object-store'

export const init = future<IDBDatabase>()

export const cache = init.then((db) => ({
  read: (id: string): Promise<any> => {
    const result = future<any>()

    const transaction = db.transaction(OBJECT_STORE_KEY, 'readonly')

    transaction.onerror = (event: any) => result.reject(new Error(event.target.errorCode))

    const store = transaction.objectStore(OBJECT_STORE_KEY)
    const request = store.get(id)

    request.onerror = (event: any) => result.reject(new Error(event.target.errorCode))
    request.onsuccess = (_event: any) => result.resolve(request.result)

    return result
  },
  write: (f: (store: IDBObjectStore) => void) => {
    const result = future<void>()

    const transaction = db.transaction(OBJECT_STORE_KEY, 'readwrite')

    transaction.onerror = (event: any) => result.reject(new Error(event.target.errorCode))
    transaction.oncomplete = (_event: any) => result.resolve()

    const store = transaction.objectStore(OBJECT_STORE_KEY)

    f(store)

    return result
  }
}))

const request = indexedDB.open('cache', 1)

request.onerror = (event) => {
  DEBUG && logger.error(`error while opening cache db`, event)
  init.reject(new Error(`error while opening cache db`))
}

request.onupgradeneeded = (event: any) => {
  const db: IDBDatabase = event.target.result

  db.createObjectStore(OBJECT_STORE_KEY)
}

request.onsuccess = (event: any) => {
  const db: IDBDatabase = event.target.result

  db.onerror = (event: any) => {
    DEBUG && logger.error(`error on cache db`, event.target.errorCode)
  }

  init.resolve(db)
}

export async function retrieve(cachedKey: string) {
  try {
    const db = await cache
    return db.read(cachedKey)
  } catch (e) {
    logger.info(`error while retrieving cache data`, e)
    return undefined
  }
}

export async function store(cachedKey: string, data: any) {
  try {
    const db = await cache
    await db.write((store) => {
      store.put(data, cachedKey)
    })
  } catch (e) {
    logger.info(`error while retrieving cache data`, e)
  }
}
