import { action } from 'typesafe-actions'

export const loadingTips = [
  {
    text: `MANA is Decentraland’s virtual currency. Use it to buy LAND and other premium items, vote on key policies and pay platform fees.`,
    image: 'images/decentraland-connect/loadingtips/Mana.png'
  },
  {
    text: `Buy and sell LAND, Estates, Avatar wearables and names in the Decentraland Marketplace: stocking the very best digital goods and paraphernalia backed by the ethereum blockchain.`,
    image: 'images/decentraland-connect/loadingtips/Marketplace.png'
  },
  {
    text: `Create scenes, artworks, challenges and more, using the simple Builder: an easy drag and drop tool. For more experienced creators, the SDK provides the tools to fill the world with social games and applications.`,
    image: 'images/decentraland-connect/loadingtips/Land.png'
  },
  {
    text: `Decentraland is made up of over 90,000 LANDs: virtual spaces backed by cryptographic tokens. Only LAND owners can determine the content that sits on their LAND.`,
    image: 'images/decentraland-connect/loadingtips/LandImg.png'
  },
  {
    text: `Except for the default set of wearables you get when you start out, each wearable model has a limited supply. The rarest ones can get to be super valuable. You can buy and sell them in the Marketplace.`,
    image: 'images/decentraland-connect/loadingtips/WearablesImg.png'
  },
  {
    text: `Decentraland is the first fully decentralized virtual world. By voting through the DAO  ('Decentralized Autonomous Organization'), you are in control of the policies created to determine how the world behaves.`,
    image: 'images/decentraland-connect/loadingtips/DAOImg.png'
  },
  {
    text: `Genesis Plaza is built and maintained by the Decentraland Foundation but is still in many ways a community project. Around here you'll find several teleports that can take you directly to special scenes marked as points of interest.`,
    image: 'images/decentraland-connect/loadingtips/GenesisPlazaImg.png'
  }
]

export const ROTATE_HELP_TEXT = 'Set Help Text'
export const rotateHelpText = () => action(ROTATE_HELP_TEXT)

export const NOT_STARTED = 'Getting things ready...'
export const notStarted = () => action(NOT_STARTED)
export const LOADING_STARTED = 'Authenticating user...'
export const loadingStarted = () => action(LOADING_STARTED)
export const AWAITING_USER_SIGNATURE = 'Awaiting your signature...'
export const awaitingUserSignature = () => action(AWAITING_USER_SIGNATURE)
export const AUTH_SUCCESSFUL = 'Authentication successful. Loading the experience...'
export const authSuccessful = () => action(AUTH_SUCCESSFUL)
export const NOT_INVITED = 'Auth error: not invited'
export const notInvited = () => action(NOT_INVITED)
export const UNITY_CLIENT_LOADED = 'Rendering engine finished loading! Setting up scene system...'
export const unityClientLoaded = () => action(UNITY_CLIENT_LOADED)
export const LOADING_SCENES = 'Loading scenes...'
export const loadingScenes = () => action(LOADING_SCENES)
export const WAITING_FOR_RENDERER = 'Uploading world information to the rendering engine...'
export const waitingForRenderer = () => action(WAITING_FOR_RENDERER)

export const ESTABLISHING_COMMS = 'Establishing communication channels...'
export const establishingComms = () => action(ESTABLISHING_COMMS)
export const COMMS_ESTABLISHED = 'Communications established. Loading profile and item catalogs...'
export const commsEstablished = () => action(COMMS_ESTABLISHED)

// ** TODO - trailing whitespace to workaround id -> label issue - moliva - 15/07/2020
export const EXPERIENCE_STARTED = 'Loading scenes... '
export const experienceStarted = () => action(EXPERIENCE_STARTED)

// ** TODO - trailing whitespaces to workaround id -> label issue - moliva - 15/07/2020
export const TELEPORT_TRIGGERED = 'Loading scenes...  '
export const teleportTriggered = (payload: string) => action(TELEPORT_TRIGGERED, payload)

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
export const commsErrorRetrying = (attempt: number) => action(COMMS_ERROR_RETRYING, attempt)
export const COMMS_COULD_NOT_BE_ESTABLISHED = 'Communications channel error'
export const commsCouldNotBeEstablished = () => action(COMMS_COULD_NOT_BE_ESTABLISHED)
export const CATALYST_COULD_NOT_LOAD = 'Catalysts Contract could not be queried'
export const catalystCouldNotLoad = () => action(CATALYST_COULD_NOT_LOAD)
export const MOBILE_NOT_SUPPORTED = 'Mobile is not supported'
export const mobileNotSupported = () => action(MOBILE_NOT_SUPPORTED)
export const NEW_LOGIN = 'New login'
export const newLogin = () => action(NEW_LOGIN)
export const NETWORK_MISMATCH = 'Network mismatch'
export const networkMismatch = () => action(NETWORK_MISMATCH)
export const FATAL_ERROR = 'fatal error'
export const fatalError = (type: string) => action(FATAL_ERROR, { type })
export const SET_ERROR_TLD = 'TLD network error'
export const setTLDError = (values: any) => action(SET_ERROR_TLD, values)

export const SET_LOADING_SCREEN = 'setting loading screen'
export const setLoadingScreen = (show: boolean) => action(SET_LOADING_SCREEN, { show })

export const SET_LOADING_WAIT_TUTORIAL = '[LOADING] waiting tutorial'
export const setLoadingWaitTutorial = (waiting: boolean) => action(SET_LOADING_WAIT_TUTORIAL, { waiting })

export const ExecutionLifecycleNotifications = {
  notStarted,
  loadingStarted,
  loadingScenes,
  notInvited,
  noWebglCouldBeCreated,
  unityClientLoaded,
  authSuccessful,
  establishingComms,
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
  commsCouldNotBeEstablished,
  catalystCouldNotLoad,
  newLogin,
  networkMismatch
}

export type ExecutionLifecycleEvent =
  | typeof NOT_STARTED
  | typeof LOADING_STARTED
  | typeof AUTH_SUCCESSFUL
  | typeof NOT_INVITED
  | typeof ESTABLISHING_COMMS
  | typeof COMMS_ESTABLISHED
  | typeof NO_WEBGL_COULD_BE_CREATED
  | typeof UNITY_CLIENT_LOADED
  | typeof LOADING_SCENES
  | typeof WAITING_FOR_RENDERER
  | typeof EXPERIENCE_STARTED
  | typeof TELEPORT_TRIGGERED
  | typeof SCENE_ENTERED
  | typeof UNEXPECTED_ERROR
  | typeof UNEXPECTED_ERROR_LOADING_CATALOG
  | typeof AUTH_ERROR_LOGGED_OUT
  | typeof MOBILE_NOT_SUPPORTED
  | typeof CONTENT_SERVER_DOWN
  | typeof FAILED_FETCHING_UNITY
  | typeof COMMS_ERROR_RETRYING
  | typeof COMMS_COULD_NOT_BE_ESTABLISHED
  | typeof CATALYST_COULD_NOT_LOAD
  | typeof NEW_LOGIN
  | typeof NETWORK_MISMATCH
  | typeof AWAITING_USER_SIGNATURE

export const ExecutionLifecycleEventsList: ExecutionLifecycleEvent[] = [
  NOT_STARTED,
  LOADING_STARTED,
  AWAITING_USER_SIGNATURE,
  AUTH_SUCCESSFUL,
  UNITY_CLIENT_LOADED,
  NOT_INVITED,
  ESTABLISHING_COMMS,
  COMMS_ESTABLISHED,
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
  COMMS_COULD_NOT_BE_ESTABLISHED,
  CATALYST_COULD_NOT_LOAD,
  NEW_LOGIN,
  NETWORK_MISMATCH
]

export const SUBSYSTEMS_EVENTS = [
  NOT_STARTED,
  LOADING_STARTED,
  AUTH_SUCCESSFUL,
  UNITY_CLIENT_LOADED,
  WAITING_FOR_RENDERER
]
