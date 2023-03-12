import { LoginState } from 'kernel-web-interface'
import { isSignupInProgress } from 'shared/session/selectors'
import { RootState } from 'shared/store/rootTypes'

/**
 * @returns true if the renderer is currently visible
 */
export function isRendererVisible(state: RootState) {
  // of course, if the renderer is not initialized, it is not visible
  if (!state.renderer.initialized) {
    return false
  }

  // some login stages requires the renderer to be turned off
  const { loginState } = state.session
  if (loginState === LoginState.WAITING_PROFILE && isSignupInProgress(state)) {
    return true
  }

  if (
    loginState === LoginState.SIGNATURE_FAILED ||
    loginState === LoginState.SIGNATURE_PENDING ||
    loginState === LoginState.WAITING_PROVIDER
  ) {
    return false
  }

  return true
}
