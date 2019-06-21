import { initializeUnity } from '../unity-interface/initializer'
import { startUnityParcelLoading } from '../unity-interface/dcl'

const container = document.getElementById('gameContainer')

if (!container) throw new Error('cannot find element #gameContainer')

initializeUnity(container)
  .then(async ret => {
    await startUnityParcelLoading(ret.net)

    document.body.classList.remove('dcl-loading')
  })
  .catch(err => {
    console['error']('Error loading Unity')
    console['error'](err)

    document.body.classList.remove('dcl-loading')
  })
