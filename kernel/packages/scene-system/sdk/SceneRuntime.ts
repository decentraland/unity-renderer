import { Script, inject, EventSubscriber } from 'decentraland-rpc'

import { Vector2 } from 'decentraland-ecs/src'
import { worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { sleep } from 'atomicHelpers/sleep'
import future, { IFuture } from 'fp-future'

import type { ScriptingTransport, ILogOpts } from 'decentraland-rpc/src/common/json-rpc/types'
import type { QueryType } from 'decentraland-ecs'
import type { DecentralandInterface, IEvents } from 'decentraland-ecs/src/decentraland/Types'
import type { IEngineAPI } from 'shared/apis/IEngineAPI'
import type { EnvironmentAPI } from 'shared/apis/EnvironmentAPI'
import type {
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
import { generatePBObject } from './Utils'

const dataUrlRE = /^data:[^/]+\/[^;]+;base64,/
const blobRE = /^blob:http/

const WEB3_PROVIDER = 'web3-provider'
const PROVIDER_METHOD = 'getProvider'

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

// NOTE(Brian): The idea is to map all string ids used by this scene to ints
//              so we avoid sending/processing big ids like "xxxxx-xxxxx-xxxxx-xxxxx"
//              that are used by i.e. raycasting queries.
let idToNumberStore: Record<string, number> = {}
let numberToIdStore: Record<number, string> = {}
let idToNumberStoreCounter: number = 10 // Starting in 10, to leave room for special cases (such as the root entity)

function addIdToStorage(id: string, idAsNumber: number) {
  idToNumberStore[id] = idAsNumber
  numberToIdStore[idAsNumber] = id
}

function getIdAsNumber(id: string): number {
  if (!idToNumberStore.hasOwnProperty(id)) {
    idToNumberStoreCounter++
    addIdToStorage(id, idToNumberStoreCounter)
    return idToNumberStoreCounter
  } else {
    return idToNumberStore[id]
  }
}

const componentNameRE = /^(engine\.)/

export abstract class SceneRuntime extends Script {
  @inject('EngineAPI')
  engine: IEngineAPI | null = null

  eventSubscriber!: EventSubscriber

  onUpdateFunctions: Array<(dt: number) => void> = []
  onStartFunctions: Array<Function> = []
  onEventFunctions: Array<(event: any) => void> = []
  events: EntityAction[] = []

  manualUpdate: boolean = false

  updateInterval: number = 1000 / 30

  didStart = false
  provider: any = null

  scenePosition: Vector2 = new Vector2()
  parcels?: Array<{ x: number; y: number }> = []

  isPreview: boolean = false

  private allowOpenExternalUrl: boolean = false

  constructor(transport: ScriptingTransport, opt?: ILogOpts) {
    super(transport, opt)
  }

  abstract async runCode(source: string, env: any): Promise<void>
  abstract onError(error: Error): void
  abstract onLog(...messages: any[]): void
  abstract startLoop(): void

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
    this.isPreview = await EnvironmentAPI.isPreviewMode()

    if (bootstrapData && bootstrapData.main) {
      const mappingName = bootstrapData.main
      const mapping = bootstrapData.mappings.find(($) => $.file === mappingName)
      const url = resolveMapping(mapping && mapping.hash, mappingName, bootstrapData.baseUrl)
      const codeRequest = await fetch(url)

      if (codeRequest.ok) {
        return [bootstrapData, await codeRequest.text()] as const
      } else {
        // even though the loading failed, we send the message initMessagesFinished to not block loading
        // in spawning points
        this.events.push(this.initMessagesFinished())
        this.sendBatch()
        throw new Error(`SDK: Error while loading ${url} (${mappingName} -> ${mapping}) the mapping was not found`)
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

    parcels.forEach((v2) => {
      center = Vector2.Add(v2, center)
    })

    center.x /= parcels.length
    center.y /= parcels.length

    return center
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

  async systemDidEnable() {
    this.eventSubscriber = new EventSubscriber(this.engine as any)

    try {
      const [sceneData, source] = await this.loadProject()

      if (!source) {
        throw new Error('Received empty source.')
      }

      const that = this

      const fullData = sceneData.data as LoadableParcelScene
      const sceneId = fullData.id

      const loadingModules: Record<string, IFuture<void>> = {}

      const dcl: DecentralandInterface = {
        DEBUG: true,
        log(...args) {
          // tslint:disable-next-line:no-console
          that.onLog(...args)
        },

        openExternalUrl(url: string) {
          if (JSON.stringify(url).length > 49000) {
            that.onError(new Error('URL payload cannot exceed 49.000 bytes'))
            return
          }

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
            let payload = { assetContractAddress, tokenId, comment }

            if (JSON.stringify(payload).length > 49000) {
              that.onError(new Error('OpenNFT payload cannot exceed 49.000 bytes'))
              return
            }

            that.events.push({
              type: 'OpenNFTDialog',
              tag: '',
              payload: payload as OpenNFTDialogPayload
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
            payload: { id: entityId } as CreateEntityPayload
          })
        },

        removeEntity(entityId: string) {
          that.events.push({
            type: 'RemoveEntity',
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
          if (json.length > 49000) {
            that.onError(new Error('Component payload cannot exceed 49.000 bytes'))
            return
          }

          if (componentNameRE.test(componentName)) {
            that.events.push({
              type: 'UpdateEntityComponent',
              tag: sceneId + '_' + entityId + '_' + classId,
              payload: {
                entityId,
                classId,
                name: componentName.replace(componentNameRE, ''),
                json: generatePBObject(classId, json)
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
          payload.queryId = getIdAsNumber(payload.queryId).toString()
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
          that.eventSubscriber.on(eventName, (event) => {
            if (eventName === 'raycastResponse') {
              let idAsNumber = parseInt(event.data.queryId, 10)
              if (numberToIdStore[idAsNumber]) {
                event.data.queryId = numberToIdStore[idAsNumber].toString()
              }
            }
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
          if (json.length > 49000) {
            that.onError(new Error('Component payload cannot exceed 49.000 bytes'))
            return
          }

          that.events.push({
            type: 'ComponentUpdated',
            tag: id,
            payload: {
              id,
              json
            } as ComponentUpdatedPayload
          })
        },

        loadModule: async (_moduleName) => {
          const loadingModule: IFuture<void> = future()
          loadingModules[_moduleName] = loadingModule

          try {
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
              methods: methods.map((name) => ({ name }))
            }
          } finally {
            loadingModule.resolve()
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
        monkeyPatchDcl.updateEntity = function () {
          throw new Error('The scene is using an outdated version of decentraland-ecs, please upgrade to >5.0.0')
        }
      }

      this.eventSubscriber.once('sceneStart', () => {
        if (!this.manualUpdate) {
          this.startLoop()
        }

        this.onStartFunctions.forEach(($) => {
          try {
            $()
          } catch (e) {
            this.onError(e)
          }
        })
      })

      if (sceneData.useFPSThrottling === true) {
        this.parcels = fullData.parcels
        if (this.parcels) {
          this.scenePosition = this.calculateSceneCenter(this.parcels)
          this.setupFpsThrottling(dcl)
        }
      }

      try {
        await this.runCode((source as any) as string, { dcl })

        let modulesNotLoaded: string[] = []

        const timeout = sleep(10000).then(() => {
          modulesNotLoaded = Object.keys(loadingModules).filter((it) => loadingModules[it].isPending)
        })

        await Promise.race([Promise.all(Object.values(loadingModules)), timeout])

        if (modulesNotLoaded.length > 0) {
          this.onLog(
            `Timed out loading modules!. The scene ${sceneId} may not work correctly. Modules not loaded: ${modulesNotLoaded}`
          )
        }

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
    dcl.onEvent((event) => {
      if (event.type !== 'positionChanged') {
        return
      }

      const e = event.data as IEvents['positionChanged']
      const playerPosition = worldToGrid(e.cameraPosition)

      // @ts-ignore
      if (playerPosition === undefined || this.scenePosition === undefined) {
        return
      }

      const playerPos = playerPosition as Vector2
      const scenePos = this.scenePosition
      const distanceToPlayer = Vector2.Distance(playerPos, scenePos)

      let fps: number = 1
      const insideScene: boolean =
        !!this.parcels && this.parcels.some((e) => e.x === playerPos.x && e.y === playerPos.y)

      if (insideScene) {
        fps = 30
      } else if (distanceToPlayer <= 2) {
        // NOTE(Brian): Yes, this could be a formula, but I prefer this pedestrian way as
        //              its easier to read and tweak (i.e. if we find out its better as some arbitrary curve, etc).
        fps = 20
      } else if (distanceToPlayer <= 3) {
        fps = 10
      } else if (distanceToPlayer <= 4) {
        fps = 5
      }

      this.updateInterval = 1000 / fps
    })
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

  private isPointerEvent(event: any): boolean {
    switch (event.type) {
      case 'uuidEvent':
        return event.data.payload.buttonId !== undefined
    }
    return false
  }
}
