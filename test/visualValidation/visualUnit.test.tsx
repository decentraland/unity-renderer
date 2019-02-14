import { loadTestParcel, saveScreenshot, wait } from '../testHelpers'

loadTestParcel('transparent material', -100, 114, function(root, futureParcelScene) {
  wait(4000)

  saveScreenshot(`transparent.png`, {
    from: [-99.8, 1.6, 114.6],
    lookAt: [-99.6, 1.6, 114.5]
  })

  saveScreenshot(`transparent.2.png`, {
    from: [-99.2, 1.6, 114.6],
    lookAt: [-99.6, 1.6, 114.5]
  })
})
