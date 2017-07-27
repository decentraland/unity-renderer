import { loadTestParcel, saveScreenshot } from '../testHelpers'

const realX = -10
const realY = 820

loadTestParcel('Reference cubes', -1, 82, function() {
  saveScreenshot(`referenceSystem.3.png`, {
    from: [realX + 1.5, 7, realY - 5],
    lookAt: [realX + 1.5, 5, realY]
  })
})
