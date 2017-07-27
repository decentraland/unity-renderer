import { loadTestParcel, saveScreenshot } from '../testHelpers'

loadTestParcel('L-shaped parcel visual validation', -1, -30, function() {
  saveScreenshot(`parcel-shape.png`, {
    from: [-25, 4, -285],
    lookAt: [5, 0, -315]
  })
})
