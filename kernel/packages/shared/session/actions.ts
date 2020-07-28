import { action } from 'typesafe-actions'

import { ETHEREUM_NETWORK } from 'config'

import { ExplorerIdentity } from './types'

export const LOGIN = '[Request] Login'
export const login = () => action(LOGIN)
export type Login = ReturnType<typeof login>

export const USER_AUTHENTIFIED = '[Success] User authentified'
export const userAuthentified = (userId: string, identity: ExplorerIdentity, network: ETHEREUM_NETWORK) =>
  action(USER_AUTHENTIFIED, { userId, identity, network })
export type UserAuthentified = ReturnType<typeof userAuthentified>

export const LOGIN_COMPLETED = '[Success] Login'
export const loginCompleted = () => action(LOGIN_COMPLETED)
export type LoginCompleted = ReturnType<typeof loginCompleted>

export const LOGOUT = '[Request] Logout'
export const logout = () => action(LOGOUT)
export type Logout = ReturnType<typeof logout>
