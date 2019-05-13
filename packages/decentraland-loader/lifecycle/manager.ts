// This gets executed from the main thread and serves as an interface
// to communicate with the Lifecycle worker, so it's a "Server" in terms of decentraland-rpc

import future, { IFuture } from 'fp-future'
import { TransportBasedServer } from 'decentraland-rpc/lib/host/TransportBasedServer'
import { WebWorkerTransport } from 'decentraland-rpc/lib/common/transports/WebWorker'

import { resolveUrl } from 'atomicHelpers/parseUrl'
import { error } from 'util'
import { ILand } from 'shared/types'

import { DEBUG, parcelLimits, getServerConfigurations } from '../../config'

/*
 * The worker is set up on the first require of this file
 */
const lifecycleWorkerRaw = require('raw-loader!../../../static/loader/lifecycle/worker.js')
const lifecycleWorkerUrl = URL.createObjectURL(new Blob([lifecycleWorkerRaw]))
const worker: Worker = new (Worker as any)(lifecycleWorkerUrl, { name: 'LifecycleWorker' })
worker.onerror = e => error('Loader worker error', e)

export class LifecycleManager extends TransportBasedServer {
  sceneCIDToRequest: { [key: string]: IFuture<ILand> } = {}
  enable() {
    super.enable()
    this.on('Scene.dataResponse', (scene: { data: ILand }) => {
      if (scene.data) {
        const sceneCIDArray = scene.data.mappingsResponse.contents.filter($ => $.file === 'scene.json')
        if (sceneCIDArray.length) {
          this.sceneCIDToRequest[sceneCIDArray[0].hash].resolve(scene.data)
        }
      }
    })
  }

  getParcelData(sceneCID: string) {
    if (!this.sceneCIDToRequest[sceneCID]) {
      this.sceneCIDToRequest[sceneCID] = future<ILand>()
      this.notify('Scene.dataRequest', { sceneCID })
    }
    return this.sceneCIDToRequest[sceneCID]
  }
}

let server: LifecycleManager

export const getServer = () => server

export async function initParcelSceneWorker(network: string) {
  server = new LifecycleManager(WebWorkerTransport(worker))

  server.enable()

  server.notify('Lifecycle.initialize', {
    contentServer: DEBUG ? resolveUrl(document.location.origin, '/local-ipfs') : getServerConfigurations().content,
    lineOfSightRadius: parcelLimits.visibleRadius
  })

  return server
}
