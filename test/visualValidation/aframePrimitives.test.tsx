import { loadTestParcel, saveScreenshot } from '../testHelpers'

const realX = -10
const realY = -270

loadTestParcel('Primitives visual validation', -1, -27, function(root, futureParcelScene) {
  saveScreenshot(`aframePrimitivesResult.0.png`, {
    from: [realX - 10, 10, realY - 10],
    lookAt: [realX + 10, 2, realY + 10]
  })
  saveScreenshot(`aframePrimitivesResult.1.png`, {
    from: [realX - 10, 10, realY + 20],
    lookAt: [realX + 10, 2, realY + 10]
  })
  saveScreenshot(`aframePrimitivesResult.2.png`, {
    from: [realX + 36, 10, realY - 10],
    lookAt: [realX + 10, 2, realY + 10]
  })
  saveScreenshot(`aframePrimitivesResult.3.png`, {
    from: [realX + 36, 10, realY + 20],
    lookAt: [realX + 10, 2, realY + 10]
  })
})
