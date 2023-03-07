import type { Avatar } from '@dcl/schemas'
import { HAS_INITIAL_POSITION_MARK, RESET_TUTORIAL } from 'config'
import type { KernelOptions } from 'kernel-web-interface'
import { trackEvent } from 'shared/analytics/trackEvent'
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
import { changeRealm } from 'shared/realm/changeRealm'
import { ensureRealmAdapter } from 'shared/realm/ensureRealmAdapter'
import { getRendererInterface } from 'shared/renderer/getRendererInterface'
import { onLoginCompleted } from 'shared/session/onLoginCompleted'
import { getCurrentIdentity } from 'shared/session/selectors'
import type { ExplorerIdentity } from 'shared/session/types'
import { store } from 'shared/store/isolatedStore'
import type { RootState } from 'shared/store/rootTypes'
import { HUDElementID } from 'shared/types'
import { foregroundChangeObservable, isForeground } from 'shared/world/worldState'
import type { IUnityInterface } from 'unity-interface/IUnityInterface'
import { kernelConfigForRenderer } from 'unity-interface/kernelConfigForRenderer'
import { logger } from './logger'
import { startPreview } from './startPreview'

export async function loadWebsiteSystems(options: KernelOptions['kernelOptions']): Promise<boolean> {
  const [renderer] = await Promise.all([getRendererInterface(), ensureMetaConfigurationInitialized()])

  let state = store.getState()

  // It's important to send FeatureFlags before initializing any other subsystem of the Renderer
  renderer.SetFeatureFlagsConfiguration(getFeatureFlags(state))

  configureDisableAssetBundles(renderer, state, options)
  configureRendererHUDElements(renderer, state)

  await onLoginCompleted()
  state = store.getState()
  const identity = getCurrentIdentity(state)!
  const profile = getCurrentUserProfile(state)

  if (!profile) {
    BringDownClientAndReportFatalError(new Error('Profile missing during unity initialization'), 'kernel#init')
    return false
  }

  await configureTutorial(renderer, state, profile)
  state = store.getState()

  configureFriendsHUD(renderer, state, identity)
  configureReportForeground(renderer)

  if (options.previewMode) {
    await startPreview(renderer)
  }
  await ensureRealmAdapter()
  return true
}

function configureDisableAssetBundles(
  renderer: IUnityInterface,
  state: RootState,
  options: { previewMode?: boolean | undefined }
) {
  const featureFlagDisableAssetBundles = getFeatureFlagEnabled(state, 'asset_bundles')
  const disabledAssetBundles = options.previewMode || !featureFlagDisableAssetBundles
  if (disabledAssetBundles) {
    renderer.SetDisableAssetBundles()
  }
}

function configureRendererHUDElements(renderer: IUnityInterface, state: RootState) {
  const questEnabled = getFeatureFlagEnabled(state, 'quests')

  renderer.ConfigureHUDElement(HUDElementID.MINIMAP, { active: true, visible: true })
  renderer.ConfigureHUDElement(HUDElementID.NOTIFICATION, { active: true, visible: true })
  renderer.ConfigureHUDElement(HUDElementID.AVATAR_EDITOR, { active: true, visible: !!getCurrentUserProfile(state) })
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

  renderer.SetKernelConfiguration(kernelConfigForRenderer())
  renderer.ConfigureHUDElement(HUDElementID.USERS_AROUND_LIST_HUD, { active: true, visible: false })
  renderer.ConfigureHUDElement(HUDElementID.GRAPHIC_CARD_WARNING, { active: true, visible: true })
}

async function configureTutorial(renderer: IUnityInterface, state: RootState, profile: Avatar) {
  const NEEDS_TUTORIAL = RESET_TUTORIAL || !profile.tutorialStep
  const worldConfig: WorldConfig | undefined = getWorldConfig(state)

  // only enable the old tutorial if the feature flag new_tutorial is off
  // this code should be removed once the "hardcoded" tutorial is removed
  // from the renderer
  if (NEEDS_TUTORIAL) {
    const NEW_TUTORIAL_FEATURE_FLAG = getFeatureFlagVariantName(state, 'new_tutorial_variant')
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
        const realm: string | undefined = getFeatureFlagVariantValue(state, 'new_tutorial_variant')
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
}

function configureFriendsHUD(renderer: IUnityInterface, state: RootState, identity: ExplorerIdentity) {
  const isGuest = !identity.hasConnectedWeb3
  const friendsActivated = !isGuest && !getFeatureFlagEnabled(state, 'matrix_disabled')

  renderer.ConfigureHUDElement(HUDElementID.FRIENDS, { active: friendsActivated, visible: false })
}

function configureReportForeground(renderer: IUnityInterface) {
  foregroundChangeObservable.add(reportForeground)
  reportForeground()
  function reportForeground() {
    if (isForeground()) {
      renderer.ReportFocusOn()
    } else {
      renderer.ReportFocusOff()
    }
  }
}
