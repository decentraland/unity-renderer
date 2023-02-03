import { CurrentUserStatus, SocialAPI } from 'dcl-social-client'
export type SocialData = {
  userId: string
  socialId: string
  conversationId?: string
}

export interface FriendRequest {
  friendRequestId: string
  userId: string
  createdAt: number
  message?: string
}

export type FriendsState = {
  client: SocialAPI | null
  socialInfo: Record<string, SocialData>
  friends: string[]
  /**
   * Outgoing requests. Friend requests from the current player to others.
   *
   * The current player has sent a request to become friends with another.
   * If said request doesn't exist, it should be created.
   * If a request to the current player exists, the request should be removed and the player added as a friend.
   */
  toFriendRequests: FriendRequest[]
  /**
   * Incoming requests. Friend requets from other players to the current one.
   *
   * A player has sent a request to become friends with the current player.
   * If said request doesn't exist, it should be created.
   * If a request to the player exists, the request should be removed and the player added as a friend.
   */
  fromFriendRequests: FriendRequest[]
  /**
   * Last status sent of friends so that we don't send duplicated states to renderer.
   */
  lastStatusOfFriends: Map<string, CurrentUserStatus>
  /**
   * Number of friend requests sent in a session (in-memory) per requested user.
   */
  numberOfFriendRequests: Map<string, number>
  /**
   * Cooldown time between friend requests sent in a session (in-memory) per requested user.
   */
  coolDownOfFriendRequests: Map<string, number>
}

export type RootFriendsState = {
  friends: FriendsState
}
