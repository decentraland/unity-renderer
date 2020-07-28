import { RootFriendsState } from './types'

export const getClient = (store: RootFriendsState) => store.friends.client
export const getPrivateMessaging = (store: RootFriendsState) => store.friends

export const findByUserId = (store: RootFriendsState, userId: string) =>
  Object.values(store.friends.socialInfo).find((socialData) => socialData.userId === userId)

export const isFriend = (store: RootFriendsState, userId: string) => store.friends.friends.includes(userId)
