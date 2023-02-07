import type { ProfileStatus, ProfileUserInfo, RootProfileState } from './types'
import type { RootSessionState } from 'shared/session/types'
import type { Avatar } from '@dcl/schemas'
import { calculateDisplayName } from 'lib/decentraland/profiles/transformations/processServerProfile'
import { getCurrentUserId as selectCurrentUserId } from 'shared/session/selectors'

export const getProfileStatusAndData = (
  store: RootProfileState,
  userId: string
): [ProfileStatus | undefined, Avatar | undefined] => [
  store?.profiles?.userInfo[userId.toLowerCase()]?.status,
  store?.profiles?.userInfo[userId.toLowerCase()]?.data
]

export const getProfileFromStore = (store: RootProfileState, userId: string): ProfileUserInfo | null =>
  getProfileValueIfOkOrLoading(
    store,
    userId,
    (info) => info,
    () => null
  )

export const getProfilesFromStore = (
  store: RootProfileState,
  userIds: string[],
  userNameOrId?: string
): Array<ProfileUserInfo> => {
  const profiles = userIds.map((userId) => getProfileFromStore(store, userId))
  return filterProfilesByUserNameOrId(profiles, userNameOrId)
}

export const getProfile = (store: RootProfileState, userId: string): Avatar | null =>
  getProfileValueIfOkOrLoading(
    store,
    userId,
    (info) => info.data as Avatar,
    () => null
  )

export const getCurrentUserProfile = (store: RootProfileState & RootSessionState): Avatar | null => {
  const userId = selectCurrentUserId(store)
  return userId ? getProfile(store, userId) : null
}

export const getCurrentUserProfileDirty = (store: RootProfileState & RootSessionState): Avatar | null => {
  const currentUserId = selectCurrentUserId(store)
  if (!currentUserId) return null
  const [_status, data] = getProfileStatusAndData(store, currentUserId)
  return data || null
}

export const getCurrentUserProfileStatusAndData = (
  store: RootProfileState & RootSessionState
): [ProfileStatus | undefined, Avatar | undefined] => {
  const currentUserId = selectCurrentUserId(store)
  return currentUserId ? getProfileStatusAndData(store, currentUserId) : [undefined, undefined]
}

export const findProfileByName = (store: RootProfileState, userName: string): Avatar | null => {
  const lowerCasedUserName = userName.toLowerCase()
  return store.profiles && store.profiles.userInfo
    ? Object.values(store.profiles.userInfo)
        .filter((user) => user.status === 'ok')
        .find(
          (user) =>
            user.data?.name.toLowerCase() === lowerCasedUserName ||
            user.data?.userId.toLowerCase() === lowerCasedUserName ||
            calculateDisplayName(user.data).toLowerCase() === lowerCasedUserName
        )?.data || null
    : null
}

export const isAddedToCatalog = (store: RootProfileState, userId: string): boolean =>
  getProfileValueIfOkOrLoading(
    store,
    userId,
    (info) => !!info.addedToCatalog,
    () => false
  )

export const getEthereumAddress = (store: RootProfileState, userId: string): string | undefined =>
  getProfileValueIfOkOrLoading(
    store,
    userId,
    (info) => (info.data as Avatar).userId,
    () => undefined
  )

export function filterProfilesByUserNameOrId(
  profiles: Array<ProfileUserInfo | null>,
  userNameOrId: string | undefined
): Array<ProfileUserInfo> {
  return profiles.filter((friend) => {
    if (!friend || friend.status !== 'ok') {
      return false
    }
    if (!userNameOrId) {
      return true
    }
    // keep the ones userId or name includes the filter
    return (
      friend.data.userId.toLowerCase().includes(userNameOrId.toLowerCase()) ||
      friend.data.name.toLowerCase().includes(userNameOrId.toLowerCase())
    )
  }) as Array<ProfileUserInfo>
}

export const getLastSentProfileVersion = (store: RootProfileState, userId: string) =>
  store.profiles.lastSentProfileVersion[userId.toLowerCase()] as number | undefined

function getProfileValueIfOkOrLoading<T>(
  store: RootProfileState,
  userId: string,
  getter: (p: ProfileUserInfo) => T,
  ifNotFound: () => T
): T {
  const prof: ProfileUserInfo | undefined = store.profiles.userInfo[userId.toLowerCase()]
  return prof?.status === 'ok' || prof?.status === 'loading' ? getter(prof) : ifNotFound()
}
