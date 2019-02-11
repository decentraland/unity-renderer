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
  ComponentUpdatedPayload
} from 'shared/types'
import { DecentralandInterface } from 'decentraland-ecs/src/decentraland/Types'
import { customEval, getES5Context } from './sdk/sandbox'
import { DevToolsAdapter } from './sdk/DevToolsAdapter'

// tslint:disable-next-line:whitespace
type IEngineAPI = import('shared/apis/EngineAPI').IEngineAPI

// tslint:disable-next-line:whitespace
type EnvironmentAPI = import('shared/apis/EnvironmentAPI').EnvironmentAPI

const FPS = 30
const UPDATE_INTERVAL = 1000 / FPS
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

  updateInterval = UPDATE_INTERVAL
  devToolsAdapter: DevToolsAdapter | null = null

  manualUpdate: boolean = false

  didStart = false
  provider: any = null

  onError(error: Error) {
    if (this.devToolsAdapter) {
      this.devToolsAdapter.error(error)
    } else {
      // tslint:disable-next-line:no-console
      console.error(error)
    }
  }

  onLog(...messages: any[]) {
    if (this.devToolsAdapter) {
      this.devToolsAdapter.log(...messages)
    } else {
      // tslint:disable-next-line:no-console
      console.log(...messages)
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
        return html.text()
      } else {
        throw new Error(`SDK: Error while loading ${url} (${mappingName} -> ${mapping})`)
      }
    }
  }

  fireEvent(event: any) {
    try {
      for (let trigger of this.onEventFunctions) {
        trigger(event)
      }
    } catch (e) {
      this.onError(e)
    }
  }

  async systemDidEnable() {
    this.eventSubscriber = new EventSubscriber(this.engine as any)
    this.devToolsAdapter = new DevToolsAdapter(this.devTools)

    try {
      const source = await this.loadProject()

      if (!source) {
        throw new Error('Received empty source.')
      }

      const that = this

      const dcl: DecentralandInterface = {
        DEBUG: true,
        log(...args) {
          // tslint:disable-next-line:no-console
          that.onLog(...args)
        },

        addEntity(entityId: string) {
          if (entityId === '0') {
            // We dont create the entity 0 in the engine.
            return
          }
          that.events.push({
            type: 'CreateEntity',
            payload: JSON.stringify({ id: entityId } as CreateEntityPayload)
          })
        },

        removeEntity(entityId: string) {
          that.events.push({
            type: 'RemoveEntity',
            payload: JSON.stringify({ id: entityId } as RemoveEntityPayload)
          })
        },

        /** update tick */
        onUpdate(cb: (deltaTime: number) => void): void {
          if (typeof cb !== 'function') {
            that.onError(new Error('onUpdate must be called with only a function argument'))
          } else {
            that.onUpdateFunctions.push(cb)
          }
        },

        /** event from the engine */
        onEvent(cb: (event: any) => void): void {
          if (typeof cb !== 'function') {
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
              payload: JSON.stringify({
                entityId,
                classId,
                name: componentName.replace(componentNameRE, ''),
                json
              } as UpdateEntityComponentPayload)
            })
          }
        },

        /** called after adding a DisposableComponent to the entity */
        attachEntityComponent(entityId: string, componentName: string, id: string): void {
          if (componentNameRE.test(componentName)) {
            that.events.push({
              type: 'AttachEntityComponent',
              payload: JSON.stringify({
                entityId,
                name: componentName.replace(componentNameRE, ''),
                id
              } as AttachEntityComponentPayload)
            })
          }
        },

        /** call after removing a component from the entity */
        removeEntityComponent(entityId: string, componentName: string): void {
          if (componentNameRE.test(componentName)) {
            that.events.push({
              type: 'ComponentRemoved',
              payload: JSON.stringify({
                entityId,
                name: componentName.replace(componentNameRE, '')
              } as ComponentRemovedPayload)
            })
          }
        },

        /** set a new parent for the entity */
        setParent(entityId: string, parentId: string): void {
          that.events.push({
            type: 'SetEntityParent',
            payload: JSON.stringify({
              entityId,
              parentId
            } as SetEntityParentPayload)
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
              payload: JSON.stringify({
                id,
                classId,
                name: componentName.replace(componentNameRE, '')
              } as ComponentCreatedPayload)
            })
          }
        },

        componentDisposed(id: string) {
          that.events.push({
            type: 'ComponentDisposed',
            payload: JSON.stringify({
              id
            } as ComponentDisposedPayload)
          })
        },

        componentUpdated(id: string, json: string) {
          that.events.push({
            type: 'ComponentUpdated',
            payload: JSON.stringify({
              id,
              json
            } as ComponentUpdatedPayload)
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

      try {
        await customEval((source as any) as string, getES5Context({ dcl }))

        if (!this.manualUpdate) {
          this.startLoop()
        }

        this.onStartFunctions.push(() => {
          const engine: IEngineAPI = this.engine as any
          engine.startSignal().catch((e: Error) => this.onError(e))
        })
      } catch (e) {
        that.onError(e)
      }

      this.sendBatch()

      setTimeout(() => {
        this.onStartFunctions.forEach($ => {
          try {
            $()
          } catch (e) {
            that.onError(e)
          }
        })
        // TODO: review this timeout
      }, 5000)
    } catch (e) {
      this.onError(e)
      // unload should be triggered here
    } finally {
      this.didStart = true
    }
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
}
