import { loadTestParcel, saveScreenshot, wait } from '../testHelpers'

describe('gamekit', () => {
  loadTestParcel('Shape primitives', -100, 106, function(root, futureParcelScene) {
    wait(3000)
    saveScreenshot(`gamekit-primitives.png`, {
      from: [-100.0, 1.6, 105.9],
      lookAt: [-99.6, 1, 106.5]
    })
  })
})
