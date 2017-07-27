import { loadTestParcel, saveScreenshot, wait } from '../testHelpers'

loadTestParcel('lookAt', -100, 110, () => {
  wait(1000)
  saveScreenshot(`lookAt.png`, {
    from: [-995, 1, 1090],
    lookAt: [-995, 6, 1105]
  })
})
