import { KernelConfigForRenderer } from 'shared/types'
import {
  getAvatarTextureAPIBaseUrl,
  commConfigurations,
  WSS_ENABLED,
  WITH_FIXED_ITEMS,
  WITH_FIXED_COLLECTIONS,
  PREVIEW,
  DEBUG,
  getTLD,
  ETHEREUM_NETWORK
} from 'config'
import { nameValidCharacterRegex, nameValidRegex } from 'lib/decentraland/profiles/names'
import { getWorld } from '@dcl/schemas'
import { store } from 'shared/store/isolatedStore'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { isGifWebSupported } from 'shared/meta/selectors'
import { getExplorerVersion } from 'shared/meta/version'

export function kernelConfigForRenderer(): KernelConfigForRenderer {
  const explorerVersion = getExplorerVersion()
  const globalState = store.getState()

  let network = 'mainnet'

  try {
    network = getSelectedNetwork(globalState)
  } catch {}

  const COLLECTIONS_OR_ITEMS_ALLOWED =
    PREVIEW || ((DEBUG || getTLD() !== 'org') && network !== ETHEREUM_NETWORK.MAINNET)

  const urlParamsForWearablesDebug = !!(WITH_FIXED_ITEMS || WITH_FIXED_COLLECTIONS || COLLECTIONS_OR_ITEMS_ALLOWED)

  console.log('[KERNEL CONFIG LOG] urlParamsForWearablesDebug: ' + urlParamsForWearablesDebug) // temporal log (for debugging purposes)

  return {
    ...globalState.meta.config.world,
    comms: {
      commRadius: commConfigurations.commRadius,
      voiceChatEnabled: true
    },
    profiles: {
      nameValidCharacterRegex: nameValidCharacterRegex.toString().replace(/[/]/g, ''),
      nameValidRegex: nameValidRegex.toString().replace(/[/]/g, '')
    },
    debugConfig: undefined,
    gifSupported:
      typeof (window as any).OffscreenCanvas !== 'undefined' &&
      typeof (window as any).OffscreenCanvasRenderingContext2D === 'function' &&
      !WSS_ENABLED &&
      isGifWebSupported(globalState),
    network,
    validWorldRanges: getWorld().validWorldRanges,
    kernelVersion: explorerVersion,
    /** @deprecated */
    rendererVersion: explorerVersion,
    avatarTextureAPIBaseUrl: getAvatarTextureAPIBaseUrl(getSelectedNetwork(globalState)),
    urlParamsForWearablesDebug: urlParamsForWearablesDebug
  }
}
