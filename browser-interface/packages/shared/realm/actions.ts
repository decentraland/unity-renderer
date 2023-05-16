import { IslandChangedMessage } from 'shared/protocol/decentraland/kernel/comms/v3/archipelago.gen'
import { action } from 'typesafe-actions'
import { IRealmAdapter, OnboardingState } from './types'

// this action is triggered by the IBff, it is used to connect a comms adapter
export const CONNECT_TO_COMMS = '[COMMS] ConnectTo'
export const connectToComms = (event: IslandChangedMessage) => action(CONNECT_TO_COMMS, { event })
export type ConnectToCommsAction = ReturnType<typeof connectToComms>

// this action is triggered by the user when changing servers/realm
export const SET_REALM_ADAPTER = '[COMMS] setRealmAdapter'
export const setRealmAdapter = (context: IRealmAdapter | undefined) => action(SET_REALM_ADAPTER, context)
export type SetRealmAdapterAction = ReturnType<typeof setRealmAdapter>

// this action is triggered when the onboarding state changes
export const SET_ONBOARDING_STATE = 'setOnboardingState'
export const setOnboardingState = (onboardingState: Partial<OnboardingState>) =>
  action(SET_ONBOARDING_STATE, onboardingState)

export const HANDLE_REALM_DISCONNECTION = '[COMMS] handleRealmDisconnection'
export const handleRealmDisconnection = (context: IRealmAdapter) => action(HANDLE_REALM_DISCONNECTION, { context })
export type HandleRealmDisconnection = ReturnType<typeof handleRealmDisconnection>
