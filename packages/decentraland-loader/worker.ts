import { TransportBasedServer } from 'decentraland-rpc/lib/host/TransportBasedServer'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { WebWorkerTransport } from 'decentraland-rpc/lib/common/transports/WebWorker'

import { resolveUrl } from 'atomicHelpers/parseUrl'

import { ETHEREUM_NETWORK, getNetworkConfigurations, parcelLimits, DEBUG } from '../config'
import { error } from '../engine/logger'

const loaderWorkerRaw = require('raw-loader!../../static/systems/loader.system.js')
const loaderWorkerBLOB = new Blob([loaderWorkerRaw])
const loaderWorkerUrl = URL.createObjectURL(loaderWorkerBLOB)
const worker: Worker = new (Worker as any)(loaderWorkerUrl, { name: 'LoaderSystem' })

export class LandLoaderServer extends TransportBasedServer {
  constructor(transport: ScriptingTransport) {
    super(transport)
  }

  enable() {
    super.enable()
  }
}

export async function initParcelSceneWorker(network: ETHEREUM_NETWORK) {
  const server = new LandLoaderServer(WebWorkerTransport(worker))

  const ethConfig = getNetworkConfigurations(network)

  server.enable()

  server.notify('ETH.start', {
    content: ethConfig.content,
    rpcUrl: ethConfig.http,
    contractAddress: ethConfig.contractAddress,
    landApi: ethConfig.landApi,
    radius: parcelLimits.visibleRadius,

    // @ts-ignore
    contentServer: DEBUG ? resolveUrl(document.location.origin, '/local-ipfs') : ethConfig.content
  })

  const prev = worker.onerror

  worker.onerror = function(e) {
    error('Loader worker error', e)
    if (prev) prev.call(worker, e)
  }

  return {
    stop() {
      server.transport.close()
    },
    server
  }
}
