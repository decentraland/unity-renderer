import { Script, inject, EventSubscriber } from 'decentraland-rpc'
import {
  RPCSendableMessage,
  EntityAction,
  CreateEntityPayload,
  RemoveEntityPayload,
  UpdateEntityComponentPayload,
  AttachEntityComponentPayload,
  ComponentRemovedPayload,
  SetEntityParentPayload,
  ComponentCreatedPayload,
  ComponentDisposedPayload,
  ComponentUpdatedPayload,
  QueryPayload,
  LoadableParcelScene,
  OpenNFTDialogPayload
} from 'shared/types'
import { DecentralandInterface, IEvents } from 'decentraland-ecs/src/decentraland/Types'
import { defaultLogger } from 'shared/logger'

import { customEval, getES5Context } from './sdk/sandbox'
import { DevToolsAdapter } from './sdk/DevToolsAdapter'
import { ScriptingTransport, ILogOpts } from 'decentraland-rpc/src/common/json-rpc/types'
import { QueryType, CLASS_ID, Transform, Vector2 } from 'decentraland-ecs/src'
import { PB_Transform, PB_Vector3, PB_Quaternion } from '../shared/proto/engineinterface_pb'
import { worldToGrid } from 'atomicHelpers/parcelScenePositions'

// tslint:disable-next-line:whitespace
type IEngineAPI = import('shared/apis/EngineAPI').IEngineAPI

// tslint:disable-next-line:whitespace
type EnvironmentAPI = import('shared/apis/EnvironmentAPI').EnvironmentAPI

const dataUrlRE = /^data:[^/]+\/[^;]+;base64,/
const blobRE = /^blob:http/

const WEB3_PROVIDER = 'web3-provider'
const PROVIDER_METHOD = 'getProvider'

const pbTransform: PB_Transform = new PB_Transform()
const pbPosition: PB_Vector3 = new PB_Vector3()
const pbRotation: PB_Quaternion = new PB_Quaternion()
const pbScale: PB_Vector3 = new PB_Vector3()

function resolveMapping(mapping: string | undefined, mappingName: string, baseUrl: string) {
  let url = mappingName

  if (mapping) {
    url = mapping
  }

  if (dataUrlRE.test(url)) {
    return url
  }

  if (blobRE.test(url)) {
    return url
  }

  return (baseUrl.endsWith('/') ? baseUrl : baseUrl + '/') + url
}

const componentNameRE = /^(engine\.)/

export default class GamekitScene extends Script {
  @inject('EngineAPI')
  engine: IEngineAPI | null = null

  @inject('DevTools')
  devTools: any

  eventSubscriber!: EventSubscriber

  onUpdateFunctions: Array<(dt: number) => void> = []
  onStartFunctions: Array<Function> = []
  onEventFunctions: Array<(event: any) => void> = []
  events: EntityAction[] = []

  devToolsAdapter: DevToolsAdapter | null = null

  manualUpdate: boolean = false

  updateInterval: number = 1000 / 30

  didStart = false
  provider: any = null

  scenePosition: Vector2 = new Vector2()
  parcels: Array<{ x: number; y: number }> = []

  private allowOpenExternalUrl: boolean = false

  constructor(transport: ScriptingTransport, opt?: ILogOpts) {
    super(transport, opt)
  }

  onError(error: Error) {
    if (this.devToolsAdapter) {
      this.devToolsAdapter.error(error)
    } else {
      defaultLogger.error('', error)
    }
  }

  onLog(...messages: any[]) {
    if (this.devToolsAdapter) {
      this.devToolsAdapter.log(...messages)
    } else {
      defaultLogger.info('', ...messages)
    }
  }

  /**
   * Get a standard ethereum provider
   * Please notice this is highly experimental and might change in the future.
   *
   * method whitelist = [
   *   'eth_sendTransaction',
   *   'eth_getTransactionReceipt',
   *   'eth_estimateGas',
   *   'eth_call',
   *   'eth_getBalance',
   *   'eth_getStorageAt',
   *   'eth_blockNumber',
   *   'eth_getBlockByNumber',
   *   'eth_gasPrice',
   *   'eth_protocolVersion',
   *   'net_version',
   *   'web3_sha3',
   *   'web3_clientVersion',
   *   'eth_getTransactionCount'
   * ]
   */
  async getEthereumProvider() {
    const { EthereumController } = await this.loadAPIs(['EthereumController'])

    return {
      // @internal
      send(message: RPCSendableMessage, callback?: (error: Error | null, result?: any) => void): void {
        if (message && callback && callback instanceof Function) {
          EthereumController.sendAsync(message)
            .then((x: any) => callback(null, x))
            .catch(callback)
        } else {
          throw new Error('Decentraland provider only allows async calls')
        }
      },
      sendAsync(message: RPCSendableMessage, callback: (error: Error | null, result?: any) => void): void {
        EthereumController.sendAsync(message)
          .then((x: any) => callback(null, x))
          .catch(callback)
      }
    } as {
      send: Function
      sendAsync: Function
    }
  }

  async loadProject() {
    const { EnvironmentAPI } = (await this.loadAPIs(['EnvironmentAPI'])) as { EnvironmentAPI: EnvironmentAPI }
    const bootstrapData = await EnvironmentAPI.getBootstrapData()

    if (bootstrapData && bootstrapData.main) {
      const mappingName = bootstrapData.main
      const mapping = bootstrapData.mappings.find($ => $.file === mappingName)
      const url = resolveMapping(mapping && mapping.hash, mappingName, bootstrapData.baseUrl)
      const html = await fetch(url)

      if (html.ok) {
        return [bootstrapData, await html.text()] as const
      } else {
        throw new Error(`SDK: Error while loading ${url} (${mappingName} -> ${mapping})`)
      }
    }

    throw new Error(`No bootstrap data`)
  }

  fireEvent(event: any) {
    try {
      if (this.isPointerEvent(event)) {
        this.allowOpenExternalUrl = true
      }
      for (let trigger of this.onEventFunctions) {
        trigger(event)
      }
    } catch (e) {
      this.onError(e)
    }
    this.allowOpenExternalUrl = false
  }

  calculateSceneCenter(parcels: Array<{ x: number; y: number }>): Vector2 {
    let center: Vector2 = new Vector2()

    parcels.forEach(v2 => {
      center = Vector2.Add(v2, center)
    })

    center.x /= parcels.length
    center.y /= parcels.length

    return center
  }

  async systemDidEnable() {
    this.eventSubscriber = new EventSubscriber(this.engine as any)
    this.devToolsAdapter = new DevToolsAdapter(this.devTools)

    try {
      const [sceneData, source] = await this.loadProject()

      if (!source) {
        throw new Error('Received empty source.')
      }

      const that = this

      const fullData = sceneData.data as LoadableParcelScene
      const sceneId = fullData.id

      const dcl: DecentralandInterface = {
        DEBUG: true,
        log(...args) {
          // tslint:disable-next-line:no-console
          that.onLog(...args)
        },

        openExternalUrl(url: string) {
          if (that.allowOpenExternalUrl) {
            that.events.push({
              type: 'OpenExternalUrl',
              tag: '',
              payload: url
            })
          } else {
            this.error('openExternalUrl can only be used inside a pointerEvent')
          }
        },

        openNFTDialog(assetContractAddress: string, tokenId: string, comment: string | null) {
          if (that.allowOpenExternalUrl) {
            that.events.push({
              type: 'OpenNFTDialog',
              tag: '',
              payload: {
                assetContractAddress,
                tokenId,
                comment
              } as OpenNFTDialogPayload
            })
          } else {
            this.error('openNFTDialog can only be used inside a pointerEvent')
          }
        },

        addEntity(entityId: string) {
          if (entityId === '0') {
            // We dont create the entity 0 in the engine.
            return
          }
          that.events.push({
            type: 'CreateEntity',
            tag: entityId,
            payload: { id: entityId } as CreateEntityPayload
          })
        },

        removeEntity(entityId: string) {
          that.events.push({
            type: 'RemoveEntity',
            tag: entityId,
            payload: { id: entityId } as RemoveEntityPayload
          })
        },

        /** update tick */
        onUpdate(cb: (deltaTime: number) => void): void {
          if (typeof (cb as any) !== 'function') {
            that.onError(new Error('onUpdate must be called with only a function argument'))
          } else {
            that.onUpdateFunctions.push(cb)
          }
        },

        /** event from the engine */
        onEvent(cb: (event: any) => void): void {
          if (typeof (cb as any) !== 'function') {
            that.onError(new Error('onEvent must be called with only a function argument'))
          } else {
            that.onEventFunctions.push(cb)
          }
        },

        /** called after adding a component to the entity or after updating a component */
        updateEntityComponent(entityId: string, componentName: string, classId: number, json: string): void {
          if (componentNameRE.test(componentName)) {
            that.events.push({
              type: 'UpdateEntityComponent',
              tag: sceneId + '_' + entityId + '_' + classId,
              payload: {
                entityId,
                classId,
                name: componentName.replace(componentNameRE, ''),
                json: that.generatePBObject(classId, json)
              } as UpdateEntityComponentPayload
            })
          }
        },

        /** called after adding a DisposableComponent to the entity */
        attachEntityComponent(entityId: string, componentName: string, id: string): void {
          if (componentNameRE.test(componentName)) {
            that.events.push({
              type: 'AttachEntityComponent',
              tag: entityId,
              payload: {
                entityId,
                name: componentName.replace(componentNameRE, ''),
                id
              } as AttachEntityComponentPayload
            })
          }
        },

        /** call after removing a component from the entity */
        removeEntityComponent(entityId: string, componentName: string): void {
          if (componentNameRE.test(componentName)) {
            that.events.push({
              type: 'ComponentRemoved',
              tag: entityId,
              payload: {
                entityId,
                name: componentName.replace(componentNameRE, '')
              } as ComponentRemovedPayload
            })
          }
        },

        /** set a new parent for the entity */
        setParent(entityId: string, parentId: string): void {
          that.events.push({
            type: 'SetEntityParent',
            tag: entityId,
            payload: {
              entityId,
              parentId
            } as SetEntityParentPayload
          })
        },

        /** queries for a specific system with a certain query configuration */
        query(queryType: QueryType, payload: any) {
          that.events.push({
            type: 'Query',
            tag: sceneId + '_' + payload.queryId,
            payload: {
              queryId: queryType,
              payload
            } as QueryPayload
          })
        },

        /** subscribe to specific events, events will be handled by the onEvent function */
        subscribe(eventName: string): void {
          that.eventSubscriber.on(eventName, event => {
            that.fireEvent({ type: eventName, data: event.data })
          })
        },

        /** unsubscribe to specific event */
        unsubscribe(eventName: string): void {
          that.eventSubscriber.off(eventName)
        },

        componentCreated(id: string, componentName: string, classId: number) {
          if (componentNameRE.test(componentName)) {
            that.events.push({
              type: 'ComponentCreated',
              tag: id,
              payload: {
                id,
                classId,
                name: componentName.replace(componentNameRE, '')
              } as ComponentCreatedPayload
            })
          }
        },

        componentDisposed(id: string) {
          that.events.push({
            type: 'ComponentDisposed',
            tag: id,
            payload: { id } as ComponentDisposedPayload
          })
        },

        componentUpdated(id: string, json: string) {
          that.events.push({
            type: 'ComponentUpdated',
            tag: id,
            payload: {
              id,
              json
            } as ComponentUpdatedPayload
          })
        },

        loadModule: async _moduleName => {
          const moduleToLoad = _moduleName.replace(/^@decentraland\//, '')
          let methods: string[] = []

          if (moduleToLoad === WEB3_PROVIDER) {
            methods.push(PROVIDER_METHOD)
            this.provider = await this.getEthereumProvider()
          } else {
            const proxy = (await this.loadAPIs([moduleToLoad]))[moduleToLoad]

            try {
              methods = await proxy._getExposedMethods()
            } catch (e) {
              throw Object.assign(new Error(`Error getting the methods of ${moduleToLoad}: ` + e.message), {
                original: e
              })
            }
          }

          return {
            rpcHandle: moduleToLoad,
            methods: methods.map(name => ({ name }))
          }
        },
        callRpc: async (rpcHandle: string, methodName: string, args: any[]) => {
          if (rpcHandle === WEB3_PROVIDER && methodName === PROVIDER_METHOD) {
            return this.provider
          }

          const module = this.loadedAPIs[rpcHandle]
          if (!module) {
            throw new Error(`RPCHandle: ${rpcHandle} is not loaded`)
          }
          return module[methodName].apply(module, args)
        },
        onStart(cb: Function) {
          that.onStartFunctions.push(cb)
        },
        error(message, data) {
          that.onError(Object.assign(new Error(message), { data }))
        }
      }

      {
        const monkeyPatchDcl: any = dcl
        monkeyPatchDcl.updateEntity = function() {
          throw new Error('The scene is using an outdated version of decentraland-ecs, please upgrade to >5.0.0')
        }
      }

      this.eventSubscriber.once('sceneStart', () => {
        if (!this.manualUpdate) {
          this.startLoop()
        }

        this.onStartFunctions.forEach($ => {
          try {
            $()
          } catch (e) {
            this.onError(e)
          }
        })
      })

      if (sceneData.useFPSThrottling === true) {
        this.parcels = fullData.parcels
        if (this.parcels !== undefined) {
          this.scenePosition = this.calculateSceneCenter(this.parcels)
          this.setupFpsThrottling(dcl)
        }
      }

      try {
        await customEval((source as any) as string, getES5Context({ dcl }))

        this.events.push(this.initMessagesFinished())

        this.onStartFunctions.push(() => {
          const engine: IEngineAPI = this.engine as any
          engine.startSignal().catch((e: Error) => this.onError(e))
        })
      } catch (e) {
        that.onError(e)

        this.events.push(this.initMessagesFinished())
      }

      this.sendBatch()
    } catch (e) {
      this.onError(e)
      // unload should be triggered here
    } finally {
      this.didStart = true
    }
  }

  private setupFpsThrottling(dcl: DecentralandInterface) {
    dcl.subscribe('positionChanged')
    dcl.onEvent(event => {
      if (event.type !== 'positionChanged') {
        return
      }

      const e = event.data as IEvents['positionChanged']
      const playerPosition = worldToGrid(e.cameraPosition)

      if (playerPosition === undefined || this.scenePosition === undefined) {
        return
      }

      const playerPos = playerPosition as Vector2
      const scenePos = this.scenePosition as Vector2
      const distanceToPlayer = Vector2.Distance(playerPos, scenePos)

      let fps: number = 1
      const insideScene: boolean = this.parcels.some(e => e.x === playerPos.x && e.y === playerPos.y)

      if (insideScene) {
        fps = 30
      } else if (distanceToPlayer <= 2) {
        //NOTE(Brian): Yes, this could be a formula, but I prefer this pedestrian way as
        //             its easier to read and tweak (i.e. if we find out its better as some arbitrary curve, etc).
        fps = 20
      } else if (distanceToPlayer <= 3) {
        fps = 10
      } else if (distanceToPlayer <= 4) {
        fps = 5
      }

      this.updateInterval = 1000 / fps
    })
  }

  update(time: number) {
    for (let trigger of this.onUpdateFunctions) {
      try {
        trigger(time)
      } catch (e) {
        this.onError(e)
      }
    }

    this.sendBatch()
  }

  private initMessagesFinished(): EntityAction {
    return {
      type: 'InitMessagesFinished',
      tag: 'scene',
      payload: '{}'
    }
  }

  private sendBatch() {
    try {
      if (this.events.length) {
        const batch = this.events.slice()
        this.events.length = 0
        ;((this.engine as any) as IEngineAPI).sendBatch(batch).catch((e: Error) => this.onError(e))
      }
    } catch (e) {
      this.onError(e)
    }
  }

  private startLoop() {
    let start = performance.now()

    const update = () => {
      const now = performance.now()
      const dt = now - start
      start = now

      setTimeout(update, this.updateInterval)

      let time = dt / 1000

      this.update(time)
    }

    update()
  }

  private generatePBObject(classId: CLASS_ID, json: string): string {
    let data: string = json

    if (classId === CLASS_ID.TRANSFORM) {
      const transform: Transform = JSON.parse(json)

      pbPosition.setX(transform.position.x)
      pbPosition.setY(transform.position.y)
      pbPosition.setZ(transform.position.z)

      pbRotation.setX(transform.rotation.x)
      pbRotation.setY(transform.rotation.y)
      pbRotation.setZ(transform.rotation.z)
      pbRotation.setW(transform.rotation.w)

      pbScale.setX(transform.scale.x)
      pbScale.setY(transform.scale.y)
      pbScale.setZ(transform.scale.z)

      pbTransform.setPosition(pbPosition)
      pbTransform.setRotation(pbRotation)
      pbTransform.setScale(pbScale)

      let arrayBuffer: Uint8Array = pbTransform.serializeBinary()
      data = btoa(String.fromCharCode(...arrayBuffer))
    }

    return data
  }

  private isPointerEvent(event: any): boolean {
    switch (event.type) {
      case 'uuidEvent':
        return event.data.payload.buttonId !== undefined
    }
    return false
  }
}
