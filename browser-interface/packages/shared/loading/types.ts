import { PARCEL_LOADING_STARTED } from 'shared/renderer/types'
import { INIT_SESSION } from 'shared/session/actions'
import { action } from 'typesafe-actions'

export const NOT_STARTED = 'Getting things ready...'
export const notStarted = () => action(NOT_STARTED)
export const AWAITING_USER_SIGNATURE = 'Awaiting your signature...'
export const awaitingUserSignature = () => action(AWAITING_USER_SIGNATURE)
export const METRICS_AUTH_SUCCESSFUL = 'Authentication successful. Loading the experience...'
export const metricsAuthSuccessful = () => action(METRICS_AUTH_SUCCESSFUL)
export const METRICS_UNITY_CLIENT_LOADED = 'Rendering engine finished loading! Setting up scene system...'
export const metricsUnityClientLoaded = () => action(METRICS_UNITY_CLIENT_LOADED)
export const WAITING_FOR_RENDERER = 'Uploading world information to the rendering engine...'
export const waitingForRenderer = () => action(WAITING_FOR_RENDERER)

export const ESTABLISHING_COMMS = '[COMMS] Establishing communication channels...'
export const establishingComms = () => action(ESTABLISHING_COMMS)
export const COMMS_ESTABLISHED = '[COMMS] Communications established.'
export const commsEstablished = () => action(COMMS_ESTABLISHED)

export const EXPERIENCE_STARTED = 'EXPERIENCE_STARTED'
export const experienceStarted = () => action(EXPERIENCE_STARTED)

export const TELEPORT_TRIGGERED = 'TELEPORT_TRIGGERED'
export const trackTeleportTriggered = (payload: string) => action(TELEPORT_TRIGGERED, payload)

export const SCENE_ENTERED = 'Entered into a new scene'
export const sceneEntered = () => action(SCENE_ENTERED)
export const UNEXPECTED_ERROR = 'Unexpected fatal error'
export const unexpectedError = (error: any) => action(UNEXPECTED_ERROR, { error })

export const COMMS_COULD_NOT_BE_ESTABLISHED = 'Communications channel error'
export const commsCouldNotBeEstablished = () => action(COMMS_COULD_NOT_BE_ESTABLISHED)
export const CATALYST_COULD_NOT_LOAD = 'Catalysts Contract could not be queried'
export const NETWORK_MISMATCH = 'Network mismatch'
export const FATAL_ERROR = 'fatal error'
export const fatalError = (type: string) => action(FATAL_ERROR, { type })
export const AVATAR_LOADING_ERROR = 'The avatar could not be loaded correctly'

export type ExecutionLifecycleEvent =
  | typeof NOT_STARTED
  | typeof INIT_SESSION
  | typeof METRICS_AUTH_SUCCESSFUL
  | typeof ESTABLISHING_COMMS
  | typeof COMMS_ESTABLISHED
  | typeof METRICS_UNITY_CLIENT_LOADED
  | typeof PARCEL_LOADING_STARTED
  | typeof WAITING_FOR_RENDERER
  | typeof EXPERIENCE_STARTED
  | typeof TELEPORT_TRIGGERED
  | typeof SCENE_ENTERED
  | typeof UNEXPECTED_ERROR
  | typeof COMMS_COULD_NOT_BE_ESTABLISHED
  | typeof CATALYST_COULD_NOT_LOAD
  | typeof NETWORK_MISMATCH
  | typeof AWAITING_USER_SIGNATURE
  | typeof AVATAR_LOADING_ERROR

export const ExecutionLifecycleEventsList: ExecutionLifecycleEvent[] = [
  NOT_STARTED,
  INIT_SESSION,
  AWAITING_USER_SIGNATURE,
  METRICS_AUTH_SUCCESSFUL,
  METRICS_UNITY_CLIENT_LOADED,
  ESTABLISHING_COMMS,
  COMMS_ESTABLISHED,
  PARCEL_LOADING_STARTED,
  WAITING_FOR_RENDERER,
  EXPERIENCE_STARTED,
  TELEPORT_TRIGGERED,
  SCENE_ENTERED,
  UNEXPECTED_ERROR,
  COMMS_COULD_NOT_BE_ESTABLISHED,
  CATALYST_COULD_NOT_LOAD,
  NETWORK_MISMATCH,
  AVATAR_LOADING_ERROR
]
