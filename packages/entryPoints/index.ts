import 'engine'

import { ETHEREUM_NETWORK, DEBUG } from '../config'
import { initBabylonClient } from '../dcl'
import { domReadyFuture, bodyReadyFuture } from '../engine/renderer/init'
import { initShared } from '../shared'
import { enableParcelSceneLoading } from '../shared/world/parcelSceneManager'
import { WebGLParcelScene } from '../dcl/WebGLParcelScene'
import { enableMiniMap } from '../dcl/widgets/minimap'

export async function loadClient(net: ETHEREUM_NETWORK) {
  await initBabylonClient()
  document.body.appendChild(enableMiniMap())

  await enableParcelSceneLoading(net, {
    parcelSceneClass: WebGLParcelScene,
    shouldLoadParcelScene: () => true
  })

  document.body.classList.remove('dcl-loading')
}

bodyReadyFuture
  .then(async body => {
    const { net } = await initShared()

    await loadClient(net)

    // Warn in case wallet is set in mainnet
    if (net === ETHEREUM_NETWORK.MAINNET && DEBUG) {
      const style = document.createElement('style') as HTMLStyleElement
      style.appendChild(
        document.createTextNode(
          `body:before{content:'You are using Mainnet Ethereum Network, real transactions are going to be made.';background:#ff0044;color:#fff;text-align:center;text-transform:uppercase;height:24px;width:100%;position:fixed;padding-top:2px}#main-canvas{padding-top:24px};`
        )
      )
      document.head.appendChild(style)
    }

    domReadyFuture.then(canvas => {
      body.appendChild(canvas)
    })
  })
  .catch(error => {
    document.body.classList.remove('dcl-loading')
    document.body.innerHTML = `<h3>${error.message}</h3>`
  })
