import { KernelConfigForRenderer } from 'shared/types'
import { getAvatarTextureAPIBaseUrl, commConfigurations, WSS_ENABLED } from 'config'
import { nameValidCharacterRegex, nameValidRegex } from 'shared/profiles/utils/names'
import { getWorld } from '@dcl/schemas'
import { injectVersions } from 'shared/rolloutVersions'
import { store } from 'shared/store/isolatedStore'
import { getSelectedNetwork } from 'shared/dao/selectors'

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
      voiceChatEnabled: false
    },
    profiles: {
      nameValidCharacterRegex: nameValidCharacterRegex.toString().replace(/[/]/g, ''),
      nameValidRegex: nameValidRegex.toString().replace(/[/]/g, '')
    },
    debugConfig: undefined,
    gifSupported:
      typeof (window as any).OffscreenCanvas !== 'undefined' &&
      typeof (window as any).OffscreenCanvasRenderingContext2D === 'function' &&
      !WSS_ENABLED,
    network,
    validWorldRanges: getWorld().validWorldRanges,
    kernelVersion: versions['@dcl/kernel'] || 'unknown-kernel-version',
    rendererVersion: versions['@dcl/unity-renderer'] || 'unknown-renderer-version',
    avatarTextureAPIBaseUrl: getAvatarTextureAPIBaseUrl(getSelectedNetwork(globalState))
  }
}
