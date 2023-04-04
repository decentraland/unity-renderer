/** All members declared here are used for Loading screen reporting only, and thus are deprecated, and will be removed */
import { action } from 'typesafe-actions'

export const RENDERING_ACTIVATED = '[RENDERER] Camera activated'
export const renderingActivated = () => action(RENDERING_ACTIVATED)

export const RENDERING_DEACTIVATED = '[RENDERER] Camera deactivated'
export const renderingDectivated = () => action(RENDERING_DEACTIVATED)

export const RENDERING_FOREGROUND = '[RENDERER] Foreground'
export const renderingInForeground = () => action(RENDERING_FOREGROUND)

export const RENDERING_BACKGROUND = '[RENDERER] Background'
export const renderingInBackground = () => action(RENDERING_BACKGROUND)
