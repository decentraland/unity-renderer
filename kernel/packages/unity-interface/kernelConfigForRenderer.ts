import { KernelConfigForRenderer } from 'shared/types'
import { commConfigurations } from 'config'

export function kernelConfigForRenderer(): KernelConfigForRenderer {
  return { comms: { commRadius: commConfigurations.commRadius } }
}
