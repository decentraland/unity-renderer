import { loadTestParcel, saveScreenshot, wait } from '../testHelpers'

loadTestParcel('transparent material', -100, 114, function(root, futureParcelScene) {
  wait(4000)

  saveScreenshot(`transparent.png`, {
    from: [-998, 1.6, 1146],
    lookAt: [-996, 1.6, 1145]
  })

  saveScreenshot(`transparent.2.png`, {
    from: [-992, 1.6, 1146],
    lookAt: [-996, 1.6, 1145]
  })
})
