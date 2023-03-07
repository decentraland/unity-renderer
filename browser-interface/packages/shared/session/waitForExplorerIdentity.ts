import { waitFor } from 'lib/redux/waitFor'
import { getCurrentIdentity } from './selectors'
import { USER_AUTHENTICATED } from './actions'

export const waitForExplorerIdentity = waitFor(getCurrentIdentity, USER_AUTHENTICATED)
