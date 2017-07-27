import { loadTestParcel, saveScreenshot, wait } from '../testHelpers'

loadTestParcel('text', -100, 112, () => {
  wait(1000)
  saveScreenshot(`text.png`, {
    from: [-995, 2, 1120],
    lookAt: [-995, 3, 1125]
  })
})
