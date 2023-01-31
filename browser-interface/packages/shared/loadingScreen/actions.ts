import { action } from 'typesafe-actions'

export const UPDATE_LOADING_SCREEN = '[RENDERER] Refresh loading screen visible'

/** @deprecated #3642 */
export const updateLoadingScreen = () => action(UPDATE_LOADING_SCREEN)
