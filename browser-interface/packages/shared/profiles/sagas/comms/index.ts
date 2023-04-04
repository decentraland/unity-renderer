import type { Avatar } from '@dcl/schemas'
import { COMMS_PROFILE_TIMEOUT } from 'config'
import { storeCondition } from 'lib/redux'
import { requestProfileFromPeers } from 'shared/comms/handlers'
import { RoomConnection } from 'shared/comms/interface'
import { incrementCounter } from 'shared/analytics/occurences'
import { RootProfileState } from 'shared/profiles/types'
import { sleep } from 'lib/javascript/sleep'

export async function fetchPeerProfile(
  room: RoomConnection,
  userId: string,
  minimumVersion?: number
): Promise<Avatar | null> {
  const targetVersion = minimumVersion || 0
  const isConnectedToPeer = requestProfileFromPeers(room, userId, targetVersion)
  if (!isConnectedToPeer) {
    return null
  }
  const result = await Promise.race([
    storeCondition((state: RootProfileState) => {
      if (state.profiles.userInfo[userId]?.data?.version >= targetVersion) {
        return state.profiles.userInfo[userId]!.data
      }
    }),
    sleep(COMMS_PROFILE_TIMEOUT)
  ])
  incrementCounter(result ? 'profile-over-comms-succesful' : 'profile-over-comms-failed')
  if (!result) {
    return null
  }

  return result as Avatar
}
