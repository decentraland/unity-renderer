import { getRealmAdapter } from './selectors'
import { storeCondition } from 'lib/redux/storeCondition'

export async function realmInitialized(): Promise<void> {
  await storeCondition(getRealmAdapter)
}
