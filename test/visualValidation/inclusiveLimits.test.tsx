import { loadTestParcel, saveScreenshot, wait } from '../testHelpers'

loadTestParcel('inclusiveLimits', 200, 90, () => {
  wait(1000)
  saveScreenshot(`inclusiveLimits.png`, {
    from: [2003, 1, 890],
    lookAt: [2008, 1, 905]
  })
})
