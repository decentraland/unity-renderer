import { loadTestParcel, saveScreenshot, wait } from '../testHelpers'

loadTestParcel('lookAt', -100, 110, () => {
  wait(1000)
  saveScreenshot(`lookAt.png`, {
    from: [-99.5, 1, 109],
    lookAt: [-99.5, 6, 110.5]
  })
})
