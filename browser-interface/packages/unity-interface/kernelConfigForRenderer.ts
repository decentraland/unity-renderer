import { KernelConfigForRenderer } from 'shared/types'
import { 
  getAvatarTextureAPIBaseUrl,
  commConfigurations,
  WSS_ENABLED, WITH_FIXED_ITEMS,
  WITH_FIXED_COLLECTIONS,
  PREVIEW,
  DEBUG,
  getTLD,
  ETHEREUM_NETWORK
} from 'config'
import { nameValidCharacterRegex, nameValidRegex } from 'shared/profiles/utils/names'
import { getWorld } from '@dcl/schemas'
import { injectVersions } from 'shared/rolloutVersions'
import { store } from 'shared/store/isolatedStore'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { isGifWebSupported } from 'shared/meta/selectors'

export function kernelConfigForRenderer(): KernelConfigForRenderer {
  const versions = injectVersions({})
  const globalState = store.getState()

  let network = 'mainnet'

  try {
    network = getSelectedNetwork(globalState)
  } catch {}

  const COLLECTIONS_OR_ITEMS_ALLOWED =
    PREVIEW || ((DEBUG || getTLD() !== 'org') && network !== ETHEREUM_NETWORK.MAINNET)
  const urlParamsForWearablesDebug =
    !!((WITH_FIXED_ITEMS && COLLECTIONS_OR_ITEMS_ALLOWED) || (WITH_FIXED_COLLECTIONS && COLLECTIONS_OR_ITEMS_ALLOWED))

  // temporal logs (for debugging purposes)
  console.log('[SANTI LOGS] PREVIEW: ' + PREVIEW)
  console.log('[SANTI LOGS] DEBUG: ' + DEBUG)
  console.log('[SANTI LOGS] TLD: ' + getTLD())
  console.log('[SANTI LOGS] network: ' + network)
  console.log('[SANTI LOGS] WITH_FIXED_ITEMS: ' + WITH_FIXED_ITEMS)
  console.log('[SANTI LOGS] WITH_FIXED_COLLECTIONS: ' + WITH_FIXED_COLLECTIONS)
  console.log('[SANTI LOGS] COLLECTIONS_OR_ITEMS_ALLOWED: ' + COLLECTIONS_OR_ITEMS_ALLOWED)
  console.log('[SANTI LOGS] urlParamsForWearablesDebug: ' + urlParamsForWearablesDebug)

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
    kernelVersion: versions['@dcl/explorer'] || 'unknown-kernel-version',
    rendererVersion: versions['@dcl/explorer'] || 'unknown-renderer-version',
    avatarTextureAPIBaseUrl: getAvatarTextureAPIBaseUrl(getSelectedNetwork(globalState)),
    urlParamsForWearablesDebug: urlParamsForWearablesDebug
  }
}
