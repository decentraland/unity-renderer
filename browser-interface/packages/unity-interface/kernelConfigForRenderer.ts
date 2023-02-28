import { KernelConfigForRenderer } from 'shared/types'
import { getAvatarTextureAPIBaseUrl, commConfigurations, WSS_ENABLED } from 'config'
import { nameValidCharacterRegex, nameValidRegex } from 'lib/decentraland/profiles/names'
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
    avatarTextureAPIBaseUrl: getAvatarTextureAPIBaseUrl(getSelectedNetwork(globalState))
  }
}
