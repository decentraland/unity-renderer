import type { SessionState } from './types'
import { isLoginCompleted } from './selectors'
import { store } from 'shared/store/isolatedStore'
import { storeCondition } from 'lib/redux'

export async function onLoginCompleted(): Promise<SessionState> {
  await storeCondition(isLoginCompleted)
  return store.getState().session
}
