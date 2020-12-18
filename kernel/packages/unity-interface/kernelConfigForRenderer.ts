import { KernelConfigForRenderer } from 'shared/types'
import { commConfigurations, ENABLE_BUILDER_IN_WORLD, WSS_ENABLED } from 'config'
import { nameValidCharacterRegex, nameValidRegex } from 'shared/profiles/utils/names'

export function kernelConfigForRenderer(): KernelConfigForRenderer {
  return {
    comms: {
      commRadius: commConfigurations.commRadius,
      voiceChatEnabled: false
    },
    profiles: {
      nameValidCharacterRegex: nameValidCharacterRegex.toString().replace(/[/]/g, ''),
      nameValidRegex: nameValidRegex.toString().replace(/[/]/g, '')
    },
    features: {
      enableBuilderInWorld: ENABLE_BUILDER_IN_WORLD
    },
    gifSupported:
      // tslint:disable-next-line
      typeof OffscreenCanvas !== 'undefined' && typeof OffscreenCanvasRenderingContext2D === 'function' && !WSS_ENABLED
  }
}
