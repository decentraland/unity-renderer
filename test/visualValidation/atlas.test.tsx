import { loadTestParcel, saveScreenshot } from '../testHelpers'

loadTestParcel('Atlas visual validation', -100, 105, function(root, futureParcelScene) {
  saveScreenshot(`atlas.png`, {
    from: [-995, 1.6, 1050],
    lookAt: [-995, 1, 1055]
  })
})
