import type { KernelOptions } from '@dcl/kernel-interface'
import { trackEvent } from 'shared/analytics'
import { changeRealm, realmInitialized } from 'shared/dao'
import { BringDownClientAndReportFatalError } from 'shared/loading/ReportFatalError'
import { ensureMetaConfigurationInitialized } from 'shared/meta'
import {
  getFeatureFlagEnabled,
  getFeatureFlags,
  getFeatureFlagVariantName,
  getFeatureFlagVariantValue,
  getWorldConfig
} from 'shared/meta/selectors'
import type { WorldConfig } from 'shared/meta/types'
import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { getRendererInterface } from 'shared/renderer/getRendererInterface'
import { onLoginCompleted } from 'shared/session/onLoginCompleted'
import { getCurrentIdentity } from 'shared/session/selectors'
import { store } from 'shared/store/isolatedStore'
import { HUDElementID } from 'shared/types'
import { foregroundChangeObservable, isForeground } from 'shared/world/worldState'
import { HAS_INITIAL_POSITION_MARK, OPEN_AVATAR_EDITOR, RESET_TUTORIAL } from '../config'
import { renderingInBackground, renderingInForeground } from '../shared/loadingScreen/types'
import { kernelConfigForRenderer } from '../unity-interface/kernelConfigForRenderer'
import { logger } from './logger'
import { startPreview } from './startPreview'

export async function loadWebsiteSystems(options: KernelOptions['kernelOptions']) {
  const renderer = await getRendererInterface()

  /**
   * MetaConfiguration is the combination of three main aspects of the environment in which we are running:
   * - which Ethereum network are we connected to
   * - what is the current global explorer configuration from https://config.decentraland.${tld}/explorer.json
   * - what feature flags are currently enabled
   */
  await ensureMetaConfigurationInitialized()

  // It's important to send FeatureFlags before initializing any other subsystem of the Renderer
  renderer.SetFeatureFlagsConfiguration(getFeatureFlags(store.getState()))

  const questEnabled = getFeatureFlagEnabled(store.getState(), 'quests')
  const worldConfig: WorldConfig | undefined = getWorldConfig(store.getState())

  // killswitch, disable asset bundles
  if (!getFeatureFlagEnabled(store.getState(), 'asset_bundles')) {
    renderer.SetDisableAssetBundles()
  }

  renderer.ConfigureHUDElement(HUDElementID.MINIMAP, { active: true, visible: true })
  renderer.ConfigureHUDElement(HUDElementID.NOTIFICATION, { active: true, visible: true })
  renderer.ConfigureHUDElement(HUDElementID.AVATAR_EDITOR, { active: true, visible: OPEN_AVATAR_EDITOR })
  renderer.ConfigureHUDElement(HUDElementID.SIGNUP, { active: true, visible: false })
  renderer.ConfigureHUDElement(HUDElementID.LOADING_HUD, { active: true, visible: false })
  renderer.ConfigureHUDElement(HUDElementID.AVATAR_NAMES, { active: true, visible: true })
  renderer.ConfigureHUDElement(HUDElementID.SETTINGS_PANEL, { active: true, visible: false })
  renderer.ConfigureHUDElement(HUDElementID.EXPRESSIONS, { active: true, visible: true })
  renderer.ConfigureHUDElement(HUDElementID.EMOTES, { active: true, visible: false })
  renderer.ConfigureHUDElement(HUDElementID.PLAYER_INFO_CARD, { active: true, visible: true })
  renderer.ConfigureHUDElement(HUDElementID.TERMS_OF_SERVICE, { active: true, visible: true })
  renderer.ConfigureHUDElement(HUDElementID.OPEN_EXTERNAL_URL_PROMPT, { active: true, visible: false })
  renderer.ConfigureHUDElement(HUDElementID.NFT_INFO_DIALOG, { active: true, visible: false })
  renderer.ConfigureHUDElement(HUDElementID.TELEPORT_DIALOG, { active: true, visible: false })
  renderer.ConfigureHUDElement(HUDElementID.QUESTS_PANEL, { active: questEnabled, visible: false })
  renderer.ConfigureHUDElement(HUDElementID.QUESTS_TRACKER, { active: questEnabled, visible: true })
  renderer.ConfigureHUDElement(HUDElementID.PROFILE_HUD, { active: true, visible: true })

  // Grouping these together as WorldChatWindow, ControlsHUD, and HelpAndSupportHUD require the Taskbar to be
  // initialized first.
  ;(function activateTaskbarElements() {
    renderer.ConfigureHUDElement(
      HUDElementID.TASKBAR,
      { active: true, visible: true },
      { enableVoiceChat: true, enableQuestPanel: questEnabled }
    )
    renderer.ConfigureHUDElement(HUDElementID.WORLD_CHAT_WINDOW, { active: true, visible: false })
    renderer.ConfigureHUDElement(HUDElementID.CONTROLS_HUD, { active: true, visible: false })
    renderer.ConfigureHUDElement(HUDElementID.HELP_AND_SUPPORT_HUD, { active: true, visible: false })
  })()

  const configForRenderer = kernelConfigForRenderer()

  renderer.SetKernelConfiguration(configForRenderer)
  renderer.ConfigureHUDElement(HUDElementID.USERS_AROUND_LIST_HUD, { active: true, visible: false })
  renderer.ConfigureHUDElement(HUDElementID.GRAPHIC_CARD_WARNING, { active: true, visible: true })

  await onLoginCompleted()

  const identity = getCurrentIdentity(store.getState())!
  const profile = getCurrentUserProfile(store.getState())!

  if (!profile) {
    BringDownClientAndReportFatalError(new Error('Profile missing during unity initialization'), 'kernel#init')
    return
  }

  const NEEDS_TUTORIAL = RESET_TUTORIAL || !profile.tutorialStep

  // only enable the old tutorial if the feature flag new_tutorial is off
  // this code should be removed once the "hardcoded" tutorial is removed
  // from the renderer
  if (NEEDS_TUTORIAL) {
    const NEW_TUTORIAL_FEATURE_FLAG = getFeatureFlagVariantName(store.getState(), 'new_tutorial_variant')
    const IS_NEW_TUTORIAL_DISABLED =
      NEW_TUTORIAL_FEATURE_FLAG === 'disabled' || NEW_TUTORIAL_FEATURE_FLAG === 'undefined' || HAS_INITIAL_POSITION_MARK
    if (IS_NEW_TUTORIAL_DISABLED) {
      const enableNewTutorialCamera = worldConfig ? worldConfig.enableNewTutorialCamera ?? false : false
      const tutorialConfig = {
        fromDeepLink: HAS_INITIAL_POSITION_MARK,
        enableNewTutorialCamera: enableNewTutorialCamera
      }

      renderer.ConfigureTutorial(profile.tutorialStep, tutorialConfig)
    } else {
      try {
        const realm: string | undefined = getFeatureFlagVariantValue(store.getState(), 'new_tutorial_variant')
        if (realm) {
          await changeRealm(realm)
          trackEvent('onboarding_started', { onboardingRealm: realm })
        } else {
          logger.warn('No realm was provided for the onboarding experience.')
        }
      } catch (err) {
        console.error(err)
      }
    }
  }

  const isGuest = !identity.hasConnectedWeb3
  const friendsActivated = !isGuest && !getFeatureFlagEnabled(store.getState(), 'matrix_disabled')

  renderer.ConfigureHUDElement(HUDElementID.FRIENDS, { active: friendsActivated, visible: false })

  function reportForeground() {
    if (isForeground()) {
      store.dispatch(renderingInForeground())
      renderer.ReportFocusOn()
    } else {
      store.dispatch(renderingInBackground())
      renderer.ReportFocusOff()
    }
  }

  foregroundChangeObservable.add(reportForeground)
  reportForeground()

  if (options.previewMode) {
    renderer.SetDisableAssetBundles()
    await startPreview(renderer)
  }

  await realmInitialized()

  return true
}
