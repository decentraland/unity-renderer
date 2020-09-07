declare const globalThis: { UnityLoader: any } & StoreContainer
declare const global: any

// IMPORTANT! This should be execd before loading 'config' module to ensure that init values are successfully loaded
global.enableWeb3 = true

import { createLogger } from 'shared/logger'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { experienceStarted, NOT_INVITED, AUTH_ERROR_LOGGED_OUT, FAILED_FETCHING_UNITY } from 'shared/loading/types'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'
import { NO_MOTD, DEBUG_PM, OPEN_AVATAR_EDITOR, ENABLE_EXPLORE_HUD, ENABLE_MANA_HUD } from '../config/index'
import { signalRendererInitialized, signalParcelLoadingStarted } from 'shared/renderer/actions'
import { lastPlayerPosition, teleportObservable } from 'shared/world/positionThings'
import { StoreContainer } from 'shared/store/rootTypes'
import { startUnitySceneWorkers } from '../unity-interface/dcl'
import { initializeUnity } from '../unity-interface/initializer'
import { HUDElementID } from 'shared/types'
import { worldRunningObservable, onNextWorldRunning } from 'shared/world/worldState'
import { getCurrentIdentity } from 'shared/session/selectors'
import { userAuthentified } from 'shared/session'
import { realmInitialized } from 'shared/dao'

const container = document.getElementById('gameContainer')

if (!container) throw new Error('cannot find element #gameContainer')

const logger = createLogger('unity.ts: ')

const start = Date.now()

const observer = worldRunningObservable.add((isRunning) => {
  if (isRunning) {
    worldRunningObservable.remove(observer)
    DEBUG_PM && logger.info(`initial load: `, Date.now() - start)
  }
})

initializeUnity(container)
  .then(async ({ instancedJS }) => {
    const i = (await instancedJS).unityInterface

    i.ConfigureHUDElement(HUDElementID.MINIMAP, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.AVATAR, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.NOTIFICATION, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.AVATAR_EDITOR, { active: true, visible: OPEN_AVATAR_EDITOR })
    i.ConfigureHUDElement(HUDElementID.SETTINGS, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.EXPRESSIONS, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.PLAYER_INFO_CARD, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.AIRDROPPING, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.TERMS_OF_SERVICE, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.TASKBAR, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.WORLD_CHAT_WINDOW, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.OPEN_EXTERNAL_URL_PROMPT, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.NFT_INFO_DIALOG, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.TELEPORT_DIALOG, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.CONTROLS_HUD, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.EXPLORE_HUD, { active: ENABLE_EXPLORE_HUD, visible: false })

    try {
      await userAuthentified()
      const identity = getCurrentIdentity(globalThis.globalStore.getState())!
      i.ConfigureHUDElement(HUDElementID.FRIENDS, { active: identity.hasConnectedWeb3, visible: false })
      i.ConfigureHUDElement(HUDElementID.MANA_HUD, { active: ENABLE_MANA_HUD && identity.hasConnectedWeb3, visible: true })
    } catch (e) {
      logger.error('error on configuring friends hud')
    }

    globalThis.globalStore.dispatch(signalRendererInitialized())

    onNextWorldRunning(() => globalThis.globalStore.dispatch(experienceStarted()))

    await realmInitialized()
    await startUnitySceneWorkers()

    globalThis.globalStore.dispatch(signalParcelLoadingStarted())

    if (!NO_MOTD) {
      i.ConfigureHUDElement(HUDElementID.MESSAGE_OF_THE_DAY, { active: false, visible: true })
    }

    teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition))

    document.body.classList.remove('dcl-loading')
    globalThis.UnityLoader.Error.handler = (error: any) => {
      console['error'](error)
      ReportFatalError(error.message)
    }
  })
  .catch((err) => {
    document.body.classList.remove('dcl-loading')
    if (err.message === AUTH_ERROR_LOGGED_OUT || err.message === NOT_INVITED) {
      ReportFatalError(NOT_INVITED)
    } else {
      console['error']('Error loading Unity', err)
      ReportFatalError(FAILED_FETCHING_UNITY)
    }
  })
