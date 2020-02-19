// This gets executed from the main thread and serves as an interface
// to communicate with the Lifecycle worker, so it's a "Server" in terms of decentraland-rpc

import future, { IFuture } from 'fp-future'
import { TransportBasedServer } from 'decentraland-rpc/lib/host/TransportBasedServer'
import { WebWorkerTransport } from 'decentraland-rpc/lib/common/transports/WebWorker'

import { resolveUrl } from 'atomicHelpers/parseUrl'
import { error } from 'util'
import { ILand } from 'shared/types'

import { DEBUG, parcelLimits, getServerConfigurations, ENABLE_EMPTY_SCENES, tutorialSceneEnabled } from '../../config'
import { getFetchContentServer } from '../../shared/dao/selectors'
import { Store } from 'redux'

/*
 * The worker is set up on the first require of this file
 */
const lifecycleWorkerRaw = require('raw-loader!../../../static/loader/lifecycle/worker.js')
const lifecycleWorkerUrl = URL.createObjectURL(new Blob([lifecycleWorkerRaw]))
const worker: Worker = new (Worker as any)(lifecycleWorkerUrl, { name: 'LifecycleWorker' })
worker.onerror = e => error('Loader worker error', e)

export class LifecycleManager extends TransportBasedServer {
  sceneIdToRequest: Map<string, IFuture<ILand>> = new Map()
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
}

let server: LifecycleManager

export const getServer = () => server

declare const window: Window & { globalStore: Store }

export async function initParcelSceneWorker() {
  server = new LifecycleManager(WebWorkerTransport(worker))

  server.enable()

  server.notify('Lifecycle.initialize', {
    contentServer: DEBUG
      ? resolveUrl(document.location.origin, '/local-ipfs')
      : getFetchContentServer(window.globalStore.getState()),
    contentServerBundles: DEBUG ? '' : getServerConfigurations().contentAsBundle,
    lineOfSightRadius: parcelLimits.visibleRadius,
    secureRadius: parcelLimits.secureRadius,
    emptyScenes: ENABLE_EMPTY_SCENES && !(globalThis as any)['isRunningTests'],
    tutorialBaseURL: getTutorialBaseURL(),
    tutorialSceneEnabled: tutorialSceneEnabled()
  })

  return server
}

function getTutorialBaseURL() {
  let pathName = location.pathname.split('/')
  if (pathName[pathName.length - 1].includes('.')) {
    pathName.pop()
  }

  const basePath = origin + pathName.join('/')
  if (basePath.endsWith('/')) return basePath.slice(0, -1)
  return basePath
}
