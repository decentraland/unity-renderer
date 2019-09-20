import { action } from 'typesafe-actions'

export const helpTexts = [
  `With blockchain-based virtual items, users are in control: buy, sell, or gift them without asking for permission.`,
  `MANA is Decentraland's virtual currency. Use it to buy LAND and other premium items, vote on Decentraland's key policy, and pay platform fees.`,
  `Decentraland is made up of over 90,000 LANDs, virtual spaces backed by cryptographic tokens. Only landowners determine the content that sits on their LAND.`
]

export const NOT_STARTED = 'Not initialized'
export const LOADING_STARTED = 'Loading started'
export const loadingStarted = () => action(LOADING_STARTED)
export const AUTH_SUCCESSFUL = 'Authentication successful'
export const authSuccessful = () => action(AUTH_SUCCESSFUL)
export const NOT_INVITED = 'Auth error: not invited'
export const notInvited = () => action(NOT_INVITED)
export const UNITY_CLIENT_LOADED = 'Unity client finished loading'
export const unityClientLoaded = () => action(UNITY_CLIENT_LOADED)
export const LOADING_SCENES = 'Loading scenes'
export const loadingScenes = () => action(LOADING_SCENES)
export const WAITING_FOR_RENDERER = 'Waiting for renderer'
export const waitingForRenderer = () => action(WAITING_FOR_RENDERER)

export const EXPERIENCE_STARTED = 'Loading finished: Experience started'
export const experienceStarted = () => action(EXPERIENCE_STARTED)

export const TELEPORT_TRIGGERED = 'Teleporting...'
export const teleportTriggered = () => action(TELEPORT_TRIGGERED)
export const SCENE_ENTERED = 'Entered into a new scene'
export const sceneEntered = () => action(SCENE_ENTERED)
export const UNEXPECTED_ERROR = 'Unexpected fatal error'
export const unexpectedError = (error: any) => action(UNEXPECTED_ERROR, { error })
export const UNEXPECTED_ERROR_LOADING_CATALOG = 'Unexpected fatal error when loading items'
export const unexpectedErrorLoadingCatalog = (error: any) => action(UNEXPECTED_ERROR_LOADING_CATALOG, { error })

export const NO_WEBGL_COULD_BE_CREATED = 'Capabilities: Could not create WebGL context'
export const noWebglCouldBeCreated = () => action(NO_WEBGL_COULD_BE_CREATED)
export const AUTH_ERROR_LOGGED_OUT = 'Auth: Logged out'
export const authErrorLoggedOut = () => action(AUTH_ERROR_LOGGED_OUT)
export const CONTENT_SERVER_DOWN = 'Content: Server is down'
export const contentServerDown = () => action(CONTENT_SERVER_DOWN)
export const FAILED_FETCHING_UNITY = 'Failed to fetch the rendering engine'
export const failedFetchingUnity = () => action(FAILED_FETCHING_UNITY)
export const COMMS_ERROR_RETRYING = 'Communications channel error (will retry)'
export const commsErrorRetrying = () => action(COMMS_ERROR_RETRYING)
export const COMMS_COULD_NOT_BE_ESTABLISHED = 'Communications channel error'
export const commsCouldNotBeEstablished = () => action(COMMS_COULD_NOT_BE_ESTABLISHED)
export const MOBILE_NOT_SUPPORTED = 'Mobile is not supported'
export const mobileNotSupported = () => action(MOBILE_NOT_SUPPORTED)

export const ExecutionLifecycleNotifications = {
  loadingStarted,
  loadingScenes,
  notInvited,
  noWebglCouldBeCreated,
  unityClientLoaded,
  authSuccessful,
  waitingForRenderer,
  experienceStarted,
  teleportTriggered,
  sceneEntered,
  unexpectedError,
  unexpectedErrorLoadingCatalog,
  mobileNotSupported,
  authErrorLoggedOut,
  contentServerDown,
  failedFetchingUnity,
  commsErrorRetrying,
  commsCouldNotBeEstablished
}

export type ExecutionLifecycleEvent =
  | typeof NOT_STARTED
  | typeof LOADING_STARTED
  | typeof AUTH_SUCCESSFUL
  | typeof NOT_INVITED
  | typeof NO_WEBGL_COULD_BE_CREATED
  | typeof UNITY_CLIENT_LOADED
  | typeof LOADING_SCENES
  | typeof WAITING_FOR_RENDERER
  | typeof EXPERIENCE_STARTED
  | typeof TELEPORT_TRIGGERED
  | typeof SCENE_ENTERED
  | typeof UNEXPECTED_ERROR
  | typeof AUTH_ERROR_LOGGED_OUT
  | typeof MOBILE_NOT_SUPPORTED
  | typeof CONTENT_SERVER_DOWN
  | typeof FAILED_FETCHING_UNITY
  | typeof COMMS_ERROR_RETRYING
  | typeof COMMS_COULD_NOT_BE_ESTABLISHED

export const ExecutionLifecycleEventsList = [
  NOT_STARTED,
  LOADING_STARTED,
  AUTH_SUCCESSFUL,
  UNITY_CLIENT_LOADED,
  NOT_INVITED,
  NO_WEBGL_COULD_BE_CREATED,
  LOADING_SCENES,
  WAITING_FOR_RENDERER,
  EXPERIENCE_STARTED,
  TELEPORT_TRIGGERED,
  SCENE_ENTERED,
  UNEXPECTED_ERROR,
  UNEXPECTED_ERROR_LOADING_CATALOG,
  AUTH_ERROR_LOGGED_OUT,
  CONTENT_SERVER_DOWN,
  FAILED_FETCHING_UNITY,
  COMMS_ERROR_RETRYING,
  MOBILE_NOT_SUPPORTED,
  COMMS_COULD_NOT_BE_ESTABLISHED
]
