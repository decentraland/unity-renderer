declare const globalThis: StoreContainer
declare const global: any
;(window as any).reactVersion = false

// IMPORTANT! This should be execd before loading 'config' module to ensure that init values are successfully loaded
global.enableWeb3 = true

import { createLogger } from 'shared/logger'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { AUTH_ERROR_LOGGED_OUT, experienceStarted, FAILED_FETCHING_UNITY, NOT_INVITED } from 'shared/loading/types'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'
import { NO_MOTD, DEBUG_PM, OPEN_AVATAR_EDITOR, HAS_INITIAL_POSITION_MARK } from '../config/index'
import { signalRendererInitialized, signalParcelLoadingStarted } from 'shared/renderer/actions'
import { lastPlayerPosition, teleportObservable } from 'shared/world/positionThings'
import { StoreContainer } from 'shared/store/rootTypes'
import { startUnitySceneWorkers } from '../unity-interface/dcl'
import { initializeUnity } from '../unity-interface/initializer'
import { HUDElementID, RenderProfile } from 'shared/types'
import {
  renderStateObservable,
  onNextRendererEnabled,
  foregroundObservable,
  isForeground
} from 'shared/world/worldState'
import { getCurrentIdentity } from 'shared/session/selectors'
import { userAuthentified } from 'shared/session'
import { realmInitialized } from 'shared/dao'
import { EnsureProfile } from 'shared/profiles/ProfileAsPromise'
import { ensureMetaConfigurationInitialized, waitForMessageOfTheDay } from 'shared/meta'
import { WorldConfig } from 'shared/meta/types'
import { isVoiceChatEnabledFor } from 'shared/meta/selectors'
import { unityInterface, UnityInterface } from 'unity-interface/UnityInterface'
import { kernelConfigForRenderer } from 'unity-interface/kernelConfigForRenderer'

const container = document.getElementById('gameContainer')

if (!container) throw new Error('cannot find element #gameContainer')

const logger = createLogger('unity.ts: ')

const start = Date.now()

const observer = renderStateObservable.add((isRunning) => {
  if (isRunning) {
    renderStateObservable.remove(observer)
    DEBUG_PM && logger.info(`initial load: `, Date.now() - start)
  }
})

function configureTaskbarDependentHUD(i: UnityInterface, voiceChatEnabled: boolean) {
  i.ConfigureHUDElement(HUDElementID.TASKBAR, { active: true, visible: true }, { enableVoiceChat: voiceChatEnabled })
  i.ConfigureHUDElement(HUDElementID.WORLD_CHAT_WINDOW, { active: true, visible: true })

  i.ConfigureHUDElement(HUDElementID.CONTROLS_HUD, { active: true, visible: false })
  i.ConfigureHUDElement(HUDElementID.EXPLORE_HUD, { active: true, visible: false })
  i.ConfigureHUDElement(HUDElementID.HELP_AND_SUPPORT_HUD, { active: true, visible: false })
}

initializeUnity(container)
  .then(async () => {
    const i = unityInterface

    i.ConfigureHUDElement(HUDElementID.MINIMAP, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.PROFILE_HUD, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.NOTIFICATION, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.AVATAR_EDITOR, {
      active: true,
      visible: OPEN_AVATAR_EDITOR
    })
    i.ConfigureHUDElement(HUDElementID.SETTINGS_PANEL, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.EXPRESSIONS, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.PLAYER_INFO_CARD, {
      active: true,
      visible: true
    })
    i.ConfigureHUDElement(HUDElementID.AIRDROPPING, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.TERMS_OF_SERVICE, { active: true, visible: true })

    i.ConfigureHUDElement(HUDElementID.OPEN_EXTERNAL_URL_PROMPT, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.NFT_INFO_DIALOG, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.TELEPORT_DIALOG, { active: true, visible: false })

    // NOTE(Brian): Scene download manager uses meta config to determine which empty parcels we want
    //              so ensuring meta configuration is initialized in this stage is a must
    // NOTE(Pablo): We also need meta configuration to know if we need to enable voice chat
    await ensureMetaConfigurationInitialized()

    try {
      await userAuthentified()
      const identity = getCurrentIdentity(globalThis.globalStore.getState())!

      const voiceChatEnabled = isVoiceChatEnabledFor(globalThis.globalStore.getState(), identity.address)

      const configForRenderer = kernelConfigForRenderer()
      configForRenderer.comms.voiceChatEnabled = voiceChatEnabled
      i.SetKernelConfiguration(configForRenderer)

      configureTaskbarDependentHUD(i, voiceChatEnabled)

      i.ConfigureHUDElement(HUDElementID.USERS_AROUND_LIST_HUD, { active: voiceChatEnabled, visible: false })
      i.ConfigureHUDElement(HUDElementID.FRIENDS, { active: identity.hasConnectedWeb3, visible: false })

      EnsureProfile(identity.address)
        .then((profile) => {
          i.ConfigureEmailPrompt(profile.tutorialStep)
          i.ConfigureTutorial(profile.tutorialStep, HAS_INITIAL_POSITION_MARK)
        })
        .catch((e) => logger.error(`error getting profile ${e}`))
    } catch (e) {
      logger.error('error on configuring taskbar & friends hud / tutorial. Trying to default to simple taskbar', e)
      configureTaskbarDependentHUD(i, false)
    }

    globalThis.globalStore.dispatch(signalRendererInitialized())

    onNextRendererEnabled(() => globalThis.globalStore.dispatch(experienceStarted()))

    await realmInitialized()

    await startUnitySceneWorkers()

    globalThis.globalStore.dispatch(signalParcelLoadingStarted())

    await ensureMetaConfigurationInitialized()

    let worldConfig: WorldConfig = globalThis.globalStore.getState().meta.config.world!

    if (worldConfig.renderProfile) {
      i.SetRenderProfile(worldConfig.renderProfile)
    } else {
      i.SetRenderProfile(RenderProfile.DEFAULT)
    }

    if (isForeground()) {
      i.ReportFocusOn()
    } else {
      i.ReportFocusOff()
    }

    foregroundObservable.add((isForeground) => {
      if (isForeground) {
        i.ReportFocusOn()
      } else {
        i.ReportFocusOff()
      }
    })

    if (!NO_MOTD) {
      waitForMessageOfTheDay().then((messageOfTheDay) => {
        i.ConfigureHUDElement(
          HUDElementID.MESSAGE_OF_THE_DAY,
          { active: !!messageOfTheDay, visible: false },
          messageOfTheDay
        )
      }).catch(logger.error)
    }

    teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition))

    document.body.classList.remove('dcl-loading')
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
