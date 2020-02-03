declare var window: any
declare var global: any

type GameInstance = {
  SendMessage(object: string, method: string, ...args: (number | string)[]): void
}

import { uuid } from 'decentraland-ecs/src'
import { EventDispatcher } from 'decentraland-rpc/lib/common/core/EventDispatcher'
import { IFuture } from 'fp-future'
import { Empty } from 'google-protobuf/google/protobuf/empty_pb'
import { avatarMessageObservable } from 'shared/comms/peers'
import { AvatarMessageType } from 'shared/comms/interface/types'
import { gridToWorld } from '../atomicHelpers/parcelScenePositions'
import { DEBUG, EDITOR, ENGINE_DEBUG_PANEL, playerConfigurations, SCENE_DEBUG_PANEL, SHOW_FPS_COUNTER } from '../config'
import { Quaternion, ReadOnlyQuaternion, ReadOnlyVector3, Vector3 } from '../decentraland-ecs/src/decentraland/math'
import { IEventNames, IEvents, ProfileForRenderer } from '../decentraland-ecs/src/decentraland/Types'
import { sceneLifeCycleObservable } from '../decentraland-loader/lifecycle/controllers/scene'
import { queueTrackingEvent } from '../shared/analytics'
import { DevTools } from '../shared/apis/DevTools'
import { ParcelIdentity } from '../shared/apis/ParcelIdentity'
import { chatObservable } from '../shared/comms/chat'
import { aborted } from '../shared/loading/ReportFatalError'
import { loadingScenes, teleportTriggered, unityClientLoaded } from '../shared/loading/types'
import { createLogger, defaultLogger, ILogger } from '../shared/logger'
import { saveAvatarRequest } from '../shared/passports/actions'
import { Avatar, Wearable } from '../shared/passports/types'
import {
  PB_AttachEntityComponent,
  PB_ComponentCreated,
  PB_ComponentDisposed,
  PB_ComponentRemoved,
  PB_ComponentUpdated,
  PB_CreateEntity,
  PB_Query,
  PB_Ray,
  PB_RayQuery,
  PB_RemoveEntity,
  PB_SendSceneMessage,
  PB_SetEntityParent,
  PB_UpdateEntityComponent,
  PB_Vector3
} from '../shared/proto/engineinterface_pb'
import { Session } from '../shared/session'
import { getPerformanceInfo } from '../shared/session/getPerformanceInfo'
import {
  AttachEntityComponentPayload,
  ComponentCreatedPayload,
  ComponentDisposedPayload,
  ComponentRemovedPayload,
  ComponentUpdatedPayload,
  CreateEntityPayload,
  EntityAction,
  EnvironmentData,
  HUDConfiguration,
  ILand,
  ILandToLoadableParcelScene,
  ILandToLoadableParcelSceneUpdate,
  InstancedSpawnPoint,
  IScene,
  LoadableParcelScene,
  MappingsResponse,
  Notification,
  QueryPayload,
  RemoveEntityPayload,
  SetEntityParentPayload,
  UpdateEntityComponentPayload
} from '../shared/types'
import { ParcelSceneAPI } from '../shared/world/ParcelSceneAPI'
import {
  enableParcelSceneLoading,
  getParcelSceneID,
  getSceneWorkerBySceneID,
  loadParcelScene,
  stopParcelSceneWorker
} from '../shared/world/parcelSceneManager'
import { positionObservable, teleportObservable } from '../shared/world/positionThings'
import { hudWorkerUrl, SceneWorker } from '../shared/world/SceneWorker'
import { ensureUiApis } from '../shared/world/uiSceneInitializer'
import { worldRunningObservable } from '../shared/world/worldState'
import { sendPublicChatMessage } from 'shared/comms'

const rendererVersion = require('decentraland-renderer')
window['console'].log('Renderer version: ' + rendererVersion)

let gameInstance!: GameInstance

export let futures: Record<string, IFuture<any>> = {}

const positionEvent = {
  position: Vector3.Zero(),
  quaternion: Quaternion.Identity,
  rotation: Vector3.Zero(),
  playerHeight: playerConfigurations.height,
  mousePosition: Vector3.Zero()
}

/////////////////////////////////// HANDLERS ///////////////////////////////////

const browserInterface = {
  /** Triggered when the camera moves */
  ReportPosition(data: { position: ReadOnlyVector3; rotation: ReadOnlyQuaternion; playerHeight?: number }) {
    positionEvent.position.set(data.position.x, data.position.y, data.position.z)
    positionEvent.quaternion.set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w)
    positionEvent.rotation.copyFrom(positionEvent.quaternion.eulerAngles)
    positionEvent.playerHeight = data.playerHeight || playerConfigurations.height
    positionObservable.notifyObservers(positionEvent)
  },

  ReportMousePosition(data: { id: string; mousePosition: ReadOnlyVector3 }) {
    positionEvent.mousePosition.set(data.mousePosition.x, data.mousePosition.y, data.mousePosition.z)
    positionObservable.notifyObservers(positionEvent)
    futures[data.id].resolve(data.mousePosition)
  },

  SceneEvent(data: { sceneId: string; eventType: string; payload: any }) {
    const scene = getSceneWorkerBySceneID(data.sceneId)
    if (scene) {
      const parcelScene = scene.parcelScene as UnityParcelScene
      parcelScene.emit(data.eventType as IEventNames, data.payload)
    } else {
      defaultLogger.error(`SceneEvent: Scene ${data.sceneId} not found`, data)
    }
  },

  OpenWebURL(data: { url: string }) {
    window.open(data.url, '_blank')
  },

  PerformanceReport(samples: string) {
    const perfReport = getPerformanceInfo(samples)
    queueTrackingEvent('performance report', perfReport)
  },

  PreloadFinished(data: { sceneId: string }) {
    // stub. there is no code about this in unity side yet
  },

  TriggerExpression(data: { id: string; timestamp: number }) {
    avatarMessageObservable.notifyObservers({
      type: AvatarMessageType.USER_EXPRESSION,
      uuid: uuid(),
      expressionId: data.id,
      timestamp: data.timestamp
    })
    const id = uuid()
    const chatMessage = `␐${data.id} ${data.timestamp}`
    sendPublicChatMessage(id, chatMessage)
  },

  LogOut() {
    Session.current.then(s => s.logout()).catch(e => defaultLogger.error('error while logging out', e))
  },

  SaveUserAvatar(data: { face: string; body: string; avatar: Avatar }) {
    global.globalStore.dispatch(saveAvatarRequest(data))
  },

  ControlEvent({ eventType, payload }: { eventType: string; payload: any }) {
    switch (eventType) {
      case 'SceneReady': {
        const { sceneId } = payload
        sceneLifeCycleObservable.notifyObservers({ sceneId, status: 'ready' })
        break
      }
      case 'ActivateRenderingACK': {
        if (!aborted) {
          worldRunningObservable.notifyObservers(true)
        }
        break
      }
      default: {
        defaultLogger.warn(`Unknown event type ${eventType}, ignoring`)
        break
      }
    }
  },

  SendScreenshot(data: { id: string; encodedTexture: string }) {
    futures[data.id].resolve(data.encodedTexture)
  },

  ReportBuilderCameraTarget(data: { id: string; cameraTarget: ReadOnlyVector3 }) {
    futures[data.id].resolve(data.cameraTarget)
  },

  EditAvatarClicked() {
    delightedSurvey()
  }
}

export function setLoadingScreenVisible(shouldShow: boolean) {
  document.getElementById('overlay')!.style.display = shouldShow ? 'block' : 'none'
  document.getElementById('load-messages-wrapper')!.style.display = shouldShow ? 'block' : 'none'
  document.getElementById('progress-bar')!.style.display = shouldShow ? 'block' : 'none'
  if (!shouldShow) {
    stopTeleportAnimation()
  }
}

function delightedSurvey() {
  const { analytics, delighted, globalStore } = global
  if (analytics && delighted && globalStore) {
    const email = ''
    const payload = {
      email: email,
      properties: {
        anonymous_id: analytics && analytics.user ? analytics.user().anonymousId() : null
      }
    }

    delighted.survey(payload)
  }
}

function ensureTeleportAnimation() {
  document
    .getElementById('gameContainer')!
    .setAttribute(
      'style',
      'background: #151419 url(images/teleport.gif) no-repeat center !important; background-size: 194px 257px !important;'
    )
  document.body.setAttribute(
    'style',
    'background: #151419 url(images/teleport.gif) no-repeat center !important; background-size: 194px 257px !important;'
  )
}

function stopTeleportAnimation() {
  document.getElementById('gameContainer')!.setAttribute('style', 'background: #151419')
  document.body.setAttribute('style', 'background: #151419')
}

const CHUNK_SIZE = 500

export function* chunkGenerator(
  parcelChunkSize: number,
  info: { name: string; type: number; parcels: { x: number; y: number }[] }[]
) {
  if (parcelChunkSize < 1) {
    throw Error(`parcel chunk size (${parcelChunkSize}) cannot be less than 1`)
  }

  // flatten scene data into parcels
  const parcels = info.reduce(
    (parcels, elem, index) =>
      parcels.concat(
        elem.parcels.map(parcel => ({
          index,
          name: elem.name,
          type: elem.type,
          parcel
        }))
      ),
    [] as { index: number; name: string; type: number; parcel: { x: number; y: number } }[]
  )

  // split into chunk size + fold into scene
  while (parcels.length > 0) {
    const chunk = parcels
      .splice(0, parcelChunkSize)
      .reduce((scenes, parcel) => {
        const scene = scenes.get(parcel.index)
        if (scene) {
          scene.parcels.push(parcel.parcel)
        } else {
          const newScene = { name: parcel.name, type: parcel.type, parcels: [parcel.parcel] }
          scenes.set(parcel.index, newScene)
        }
        return scenes
      }, new Map())
      .values()

    yield [...chunk]
  }
}

export const unityInterface = {
  debug: false,
  SetDebug() {
    gameInstance.SendMessage('SceneController', 'SetDebug')
  },
  LoadProfile(profile: ProfileForRenderer) {
    gameInstance.SendMessage('SceneController', 'LoadProfile', JSON.stringify(profile))
  },
  CreateUIScene(data: { id: string; baseUrl: string }) {
    /**
     * UI Scenes are scenes that does not check any limit or boundary. The
     * position is fixed at 0,0 and they are universe-wide. An example of this
     * kind of scenes is the Avatar scene. All the avatars are just GLTFs in
     * a scene.
     */
    gameInstance.SendMessage('SceneController', 'CreateUIScene', JSON.stringify(data))
  },
  /** Sends the camera position & target to the engine */
  Teleport({ position: { x, y, z }, cameraTarget }: InstancedSpawnPoint) {
    const theY = y <= 0 ? 2 : y

    ensureTeleportAnimation()
    gameInstance.SendMessage('CharacterController', 'Teleport', JSON.stringify({ x, y: theY, z }))
    gameInstance.SendMessage('CameraController', 'SetRotation', JSON.stringify({ x, y: theY, z, cameraTarget }))
  },
  /** Tells the engine which scenes to load */
  LoadParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!')
    }
    gameInstance.SendMessage('SceneController', 'LoadParcelScenes', JSON.stringify(parcelsToLoad[0]))
  },
  UpdateParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!')
    }
    gameInstance.SendMessage('SceneController', 'UpdateParcelScenes', JSON.stringify(parcelsToLoad[0]))
  },
  UnloadScene(sceneId: string) {
    gameInstance.SendMessage('SceneController', 'UnloadScene', sceneId)
  },
  SendSceneMessage(messages: string) {
    gameInstance.SendMessage(`SceneController`, `SendSceneMessage`, messages)
  },
  SetSceneDebugPanel() {
    gameInstance.SendMessage('SceneController', 'SetSceneDebugPanel')
  },
  ShowFPSPanel() {
    gameInstance.SendMessage('SceneController', 'ShowFPSPanel')
  },
  HideFPSPanel() {
    gameInstance.SendMessage('SceneController', 'HideFPSPanel')
  },
  SetEngineDebugPanel() {
    gameInstance.SendMessage('SceneController', 'SetEngineDebugPanel')
  },
  // @internal
  SendBuilderMessage(method: string, payload: string = '') {
    gameInstance.SendMessage(`BuilderController`, method, payload)
  },
  ActivateRendering() {
    gameInstance.SendMessage('SceneController', 'ActivateRendering')
  },
  DeactivateRendering() {
    gameInstance.SendMessage('SceneController', 'DeactivateRendering')
  },
  UnlockCursor() {
    gameInstance.SendMessage('MouseCatcher', 'UnlockCursor')
  },
  SetBuilderReady() {
    gameInstance.SendMessage('SceneController', 'BuilderReady')
  },
  AddUserProfileToCatalog(peerProfile: ProfileForRenderer) {
    gameInstance.SendMessage('SceneController', 'AddUserProfileToCatalog', JSON.stringify(peerProfile))
  },
  AddWearablesToCatalog(wearables: Wearable[]) {
    for (let wearable of wearables) {
      gameInstance.SendMessage('SceneController', 'AddWearableToCatalog', JSON.stringify(wearable))
    }
  },
  RemoveWearablesFromCatalog(wearableIds: string[]) {
    gameInstance.SendMessage('SceneController', 'RemoveWearablesFromCatalog', JSON.stringify(wearableIds))
  },
  ClearWearableCatalog() {
    gameInstance.SendMessage('SceneController', 'ClearWearableCatalog')
  },
  ShowNewWearablesNotification(wearableNumber: number) {
    gameInstance.SendMessage('HUDController', 'ShowNewWearablesNotification', wearableNumber.toString())
  },
  ShowNotification(notification: Notification) {
    gameInstance.SendMessage('HUDController', 'ShowNotificationFromJson', JSON.stringify(notification))
  },
  ConfigureMinimapHUD(configuration: HUDConfiguration) {
    gameInstance.SendMessage('HUDController', 'ConfigureMinimapHUD', JSON.stringify(configuration))
  },
  ConfigureAvatarHUD(configuration: HUDConfiguration) {
    gameInstance.SendMessage('HUDController', 'ConfigureAvatarHUD', JSON.stringify(configuration))
  },
  ConfigureNotificationHUD(configuration: HUDConfiguration) {
    gameInstance.SendMessage('HUDController', 'ConfigureNotificationHUD', JSON.stringify(configuration))
  },
  ConfigureAvatarEditorHUD(configuration: HUDConfiguration) {
    gameInstance.SendMessage('HUDController', 'ConfigureAvatarEditorHUD', JSON.stringify(configuration))
  },
  ConfigureSettingsHUD(configuration: HUDConfiguration) {
    gameInstance.SendMessage('HUDController', 'ConfigureSettingsHUD', JSON.stringify(configuration))
  },
  ConfigureExpressionsHUD(configuration: HUDConfiguration) {
    gameInstance.SendMessage('HUDController', 'ConfigureExpressionsHUD', JSON.stringify(configuration))
  },
  TriggerSelfUserExpression(expressionId: string) {
    gameInstance.SendMessage('HUDController', 'TriggerSelfUserExpression', expressionId)
  },
  ConfigurePlayerInfoCardHUD(configuration: HUDConfiguration) {
    gameInstance.SendMessage('HUDController', 'ConfigurePlayerInfoCardHUD', JSON.stringify(configuration))
  },
  UpdateMinimapSceneInformation(info: { name: string; type: number; parcels: { x: number; y: number }[] }[]) {
    const chunks = chunkGenerator(CHUNK_SIZE, info)

    for (const chunk of chunks) {
      gameInstance.SendMessage('SceneController', 'UpdateMinimapSceneInformation', JSON.stringify(chunk))
    }
  },
  SelectGizmoBuilder(type: string) {
    this.SendBuilderMessage('SelectGizmo', type)
  },
  ResetBuilderObject() {
    this.SendBuilderMessage('ResetObject')
  },
  SetCameraZoomDeltaBuilder(delta: number) {
    this.SendBuilderMessage('ZoomDelta', delta.toString())
  },
  GetCameraTargetBuilder(futureId: string) {
    this.SendBuilderMessage('GetCameraTargetBuilder', futureId)
  },
  SetPlayModeBuilder(on: string) {
    this.SendBuilderMessage('SetPlayMode', on)
  },
  PreloadFileBuilder(url: string) {
    this.SendBuilderMessage('PreloadFile', url)
  },
  GetMousePositionBuilder(x: string, y: string, id: string) {
    this.SendBuilderMessage('GetMousePosition', `{"x":"${x}", "y": "${y}", "id": "${id}" }`)
  },
  TakeScreenshotBuilder(id: string) {
    this.SendBuilderMessage('TakeScreenshot', id)
  },
  SetCameraPositionBuilder(position: Vector3) {
    this.SendBuilderMessage('SetBuilderCameraPosition', position.x + ',' + position.y + ',' + position.z)
  },
  SetCameraRotationBuilder(aplha: number, beta: number) {
    this.SendBuilderMessage('SetBuilderCameraRotation', aplha + ',' + beta)
  },
  ResetCameraZoomBuilder() {
    this.SendBuilderMessage('ResetBuilderCameraZoom')
  },
  SetBuilderGridResolution(position: number, rotation: number, scale: number) {
    this.SendBuilderMessage(
      'SetGridResolution',
      JSON.stringify({ position: position, rotation: rotation, scale: scale })
    )
  },
  SetBuilderSelectedEntities(entities: string[]) {
    this.SendBuilderMessage('SetSelectedEntities', JSON.stringify({ entities: entities }))
  },
  ResetBuilderScene() {
    this.SendBuilderMessage('ResetBuilderScene')
  },
  OnBuilderKeyDown(key: string) {
    this.SendBuilderMessage('OnBuilderKeyDown', key)
  }
}

export const HUD: Record<string, { configure: (config: HUDConfiguration) => void }> = {
  Minimap: {
    configure: unityInterface.ConfigureMinimapHUD
  },
  Avatar: {
    configure: unityInterface.ConfigureAvatarHUD
  },
  Notification: {
    configure: unityInterface.ConfigureNotificationHUD
  },
  AvatarEditor: {
    configure: unityInterface.ConfigureAvatarEditorHUD
  },
  Settings: {
    configure: unityInterface.ConfigureSettingsHUD
  },
  Expressions: {
    configure: unityInterface.ConfigureExpressionsHUD
  },
  PlayerInfoCard: {
    configure: unityInterface.ConfigurePlayerInfoCardHUD
  }
}

window['unityInterface'] = unityInterface

////////////////////////////////////////////////////////////////////////////////

// protobuf message instances
const createEntity: PB_CreateEntity = new PB_CreateEntity()
const removeEntity: PB_RemoveEntity = new PB_RemoveEntity()
const updateEntityComponent: PB_UpdateEntityComponent = new PB_UpdateEntityComponent()
const attachEntity: PB_AttachEntityComponent = new PB_AttachEntityComponent()
const removeEntityComponent: PB_ComponentRemoved = new PB_ComponentRemoved()
const setEntityParent: PB_SetEntityParent = new PB_SetEntityParent()
const query: PB_Query = new PB_Query()
const rayQuery: PB_RayQuery = new PB_RayQuery()
const ray: PB_Ray = new PB_Ray()
const origin: PB_Vector3 = new PB_Vector3()
const direction: PB_Vector3 = new PB_Vector3()
const componentCreated: PB_ComponentCreated = new PB_ComponentCreated()
const componentDisposed: PB_ComponentDisposed = new PB_ComponentDisposed()
const componentUpdated: PB_ComponentUpdated = new PB_ComponentUpdated()

class UnityScene<T> implements ParcelSceneAPI {
  eventDispatcher = new EventDispatcher()
  worker!: SceneWorker
  logger: ILogger

  constructor(public data: EnvironmentData<T>) {
    this.logger = createLogger(getParcelSceneID(this) + ': ')
  }

  sendBatch(actions: EntityAction[]): void {
    const sceneId = getParcelSceneID(this)
    let messages = ''
    for (let i = 0; i < actions.length; i++) {
      const action = actions[i]
      messages += this.encodeSceneMessage(sceneId, action.type, action.payload, action.tag)
      messages += '\n'
    }

    unityInterface.SendSceneMessage(messages)
  }

  registerWorker(worker: SceneWorker): void {
    this.worker = worker
  }

  dispose(): void {
    // TODO: do we need to release some resource after releasing a scene worker?
  }

  on<T extends IEventNames>(event: T, cb: (event: IEvents[T]) => void): void {
    this.eventDispatcher.on(event, cb)
  }

  emit<T extends IEventNames>(event: T, data: IEvents[T]): void {
    this.eventDispatcher.emit(event, data)
  }

  encodeSceneMessage(parcelSceneId: string, method: string, payload: any, tag: string = ''): string {
    if (unityInterface.debug) {
      defaultLogger.info(parcelSceneId, method, payload, tag)
    }

    let message: PB_SendSceneMessage = new PB_SendSceneMessage()
    message.setSceneid(parcelSceneId)
    message.setTag(tag)

    switch (method) {
      case 'CreateEntity':
        message.setCreateentity(this.encodeCreateEntity(payload))
        break
      case 'RemoveEntity':
        message.setRemoveentity(this.encodeRemoveEntity(payload))
        break
      case 'UpdateEntityComponent':
        message.setUpdateentitycomponent(this.encodeUpdateEntityComponent(payload))
        break
      case 'AttachEntityComponent':
        message.setAttachentitycomponent(this.encodeAttachEntityComponent(payload))
        break
      case 'ComponentRemoved':
        message.setComponentremoved(this.encodeComponentRemoved(payload))
        break
      case 'SetEntityParent':
        message.setSetentityparent(this.encodeSetEntityParent(payload))
        break
      case 'Query':
        message.setQuery(this.encodeQuery(payload))
        break
      case 'ComponentCreated':
        message.setComponentcreated(this.encodeComponentCreated(payload))
        break
      case 'ComponentDisposed':
        message.setComponentdisposed(this.encodeComponentDisposed(payload))
        break
      case 'ComponentUpdated':
        message.setComponentupdated(this.encodeComponentUpdated(payload))
        break
      case 'InitMessagesFinished':
        message.setScenestarted(new Empty()) // don't know if this is necessary
        break
    }

    let arrayBuffer: Uint8Array = message.serializeBinary()
    return btoa(String.fromCharCode(...arrayBuffer))
  }

  encodeCreateEntity(createEntityPayload: CreateEntityPayload): PB_CreateEntity {
    createEntity.setId(createEntityPayload.id)
    return createEntity
  }

  encodeRemoveEntity(removeEntityPayload: RemoveEntityPayload): PB_RemoveEntity {
    removeEntity.setId(removeEntityPayload.id)
    return removeEntity
  }

  encodeUpdateEntityComponent(updateEntityComponentPayload: UpdateEntityComponentPayload): PB_UpdateEntityComponent {
    updateEntityComponent.setClassid(updateEntityComponentPayload.classId)
    updateEntityComponent.setEntityid(updateEntityComponentPayload.entityId)
    updateEntityComponent.setData(updateEntityComponentPayload.json)
    return updateEntityComponent
  }

  encodeAttachEntityComponent(attachEntityPayload: AttachEntityComponentPayload): PB_AttachEntityComponent {
    attachEntity.setEntityid(attachEntityPayload.entityId)
    attachEntity.setName(attachEntityPayload.name)
    attachEntity.setId(attachEntityPayload.id)
    return attachEntity
  }

  encodeComponentRemoved(removeEntityComponentPayload: ComponentRemovedPayload): PB_ComponentRemoved {
    removeEntityComponent.setEntityid(removeEntityComponentPayload.entityId)
    removeEntityComponent.setName(removeEntityComponentPayload.name)
    return removeEntityComponent
  }

  encodeSetEntityParent(setEntityParentPayload: SetEntityParentPayload): PB_SetEntityParent {
    setEntityParent.setEntityid(setEntityParentPayload.entityId)
    setEntityParent.setParentid(setEntityParentPayload.parentId)
    return setEntityParent
  }

  encodeQuery(queryPayload: QueryPayload): PB_Query {
    origin.setX(queryPayload.payload.ray.origin.x)
    origin.setY(queryPayload.payload.ray.origin.y)
    origin.setZ(queryPayload.payload.ray.origin.z)
    direction.setX(queryPayload.payload.ray.direction.x)
    direction.setY(queryPayload.payload.ray.direction.y)
    direction.setZ(queryPayload.payload.ray.direction.z)
    ray.setOrigin(origin)
    ray.setDirection(direction)
    ray.setDistance(queryPayload.payload.ray.distance)
    rayQuery.setRay(ray)
    rayQuery.setQueryid(queryPayload.payload.queryId)
    rayQuery.setQuerytype(queryPayload.payload.queryType)
    query.setQueryid(queryPayload.queryId)
    let arrayBuffer: Uint8Array = rayQuery.serializeBinary()
    let base64: string = btoa(String.fromCharCode(...arrayBuffer))
    query.setPayload(base64)
    return query
  }

  encodeComponentCreated(componentCreatedPayload: ComponentCreatedPayload): PB_ComponentCreated {
    componentCreated.setId(componentCreatedPayload.id)
    componentCreated.setClassid(componentCreatedPayload.classId)
    componentCreated.setName(componentCreatedPayload.name)
    return componentCreated
  }

  encodeComponentDisposed(componentDisposedPayload: ComponentDisposedPayload) {
    componentDisposed.setId(componentDisposedPayload.id)
    return componentDisposed
  }

  encodeComponentUpdated(componentUpdatedPayload: ComponentUpdatedPayload): PB_ComponentUpdated {
    componentUpdated.setId(componentUpdatedPayload.id)
    componentUpdated.setJson(componentUpdatedPayload.json)
    return componentUpdated
  }
}

export class UnityParcelScene extends UnityScene<LoadableParcelScene> {
  constructor(public data: EnvironmentData<LoadableParcelScene>) {
    super(data)
    this.logger = createLogger(data.data.basePosition.x + ',' + data.data.basePosition.y + ': ')
  }

  registerWorker(worker: SceneWorker): void {
    super.registerWorker(worker)

    gridToWorld(this.data.data.basePosition.x, this.data.data.basePosition.y, worker.position)

    this.worker.system
      .then(system => {
        system.getAPIInstance(DevTools).logger = this.logger

        const parcelIdentity = system.getAPIInstance(ParcelIdentity)
        parcelIdentity.land = this.data.data.land
        parcelIdentity.cid = getParcelSceneID(worker.parcelScene)
      })
      .catch(e => this.logger.error('Error initializing system', e))
  }
}

////////////////////////////////////////////////////////////////////////////////

/**
 *
 * Common initialization logic for the unity engine
 *
 * @param _gameInstance Unity game instance
 */
export async function initializeEngine(_gameInstance: GameInstance) {
  gameInstance = _gameInstance

  global['globalStore'].dispatch(unityClientLoaded())
  setLoadingScreenVisible(true)

  unityInterface.DeactivateRendering()

  if (DEBUG) {
    unityInterface.SetDebug()
  }

  if (SCENE_DEBUG_PANEL) {
    unityInterface.SetSceneDebugPanel()
  }

  if (SHOW_FPS_COUNTER) {
    unityInterface.ShowFPSPanel()
  }

  if (ENGINE_DEBUG_PANEL) {
    unityInterface.SetEngineDebugPanel()
  }
  if (!EDITOR) {
    await initializeDecentralandUI()
  }
  return {
    unityInterface,
    onMessage(type: string, message: any) {
      if (type in browserInterface) {
        // tslint:disable-next-line:semicolon
        ;(browserInterface as any)[type](message)
      } else {
        defaultLogger.info(`Unknown message (did you forget to add ${type} to unity-interface/dcl.ts?)`, message)
      }
    }
  }
}

export async function startUnityParcelLoading() {
  global['globalStore'].dispatch(loadingScenes())
  await enableParcelSceneLoading({
    parcelSceneClass: UnityParcelScene,
    preloadScene: async _land => {
      // TODO:
      // 1) implement preload call
      // 2) await for preload message or timeout
      // 3) return
    },
    onLoadParcelScenes: lands => {
      unityInterface.LoadParcelScenes(
        lands.map($ => {
          const x = Object.assign({}, ILandToLoadableParcelScene($).data)
          delete x.land
          return x
        })
      )
    },
    onUnloadParcelScenes: lands => {
      lands.forEach($ => {
        unityInterface.UnloadScene($.sceneId)
      })
    },
    onPositionSettled: spawnPoint => {
      if (!aborted) {
        unityInterface.Teleport(spawnPoint)
        unityInterface.ActivateRendering()
      }
    },
    onPositionUnsettled: () => {
      unityInterface.DeactivateRendering()
    }
  })
}

async function initializeDecentralandUI() {
  const sceneId = 'dcl-ui-scene'

  const scene = new UnityScene({
    sceneId,
    name: 'ui',
    baseUrl: location.origin,
    main: hudWorkerUrl,
    useFPSThrottling: false,
    data: {},
    mappings: []
  })

  const worker = loadParcelScene(scene)
  worker.persistent = true

  await ensureUiApis(worker)

  unityInterface.CreateUIScene({ id: getParcelSceneID(scene), baseUrl: scene.data.baseUrl })
}

// Builder functions

let currentLoadedScene: SceneWorker | null

export async function loadPreviewScene() {
  const result = await fetch('/scene.json?nocache=' + Math.random())

  let lastId: string | null = null

  if (currentLoadedScene) {
    lastId = currentLoadedScene.parcelScene.data.sceneId
    stopParcelSceneWorker(currentLoadedScene)
  }

  if (result.ok) {
    // we load the scene to get the metadata
    // about rhe bounds and position of the scene
    // TODO(fmiras): Validate scene according to https://github.com/decentraland/proposals/blob/master/dsp/0020.mediawiki
    const scene = (await result.json()) as IScene
    const mappingsFetch = await fetch('/mappings')
    const mappingsResponse = (await mappingsFetch.json()) as MappingsResponse

    let defaultScene: ILand = {
      name: scene.name,
      sceneId: 'previewScene',
      baseUrl: location.toString().replace(/\?[^\n]+/g, ''),
      baseUrlBundles: '',
      scene,
      mappingsResponse: mappingsResponse
    }

    const parcelScene = new UnityParcelScene(ILandToLoadableParcelScene(defaultScene))
    currentLoadedScene = loadParcelScene(parcelScene)

    const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(defaultScene).data }
    delete target.land

    defaultLogger.info('Reloading scene...')

    if (lastId) {
      unityInterface.UnloadScene(lastId)
    }

    unityInterface.LoadParcelScenes([target])

    defaultLogger.info('finish...')

    return defaultScene
  } else {
    throw new Error('Could not load scene.json')
  }
}

export function loadBuilderScene(sceneData: ILand) {
  unloadCurrentBuilderScene()

  const parcelScene = new UnityParcelScene(ILandToLoadableParcelScene(sceneData))
  currentLoadedScene = loadParcelScene(parcelScene)

  const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(sceneData).data }
  delete target.land

  unityInterface.LoadParcelScenes([target])
  return parcelScene
}

export function unloadCurrentBuilderScene() {
  if (currentLoadedScene) {
    const parcelScene = currentLoadedScene.parcelScene as UnityParcelScene
    parcelScene.emit('builderSceneUnloaded', {})

    stopParcelSceneWorker(currentLoadedScene)
    unityInterface.SendBuilderMessage('UnloadBuilderScene', parcelScene.data.sceneId)
    currentLoadedScene = null
  }
}

export function updateBuilderScene(sceneData: ILand) {
  if (currentLoadedScene) {
    const target: LoadableParcelScene = { ...ILandToLoadableParcelSceneUpdate(sceneData).data }
    delete target.land
    unityInterface.UpdateParcelScenes([target])
  }
}

teleportObservable.add((position: { x: number; y: number; text?: string }) => {
  // before setting the new position, show loading screen to avoid showing an empty world
  setLoadingScreenVisible(true)
  const globalStore = global['globalStore']
  globalStore.dispatch(teleportTriggered(position.text || `Teleporting to ${position.x}, ${position.y}`))
  delightedSurvey()
})

worldRunningObservable.add(isRunning => {
  if (isRunning) {
    setLoadingScreenVisible(false)
  }
})

window['messages'] = (e: any) => chatObservable.notifyObservers(e)

document.addEventListener('pointerlockchange', e => {
  if (!document.pointerLockElement) {
    unityInterface.UnlockCursor()
  }
})
