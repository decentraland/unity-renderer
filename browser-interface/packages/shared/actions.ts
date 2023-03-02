import { action } from 'typesafe-actions'

export const BEFORE_UNLOAD = 'BEFORE_UNLOAD'
export const beforeUnloadAction = () => action(BEFORE_UNLOAD)
