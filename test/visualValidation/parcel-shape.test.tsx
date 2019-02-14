import { loadTestParcel, saveScreenshot } from '../testHelpers'

loadTestParcel('L-shaped parcel visual validation', -1, -30, function() {
  saveScreenshot(`parcel-shape.png`, {
    from: [-2.5, 4, -28.5],
    lookAt: [0.5, 0, -31.5]
  })
})
