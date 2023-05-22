import type { KernelOptions } from '@dcl/kernel-interface'
import { realmInitialized } from 'shared/dao'
import { BringDownClientAndReportFatalError } from 'shared/loading/ReportFatalError'
import { ensureMetaConfigurationInitialized } from 'shared/meta'
import { getFeatureFlagEnabled, getFeatureFlags } from 'shared/meta/selectors'
import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { getRendererInterface } from 'shared/renderer/getRendererInterface'
import { onLoginCompleted } from 'shared/session/onLoginCompleted'
import { getCurrentIdentity } from 'shared/session/selectors'
import { store } from 'shared/store/isolatedStore'
import { HUDElementID } from 'shared/types'
import { foregroundChangeObservable, isForeground } from 'shared/world/worldState'
import { renderingInBackground, renderingInForeground } from 'shared/loadingScreen/types'
import { kernelConfigForRenderer } from 'unity-interface/kernelConfigForRenderer'
import { startPreview } from './startPreview'

export async function loadWebsiteSystems(options: KernelOptions['kernelOptions']) {
  const [renderer] = await Promise.all([
    getRendererInterface(),
    /**
     * MetaConfiguration is the combination of three main aspects of the environment in which we are running:
     * - which Ethereum network are we connected to
     * - what is the current global explorer configuration from https://config.decentraland.${tld}/explorer.json
     * - what feature flags are currently enabled
     */
    ensureMetaConfigurationInitialized()
  ])

  // It's important to send FeatureFlags before initializing any other subsystem of the Renderer
  renderer.SetFeatureFlagsConfiguration(getFeatureFlags(store.getState()))

  const questEnabled = getFeatureFlagEnabled(store.getState(), 'quests')

  // killswitch, disable asset bundles
  if (!getFeatureFlagEnabled(store.getState(), 'asset_bundles')) {
    renderer.SetDisableAssetBundles()
  }

  renderer.ConfigureHUDElement(HUDElementID.MINIMAP, { active: true, visible: true })
  renderer.ConfigureHUDElement(HUDElementID.NOTIFICATION, { active: true, visible: true })
  renderer.ConfigureHUDElement(HUDElementID.AVATAR_EDITOR, { active: true, visible: false })
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

  renderer.SetKernelConfiguration(kernelConfigForRenderer())
  renderer.ConfigureHUDElement(HUDElementID.USERS_AROUND_LIST_HUD, { active: true, visible: false })
  renderer.ConfigureHUDElement(HUDElementID.GRAPHIC_CARD_WARNING, { active: true, visible: true })

  await onLoginCompleted()

  const identity = getCurrentIdentity(store.getState())!
  const profile = getCurrentUserProfile(store.getState())!

  if (!profile) {
    BringDownClientAndReportFatalError(new Error('Profile missing during unity initialization'), 'kernel#init')
    return
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
