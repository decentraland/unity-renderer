import { loadTestParcel, saveScreenshot } from '../testHelpers'

const realX = -1.0
const realY = 82.0

loadTestParcel('Reference cubes', -1, 82, function() {
  saveScreenshot(`referenceSystem.3.png`, {
    from: [realX + 0.15, 7, realY - 0.5],
    lookAt: [realX + 0.15, 5, realY]
  })
})
