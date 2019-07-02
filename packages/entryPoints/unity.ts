import { initializeUnity } from '../unity-interface/initializer'
import { startUnityParcelLoading } from '../unity-interface/dcl'

const container = document.getElementsByTagName('body')[0]

if (!container) throw new Error('cannot find element #gameContainer')

initializeUnity(container)
  .then(async _ => {
    await startUnityParcelLoading()

    document.body.classList.remove('dcl-loading')
  })
  .catch(err => {
    if (err.message.includes('Authentication error')) {
      window.location.reload()
    }

    console['error']('Error loading Unity')
    console['error'](err)

    container.innerText = err.toString()

    document.body.classList.remove('dcl-loading')
  })
