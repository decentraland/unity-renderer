// This gets executed from the main thread and serves as an interface
// to communicate with the Lifecycle worker, so it's a "Server" in terms of decentraland-rpc

import future, { IFuture } from 'fp-future'

import { TransportBasedServer } from 'decentraland-rpc/lib/host/TransportBasedServer'
import { WebWorkerTransport } from 'decentraland-rpc/lib/common/transports/WebWorker'

import { resolveUrl } from 'atomicHelpers/parseUrl'

import { DEBUG, parcelLimits, getServerConfigurations, ENABLE_EMPTY_SCENES, LOS, PIN_CATALYST } from 'config'

import { ILand } from 'shared/types'
import { getFetchContentServer, getCatalystServer, getFetchMetaContentService } from 'shared/dao/selectors'
import defaultLogger from 'shared/logger'
import { StoreContainer } from 'shared/store/rootTypes'

declare const globalThis: StoreContainer & { workerManager: LifecycleManager }

/*
 * The worker is set up on the first require of this file
 */
const lifecycleWorkerRaw = require('raw-loader!../../../static/loader/lifecycle/worker.js')
const lifecycleWorkerUrl = URL.createObjectURL(new Blob([lifecycleWorkerRaw]))
const worker: Worker = new (Worker as any)(lifecycleWorkerUrl, { name: 'LifecycleWorker' })
worker.onerror = (e) => defaultLogger.error('Loader worker error', e)

export class LifecycleManager extends TransportBasedServer {
  sceneIdToRequest: Map<string, IFuture<ILand>> = new Map()
  positionToRequest: Map<string, IFuture<string>> = new Map()

  enable() {
    super.enable()
    this.on('Scene.dataResponse', (scene: { data: ILand }) => {
      if (scene.data) {
        const future = this.sceneIdToRequest.get(scene.data.sceneId)

        if (future) {
          future.resolve(scene.data)
        }
      }
    })

    this.on('Scene.idResponse', (scene: { position: string; data: string }) => {
      const future = this.positionToRequest.get(scene.position)

      if (future) {
        future.resolve(scene.data)
      }
    })
  }

  getParcelData(sceneId: string) {
    let theFuture = this.sceneIdToRequest.get(sceneId)
    if (!theFuture) {
      theFuture = future<ILand>()
      this.sceneIdToRequest.set(sceneId, theFuture)
      this.notify('Scene.dataRequest', { sceneId })
    }
    return theFuture
  }

  getSceneIds(parcels: string[]): Promise<string | null>[] {
    const futures: IFuture<string>[] = []
    const missing: string[] = []

    for (let id of parcels) {
      let theFuture = this.positionToRequest.get(id)

      if (!theFuture) {
        theFuture = future<string>()
        this.positionToRequest.set(id, theFuture)

        missing.push(id)
      }

      futures.push(theFuture)
    }

    this.notify('Scene.idRequest', { sceneIds: missing })
    return futures
  }

  async reloadScene(sceneId: string) {
    const landFuture = this.sceneIdToRequest.get(sceneId)
    if (landFuture) {
      const land = await landFuture
      const parcels = land.sceneJsonData.scene.parcels
      for (let parcel of parcels) {
        this.positionToRequest.delete(parcel)
      }
      this.notify('Scene.reload', { sceneId })
    }
  }
}

let server: LifecycleManager

export const getServer = () => server

export async function initParcelSceneWorker() {
  server = new LifecycleManager(WebWorkerTransport(worker))

  globalThis.workerManager = server

  server.enable()

  const state = globalThis.globalStore.getState()
  const localServer = resolveUrl(`${location.protocol}//${location.hostname}:${8080}`, '/local-ipfs')

  // NOTE(Brian): In branch urls we can't just use location.source - the value returned doesn't include
  //              the branch full path! With this, we ensure the /branch/<branch-name> is included in the root url.
  //              This is used for empty parcels and should be used for fetching any other local resource.
  const fullRootUrl = `${location.protocol}//${location.host}${location.pathname}`.replace('index.html', '')

  server.notify('Lifecycle.initialize', {
    contentServer: DEBUG ? localServer : getFetchContentServer(state),
    catalystServer: DEBUG ? localServer : getCatalystServer(state),
    metaContentService: DEBUG ? localServer : getFetchMetaContentService(state),
    contentServerBundles: DEBUG || PIN_CATALYST ? '' : getServerConfigurations().contentAsBundle + '/',
    rootUrl: fullRootUrl,
    lineOfSightRadius: LOS ? Number.parseInt(LOS, 10) : parcelLimits.visibleRadius,
    secureRadius: parcelLimits.secureRadius,
    emptyScenes: ENABLE_EMPTY_SCENES && !(globalThis as any)['isRunningTests'],
    worldConfig: globalThis.globalStore.getState().meta.config.world
  })

  return server
}
