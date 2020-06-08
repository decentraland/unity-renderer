// This gets executed from the main thread and serves as an interface
// to communicate with the Lifecycle worker, so it's a "Server" in terms of decentraland-rpc

import future, { IFuture } from 'fp-future'
import { TransportBasedServer } from 'decentraland-rpc/lib/host/TransportBasedServer'
import { WebWorkerTransport } from 'decentraland-rpc/lib/common/transports/WebWorker'

import { resolveUrl } from 'atomicHelpers/parseUrl'
import { ILand } from 'shared/types'

import { DEBUG, parcelLimits, getServerConfigurations, ENABLE_EMPTY_SCENES, tutorialSceneEnabled } from '../../config'
import { getFetchContentServer, getFetchMetaContentServer, getFetchMetaContentService } from 'shared/dao/selectors'
import { Store } from 'redux'

import { getTutorialBaseURL } from 'shared/location'
import defaultLogger from 'shared/logger'

/*
 * The worker is set up on the first require of this file
 */
const lifecycleWorkerRaw = require('raw-loader!../../../static/loader/lifecycle/worker.js')
const lifecycleWorkerUrl = URL.createObjectURL(new Blob([lifecycleWorkerRaw]))
const worker: Worker = new (Worker as any)(lifecycleWorkerUrl, { name: 'LifecycleWorker' })
worker.onerror = e => defaultLogger.error('Loader worker error', e)

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

  getSceneIds(sceneIds: string[]): Promise<string | null>[] {
    const futures: IFuture<string>[] = []
    const missing: string[] = []

    for (let id of sceneIds) {
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
}

let server: LifecycleManager

export const getServer = () => server

declare const window: Window & { globalStore: Store; workerManager: any }

export async function initParcelSceneWorker() {
  server = new LifecycleManager(WebWorkerTransport(worker))
  window.workerManager = server

  server.enable()

  server.notify('Lifecycle.initialize', {
    contentServer: DEBUG
      ? resolveUrl(document.location.origin, '/local-ipfs')
      : getFetchContentServer(window.globalStore.getState()),
    metaContentServer: DEBUG
      ? resolveUrl(document.location.origin, '/local-ipfs')
      : getFetchMetaContentServer(window.globalStore.getState()),
    metaContentService: DEBUG
      ? resolveUrl(document.location.origin, '/local-ipfs')
      : getFetchMetaContentService(window.globalStore.getState()),
    contentServerBundles: DEBUG ? '' : getServerConfigurations().contentAsBundle + '/',
    lineOfSightRadius: parcelLimits.visibleRadius,
    secureRadius: parcelLimits.secureRadius,
    emptyScenes: ENABLE_EMPTY_SCENES && !(globalThis as any)['isRunningTests'],
    tutorialBaseURL: getTutorialBaseURL(),
    tutorialSceneEnabled: tutorialSceneEnabled()
  })

  return server
}
