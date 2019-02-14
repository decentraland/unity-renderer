import { loadTestParcel, saveScreenshot, wait } from '../testHelpers'

loadTestParcel('text', -100, 112, () => {
  wait(1000)
  saveScreenshot(`text.png`, {
    from: [-99.5, 2, 112.0],
    lookAt: [-99.5, 3, 112.5]
  })
})
