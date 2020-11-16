import { KernelConfigForRenderer } from 'shared/types'
import { commConfigurations } from 'config'
import { nameValidCharacterRegex, nameValidRegex } from 'shared/profiles/utils/names'

export function kernelConfigForRenderer(): KernelConfigForRenderer {
  return {
    comms: {
      commRadius: commConfigurations.commRadius,
      voiceChatEnabled: false
    },
    profiles: {
      nameValidCharacterRegex: nameValidCharacterRegex.toString().replace(/[/]/g, ""),
      nameValidRegex: nameValidRegex.toString().replace(/[/]/g, "")
    }
  }
}
