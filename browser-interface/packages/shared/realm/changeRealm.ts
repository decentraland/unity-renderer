import { commsLogger } from 'shared/comms/logger'
import { getDisabledCatalystConfig } from 'shared/meta/selectors'
import { getCurrentIdentity } from 'shared/session/selectors'
import { store } from 'shared/store/isolatedStore'
import { setRealmAdapter } from './actions'
import { adapterForRealmConfig } from './resolver'
import { resolveRealmConfigFromString } from './resolvers/resolveRealmConfigFromString'
import { getRealmAdapter } from './selectors'

export async function changeRealm(realmString: string, forceChange: boolean = false): Promise<void> {
  const realmConfig = await resolveRealmConfigFromString(realmString)

  if (!realmConfig) {
    throw new Error(`The realm ${realmString} isn't available right now.`)
  }

  const catalystURL = new URL(realmConfig.baseUrl)

  if (!forceChange) {
    const denylistedCatalysts: string[] = getDisabledCatalystConfig(store.getState()) ?? []
    if (denylistedCatalysts.find((denied) => new URL(denied).host === catalystURL.host)) {
      throw new Error(`The realm is denylisted.`)
    }
  }

  const currentRealmAdapter = getRealmAdapter(store.getState())
  const identity = getCurrentIdentity(store.getState())

  // if not forceChange, then cancel operation if we are inside the desired realm
  if (!forceChange && currentRealmAdapter && currentRealmAdapter.baseUrl === realmConfig.baseUrl) {
    return
  }

  if (!identity) throw new Error('Cant change realm without a valid identity')

  commsLogger.info('Connecting to realm', realmString)

  const newAdapter = await adapterForRealmConfig(realmConfig.baseUrl, realmConfig.about, identity)

  if (newAdapter) {
    store.dispatch(setRealmAdapter(newAdapter))
  } else {
    throw new Error(`Can't connect to realm ${realmString} right now.`)
  }

  return
}
