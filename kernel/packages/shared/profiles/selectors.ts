import { Profile, ProfileStatus, ProfileUserInfo, RootProfileState } from './types'
import { getCurrentUserId } from 'shared/session/selectors'
import { RootSessionState } from 'shared/session/types'

export const getProfileStatusAndData = (
  store: RootProfileState,
  userId: string
): [ProfileStatus | undefined, Profile | undefined] => [
  store?.profiles?.userInfo[userId]?.status,
  store?.profiles?.userInfo[userId]?.data
]

export const getProfile = (store: RootProfileState, userId: string): Profile | null =>
  getProfileValueIfOkOrLoading(
    store,
    userId,
    (info) => info.data as Profile,
    () => null
  )

export const getCurrentUserProfile = (store: RootProfileState & RootSessionState): Profile | null => {
  const currentUserId = getCurrentUserId(store)
  return currentUserId ? getProfile(store, currentUserId) : null
}

export const getCurrentUserProfileStatusAndData = (
  store: RootProfileState & RootSessionState
): [ProfileStatus | undefined, Profile | undefined] => {
  const currentUserId = getCurrentUserId(store)
  return currentUserId ? getProfileStatusAndData(store, currentUserId) : [undefined, undefined]
}

export const hasConnectedWeb3 = (store: RootProfileState, userId: string): boolean =>
  getProfileValueIfOkOrLoading(
    store,
    userId,
    (info) => !!info.hasConnectedWeb3,
    () => false
  )

export const findProfileByName = (store: RootProfileState, userName: string): Profile | null =>
  store.profiles && store.profiles.userInfo
    ? Object.values(store.profiles.userInfo)
        .filter((user) => user.status === 'ok')
        .find((user) => user.data.name?.toLowerCase() === userName.toLowerCase())?.data
    : null

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
    (info) => (info.data as Profile).userId,
    () => undefined
  )

function getProfileValueIfOkOrLoading<T>(
  store: RootProfileState,
  userId: string,
  getter: (p: ProfileUserInfo) => T,
  ifNotFound: () => T
): T {
  return store.profiles &&
    store.profiles.userInfo &&
    store.profiles.userInfo[userId] &&
    (store.profiles.userInfo[userId].status === 'ok' || store.profiles.userInfo[userId].status === 'loading')
    ? getter(store.profiles.userInfo[userId])
    : ifNotFound()
}
