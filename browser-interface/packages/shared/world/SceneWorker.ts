import { Quaternion, Vector3 } from '@dcl/ecs-math'
import { EventDataType } from 'shared/protocol/decentraland/kernel/apis/engine_api.gen'
import { PermissionItem, permissionItemFromJSON } from 'shared/protocol/decentraland/kernel/apis/permissions.gen'
import { RpcSceneControllerServiceDefinition } from 'shared/protocol/decentraland/renderer/renderer_services/scene_controller.gen'
import { createRpcServer, RpcClient, RpcClientPort, RpcServer, Transport } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import { WebWorkerTransport } from '@dcl/rpc/dist/transports/WebWorker'
import { Scene } from '@dcl/schemas'
import {
  DEBUG_SCENE_LOG,
  ETHEREUM_NETWORK,
  FORCE_SEND_MESSAGE,
  getAssetBundlesBaseUrl,
  PIPE_SCENE_CONSOLE,
  playerHeight,
  WSS_ENABLED
} from 'config'
import { gridToWorld } from 'lib/decentraland/parcels/gridToWorld'
import { parseParcelPosition } from 'lib/decentraland/parcels/parseParcelPosition'
import { getSceneNameFromJsonData } from 'lib/decentraland/sceneJson/getSceneNameFromJsonData'
import defaultLogger, { createDummyLogger, createForwardedLogger, createLogger, ILogger } from 'lib/logger'
import mitt from 'mitt'
import { trackEvent } from 'shared/analytics/trackEvent'
import { registerServices } from 'shared/apis/host'
import { PortContext } from 'shared/apis/host/context'
import {
  SceneFail,
  SceneLoad,
  SceneStart,
  SceneUnload,
  SCENE_FAIL,
  SCENE_LOAD,
  SCENE_START,
  SCENE_UNLOAD,
  signalSceneFail,
  signalSceneLoad,
  signalSceneStart,
  signalSceneUnload
} from 'shared/loading/actions'
import { incrementAvatarSceneMessages } from 'shared/session/getPerformanceInfo'
import { EntityAction, LoadableScene } from 'shared/types'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { nativeMsgBridge } from 'unity-interface/nativeMessagesBridge'
import { protobufMsgBridge } from 'unity-interface/protobufMessagesBridge'
import { PositionReport } from './positionThings'
import { joinBuffers } from 'lib/javascript/uint8arrays'

export enum SceneWorkerReadyState {
  LOADING = 1 << 0,
  LOADED = 1 << 1,
  INITIALIZED = 1 << 2,
  STARTED = 1 << 3,
  RECEIVED_MESSAGES = 1 << 4,
  LOADING_FAILED = 1 << 5,
  SYSTEM_FAILED = 1 << 6,
  DISPOSING = 1 << 7,
  SYSTEM_DISPOSED = 1 << 8,
  DISPOSED = 1 << 9
}

const sdk6RuntimeRaw =
  process.env.NODE_ENV === 'production'
    ? // eslint-disable-next-line @typescript-eslint/no-var-requires
      require('@dcl/scene-runtime/dist/sdk6-webworker.js').default
    : // eslint-disable-next-line @typescript-eslint/no-var-requires
      require('@dcl/scene-runtime/dist/sdk6-webworker.dev.js').default

const sdk6RuntimeBLOB = new Blob([sdk6RuntimeRaw])
const sdk6RuntimeUrl = URL.createObjectURL(sdk6RuntimeBLOB)

const sdk7RuntimeRaw =
  process.env.NODE_ENV === 'production'
    ? // eslint-disable-next-line @typescript-eslint/no-var-requires
      require('@dcl/scene-runtime/dist/sdk7-webworker.js').default
    : // eslint-disable-next-line @typescript-eslint/no-var-requires
      require('@dcl/scene-runtime/dist/sdk7-webworker.dev.js').default

const sdk7RuntimeBLOB = new Blob([sdk7RuntimeRaw])
const sdk7RuntimeUrl = URL.createObjectURL(sdk7RuntimeBLOB)

export type SceneLifeCycleStatusType = 'unloaded' | 'awake' | 'loaded' | 'ready' | 'failed'
export type SceneLifeCycleStatusReport = { sceneId: string; status: SceneLifeCycleStatusType }

export const sceneEvents = mitt<{
  [SCENE_LOAD]: SceneLoad
  [SCENE_START]: SceneStart
  [SCENE_FAIL]: SceneFail
  [SCENE_UNLOAD]: SceneUnload
}>()

function buildWebWorkerTransport(loadableScene: LoadableScene, sdk7: boolean): Transport {
  const loggerName = getSceneNameFromJsonData(loadableScene.entity.metadata) || loadableScene.id

  const workerName = sdk7 ? 'SDK7' : 'LegacyScene'

  const worker = new Worker(sdk7 ? sdk7RuntimeUrl : sdk6RuntimeUrl, {
    name: `${workerName}(${loggerName},${(loadableScene.entity.metadata as Scene).scene?.base})`
  })

  worker.addEventListener('error', (err) => {
    trackEvent('errorInSceneWorker', {
      message: err.message,
      scene: loadableScene.id,
      pointers: loadableScene.entity.pointers
    })
  })

  return WebWorkerTransport(worker)
}

let globalSceneNumberCounter = 0

export class SceneWorker {
  public ready: SceneWorkerReadyState = SceneWorkerReadyState.LOADING

  public rpcContext!: PortContext
  private rpcServer!: RpcServer<PortContext>

  private sceneStarted: boolean = false

  private position: Vector3 = new Vector3()
  private readonly lastSentPosition = new Vector3(0, 0, 0)
  private readonly lastSentRotation = new Quaternion(0, 0, 0, 1)
  private readonly startLoadingTime = performance.now()
  // this is the transport for the worker
  public transport?: Transport

  metadata: Scene
  logger: ILogger

  static async createSceneWorker(
    loadableScene: Readonly<LoadableScene>,
    rpcClient: RpcClient,
    transportBuilder: () => Transport | undefined
  ) {
    ++globalSceneNumberCounter
    const sceneNumber = globalSceneNumberCounter
    const scenePort = await rpcClient.createPort(`scene-${sceneNumber}`)
    const worker = new SceneWorker(loadableScene, sceneNumber, scenePort, transportBuilder)
    await worker.attachTransport()
    return worker
  }

  protected constructor(
    public readonly loadableScene: Readonly<LoadableScene>,
    sceneNumber: number,
    scenePort: RpcClientPort,
    private transportBuilder: () => Transport | undefined
  ) {
    const skipErrors = ['Transport closed while waiting the ACK']

    this.metadata = loadableScene.entity.metadata

    const loggerName = getSceneNameFromJsonData(this.metadata) || loadableScene.id
    const loggerPrefix = `scene: [${loggerName}]`
    this.logger = DEBUG_SCENE_LOG
      ? PIPE_SCENE_CONSOLE
        ? createForwardedLogger('kernel', loggerPrefix)
        : createLogger(loggerPrefix)
      : createDummyLogger()

    if (!Scene.validate(loadableScene.entity.metadata)) {
      defaultLogger.error('Invalid scene metadata', loadableScene.entity.metadata, Scene.validate.errors)
    }

    const IS_SDK7 =
      loadableScene.entity.metadata.runtimeVersion === '7' ||
      !!loadableScene.entity.metadata.ecs7 ||
      !!loadableScene.entity.metadata.sdk7

    const rpcSceneControllerService = codegen.loadService<any>(scenePort, RpcSceneControllerServiceDefinition)

    this.rpcContext = {
      sdk7: IS_SDK7,
      scenePort,
      rpcSceneControllerService,
      sceneData: {
        isPortableExperience: false,
        useFPSThrottling: false,
        ...loadableScene,
        sceneNumber
      },
      logger: this.logger,
      permissionGranted: new Set(),
      subscribedEvents: new Set(['sceneStart']),
      events: [],
      sendSceneEvent: (type, data) => {
        if (this.rpcContext.subscribedEvents.has(type)) {
          this.rpcContext.events.push({
            type: EventDataType.EDT_GENERIC,
            generic: {
              eventId: type,
              eventData: JSON.stringify(data)
            }
          })
        }
      },
      sendProtoSceneEvent: (e) => {
        this.rpcContext.events.push(e)
      },
      sendBatch: this.sendBatch.bind(this),
      readFile: this.readFile.bind(this),
      initialEntitiesTick0: Uint8Array.of(),
      hasMainCrdt: false
    }

    // if the scene metadata has a base parcel, then we set it as the position
    // used for the zero of coordinates
    if (loadableScene.entity.metadata.scene?.base) {
      const metadata: Scene = loadableScene.entity.metadata
      const basePosition = parseParcelPosition(metadata.scene?.base)

      gridToWorld(basePosition.x, basePosition.y, this.position)
    }

    this.rpcServer = createRpcServer<PortContext>({
      logger: {
        ...this.logger,
        debug: this.logger.log,
        error: (error: string | Error, extra?: Record<string, string | number>) => {
          if (!(error instanceof Error && skipErrors.includes(error.message))) {
            this.logger.error(error, extra)
          }
        }
      }
    })

    if (this.metadata.requiredPermissions) {
      for (const permissionItemString of this.metadata.requiredPermissions) {
        const item = permissionItemFromJSON(`PI_${permissionItemString}`)
        if (item !== PermissionItem.UNRECOGNIZED) {
          this.rpcContext.permissionGranted.add(item)
        } else {
          defaultLogger.error('Invalid permission in metadata', permissionItemString)
        }
      }
    }
  }

  async readFile(fileName: string) {
    // filenames are lower cased as per https://adr.decentraland.org/adr/ADR-80
    const normalized = fileName.toLowerCase()

    // and we iterate over the entity content mappings to resolve the file hash
    for (const { file, hash } of this.rpcContext.sceneData.entity.content) {
      if (file.toLowerCase() === normalized) {
        // fetch the actual content
        const baseUrl = this.rpcContext.sceneData.baseUrl.endsWith('/')
          ? this.rpcContext.sceneData.baseUrl
          : this.rpcContext.sceneData.baseUrl + '/'
        const url = baseUrl + hash
        const response = await fetch(url)

        if (!response.ok) throw new Error(`Error fetching file ${file} from ${url}`)

        return { hash, content: new Uint8Array(await response.arrayBuffer()) }
      }
    }

    throw new Error(`File ${fileName} not found`)
  }

  dispose() {
    const disposingFlags =
      SceneWorkerReadyState.DISPOSING | SceneWorkerReadyState.SYSTEM_DISPOSED | SceneWorkerReadyState.DISPOSED

    queueMicrotask(() => {
      // this NEEDS to run in a microtask because sagas control this .dispose
      sceneEvents.emit(SCENE_UNLOAD, signalSceneUnload(this.loadableScene))
    })

    void this.rpcContext.rpcSceneControllerService.unloadScene({})

    if ((this.ready & disposingFlags) === 0) {
      this.ready |= SceneWorkerReadyState.DISPOSING

      this.transport?.close()
    }

    this.ready |= SceneWorkerReadyState.DISPOSED

    queueMicrotask(() => {
      this.rpcContext.scenePort.close()
    })
  }

  // when the engine says "the scene is ready" or it did fail to load. it is of
  // extreme importance that this method always emits a SCENE_START signal that
  // later will be injected into the redux-saga context
  onReady() {
    this.ready |= SceneWorkerReadyState.STARTED

    if (!this.sceneStarted) {
      this.sceneStarted = true
      this.rpcContext.sendSceneEvent('sceneStart', {})

      const baseParcel = this.metadata.scene.base

      trackEvent('scene_start_event', {
        scene_id: this.loadableScene.id,
        time_since_creation: performance.now() - this.startLoadingTime,
        base: baseParcel
      })

      sceneEvents.emit(SCENE_START, signalSceneStart(this.loadableScene))
    }
  }

  // when an user enters the scene
  onEnter(userId: string) {
    // if the scene is a portable experience, then people never enters the scene
    if (this.rpcContext.sceneData.isPortableExperience) return
    this.rpcContext.sendSceneEvent('onEnterScene', { userId })
  }

  // when an user leaves the scene
  onLeave(userId: string, _self: boolean) {
    // if the scene is a portable experience, then people never leaves the scene
    if (this.rpcContext.sceneData.isPortableExperience) return
    this.rpcContext.sendSceneEvent('onLeaveScene', { userId })
  }

  isStarted(): boolean {
    return !!(this.ready & SceneWorkerReadyState.STARTED)
  }

  // attachTransport is executed in a microtask to defer its execution stack
  // and enable external customizations to this.rpcContext as it could be the
  // permissions of the scene or the FPS limit
  protected async attachTransport() {
    const isGlobalScene = this.loadableScene.isGlobalScene || this.loadableScene.isPortableExperience || false
    const showAsPortableExperience = (isGlobalScene && this.loadableScene.isPortableExperience) || false

    // first initialize the scene in the renderer
    await this.rpcContext.rpcSceneControllerService.loadScene({
      baseUrl: this.loadableScene.baseUrl,
      baseUrlAssetBundles: getAssetBundlesBaseUrl(ETHEREUM_NETWORK.MAINNET) + '/',
      entity: {
        content: this.loadableScene.entity.content,
        pointers: this.loadableScene.entity.pointers,
        timestamp: this.loadableScene.entity.timestamp,
        metadata: JSON.stringify(this.loadableScene.entity.metadata || null),
        id: this.loadableScene.id
      },
      isGlobalScene,
      isPortableExperience: showAsPortableExperience,
      sceneNumber: this.rpcContext.sceneData.sceneNumber,
      sceneName: getSceneNameFromJsonData(this.loadableScene.entity.metadata),
      sdk7: this.rpcContext.sdk7
    })

    let mainCrdt = Uint8Array.of()

    try {
      if (this.rpcContext.sceneData.entity.content.some(($) => $.file.toLowerCase() === 'main.crdt')) {
        const file = await this.readFile('main.crdt')
        mainCrdt = file.content
      }
    } catch (err: any) {
      this.logger.error(err)
    }

    // this is the tick#0 as specified in ADR-133 and ADR-148
    const result = await this.rpcContext.rpcSceneControllerService.sendCrdt({ payload: mainCrdt })
    this.rpcContext.initialEntitiesTick0 = joinBuffers(mainCrdt, result.payload)
    this.rpcContext.hasMainCrdt = mainCrdt.length > 0

    // from now on, the sceneData object is read-only
    Object.freeze(this.rpcContext.sceneData)

    this.transport = this.transportBuilder() || buildWebWorkerTransport(this.loadableScene, this.rpcContext.sdk7)

    this.rpcServer.setHandler(registerServices)
    this.rpcServer.attachTransport(this.transport, this.rpcContext)
    this.ready |= SceneWorkerReadyState.LOADED

    sceneEvents.emit(SCENE_LOAD, signalSceneLoad(this.loadableScene))

    const WORKER_TIMEOUT = 120_000
    setTimeout(() => this.onLoadTimeout(), WORKER_TIMEOUT)
  }

  private onLoadTimeout() {
    if (!this.sceneStarted) {
      this.ready |= SceneWorkerReadyState.LOADING_FAILED

      const state: string[] = []

      for (const i in SceneWorkerReadyState) {
        if (!isNaN(i as any)) {
          if (this.ready & (i as any)) {
            state.push(SceneWorkerReadyState[i])
          }
        }
      }

      this.logger.warn('SceneTimedOut', state.join('+'))

      this.sceneStarted = true
      this.rpcContext.sendSceneEvent('sceneStart', {})

      if (!(this.ready & SceneWorkerReadyState.INITIALIZED)) {
        // this message should be sent upon failure to unlock the loading screen
        // when a scene is malformed and never emits InitMessagesFinished
        this.sendBatch([{ payload: {}, type: 'InitMessagesFinished' }])
      }

      sceneEvents.emit(SCENE_FAIL, signalSceneFail(this.loadableScene))
    }
  }

  private sendBatch(actions: EntityAction[]): void {
    if (this.loadableScene.id === 'dcl-gs-avatars') {
      incrementAvatarSceneMessages(actions.length)
    }

    this.ready |= SceneWorkerReadyState.RECEIVED_MESSAGES

    if (!(this.ready & SceneWorkerReadyState.INITIALIZED)) {
      let present = false
      for (const action of actions) {
        if (action.type === 'InitMessagesFinished') {
          present = true
          break
        }
      }
      if (!present) {
        actions.push({
          payload: {},
          type: 'InitMessagesFinished'
        })
      }
      this.ready |= SceneWorkerReadyState.INITIALIZED
    }

    if (WSS_ENABLED || FORCE_SEND_MESSAGE) {
      this.sendBatchWss(actions)
    } else {
      this.sendBatchNative(actions)
    }
  }

  private sendBatchWss(actions: EntityAction[]): void {
    const sceneId = this.loadableScene.id
    const sceneNumber = this.rpcContext.sceneData.sceneNumber
    const messages: string[] = []
    let len = 0

    function flush() {
      if (len) {
        getUnityInstance().SendSceneMessage(messages.join('\n'))
        messages.length = 0
        len = 0
      }
    }

    for (let i = 0; i < actions.length; i++) {
      const action = actions[i]

      // Check moved from SceneRuntime.ts->DecentralandInterface.componentUpdate() here until we remove base64 support.
      // This way we can still initialize problematic scenes in the Editor, otherwise the protobuf encoding explodes with such messages.
      if (action.payload.json?.length > 49000) {
        this.logger.error('Component payload cannot exceed 49.000 bytes. Skipping message.')

        continue
      }

      const part = protobufMsgBridge.encodeSceneMessage(sceneId, sceneNumber, action.type, action.payload, action.tag)
      messages.push(part)
      len += part.length

      if (len > 1024 * 1024) {
        flush()
      }
    }

    flush()
  }

  private sendBatchNative(actions: EntityAction[]): void {
    const sceneId = this.loadableScene.id
    const sceneNumber = this.rpcContext.sceneData.sceneNumber
    for (let i = 0; i < actions.length; i++) {
      const action = actions[i]
      nativeMsgBridge.SendNativeMessage(sceneId, sceneNumber, action)
    }
  }

  public sendUserViewMatrix(positionReport: Readonly<PositionReport>) {
    if (this.rpcContext.subscribedEvents.has('positionChanged')) {
      if (!this.lastSentPosition.equals(positionReport.position)) {
        this.rpcContext.sendProtoSceneEvent({
          type: EventDataType.EDT_POSITION_CHANGED,
          positionChanged: {
            position: {
              x: positionReport.position.x - this.position.x,
              z: positionReport.position.z - this.position.z,
              y: positionReport.position.y
            },
            cameraPosition: positionReport.position,
            playerHeight: playerHeight
          }
        })

        this.lastSentPosition.copyFrom(positionReport.position)
      }
    }
    if (this.rpcContext.subscribedEvents.has('rotationChanged')) {
      if (positionReport.cameraQuaternion && !this.lastSentRotation.equals(positionReport.cameraQuaternion)) {
        this.rpcContext.sendProtoSceneEvent({
          type: EventDataType.EDT_ROTATION_CHANGED,
          rotationChanged: {
            rotation: positionReport.cameraEuler,
            quaternion: positionReport.cameraQuaternion
          }
        })
        this.lastSentRotation.copyFrom(positionReport.cameraQuaternion)
      }
    }
  }
}
