import { loadTestParcel, saveScreenshot, wait } from '../testHelpers'

loadTestParcel('inclusiveLimits', 200, 90, () => {
  wait(1000)
  saveScreenshot(`inclusiveLimits.png`, {
    from: [200.3, 1, 89.0],
    lookAt: [200.8, 1, 90.5]
  })
})
