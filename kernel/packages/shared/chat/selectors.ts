import { RootChatState } from './types'

export const getClient = (store: RootChatState) => store.chat.privateMessaging.client
export const getPrivateMessaging = (store: RootChatState) => store.chat.privateMessaging

export const findByUserId = (store: RootChatState, userId: string) =>
  Object.values(store.chat.privateMessaging.socialInfo).find(socialData => socialData.userId === userId)

export const isFriend = (store: RootChatState, userId: string) => store.chat.privateMessaging.friends.includes(userId)
