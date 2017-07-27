import { loadTestParcel, saveScreenshot, wait } from '../testHelpers'

describe('gamekit', () => {
  loadTestParcel('Shape primitives', -100, 106, function(root, futureParcelScene) {
    wait(3000)
    saveScreenshot(`gamekit-primitives.png`, {
      from: [-1000, 1.6, 1059],
      lookAt: [-996, 1, 1065]
    })
  })
})
