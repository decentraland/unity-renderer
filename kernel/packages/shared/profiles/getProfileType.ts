import { ExplorerIdentity } from 'shared/session/types'
import { ProfileType } from './types'

export function getProfileType(identity?: ExplorerIdentity): ProfileType {
  return identity?.hasConnectedWeb3 ? ProfileType.DEPLOYED : ProfileType.LOCAL
}
