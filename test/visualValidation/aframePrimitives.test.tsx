import { loadTestParcel, saveScreenshot } from '../testHelpers'

const realX = -1.0
const realY = -27.0

loadTestParcel('Primitives visual validation', -1, -27, function(root, futureParcelScene) {
  saveScreenshot(`aframePrimitivesResult.0.png`, {
    from: [realX - 1, 10, realY - 1.0],
    lookAt: [realX + 1, 2, realY + 1.0]
  })
  saveScreenshot(`aframePrimitivesResult.1.png`, {
    from: [realX - 1, 10, realY + 2.0],
    lookAt: [realX + 1, 2, realY + 1.0]
  })
  saveScreenshot(`aframePrimitivesResult.2.png`, {
    from: [realX + 3.6, 10, realY - 1.0],
    lookAt: [realX + 1, 2, realY + 1.0]
  })
  saveScreenshot(`aframePrimitivesResult.3.png`, {
    from: [realX + 3.6, 10, realY + 2.0],
    lookAt: [realX + 1.0, 2, realY + 1.0]
  })
})
