// this is because web3 requires window. jesus..
// tslint:disable-next-line:semicolon
;(global as any)['window'] = self

import { WebWorkerTransport } from 'decentraland-rpc'
import { error } from 'engine/logger'

import { Adapter } from './src/adapter'
import { enableParcelSceneLoading, setUserPosition } from './src/parcelSceneManager'
import { ParcelScene } from './src/parcelScene'
import { InitializationOptions, configure } from './src/config'

const connector = new Adapter(WebWorkerTransport(self as any))

{
  connector.on('ETH.start', (opt: InitializationOptions) => {
    initialize(opt).catch($ => error('error initializing decentraland-loader', $))
  })

  connector.on('User.setPosition', (opt: { position: { x: number; y: number } }) => {
    setUserPosition(opt.position.x, opt.position.y)
  })
}

async function initialize(opt: InitializationOptions) {
  configure(opt, async function(parcelScenes: ParcelScene[]) {
    connector.notify('ParcelScenes.notify', { parcelScenes: parcelScenes.filter($ => !!$.data).map($ => $.data) })
  })

  await enableParcelSceneLoading()
}
