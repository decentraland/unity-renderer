import { waitFor } from 'lib/redux/waitFor'
import { SET_REALM_ADAPTER } from './actions'
import { getRealmAdapter } from './selectors'

export const waitForRealm = waitFor(getRealmAdapter, SET_REALM_ADAPTER)
