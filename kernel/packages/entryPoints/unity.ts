import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { NOT_INVITED, AUTH_ERROR_LOGGED_OUT, FAILED_FETCHING_UNITY } from 'shared/loading/types'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'
import { NO_MOTD, OPEN_AVATAR_EDITOR } from '../config/index'
import { experienceStarted } from '../shared/loading/types'
import defaultLogger from '../shared/logger'
import { signalRendererInitialized } from '../shared/renderer/actions'
import { lastPlayerPosition, teleportObservable } from '../shared/world/positionThings'
import { hasWallet, startUnityParcelLoading, unityInterface } from '../unity-interface/dcl'
import { initializeUnity } from '../unity-interface/initializer'

const container = document.getElementById('gameContainer')

declare var global: any

if (!container) throw new Error('cannot find element #gameContainer')

initializeUnity(container)
  .then(async _ => {
    const i = unityInterface

    i.ConfigureMinimapHUD({ active: true, visible: true })
    i.ConfigureAvatarHUD({ active: true, visible: true })
    i.ConfigureNotificationHUD({ active: true, visible: true })
    i.ConfigureAvatarEditorHUD({ active: true, visible: OPEN_AVATAR_EDITOR })
    i.ConfigureSettingsHUD({ active: true, visible: false })
    i.ConfigureExpressionsHUD({ active: true, visible: true })
    i.ConfigurePlayerInfoCardHUD({ active: true, visible: true })
    i.ConfigureAirdroppingHUD({ active: true, visible: true })
    i.ConfigureTermsOfServiceHUD({ active: true, visible: true })

    global['globalStore'].dispatch(signalRendererInitialized())
    await startUnityParcelLoading()

    if (!NO_MOTD) {
      i.ConfigureWelcomeHUD({ active: true, visible: true, hasWallet: hasWallet })
    }

    _.instancedJS
      .then($ => {
        teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition))
        global['globalStore'].dispatch(experienceStarted())
      })
      .catch(defaultLogger.error)

    document.body.classList.remove('dcl-loading')
    ;(window as any).UnityLoader.Error.handler = (error: any) => {
      console['error'](error)
      ReportFatalError(error.message)
    }
  })
  .catch(err => {
    document.body.classList.remove('dcl-loading')
    if (err.message === AUTH_ERROR_LOGGED_OUT || err.message === NOT_INVITED) {
      ReportFatalError(NOT_INVITED)
    } else {
      console['error']('Error loading Unity')
      console['error'](err)
      ReportFatalError(FAILED_FETCHING_UNITY)
    }
  })
