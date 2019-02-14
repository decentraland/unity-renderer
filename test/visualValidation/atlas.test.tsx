import { loadTestParcel, saveScreenshot } from '../testHelpers'

loadTestParcel('Atlas visual validation', -100, 105, function(root, futureParcelScene) {
  saveScreenshot(`atlas.png`, {
    from: [-99.5, 1.6, 105.0],
    lookAt: [-99.5, 1, 105.5]
  })
})
