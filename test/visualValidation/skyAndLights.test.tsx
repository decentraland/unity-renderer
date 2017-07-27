import { loadTestParcel, saveScreenshot } from '../testHelpers'

const realX = -10
const realY = 810

loadTestParcel('Sky and Lights visual validation', -1, 81, function(root, futureParcelScene) {
  saveScreenshot(`skyAndLights.0.png`, {
    from: [realX + 10, 2, realY + 0],
    lookAt: [realX + 5, 2, realY + 5]
  })

  saveScreenshot(`skyAndLights.1.png`, {
    from: [realX + 10, 2, realY - 10],
    lookAt: [realX + 5, 2, realY + 5]
  })

  saveScreenshot(`skyAndLights.2.png`, {
    from: [realX + 90000000, 3, realY + 90000000],
    lookAt: [realX + 0, 0, realY + 0]
  })

  saveScreenshot(`skyAndLights.3.png`, {
    from: [realX + 0, 2, realY - 10],
    lookAt: [realX + 5, 2, realY + 5]
  })

  saveScreenshot(`skyAndLights.4.png`, {
    from: [realX - 90000000, 3, realY + 90000000],
    lookAt: [realX + 0, 0, realY + 0]
  })

  saveScreenshot(`skyAndLights.5.png`, {
    from: [realX - 90000000, 3, realY - 90000000],
    lookAt: [realX + 0, 0, realY + 0]
  })

  saveScreenshot(`skyAndLights.6.png`, {
    from: [realX + 90000000, 3, realY - 90000000],
    lookAt: [realX + 0, 0, realY + 0]
  })

  saveScreenshot(`skyAndLights.7.png`, {
    from: [realX - 90000000, 3, realY - 90000000],
    lookAt: [realX + 90000000, 0, realY + 0]
  })

  saveScreenshot(`skyAndLights.8.png`, {
    from: [realX + 90000000, 3, realY + 90000000],
    lookAt: [realX - 90000000, 0, realY - 90000000]
  })

  saveScreenshot(`sun.png`, {
    from: [realX + 0, 10, realY + 0],
    lookAt: [realX + 1000, 1000, realY + 1000]
  })
})
