import { parseParcelPosition } from 'atomicHelpers/parcelScenePositions'

export type MappingsResponse = {
  parcel_id: string
  publisher: string
  root_cid: string
  contents: Array<ContentMapping>
}

export type ContentMapping = { file: string; hash: string }

export interface MessageDict {
  [key: string]: string
}

export type UserData = {
  displayName: string
  publicKey: string
}

export type MessageEntry = {
  id: string
  sender: string
  message: string
  isCommand?: boolean
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

export type CreateEntityPayload = { id: string }

export type RemoveEntityPayload = { id: string }

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
  payload: string
}

export type LoadableParcelScene = {
  id: string
  basePosition: { x: number; y: number }
  parcels: Array<{ x: number; y: number }>
  contents: Array<ContentMapping>
  baseUrl: string
  land: ILand
}

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

export type IBillboardModes = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7

export const TransparencyModes = {
  OPAQUE: 0,
  ALPHATEST: 1,
  ALPHABLEND: 2,
  ALPHATESTANDBLEND: 3
}

export type ITransparencyModes = 0 | 1 | 2 | 3

export interface ISceneCommunications {
  commServerUrl: string | null
}

/// https://github.com/decentraland/proposals/blob/master/dsp/0020.mediawiki
export interface IScene {
  assets?: Record<any, string>
  main: string
  communications: ISceneCommunications | null
  scene: {
    base: string
    parcels: string[]
  }
  _mappings?: Record<string, string>
}

export type EnvironmentData<T> = {
  id: string
  main: string
  baseUrl: string
  mappings: Array<ContentMapping>
  data: T
}

export interface ILand {
  scene: IScene
  baseUrl: string
  mappingsResponse: MappingsResponse
}

/**
 * This data type defines a two component vector.
 */
export type Vector2Component = {
  x: number
  y: number
}

/**
 * This data type defines a three component vector. It is used for scaling, positioning and rotations
 */
export type Vector3Component = {
  x: number
  y: number
  z: number
}

export type QuaternionComponent = {
  x: number
  y: number
  z: number
  w: number
}

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
   * Name or index of the animation in the model
   */
  clip: string | number

  /**
   * Does the animation loop?, default: true
   */
  loop?: boolean

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
}

export type SkeletalAnimationComponent = {
  states: SkeletalAnimationValue[]
}

export function normalizeContentMappings(
  mappings: Record<string, string> | Array<ContentMapping>
): Array<ContentMapping> {
  const ret: Array<ContentMapping> = []

  if (typeof mappings.length === 'number' || mappings instanceof Array) {
    ret.push(...(mappings as any))
  } else {
    for (let key in mappings) {
      const file = key.toLowerCase()

      ret.push({ file, hash: mappings[key] })
    }
  }

  return ret
}

export function ILandToLoadableParcelScene(land: ILand): EnvironmentData<LoadableParcelScene> {
  const mappings: ContentMapping[] = normalizeContentMappings(land.mappingsResponse.contents)

  const ret: EnvironmentData<LoadableParcelScene> = {
    id: land.mappingsResponse.root_cid,
    baseUrl: land.baseUrl,
    main: land.scene.main,
    mappings,
    data: {
      id: land.mappingsResponse.parcel_id,
      basePosition: parseParcelPosition(land.scene.scene.base),
      parcels: (land.scene && land.scene.scene && land.scene.scene.parcels.map(parseParcelPosition)) || [],
      baseUrl: land.baseUrl,
      contents: mappings,
      land
    }
  }

  return ret
}
