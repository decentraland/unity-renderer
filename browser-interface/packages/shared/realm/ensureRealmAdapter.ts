import { storeCondition } from 'lib/redux'
import type { IRealmAdapter } from './types'
import { getRealmAdapter } from './selectors'

export async function ensureRealmAdapter(): Promise<IRealmAdapter> {
  return (await storeCondition(getRealmAdapter))!
}
