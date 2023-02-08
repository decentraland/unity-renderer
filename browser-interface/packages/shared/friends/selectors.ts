import { Conversation, ConversationType } from 'dcl-social-client'
import { UpdateTotalFriendRequestsPayload } from 'shared/types'
import { FriendRequest, RootFriendsState } from './types'
import { getUserIdFromMatrix } from './utils'

export const getSocialClient = (store: RootFriendsState) => store.friends.client

/**
 * Get all conversations `ConversationType.CHANNEL` that the user has
 * @return `conversation` & `unreadMessages` boolean that indicates whether the chat has unread messages.
 */
export const getChannels = (
  store: RootFriendsState
): Array<{ conversation: Conversation; unreadMessages: boolean }> => {
  return getConversations(store, ConversationType.CHANNEL)
}

/**
 * Get all current conversations the user has including DMs, channels, etc
 * @return `conversation` & `unreadMessages` boolean that indicates whether the conversation has unread messages.
 */
export const getConversations = (
  store: RootFriendsState,
  conversationType: ConversationType
): Array<{ conversation: Conversation; unreadMessages: boolean }> => {
  const client = getSocialClient(store)
  if (!client) return []

  const conversations = client.getAllCurrentConversations()
  return conversations
    .filter((conv) => conv.conversation.type === conversationType)
    .map((conv) => ({
      ...conv,
      conversation: {
        ...conv.conversation,
        userIds: conv.conversation.userIds?.map((userId) => getUserIdFromMatrix(userId))
      }
    }))
}

/**
 * Get all conversations `ConversationType.DIRECT` with friends the user has befriended
 * @return `conversation` & `unreadMessages` boolean that indicates whether the conversation has unread messages.
 */
export const getAllFriendsConversationsWithMessages = (
  store: RootFriendsState
): Array<{ conversation: Conversation; unreadMessages: boolean }> => {
  const client = getSocialClient(store)
  if (!client) return []

  const conversations = client.getAllCurrentFriendsConversations()

  return conversations
    .filter((conv) => conv.conversation.hasMessages)
    .map((conv) => ({
      ...conv,
      conversation: {
        ...conv.conversation,
        userIds: conv.conversation.userIds?.map((userId) => getUserIdFromMatrix(userId))
      }
    }))
}

export const getTotalFriendRequests = (store: RootFriendsState): UpdateTotalFriendRequestsPayload => ({
  totalReceivedRequests: store.friends.fromFriendRequests.length,
  totalSentRequests: store.friends.toFriendRequests.length
})

export const getTotalFriends = (store: RootFriendsState): number => store.friends.friends.length

export const getPrivateMessaging = (store: RootFriendsState) => store.friends
export const getPrivateMessagingFriends = (store: RootFriendsState): string[] => store.friends?.friends || []

export const findPrivateMessagingFriendsByUserId = (store: RootFriendsState, userId: string) =>
  Object.values(store.friends.socialInfo).find((socialData) => socialData.userId === userId)

export const isFriend = (store: RootFriendsState, userId: string) => store.friends.friends.includes(userId)

/**
 * Return true if the friend request has already been sent (toFriendRequests). Otherwise, false.
 */
export const isToPendingRequest = (store: RootFriendsState, userId: string) => {
  return store.friends.toFriendRequests.filter((request) => request.userId === userId).length > 0
}

/**
 * Return true if the friend request has already been received (fromFriendRequests). Otherwise, false.
 */
export const isFromPendingRequest = (store: RootFriendsState, userId: string) => {
  return store.friends.fromFriendRequests.filter((request) => request.userId === userId).length > 0
}

/**
 * Return true if the user is a pending request. Otherwise, false.
 */
export const isPendingRequest = (store: RootFriendsState, userId: string) => {
  return isFromPendingRequest(store, userId) || isToPendingRequest(store, userId)
}

export const getLastStatusOfFriends = (store: RootFriendsState) => store.friends.lastStatusOfFriends

export const getOwnId = (store: RootFriendsState) => store.friends.client?.getUserId()

export const getMessageBody = (store: RootFriendsState, friendRequestId: string): string | undefined => {
  const messageBody = getPendingRequests(store).find((friend) => friend.friendRequestId === friendRequestId)?.message

  return messageBody
}

/**
 * Get all sent and received pending requests.
 */
const getPendingRequests = (store: RootFriendsState): FriendRequest[] => {
  return store.friends.fromFriendRequests.concat(store.friends.toFriendRequests)
}

/**
 * Number of friend requests sent in a session (in-memory) per requested user.
 */
export const getNumberOfFriendRequests = (store: RootFriendsState) => store.friends.numberOfFriendRequests

/**
 * Time between friend requests sent in a session (in-memory) per requested user.
 */
export const getCoolDownOfFriendRequests = (store: RootFriendsState) => store.friends.coolDownOfFriendRequests
