/* eslint-disable @typescript-eslint/ban-types */
import type { Vector3 } from 'lib/math/Vector3'
import type { Vector2 } from 'lib/math/Vector2'
import { Entity } from '@dcl/schemas'
import { MinimapSceneInfo } from '@dcl/legacy-ecs/dist/decentraland/Types'
export type { WearableId, Wearable, WearableV2 } from './catalogs/types'

export interface MessageDict {
  [key: string]: string
}

export type MessageEntry = {
  id: string
  isCommand: boolean
  sender: string | undefined
  recipient?: string | undefined
  message: string
  timestamp: number
}

export interface IChatCommand {
  name: string
  description: string
  run: (message: string) => MessageEntry
}

export type RPCSendableMessage = {
  jsonrpc: '2.0'
  id: number
  method: string
  params: any[]
}

export type EntityActionType =
  | 'CreateEntity'
  | 'RemoveEntity'
  | 'SetEntityParent'
  | 'UpdateEntityComponent'
  | 'AttachEntityComponent'
  | 'ComponentCreated'
  | 'ComponentDisposed'
  | 'ComponentRemoved'
  | 'ComponentUpdated'
  | 'Query'
  | 'InitMessagesFinished'
  | 'OpenExternalUrl'
  | 'OpenNFTDialog'

export type QueryPayload = { queryId: string; payload: RayQuery }

export type CreateEntityPayload = { id: string }

export type RemoveEntityPayload = { id: string }
export type SceneStartedPayload = {}

export type SetEntityParentPayload = {
  entityId: string
  parentId: string
}

export type ComponentRemovedPayload = {
  entityId: string
  name: string
}

export type UpdateEntityComponentPayload = {
  entityId: string
  classId: number
  name: string
  json: string
}

export type ComponentCreatedPayload = {
  id: string
  classId: number
  name: string
}

export type AttachEntityComponentPayload = {
  entityId: string
  name: string
  id: string
}

export type ComponentDisposedPayload = {
  id: string
}

export type ComponentUpdatedPayload = {
  id: string
  json: string
}

export type EntityAction = {
  type: EntityActionType
  tag?: string
  payload: any
}

export type OpenNFTDialogPayload = { assetContractAddress: string; tokenId: string; comment: string | null }

export const BillboardModes = {
  BILLBOARDMODE_NONE: 0,
  BILLBOARDMODE_X: 1,
  BILLBOARDMODE_Y: 2,
  BILLBOARDMODE_Z: 4,
  BILLBOARDMODE_ALL: 7
}

export const TextureSamplingMode = {
  NEAREST: 1,
  BILINEAR: 2,
  TRILINEAR: 3
}

export const TextureWrapping = {
  CLAMP: 0,
  WRAP: 1,
  MIRROR: 2
}

export type BillboardModes = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7

export const TransparencyModes = {
  OPAQUE: 0,
  ALPHATEST: 1,
  ALPHABLEND: 2,
  ALPHATESTANDBLEND: 3
}

export type TransparencyModes = 0 | 1 | 2 | 3

export type SceneCommunications = {
  commServerUrl: string | null
}

export type SceneDisplay = {
  title?: string
  favicon?: string
  description?: string
  navmapThumbnail?: string
}

export type SceneParcels = {
  base: string // base parcel
  parcels: string[]
}

export type SceneContact = {
  name?: string
  email?: string
  im?: string
  url?: string
}

export type ScenePolicy = {
  contentRating?: string
  fly?: boolean
  voiceEnabled?: boolean
  blacklist?: string[]
  teleportPosition?: string
}

export type SceneSource = {
  origin?: string
  projectId?: string
  version?: number
  rotation?: SceneSourcePlacement['rotation']
  point?: SceneSourcePlacement['point']
  layout?: SceneSourcePlacement['layout']
  isEmpty?: boolean
}

export type SceneSourcePlacement = {
  rotation: 'north' | 'east' | 'south' | 'west'
  point: { x: number; y: number }
  layout: { cols: number; rows: number }
}

export type SceneFeatureToggle = {
  name: string
  default: 'enabled' | 'disabled'
}

export const VOICE_CHAT_FEATURE_TOGGLE: SceneFeatureToggle = { name: 'voiceChat', default: 'enabled' }

export type LoadableScene = {
  readonly entity: Readonly<Omit<Entity, 'id'>>
  readonly baseUrl: string
  readonly id: string
  /** Id of the parent scene that spawned this scene experience */
  readonly parentCid?: string
  readonly isGlobalScene?: boolean
  readonly isPortableExperience?: boolean
  readonly useFPSThrottling?: boolean
}

export type SceneSpawnPoint = {
  name?: string
  position: {
    x: number | number[]
    y: number | number[]
    z: number | number[]
  }
  default?: boolean
  cameraTarget?: Vector3
}

export type InstancedSpawnPoint = { position: Vector3; cameraTarget?: Vector3 }

export type SoundComponent = {
  /** Distance fading model, default: 'linear' */
  distanceModel?: 'linear' | 'inverse' | 'exponential'
  /** Does the sound loop? default: false */
  loop?: boolean
  /** The src of the sound to be played */
  src: string
  /** Volume of the sound, values 0 to 1, default: 1 */
  volume?: number
  /** Used in inverse and exponential distance models, default: 1 */
  rolloffFactor?: number
  /** Is the sound playing?, default: true */
  playing?: boolean
}

export type TransitionValue = {
  duration: number
  timing?: TimingFunction
  delay?: number
}

export type TimingFunction =
  | 'linear'
  | 'ease-in'
  | 'ease-out'
  | 'ease-in-out'
  | 'quadratic-in'
  | 'quadratic-out'
  | 'quadratic-inout'
  | 'cubic-in'
  | 'cubic-out'
  | 'cubic-inout'
  | 'quartic-in'
  | 'quartic-out'
  | 'quartic-inout'
  | 'quintic-in'
  | 'quintic-out'
  | 'quintic-inout'
  | 'sin-in'
  | 'sin-out'
  | 'sin-inout'
  | 'exponential-in'
  | 'exponential-out'
  | 'exponential-inout'
  | 'bounce-in'
  | 'bounce-out'
  | 'bounce-inout'
  | 'elastic-in'
  | 'elastic-out'
  | 'elastic-inout'
  | 'circular-in'
  | 'circular-out'
  | 'circular-inout'
  | 'back-in'
  | 'back-out'
  | 'back-inout'

export type TransitionComponent = {
  position?: TransitionValue
  rotation?: TransitionValue
  scale?: TransitionValue
  color?: TransitionValue
  lookAt?: TransitionValue
}

export type SkeletalAnimationValue = {
  /**
   * Name of the clip (ID)
   */
  name: string

  /**
   * Name of the animation in the model
   */
  clip: string

  /**
   * Does the animation loop?, default: true
   */
  looping?: boolean

  /**
   * Weight of the animation, values from 0 to 1, used to blend several animations. default: 1
   */
  weight?: number

  /**
   * The animation speed
   */
  speed?: number

  /**
   * Is the animation playing? default: true
   */
  playing?: boolean

  /**
   * Does any anyone asked to reset the animation? default: false
   */
  shouldReset?: boolean
}

export type SkeletalAnimationComponent = {
  states: SkeletalAnimationValue[]
}

export type Ray = {
  origin: Vector3
  direction: Vector3
  distance: number
}

export type QueryType = 'HitFirst' | 'HitAll' | 'HitFirstAvatar' | 'HitAllAvatars'

export type RayQuery = {
  queryId: string
  queryType: QueryType
  ray: Ray
}

export enum NotificationType {
  GENERIC = 0,
  SCRIPTING_ERROR = 1,
  COMMS_ERROR = 2
}

export type Notification = {
  type: NotificationType
  message: string
  buttonMessage: string
  timer: number // in seconds
  scene?: string
  externalCallbackID?: string
}

export enum RenderProfile {
  DEFAULT = 0,
  HALLOWEEN = 1,
  XMAS = 2,
  NIGHT = 3
}

export enum HUDElementID {
  NONE = 0,
  MINIMAP = 1,
  PROFILE_HUD = 2,
  NOTIFICATION = 3,
  AVATAR_EDITOR = 4,
  SETTINGS_PANEL = 5,

  /** @deprecaed */
  EXPRESSIONS = 6,

  PLAYER_INFO_CARD = 7,

  /** @deprecaed */
  AIRDROPPING = 8,

  TERMS_OF_SERVICE = 9,
  WORLD_CHAT_WINDOW = 10,
  TASKBAR = 11,
  MESSAGE_OF_THE_DAY = 12,
  FRIENDS = 13,
  OPEN_EXTERNAL_URL_PROMPT = 14,
  NFT_INFO_DIALOG = 16,
  TELEPORT_DIALOG = 17,
  CONTROLS_HUD = 18,

  /** @deprecated */
  EXPLORE_HUD = 19,

  HELP_AND_SUPPORT_HUD = 20,

  /** @deprecated */
  EMAIL_PROMPT = 21,

  USERS_AROUND_LIST_HUD = 22,
  GRAPHIC_CARD_WARNING = 23,
  BUILD_MODE = 24,
  QUESTS_PANEL = 26,
  QUESTS_TRACKER = 27,
  BUILDER_PROJECTS_PANEL = 28,
  SIGNUP = 29,
  LOADING_HUD = 30,
  AVATAR_NAMES = 31,
  EMOTES = 32
}

export type HUDConfiguration = {
  active: boolean
  visible: boolean
}

export type WelcomeHUDControllerModel = HUDConfiguration & {
  hasWallet: boolean
}

export enum ChatMessageType {
  NONE,
  PUBLIC,
  PRIVATE,
  SYSTEM
}

export type WorldPosition = {
  realm: {
    serverName: string
    layer: string
  }
  gridPosition: {
    x: number
    y: number
  }
}

export type SetAudioDevicesPayload = {
  outputDevices: {
    label: string
    deviceId: string
  }[]
  inputDevices: {
    label: string
    deviceId: string
  }[]
}

export type MiniMapSceneInfoPayload = {
  isWorld: boolean
  scenesInfo: MinimapSceneInfo[]
}

export enum ChatMessagePlayerType {
  WALLET = 0,
  GUEST = 1
}

export type ChatMessage = {
  messageId: string
  messageType: ChatMessageType
  sender?: string | undefined
  senderName?: string | undefined
  recipient?: string | undefined
  timestamp: number
  body: string
  receiverId?: string | undefined
  channelName?: string | undefined
}

export type AddChatMessagesPayload = {
  messages: ChatMessage[]
}

export interface UnseenPrivateMessage {
  count: number
}

export interface FriendsInitializeChatPayload {
  totalUnseenMessages: number
  channelToJoin?: string
}

export type FriendsInitializationMessage = {
  totalReceivedRequests: number
}

export interface GetFriendsPayload {
  userNameOrId?: string // text to match
  limit: number // max amount of entries to request
  skip: number // amount of entries to skip
}

// @TODO! @deprecated
export interface GetFriendRequestsPayload {
  sentLimit: number // max amount of entries of sent friend requests to request
  sentSkip: number // the amount of entries of sent friend requests to skip
  receivedLimit: number // max amount of entries of received friend requests to request
  receivedSkip: number // the amount of entries of received friend requests to skip
}

export enum FriendshipAction {
  NONE,
  APPROVED,
  REJECTED,
  CANCELED,
  REQUESTED_FROM,
  REQUESTED_TO,
  DELETED
}

export type FriendshipUpdateStatusMessage = {
  userId: string
  action: FriendshipAction
}

export enum PresenceStatus {
  NONE,
  OFFLINE,
  ONLINE,
  UNAVAILABLE
}

type Realm = {
  serverName: string
}

export type UpdateUserStatusMessage = {
  userId: string
  realm: Realm | undefined
  position: Vector2 | undefined
  presence: PresenceStatus
}

export interface AddFriendsPayload {
  friends: string[] // ids of each friend added
  totalFriends: number // total amount of friends
}

// @TODO! - @deprecated
export interface AddFriendRequestsPayload {
  requestedTo: string[] // user ids which you sent a request
  requestedFrom: string[] // user ids which you received a request
  totalReceivedFriendRequests: number // total amount of friend requests received
  totalSentFriendRequests: number // total amount of friend requests sent
}

export interface MarkMessagesAsSeenPayload {
  userId: string
}

export interface GetPrivateMessagesPayload {
  userId: string
  limit: number
  fromMessageId: string
}

export interface UpdateTotalUnseenMessagesPayload {
  total: number
}

export interface UpdateUserUnseenMessagesPayload {
  userId: string
  total: number
}

export interface UpdateTotalUnseenMessagesByUserPayload {
  unseenPrivateMessages: Array<{ userId: string; count: number }> // the unseen private messages for each user
}

export interface UpdateTotalFriendRequestsPayload {
  totalReceivedRequests: number
  totalSentRequests: number
}

export interface UpdateTotalFriendsPayload {
  totalFriends: number
}

export interface GetFriendsWithDirectMessagesPayload {
  userNameOrId: string // text to match
  limit: number // max amount of entries to receive
  skip: number // amount of messages already received
}

export interface AddFriendsWithDirectMessagesPayload {
  currentFriendsWithDirectMessages: {
    userId: string // id of the friend with direct messages
    lastMessageTimestamp: number //: timestamp of the last message
  }[]
  totalFriendsWithDirectMessages: number // total amount of friends with direct messages
}

export type BuilderConfiguration = {
  camera: {
    zoomMin: number
    zoomMax: number
    zoomDefault: number
  }
  environment: {
    disableFloor: boolean
  }
}

export type KernelConfigForRenderer = {
  comms: {
    commRadius: number
    voiceChatEnabled: boolean
  }
  profiles: {
    nameValidRegex: string
    nameValidCharacterRegex: string
  }
  debugConfig?: Partial<{
    sceneDebugPanelEnabled?: boolean
    sceneDebugPanelTargetSceneId?: string
    sceneLimitsWarningSceneId?: string
  }>
  gifSupported: boolean
  network: string
  validWorldRanges: Object
  kernelVersion: string
  rendererVersion: string
  avatarTextureAPIBaseUrl: string
  urlParamsForWearablesDebug: boolean // temporal field until the whole the wearables catalog sagas flow is migrated to Unity
}

export type RealmsInfoForRenderer = {
  current: CurrentRealmInfoForRenderer
  realms: {
    layer: string
    serverName: string
    url: string
    usersCount: number
    usersMax: number
    userParcels: [number, number][]
  }[]
}

export type CurrentRealmInfoForRenderer = {
  layer: string
  serverName: string
  domain: string
  contentServerUrl: string
}

export type TutorialInitializationMessage = {
  fromDeepLink: boolean
  enableNewTutorialCamera: boolean
}

export type HeaderRequest = {
  endpoint: string
  headers: Record<string, string>
}

export enum AvatarRendererMessageType {
  SCENE_CHANGED = 'SceneChanged',
  REMOVED = 'Removed'
}

export type AvatarRendererBasePayload = {
  avatarShapeId: string
}

export type AvatarRendererPositionMessage = {
  type: AvatarRendererMessageType.SCENE_CHANGED
  sceneId?: string
  sceneNumber?: number
} & AvatarRendererBasePayload

export type AvatarRendererRemovedMessage = {
  type: AvatarRendererMessageType.REMOVED
} & AvatarRendererBasePayload

export type AvatarRendererMessage = AvatarRendererRemovedMessage | AvatarRendererPositionMessage

export enum ChannelErrorCode {
  UNKNOWN = 0, // Any uncategorized channel related error
  LIMIT_EXCEEDED = 1, // Reached the max amount of joined channels allowed per user
  WRONG_FORMAT = 2, // Does not meet the name rules
  RESERVED_NAME = 3, // Such as nearby
  ALREADY_EXISTS = 4 // The name has already been used
}

export type JoinOrCreateChannelPayload = CreateChannelPayload

export type CreateChannelPayload = {
  channelId: string
}

export type ChannelErrorPayload = {
  channelId: string
  errorCode: number
}

export type ChannelInfoPayload = {
  name: string // the name of the channel
  channelId: string // the conversation id
  unseenMessages: number
  lastMessageTimestamp: number | undefined
  memberCount: number
  description: string
  joined: boolean
  muted: boolean
}

export type ChannelInfoPayloads = {
  channelInfoPayload: ChannelInfoPayload[]
}

export type ChannelSearchResultsPayload = {
  since: string | null // nullable pagination token
  channels: ChannelInfoPayload[]
}

export type MarkChannelMessagesAsSeenPayload = {
  channelId: string
}

export type UpdateTotalUnseenMessagesByChannelPayload = {
  unseenChannelMessages: {
    channelId: string
    count: number
  }[] // the unseen messages for each channel
}

export type GetChannelMessagesPayload = {
  channelId: string
  limit: number // max amount of entries to request
  from: string // pivot id to skip entries
}

export type GetChannelsPayload = {
  limit: number // max amount of entries to request
  name: string // text to match
  since?: string // a pagination token
}

export type GetJoinedChannelsPayload = {
  limit: number // max amount of entries to request
  skip: number // amount of entries to skip
}

export type LeaveChannelPayload = {
  channelId: string
}

export type MuteChannelPayload = {
  channelId: string
  muted: boolean
}

export type GetChannelInfoPayload = {
  channelIds: string[]
}

export type GetChannelMembersPayload = {
  channelId: string
  limit: number
  skip: number
  userName: string // text to match
}

export type ChannelMember = {
  userId: string
  name: string
  isOnline?: boolean
}

export type UpdateChannelMembersPayload = {
  channelId: string
  members: ChannelMember[]
}

// Users allowed to create channels
export type UsersAllowed = {
  mode: number
  allowList: string[]
}

export type AntiSpamConfig = {
  maxNumberRequest: number
  cooldownTimeMs: number
}
