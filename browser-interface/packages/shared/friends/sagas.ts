import type { AuthChain } from '@dcl/crypto'
import { Authenticator } from '@dcl/crypto'
import {
  FriendRequestInfo,
  FriendshipErrorCode
} from 'shared/protocol/decentraland/renderer/common/friend_request_common.gen'
import {
  FriendshipStatus,
  GetFriendshipStatusRequest
} from 'shared/protocol/decentraland/renderer/kernel_services/friends_kernel.gen'
import {
  AcceptFriendRequestPayload,
  AcceptFriendRequestReplyOk,
  CancelFriendRequestPayload,
  CancelFriendRequestReplyOk,
  GetFriendRequestsReplyOk,
  RejectFriendRequestPayload,
  RejectFriendRequestReplyOk,
  SendFriendRequestPayload,
  SendFriendRequestReplyOk
} from 'shared/protocol/decentraland/renderer/kernel_services/friend_request_kernel.gen'
import { GetMutualFriendsRequest } from 'shared/protocol/decentraland/renderer/kernel_services/mutual_friends_kernel.gen'
import { ReceiveFriendRequestPayload } from 'shared/protocol/decentraland/renderer/renderer_services/friend_request_renderer.gen'
import { Avatar, EthAddress } from '@dcl/schemas'
import { CHANNEL_TO_JOIN_CONFIG_URL, DEBUG_KERNEL_LOG, ethereumConfigurations } from 'config'
import {
  ChannelErrorKind,
  ChannelsError,
  Conversation,
  ConversationType,
  CurrentUserStatus,
  FriendshipRequest,
  GetOrCreateConversationResponse,
  PresenceType,
  SocialAPI,
  SocialClient,
  UnknownUsersError,
  UpdateUserStatus
} from 'dcl-social-client'
import { isAddress } from 'eth-connect/eth-connect'
import future from 'fp-future'
import { calculateDisplayName } from 'lib/decentraland/profiles/transformations/processServerProfile'
import {
  defaultProfile,
  profileToRendererFormat
} from 'lib/decentraland/profiles/transformations/profileToRendererFormat'
import { NewProfileForRenderer } from 'lib/decentraland/profiles/transformations/types'
import { deepEqual } from 'lib/javascript/deepEqual'
import { now } from 'lib/javascript/now'
import { uuid } from 'lib/javascript/uuid'
import defaultLogger, { createDummyLogger, createLogger } from 'lib/logger'
import { fetchENSOwner } from 'lib/web3/fetchENSOwner'
import { apply, call, delay, fork, put, race, select, take, takeEvery } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import { SendPrivateMessage, SEND_PRIVATE_MESSAGE } from 'shared/chat/actions'
import { SET_ROOM_CONNECTION } from 'shared/comms/actions'
import { getPeer } from 'shared/comms/peers'
import { waitForRoomConnection } from 'shared/dao/sagas'
import { getSelectedNetwork } from 'shared/dao/selectors'
import {
  JoinOrCreateChannel,
  JOIN_OR_CREATE_CHANNEL,
  LeaveChannel,
  LEAVE_CHANNEL,
  SendChannelMessage,
  SEND_CHANNEL_MESSAGE,
  setMatrixClient,
  SetMatrixClient,
  SET_MATRIX_CLIENT,
  updateFriendship,
  UpdateFriendship,
  updatePrivateMessagingState,
  updateUserData,
  UPDATE_FRIENDSHIP
} from 'shared/friends/actions'
import {
  findPrivateMessagingFriendsByUserId,
  getAllFriendsConversationsWithMessages,
  getChannels,
  getCoolDownOfFriendRequests,
  getLastStatusOfFriends,
  getMessageBody,
  getNumberOfFriendRequests,
  getOwnId,
  getPrivateMessaging,
  getPrivateMessagingFriends,
  getSocialClient,
  getTotalFriendRequests,
  getTotalFriends,
  isFriend,
  isFromPendingRequest,
  isPendingRequest,
  isToPendingRequest
} from 'shared/friends/selectors'
import { FriendRequest, FriendsState, SocialData } from 'shared/friends/types'
import { waitForMetaConfigurationInitialization } from 'shared/meta/sagas'
import { getFeatureFlagEnabled, getSynapseUrl } from 'shared/meta/selectors'
import { addedProfilesToCatalog } from 'shared/profiles/actions'
import {
  findProfileByName,
  getCurrentUserProfile,
  getProfile,
  getProfilesFromStore,
  isAddedToCatalog
} from 'shared/profiles/selectors'
import { ProfileUserInfo } from 'shared/profiles/types'
import { ensureRealmAdapter } from 'shared/realm/ensureRealmAdapter'
import { getFetchContentUrlPrefixFromRealmAdapter, getRealmConnectionString } from 'shared/realm/selectors'
import { OFFLINE_REALM } from 'shared/realm/types'
import { waitForRendererInstance } from 'shared/renderer/sagas-helper'
import { getRendererModules } from 'shared/renderer/selectors'
import { getParcelPosition } from 'shared/scene-loader/selectors'
import { USER_AUTHENTICATED } from 'shared/session/actions'
import { getCurrentIdentity, getCurrentUserId, isGuestLogin } from 'shared/session/selectors'
import { ExplorerIdentity } from 'shared/session/types'
import { mutePlayers, unmutePlayers } from 'shared/social/actions'
import { store } from 'shared/store/isolatedStore'
import { RootState } from 'shared/store/rootTypes'
import {
  AddChatMessagesPayload,
  AddFriendRequestsPayload,
  AddFriendsPayload,
  AddFriendsWithDirectMessagesPayload,
  ChannelErrorCode,
  ChannelErrorPayload,
  ChannelInfoPayload,
  ChannelMember,
  ChannelSearchResultsPayload,
  ChatMessage,
  ChatMessageType,
  CreateChannelPayload,
  FriendshipAction,
  FriendsInitializationMessage,
  FriendsInitializeChatPayload,
  GetChannelInfoPayload,
  GetChannelMembersPayload,
  GetChannelMessagesPayload,
  GetChannelsPayload,
  GetFriendRequestsPayload,
  GetFriendsPayload,
  GetFriendsWithDirectMessagesPayload,
  GetJoinedChannelsPayload,
  GetPrivateMessagesPayload,
  JoinOrCreateChannelPayload,
  MarkChannelMessagesAsSeenPayload,
  MarkMessagesAsSeenPayload,
  MuteChannelPayload,
  NotificationType,
  PresenceStatus,
  UpdateChannelMembersPayload,
  UpdateTotalFriendRequestsPayload,
  UpdateTotalUnseenMessagesByChannelPayload,
  UpdateTotalUnseenMessagesByUserPayload,
  UpdateTotalUnseenMessagesPayload,
  UpdateUserUnseenMessagesPayload
} from 'shared/types'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { ensureFriendProfile } from './ensureFriendProfile'
import {
  areChannelsEnabled,
  COOLDOWN_TIME_MS,
  decodeFriendRequestId,
  encodeFriendRequestId,
  getAntiSpamLimits,
  getMatrixIdFromUser,
  getMaxChannels,
  getNormalizedRoomName,
  getUserIdFromMatrix,
  getUsersAllowedToCreate,
  isBlocked,
  isNewFriendRequestEnabled,
  validateFriendRequestId
} from './utils'

const logger = DEBUG_KERNEL_LOG ? createLogger('chat: ') : createDummyLogger()

const receivedMessages: Record<string, number> = {}

const MESSAGE_LIFESPAN_MILLIS = 1_000
const SEND_STATUS_INTERVAL_MILLIS = 60_000
const MIN_TIME_BETWEEN_FRIENDS_INITIALIZATION_RETRIES_MILLIS = 1000
const MAX_TIME_BETWEEN_FRIENDS_INITIALIZATION_RETRIES_MILLIS = 256000

export function* friendsSaga() {
  // We don't want to initialize the friends & chat feature if we are on preview or builder mode
  yield fork(initializeFriendsSaga)
  yield fork(initializeStatusUpdateInterval)
  yield fork(initializeReceivedMessagesCleanUp)

  yield takeEvery(SET_MATRIX_CLIENT, configureMatrixClient)
  yield takeEvery(UPDATE_FRIENDSHIP, trackEvents)
  yield takeEvery(UPDATE_FRIENDSHIP, handleUpdateFriendship)
  yield takeEvery(SEND_PRIVATE_MESSAGE, handleSendPrivateMessage)
  yield takeEvery(SEND_CHANNEL_MESSAGE, handleSendChannelMessage)
  yield takeEvery(JOIN_OR_CREATE_CHANNEL, handleJoinOrCreateChannel)
  yield takeEvery(LEAVE_CHANNEL, handleLeaveChannel)
}

function* initializeFriendsSaga() {
  let secondsToRetry = MIN_TIME_BETWEEN_FRIENDS_INITIALIZATION_RETRIES_MILLIS

  yield call(waitForMetaConfigurationInitialization)

  // this reconnection breaks the server. setting to false
  const shouldRetryReconnection = yield select(getFeatureFlagEnabled, 'retry_matrix_login')
  const chatDisabled = yield select(getFeatureFlagEnabled, 'matrix_disabled')

  if (chatDisabled) return

  do {
    yield race({
      auth: take(USER_AUTHENTICATED),
      delay: delay(secondsToRetry)
    })

    yield call(waitForRoomConnection)
    yield call(waitForRendererInstance)
    const { currentIdentity, isGuest, client } = (yield select(getInformationForFriendsSaga)) as ReturnType<
      typeof getInformationForFriendsSaga
    >

    // guests must not use the friends & private messaging features.
    if (isGuest) {
      getUnityInstance().InitializeChat({
        totalUnseenMessages: 0,
        channelToJoin: undefined
      })
      return
    }

    try {
      const isLoggedIn: boolean = (currentIdentity && client && (yield apply(client, client.isLoggedIn, []))) || false

      const shouldRetry = !isLoggedIn && !isGuest

      if (shouldRetry) {
        try {
          logger.log('[Social client] Initializing')
          yield call(initializePrivateMessaging)
          logger.log('[Social client] Initialized')
          // restart the debounce
          secondsToRetry = MIN_TIME_BETWEEN_FRIENDS_INITIALIZATION_RETRIES_MILLIS
        } catch (e) {
          logAndTrackError(`Error initializing private messaging`, e)

          if (secondsToRetry < MAX_TIME_BETWEEN_FRIENDS_INITIALIZATION_RETRIES_MILLIS) {
            secondsToRetry *= 1.5
          }
        }
      }
    } catch (e) {
      logAndTrackError('Error while logging in to chat service', e)
    }
  } while (shouldRetryReconnection)
}

function getInformationForFriendsSaga(state: RootState) {
  return {
    currentIdentity: getCurrentIdentity(state),
    isGuest: isGuestLogin(state),
    client: getSocialClient(state)
  }
}

async function handleIncomingFriendshipUpdateStatus(
  action: FriendshipAction,
  socialId: string,
  messageBody?: string | undefined
) {
  logger.info(`handleIncomingFriendshipUpdateStatus`, action, socialId)

  // map social id to user id
  const userId = parseUserId(socialId)

  if (!userId) {
    logger.warn(`cannot parse user id from social id`, socialId)
    return null
  }

  store.dispatch(updateUserData(userId, socialId))

  // ensure user profile is initialized and send to renderer
  await ensureFriendProfile(userId)

  // add to friendRequests & update renderer
  await UpdateFriendshipAsPromise(action, userId, true, messageBody)
}

function* configureMatrixClient(action: SetMatrixClient) {
  const client = action.payload.socialApi
  const identity: ExplorerIdentity | undefined = yield select(getCurrentIdentity)

  const friendsResponse: { friendsSocial: SocialData[]; ownId: string } | undefined = yield call(refreshFriends)

  if (!friendsResponse) {
    // refreshFriends might fail and return with no actual data
    return
  }

  const { ownId } = friendsResponse

  if (!identity) {
    return
  }

  // check channels feature is enabled
  const channelsDisabled = !areChannelsEnabled()

  // initialize conversations
  client.onStatusChange(async (socialId, status) => {
    try {
      const userId = parseUserId(socialId)
      if (userId) {
        // When it's a friend and is not added to catalog
        // unity needs to know this information to show that the user has connected
        if (isFriend(store.getState(), userId)) {
          if (!isAddedToCatalog(store.getState(), userId)) {
            await ensureFriendProfile(userId)
          }
          getUnityInstance().AddFriends({
            friends: [userId],
            totalFriends: getTotalFriends(store.getState())
          })
        }

        sendUpdateUserStatus(userId, status)
      }
    } catch (error) {
      const message = 'Failed while processing friend status change'
      defaultLogger.error(message, error)

      trackEvent('error', {
        context: 'kernel#saga',
        message: message,
        stack: '' + error
      })
    }
  })

  client.onMessage(async (conversation, message) => {
    try {
      const isChannelType = conversation.type === ConversationType.CHANNEL

      if (isChannelType && channelsDisabled) {
        return
      }
      if (receivedMessages.hasOwnProperty(message.id)) {
        // message already processed, skipping
        return
      } else {
        receivedMessages[message.id] = Date.now()
      }

      const senderUserId = parseUserId(message.sender)

      if (!senderUserId) {
        logger.error('unknown message', message, conversation)
        return
      }

      const profile = getProfile(store.getState(), identity.address)
      const blocked = profile?.blocked ?? []
      if (blocked.includes(senderUserId)) {
        return
      }

      const recipient = isChannelType ? conversation.id : message.sender === ownId ? senderUserId : identity.address
      const messageType = isChannelType ? ChatMessageType.PUBLIC : ChatMessageType.PRIVATE
      const chatMessage = {
        messageId: message.id,
        messageType,
        timestamp: message.timestamp,
        body: message.text,
        sender: message.sender === ownId ? identity.address : senderUserId,
        recipient
      }

      const userProfile = getProfile(store.getState(), senderUserId)
      if (!userProfile || !isAddedToCatalog(store.getState(), senderUserId)) {
        await ensureFriendProfile(senderUserId)
      }

      if (message.sender === ownId && !isChannelType) {
        // ignore messages sent to private chats by the local user
        return
      }

      if (isPendingRequest(store.getState(), getUserIdFromMatrix(message.sender))) {
        // ignore messages sent in the request event
        return
      }

      addNewChatMessage(chatMessage)

      if (isChannelType) {
        const muted = profile?.muted ?? []
        if (!muted.includes(conversation.id)) {
          // send update with unseen messages by channel
          getUnseenMessagesByChannel()
        }
      } else {
        const unreadMessages = client.getConversationUnreadMessages(conversation.id).length

        const updateUnseenMessages: UpdateUserUnseenMessagesPayload = {
          userId: senderUserId,
          total: unreadMessages
        }

        getUnityInstance().UpdateUserUnseenMessages(updateUnseenMessages)
      }

      // send total unseen messages update
      const friends = await getFriendIds(client)
      const totalUnreadMessages = getTotalUnseenMessages(client, ownId, friends)
      const updateTotalUnseenMessages: UpdateTotalUnseenMessagesPayload = {
        total: totalUnreadMessages
      }
      getUnityInstance().UpdateTotalUnseenMessages(updateTotalUnseenMessages)
    } catch (error) {
      const message = 'Failed while processing message'
      defaultLogger.error(message, error)

      trackEvent('error', {
        context: 'kernel#saga',
        message: message,
        stack: '' + error
      })
    }
  })

  client.onFriendshipRequest((socialId, messageBody) =>
    handleIncomingFriendshipUpdateStatus(FriendshipAction.REQUESTED_FROM, socialId, messageBody).catch((error) => {
      const message = 'Failed while processing friendship request'
      defaultLogger.error(message, error)

      trackEvent('error', {
        context: 'kernel#saga',
        message: message,
        stack: '' + error
      })
    })
  )

  client.onFriendshipRequestCancellation((socialId) =>
    handleIncomingFriendshipUpdateStatus(FriendshipAction.CANCELED, socialId)
  )

  client.onFriendshipRequestApproval(async (socialId) => {
    await handleIncomingFriendshipUpdateStatus(FriendshipAction.APPROVED, socialId)
    updateUserStatus(client, socialId)
  })

  client.onFriendshipDeletion((socialId) => handleIncomingFriendshipUpdateStatus(FriendshipAction.DELETED, socialId))

  client.onFriendshipRequestRejection((socialId) =>
    handleIncomingFriendshipUpdateStatus(FriendshipAction.REJECTED, socialId)
  )

  client.onChannelMembership(async (conversation, membership) => {
    if (!areChannelsEnabled()) return

    switch (membership) {
      case 'join':
        if (!conversation.name || conversation.name?.startsWith('Empty room')) {
          break
        }

        const onlineMembers = getOnlineOrJoinedMembersCount(client, conversation)

        const channel: ChannelInfoPayload = {
          name: getNormalizedRoomName(conversation.name),
          channelId: conversation.id,
          unseenMessages: conversation.unreadMessages?.length ?? 0,
          lastMessageTimestamp: conversation.lastEventTimestamp ?? undefined,
          memberCount: onlineMembers,
          description: '',
          joined: true,
          muted: false
        }

        getUnityInstance().JoinChannelConfirmation({ channelInfoPayload: [channel] })
        break
      case 'leave':
        const joinedMembers = client.getChannel(conversation.id)?.userIds?.length ?? 0
        const leavingChannelPayload: ChannelInfoPayload = {
          name: conversation.name ?? '',
          channelId: conversation.id,
          unseenMessages: 0,
          lastMessageTimestamp: undefined,
          memberCount: joinedMembers,
          description: '',
          joined: false,
          muted: false
        }

        // send total unseen messages update
        const friends = await getFriendIds(client)
        const totalUnreadMessages = getTotalUnseenMessages(client, client.getUserId(), friends)
        const updateTotalUnseenMessages: UpdateTotalUnseenMessagesPayload = {
          total: totalUnreadMessages
        }

        getUnityInstance().UpdateTotalUnseenMessages(updateTotalUnseenMessages)
        getUnityInstance().UpdateChannelInfo({ channelInfoPayload: [leavingChannelPayload] })
        break
    }
  })
}

/**
 * Returns the count of channel members where the count will be the online members when presence feature is enabled
 * or the total number of users joined when it is not.
 *
 * @return `number` of the members of the channel given the above criteria
 */
function getOnlineOrJoinedMembersCount(client: SocialAPI, conversation: Conversation): number {
  const presenceDisabled = getFeatureFlagEnabled(store.getState(), 'matrix_presence_disabled')

  if (presenceDisabled) {
    return conversation.userIds?.length || 0
  }

  return getOnlineMembersCount(client, conversation.userIds)
}

// this saga needs to throw in case of failure
function* initializePrivateMessaging() {
  const { synapseUrl, identity, disablePresence, profile } = (yield select(
    getPrivateMessagingIdentityInfo
  )) as ReturnType<typeof getPrivateMessagingIdentityInfo>

  if (!identity) return

  const { address: ethAddress } = identity
  const timestamp: number = now()

  // TODO: the "timestamp" should be a message also signed by a catalyst.
  const messageToSign = `${timestamp}`

  const authChain = Authenticator.signPayload(identity, messageToSign)

  const client: SocialAPI = yield apply(SocialClient, SocialClient.loginToServer, [
    synapseUrl,
    ethAddress,
    timestamp,
    authChain as AuthChain,
    {
      disablePresence
    }
  ])

  if (profile) {
    const displayName = calculateDisplayName(profile)
    yield apply(client, client.setProfileInfo, [{ displayName }])
  }

  yield put(setMatrixClient(client))
}
function getPrivateMessagingIdentityInfo(state: RootState) {
  return {
    synapseUrl: getSynapseUrl(state),
    identity: getCurrentIdentity(state),
    disablePresence: getFeatureFlagEnabled(state, 'matrix_presence_disabled'),
    profile: getCurrentUserProfile(state)
  }
}

function* refreshFriends() {
  try {
    const client: SocialAPI | null = yield select(getSocialClient)

    if (!client) return

    const ownId = client.getUserId()

    // init friends
    const friendIds: string[] = yield call(async () => await getFriendIds(client))
    const friendsSocial: SocialData[] = []

    // init friend requests
    const friendRequests: FriendshipRequest[] = yield client.getPendingRequests()

    // filter my requests to others
    const toFriendRequests = friendRequests.filter((request) => request.from === ownId)
    const toFriendRequestsIds = toFriendRequests.map((request) => request.to)
    const toFriendRequestsSocial = toSocialData(toFriendRequestsIds)

    // filter other requests to me
    const fromFriendRequests = friendRequests.filter((request) => request.to === ownId)
    const fromFriendRequestsIds = fromFriendRequests.map((request) => request.from)
    const fromFriendRequestsSocial = toSocialData(fromFriendRequestsIds)

    const socialInfo: Record<string, SocialData> = [
      ...friendsSocial,
      ...toFriendRequestsSocial,
      ...fromFriendRequestsSocial
    ].reduce(
      (acc, current) => ({
        ...acc,
        [current.socialId]: current
      }),
      {}
    )

    const requestedFromIds = fromFriendRequests.map(
      (request): FriendRequest => ({
        friendRequestId: encodeFriendRequestId(ownId, request.from, true, FriendshipAction.REQUESTED_FROM),
        createdAt: request.createdAt,
        userId: getUserIdFromMatrix(request.from),
        message: request.message
      })
    )
    const requestedToIds = toFriendRequests.map(
      (request): FriendRequest => ({
        friendRequestId: encodeFriendRequestId(ownId, request.to, false, FriendshipAction.REQUESTED_TO),
        createdAt: request.createdAt,
        userId: getUserIdFromMatrix(request.to),
        message: request.message
      })
    )

    // explorer information
    const totalUnseenMessages = getTotalUnseenMessages(client, ownId, friendIds)

    const initFriendsMessage: FriendsInitializationMessage = {
      totalReceivedRequests: requestedFromIds.length
    }
    const initChatMessage: FriendsInitializeChatPayload = {
      totalUnseenMessages: totalUnseenMessages,
      channelToJoin: CHANNEL_TO_JOIN_CONFIG_URL?.toString()
    }

    defaultLogger.log('____ initMessage ____', initFriendsMessage)
    defaultLogger.log('____ initChatMessage ____', initChatMessage)

    // all profiles to obtain, deduped
    const allProfilesToObtain: string[] = friendIds
      .concat(requestedFromIds.map((x) => x.userId))
      .concat(requestedToIds.map((x) => x.userId))
      .filter((each, i, elements) => elements.indexOf(each) === i)

    const ensureFriendProfilesPromises = allProfilesToObtain.map((userId) => ensureFriendProfile(userId))
    yield Promise.all(ensureFriendProfilesPromises).catch(logger.error)

    let token = client.getAccessToken()

    if (token) {
      getUnityInstance().InitializeMatrix(token)
    }

    getUnityInstance().InitializeFriends(initFriendsMessage)
    getUnityInstance().InitializeChat(initChatMessage)
    const { lastStatusOfFriends, numberOfFriendRequests, coolDownOfFriendRequests } = (yield select(
      getFriendStatusInfo
    )) as ReturnType<typeof getFriendStatusInfo>

    yield put(
      updatePrivateMessagingState({
        client,
        socialInfo,
        friends: friendIds,
        fromFriendRequests: requestedFromIds,
        toFriendRequests: requestedToIds,
        lastStatusOfFriends,
        numberOfFriendRequests,
        coolDownOfFriendRequests
      })
    )

    return { friendsSocial, ownId }
  } catch (e) {
    logAndTrackError('Error while refreshing friends', e)
  }
}

function getFriendStatusInfo(state: RootState) {
  // check if lastStatusOfFriends has current statuses. if so, we keep them. if not, we initialize an empty map
  // initialize an empty map because there is no way to get the current statuses at the very begining of the initialization, the matrix client store is empty at that point
  const lastStatusOfFriends: Map<string, CurrentUserStatus> = getLastStatusOfFriends(state) ?? new Map()

  // Check if numberOfFriendRequests has values. If so, we keep them. If not, we initialize an empty map.
  const numberOfFriendRequests: Map<string, number> = getNumberOfFriendRequests(state) ?? new Map()

  // Check if coolDownOfFriendRequests has values. If so, we keep them. If not, we initialize an empty map.
  const coolDownOfFriendRequests: Map<string, number> = getCoolDownOfFriendRequests(state) ?? new Map()

  return {
    lastStatusOfFriends,
    numberOfFriendRequests,
    coolDownOfFriendRequests
  }
}

async function getFriendIds(client: SocialAPI): Promise<string[]> {
  let friends: string[]
  if (shouldUseSocialServiceForFriendships()) {
    friends = await client.getAllFriendsAddresses()
  } else {
    friends = client.getAllFriends()
  }

  return friends.map(($) => parseUserId($)).filter(Boolean) as string[]
}

function shouldUseSocialServiceForFriendships() {
  return (
    !getFeatureFlagEnabled(store.getState(), 'use-synapse-server') &&
    getFeatureFlagEnabled(store.getState(), 'use-social-server-friendships')
  )
}

function getTotalUnseenMessages(client: SocialAPI, ownId: string, friendIds: string[]): number {
  const channelsDisabled = !areChannelsEnabled()
  const profile = getCurrentUserProfile(store.getState())

  const conversationsWithUnreadMessages: Conversation[] = client.getAllConversationsWithUnreadMessages()
  let totalUnseenMessages = 0

  for (const conv of conversationsWithUnreadMessages) {
    if (conv.type === ConversationType.CHANNEL) {
      if (channelsDisabled || profile?.muted?.includes(conv.id)) {
        continue
      }
    } else if (conv.type === ConversationType.DIRECT) {
      const socialId = conv.userIds?.find((userId) => userId !== ownId)
      if (!socialId) {
        continue
      }

      const userId = getUserIdFromMatrix(socialId)

      if (!friendIds.some((friendIds) => friendIds === userId)) {
        continue
      }
    }
    totalUnseenMessages += conv.unreadMessages?.length || 0
  }

  return totalUnseenMessages
}

export async function getFriends(request: GetFriendsPayload) {
  // ensure friend profiles are sent to renderer
  const realmAdapter = await ensureRealmAdapter()
  const fetchContentServerWithPrefix = getFetchContentUrlPrefixFromRealmAdapter(realmAdapter)
  const friendsIds: string[] = getPrivateMessagingFriends(store.getState())

  const filteredFriends: Array<ProfileUserInfo> = getProfilesFromStore(
    store.getState(),
    friendsIds,
    request.userNameOrId
  )

  const friendsToReturn = filteredFriends.slice(request.skip, request.skip + request.limit)

  const profilesForRenderer = friendsToReturn.map((profile) =>
    profileToRendererFormat(profile.data, {
      baseUrl: fetchContentServerWithPrefix
    })
  )
  getUnityInstance().AddUserProfilesToCatalog({ users: profilesForRenderer })

  const friendIdsToReturn = friendsToReturn.map((friend) => friend.data.userId)

  const addFriendsPayload: AddFriendsPayload = {
    friends: friendIdsToReturn,
    totalFriends: friendsIds.length
  }

  getUnityInstance().AddFriends(addFriendsPayload)

  store.dispatch(addedProfilesToCatalog(friendsToReturn.map((friend) => friend.data)))

  const client = getSocialClient(store.getState())
  if (!client) {
    return
  }

  const friendsSocialIds = friendIdsToReturn.map(getMatrixIdFromUser)
  updateUserStatus(client, ...friendsSocialIds)
}

export function getFriendshipStatus(request: GetFriendshipStatusRequest) {
  // The userId is expected to always come from Renderer and follow the pattern `0x1111ada11111`
  const userId = request.userId
  const state = store.getState()

  // Check user status
  if (isFriend(state, userId)) return FriendshipStatus.APPROVED
  if (isToPendingRequest(state, userId)) return FriendshipStatus.REQUESTED_TO
  if (isFromPendingRequest(state, userId)) return FriendshipStatus.REQUESTED_FROM
  return FriendshipStatus.NONE
}

// @TODO! @deprecated
export async function getFriendRequests(request: GetFriendRequestsPayload) {
  const friends: FriendsState = getPrivateMessaging(store.getState())
  const realmAdapter = await ensureRealmAdapter()
  const fetchContentServerWithPrefix = getFetchContentUrlPrefixFromRealmAdapter(realmAdapter)

  const fromFriendRequests = friends.fromFriendRequests.slice(
    request.receivedSkip,
    request.receivedSkip + request.receivedLimit
  )
  const toFriendRequests = friends.toFriendRequests.slice(request.sentSkip, request.sentSkip + request.sentLimit)

  const addFriendRequestsPayload: AddFriendRequestsPayload = {
    requestedTo: toFriendRequests.map((friend) => friend.userId),
    requestedFrom: fromFriendRequests.map((friend) => friend.userId),
    totalReceivedFriendRequests: friends.fromFriendRequests.length,
    totalSentFriendRequests: friends.toFriendRequests.length
  }

  // get friend requests profiles
  const friendsIds = addFriendRequestsPayload.requestedTo.concat(addFriendRequestsPayload.requestedFrom)
  const friendRequestsProfiles: ProfileUserInfo[] = getProfilesFromStore(store.getState(), friendsIds)
  const profilesForRenderer = friendRequestsProfiles.map((friend) =>
    profileToRendererFormat(friend.data, {
      baseUrl: fetchContentServerWithPrefix
    })
  )

  // send friend requests profiles
  getUnityInstance().AddUserProfilesToCatalog({ users: profilesForRenderer })
  store.dispatch(addedProfilesToCatalog(friendRequestsProfiles.map((friend) => friend.data)))

  // send friend requests
  getUnityInstance().AddFriendRequests(addFriendRequestsPayload)
}

export async function getFriendRequestsProtocol(request: GetFriendRequestsPayload) {
  try {
    // Get friends
    const friends: FriendsState = getPrivateMessaging(store.getState())

    // Reject blocked users
    const blockedUsers = await handleBlockedUsers(friends.fromFriendRequests)
    blockedUsers
      .filter((blockedUser) => blockedUser.error)
      .forEach((blockedUser) =>
        defaultLogger.warn(`Failed while processing friend request from blocked user ${blockedUser.userId}`)
      )

    const realmAdapter = await ensureRealmAdapter()
    const fetchContentServerWithPrefix = getFetchContentUrlPrefixFromRealmAdapter(realmAdapter)

    // Paginate incoming requests
    const fromFriendRequests = friends.fromFriendRequests.slice(
      request.receivedSkip,
      request.receivedSkip + request.receivedLimit
    )
    // Paginate outgoing requests
    const toFriendRequests = friends.toFriendRequests.slice(request.sentSkip, request.sentSkip + request.sentLimit)

    // Map usersIds we need to get the profiles
    const fromIds = fromFriendRequests.map((friend) => friend.userId)
    const toIds = toFriendRequests.map((friend) => friend.userId)

    // Get profiles
    const friendsIds = toIds.concat(fromIds)
    const friendRequestsProfiles: ProfileUserInfo[] = getProfilesFromStore(store.getState(), friendsIds)
    const profilesForRenderer = friendRequestsProfiles.map((friend) =>
      profileToRendererFormat(friend.data, {
        baseUrl: fetchContentServerWithPrefix
      })
    )

    // Send profiles to unity
    getUnityInstance().AddUserProfilesToCatalog({ users: profilesForRenderer })
    store.dispatch(addedProfilesToCatalog(friendRequestsProfiles.map((friend) => friend.data)))

    // Map response
    const friendRequests: GetFriendRequestsReplyOk = {
      requestedTo: toFriendRequests.map((friend) => getFriendRequestInfo(friend, false)),
      requestedFrom: fromFriendRequests.map((friend) => getFriendRequestInfo(friend, true)),
      totalReceivedFriendRequests: friends.fromFriendRequests.length,
      totalSentFriendRequests: friends.toFriendRequests.length
    }

    // Return requests
    return { reply: friendRequests, error: undefined }
  } catch (err) {
    logAndTrackError('Error while getting friend requests via rpc', err)

    // Return error
    return { reply: undefined, error: FriendshipErrorCode.FEC_UNKNOWN }
  }
}

/**
 * Map FriendRequest to FriendRequestInfo
 * @param friend a FriendRequest type we want to map to FriendRequestInfo
 * @param incoming boolean indicating whether a request is an incoming one (requestedFrom) or not (requestedTo)
 */
function getFriendRequestInfo(friend: FriendRequest, incoming: boolean) {
  const ownId = store.getState().friends.client?.getUserId() ?? ''
  const friendRequest: FriendRequestInfo = incoming
    ? {
        friendRequestId: friend.friendRequestId,
        timestamp: friend.createdAt,
        from: friend.userId,
        to: getUserIdFromMatrix(ownId),
        messageBody: friend.message
      }
    : {
        friendRequestId: friend.friendRequestId,
        timestamp: friend.createdAt,
        from: getUserIdFromMatrix(ownId),
        to: friend.userId,
        messageBody: friend.message
      }

  return friendRequest
}

// Get mutual friends
export async function getMutualFriends(request: GetMutualFriendsRequest) {
  const client: SocialAPI | null = getSocialClient(store.getState())
  if (!client) return

  const mutuals = await client.getMutualFriends(request.userId)

  return mutuals
}

export async function markAsSeenPrivateChatMessages(userId: MarkMessagesAsSeenPayload) {
  const client: SocialAPI | null = getSocialClient(store.getState())
  if (!client) return

  // get conversation id
  const conversationId = await getConversationId(client, userId.userId)

  // get user's chat unread messages
  const unreadMessages = client.getConversationUnreadMessages(conversationId).length

  if (unreadMessages > 0) {
    // mark as seen all the messages in the conversation
    await client.markMessagesAsSeen(conversationId)
  }

  // get total user unread messages
  const friends = await getFriendIds(client)
  const totalUnreadMessages = getTotalUnseenMessages(client, client.getUserId(), friends)

  const updateUnseenMessages: UpdateUserUnseenMessagesPayload = {
    userId: userId.userId,
    total: 0
  }
  const updateTotalUnseenMessages: UpdateTotalUnseenMessagesPayload = {
    total: totalUnreadMessages
  }

  getUnityInstance().UpdateUserUnseenMessages(updateUnseenMessages)
  getUnityInstance().UpdateTotalUnseenMessages(updateTotalUnseenMessages)
}

export async function getPrivateMessages(getPrivateMessagesPayload: GetPrivateMessagesPayload) {
  const client: SocialAPI | null = getSocialClient(store.getState())
  if (!client) return

  // get the conversation.id
  const conversationId = await getConversationId(client, getPrivateMessagesPayload.userId)

  const ownId = client.getUserId()

  // get cursor of the conversation located on the given message or at the end of the conversation if there is no given message.
  const messageId: string | undefined = !getPrivateMessagesPayload.fromMessageId
    ? undefined
    : getPrivateMessagesPayload.fromMessageId

  // the message in question is in the middle of a window, so we multiply by two the limit in order to get the required messages.
  let limit = getPrivateMessagesPayload.limit
  if (messageId !== undefined) {
    limit = limit * 2
  }

  const cursorMessage = await client.getCursorOnMessage(conversationId, messageId, {
    initialSize: limit,
    limit
  })

  if (!cursorMessage) return

  const messages = cursorMessage.getMessages()
  if (messageId !== undefined) {
    // we remove the messages they already have.
    const index = messages.map((messages) => messages.id).indexOf(messageId)
    messages.splice(index)
  }

  // parse messages
  const addChatMessagesPayload: AddChatMessagesPayload = {
    messages: messages.map((message) => ({
      messageId: message.id,
      messageType: ChatMessageType.PRIVATE,
      timestamp: message.timestamp,
      body: message.text,
      sender: message.sender === ownId ? getUserIdFromMatrix(ownId) : getPrivateMessagesPayload.userId,
      recipient: message.sender === ownId ? getPrivateMessagesPayload.userId : getUserIdFromMatrix(ownId)
    }))
  }

  getUnityInstance().AddChatMessages(addChatMessagesPayload)
}

export function getUnseenMessagesByUser() {
  const conversationsWithMessages = getAllFriendsConversationsWithMessages(store.getState())

  if (conversationsWithMessages.length === 0) {
    return
  }

  const updateTotalUnseenMessagesByUserPayload: UpdateTotalUnseenMessagesByUserPayload = { unseenPrivateMessages: [] }

  for (const conversation of conversationsWithMessages) {
    updateTotalUnseenMessagesByUserPayload.unseenPrivateMessages.push({
      count: conversation.conversation.unreadMessages?.length || 0,
      userId: conversation.conversation.userIds![1]
    })
  }

  getUnityInstance().UpdateTotalUnseenMessagesByUser(updateTotalUnseenMessagesByUserPayload)
}

export async function getFriendsWithDirectMessages(request: GetFriendsWithDirectMessagesPayload) {
  const realmAdapter = await ensureRealmAdapter()
  const fetchContentServerWithPrefix = getFetchContentUrlPrefixFromRealmAdapter(realmAdapter)
  const conversationsWithMessages = getAllFriendsConversationsWithMessages(store.getState())

  if (conversationsWithMessages.length === 0) {
    return
  }

  const friendsIds: string[] = conversationsWithMessages
    .slice(request.skip, request.skip + request.limit)
    .map((conv) => conv.conversation.userIds![1])

  const filteredFriends: Array<ProfileUserInfo> = getProfilesFromStore(
    store.getState(),
    friendsIds,
    request.userNameOrId
  )

  const friendsConversations: Array<{ userId: string; conversation: Conversation; avatar: Avatar }> = []

  for (const friend of filteredFriends) {
    const conversation = conversationsWithMessages.find((conv) => conv.conversation.userIds![1] === friend.data.userId)

    if (conversation) {
      friendsConversations.push({
        userId: friend.data.userId,
        conversation: conversation.conversation,
        avatar: friend.data
      })
    }
  }

  const addFriendsWithDirectMessagesPayload: AddFriendsWithDirectMessagesPayload = {
    currentFriendsWithDirectMessages: friendsConversations.map((friend) => ({
      lastMessageTimestamp: friend.conversation.lastEventTimestamp!,
      userId: friend.userId
    })),
    totalFriendsWithDirectMessages: conversationsWithMessages.length
  }

  const profilesForRenderer = friendsConversations.map((friend) =>
    profileToRendererFormat(friend.avatar, {
      baseUrl: fetchContentServerWithPrefix
    })
  )

  getUnityInstance().AddUserProfilesToCatalog({ users: profilesForRenderer })
  store.dispatch(addedProfilesToCatalog(friendsConversations.map((friend) => friend.avatar)))

  getUnityInstance().AddFriendsWithDirectMessages(addFriendsWithDirectMessagesPayload)

  const client = getSocialClient(store.getState())
  if (!client) {
    return
  }

  const friendsSocialIds = filteredFriends.map((friend) => getMatrixIdFromUser(friend.data.userId))
  updateUserStatus(client, ...friendsSocialIds)
}

function* initializeReceivedMessagesCleanUp() {
  while (true) {
    yield delay(MESSAGE_LIFESPAN_MILLIS)
    const now = Date.now()

    Object.entries(receivedMessages)
      .filter(([, timestamp]) => now - timestamp > MESSAGE_LIFESPAN_MILLIS)
      .forEach(([id]) => delete receivedMessages[id])
  }
}

function isPeerAvatarAvailable(userId: string) {
  return !!getPeer(userId.toLowerCase())
}

function sendUpdateUserStatus(id: string, status: CurrentUserStatus) {
  const userId = parseUserId(id)

  if (!userId) return

  // treat 'unavailable' status as 'online'
  const isOnline = isPeerAvatarAvailable(userId) || status.presence !== PresenceType.OFFLINE

  const updateMessage = {
    userId,
    realm: status.realm,
    position: status.position,
    presence: isOnline ? PresenceStatus.ONLINE : PresenceStatus.OFFLINE
  }

  getUnityInstance().UpdateUserPresence(updateMessage)
}

function updateUserStatus(client: SocialAPI, ...socialIds: string[]) {
  const statuses = client.getUserStatuses(...socialIds)
  const lastStatuses = getLastStatusOfFriends(store.getState())

  statuses.forEach((value: CurrentUserStatus, key: string) => {
    const lastStatus = lastStatuses.get(key)
    // we do this in order to avoid sending already sent states.
    if (!lastStatus || !deepEqual(lastStatus, value)) {
      sendUpdateUserStatus(key, value)
      lastStatuses.set(key, value)
    }
  })
}

/**
 * This saga updates the status of our player for the Presence feature
 */
export function* initializeStatusUpdateInterval() {
  let lastStatus: UpdateUserStatus | undefined = undefined

  while (true) {
    yield race({
      SET_MATRIX_CLIENT: take(SET_MATRIX_CLIENT),
      SET_WORLD_CONTEXT: take(SET_ROOM_CONNECTION),
      timeout: delay(SEND_STATUS_INTERVAL_MILLIS)
    })

    const { client, realmConnectionString, position, rawFriends } = (yield select(
      getStatusUpdateIntervalInfo
    )) as ReturnType<typeof getStatusUpdateIntervalInfo>

    if (!client || realmConnectionString === OFFLINE_REALM) {
      continue
    }

    const friends = rawFriends.map((x) => getMatrixIdFromUser(x))

    updateUserStatus(client, ...friends)

    const updateStatus: UpdateUserStatus = {
      realm: {
        layer: '',
        serverName: realmConnectionString
      },
      position,
      presence: PresenceType.ONLINE
    }

    const shouldSendNewStatus = !deepEqual(updateStatus, lastStatus)

    if (shouldSendNewStatus) {
      logger.log('Sending new comms status', updateStatus)
      client.setStatus(updateStatus).catch((e) => logger.error(`error while setting status`, e))
      lastStatus = updateStatus
    }
  }
}
export function getStatusUpdateIntervalInfo(state: RootState) {
  return {
    client: getSocialClient(state),
    realmConnectionString: getRealmConnectionString(state),
    position: getParcelPosition(state),
    rawFriends: getPrivateMessagingFriends(state)
  }
}

/**
 * The social id for the time being should always be of the form `@ethAddress:server`
 * @param socialId a string with the aforementioned pattern
 */
function parseUserId(socialId: string) {
  if (EthAddress.validate(socialId) as any) return socialId
  const result = socialId.match(/@(\w+):.*/)
  if (!result || result.length < 2) {
    logger.warn(`Could not match social id with ethereum address, this should not happen`)
    return null
  }
  return result[1]
}

function addNewChatMessage(chatMessage: ChatMessage) {
  getUnityInstance().AddMessageToChatWindow(chatMessage)
}

function* handleSendChannelMessage(action: SendChannelMessage) {
  const { message, channelId } = action.payload

  const client: SocialAPI | null = yield select(getSocialClient)

  if (!client) {
    logger.error(`Social client should be initialized by now`)
    return
  }

  try {
    const conversation: Conversation | undefined = yield apply(client, client.getChannel, [channelId])
    if (conversation) {
      const messageId = yield apply(client, client.sendMessageTo, [conversation.id, message.body])

      if (messageId) {
        message.messageId = messageId
      }
      getUnityInstance().AddMessageToChatWindow(message)
    }
  } catch (e: any) {
    logger.error(e)
    trackEvent('error', {
      context: 'handleSendChannelMessage',
      message: e.message,
      stack: e.stack,
      saga_stack: e.toString()
    })
  }
}

function* handleSendPrivateMessage(action: SendPrivateMessage) {
  const { message, userId } = action.payload

  const client: SocialAPI | null = yield select(getSocialClient)

  if (!client) {
    logger.error(`Social client should be initialized by now`)
    return
  }

  const userData: ReturnType<typeof findPrivateMessagingFriendsByUserId> = yield select(
    findPrivateMessagingFriendsByUserId,
    userId
  )

  if (!userData) {
    logger.error(`User not found ${userId}`)
    return
  }

  try {
    const conversation: Conversation = yield apply(client, client.createDirectConversation, [userData.socialId])
    const messageId = yield apply(client, client.sendMessageTo, [conversation.id, message.body])
    if (messageId) {
      message.messageId = messageId
    }
    getUnityInstance().AddMessageToChatWindow(message)
  } catch (e: any) {
    logger.error(e)
    trackEvent('error', {
      context: 'handleSendPrivateMessage',
      message: e.message,
      stack: e.stack,
      saga_stack: e.toString()
    })
  }
}

function* handleUpdateFriendship({ payload, meta }: UpdateFriendship) {
  const { action, userId, future, messageBody } = payload

  let queryResult: ReturnType<typeof getTotalFriendsAndSocialData> | null = null

  try {
    queryResult = (yield select(getTotalFriendsAndSocialData, userId)) as ReturnType<
      typeof getTotalFriendsAndSocialData
    >
  } catch (err) {
    yield call(future.resolve, { userId, error: FriendshipErrorCode.FEC_UNKNOWN })
    return
  }
  const { client, rendererModules, newFriendRequestFlow } = queryResult

  // Get friend request module
  const friendRequestModule = rendererModules.friendRequest
  if (!friendRequestModule) {
    yield call(future.resolve, { userId, error: FriendshipErrorCode.FEC_UNKNOWN })
    return
  }

  try {
    const { state, socialData, conversationIdPromise, friendsPromise } = queryResult
    let { updateTotalFriendRequestsPayload, totalFriends } = queryResult
    let newState: FriendsState | undefined

    if (socialData) {
      try {
        yield apply(client, client.createDirectConversation, [socialData.socialId])
      } catch (e) {
        logAndTrackError('Error while creating direct conversation for friendship', e)
        yield call(future.resolve, { userId, error: FriendshipErrorCode.FEC_UNKNOWN })
        return
      }
    } else {
      // if this is the case, a previous call to ensure data load is missing, this is an issue on our end
      logger.error(`handleUpdateFriendship, user not loaded`, userId)
      yield call(future.resolve, { userId, error: FriendshipErrorCode.FEC_UNKNOWN })
      return
    }

    const incoming = meta.incoming
    const hasSentFriendshipRequest = state.toFriendRequests.some((request) => request.userId === userId)

    const ownId = client.getUserId()
    const friendRequestId = encodeFriendRequestId(ownId, userId, incoming, action)

    const friendRequestTypeSelector = hasSentFriendshipRequest ? 'toFriendRequests' : 'fromFriendRequests'
    const updateTotalFriendRequestsPayloadSelector: keyof UpdateTotalFriendRequestsPayload = hasSentFriendshipRequest
      ? 'totalSentRequests'
      : 'totalReceivedRequests'

    switch (action) {
      case FriendshipAction.NONE: {
        // do nothing
        break
      }
      case FriendshipAction.APPROVED: {
        totalFriends += 1

        // TODO!: remove FF validation once the new flow is the only one
        if (newFriendRequestFlow && incoming) {
          // Build message
          const approveFriendRequest = {
            userId: getUserIdFromMatrix(userId)
          }

          // Send message to renderer via rpc
          yield apply(friendRequestModule, friendRequestModule.approveFriendRequest, [approveFriendRequest])

          // Update notification badges
          const conversationId = yield call(async () => await conversationIdPromise)
          const unreadMessages = client.getConversationUnreadMessages(conversationId).length
          getUnityInstance().UpdateUserUnseenMessages({
            userId: getUserIdFromMatrix(userId),
            total: unreadMessages
          })

          // Update notification badges
          const friends = yield call(async () => await friendsPromise)
          const totalUnseenMessages = getTotalUnseenMessages(client, getUserIdFromMatrix(ownId), friends)
          getUnityInstance().UpdateTotalUnseenMessages({ total: totalUnseenMessages })
        }
      }
      // The approved should not have a break since it should execute all the code as the rejected case
      // Also the rejected needs to be directly after the Approved to make sure this works
      case FriendshipAction.REJECTED: {
        const requests = [...state[friendRequestTypeSelector]]

        const index = requests.findIndex((request) => request.userId === userId)

        logger.info(`requests[${friendRequestTypeSelector}]`, requests, index, userId)
        if (index !== -1) {
          requests.splice(index, 1)

          newState = { ...state, [friendRequestTypeSelector]: requests }

          if (action === FriendshipAction.APPROVED && !state.friends.includes(userId)) {
            newState.friends.push(userId)

            try {
              const conversation: Conversation = yield client.createDirectConversation(socialData.socialId)

              logger.info(`userData`, userId, socialData.socialId, conversation.id)
              newState.socialInfo[userId] = { userId, socialId: socialData.socialId, conversationId: conversation.id }
            } catch (e) {
              logAndTrackError('Error while approving/rejecting friendship', e)
            }
          }
        }

        updateTotalFriendRequestsPayload = {
          ...updateTotalFriendRequestsPayload,
          [updateTotalFriendRequestsPayloadSelector]:
            updateTotalFriendRequestsPayload[updateTotalFriendRequestsPayloadSelector] - 1
        }

        // TODO!: remove FF validation once the new flow is the only one
        if (newFriendRequestFlow && incoming && action === FriendshipAction.REJECTED) {
          // Build message
          const rejectFriendRequest = {
            userId: getUserIdFromMatrix(userId)
          }

          // Send message to renderer via rpc
          yield call(async () => await friendRequestModule.rejectFriendRequest(rejectFriendRequest))
        }

        break
      }

      case FriendshipAction.CANCELED: {
        const requests = [...state[friendRequestTypeSelector]]

        const index = requests.findIndex((request) => request.userId === userId)

        if (index !== -1) {
          requests.splice(index, 1)

          newState = { ...state, [friendRequestTypeSelector]: requests }
        }

        updateTotalFriendRequestsPayload = {
          ...updateTotalFriendRequestsPayload,
          [updateTotalFriendRequestsPayloadSelector]:
            updateTotalFriendRequestsPayload[updateTotalFriendRequestsPayloadSelector] - 1
        }

        // TODO!: remove FF validation once the new flow is the only one
        if (newFriendRequestFlow && incoming) {
          // Build message
          const cancelFriendRequest = {
            userId: getUserIdFromMatrix(userId)
          }

          // Send message to renderer via rpc
          yield apply(friendRequestModule, friendRequestModule.cancelFriendRequest, [cancelFriendRequest])
        }

        break
      }
      case FriendshipAction.REQUESTED_FROM: {
        const request = state.fromFriendRequests.find((request) => request.userId === userId)

        if (!request) {
          newState = {
            ...state,
            fromFriendRequests: [
              ...state.fromFriendRequests,
              { createdAt: Date.now(), userId, friendRequestId, message: messageBody }
            ]
          }
        }

        updateTotalFriendRequestsPayload = {
          ...updateTotalFriendRequestsPayload,
          totalReceivedRequests: updateTotalFriendRequestsPayload.totalReceivedRequests + 1
        }

        // TODO!: remove FF validation once the new flow is the only one
        if (newFriendRequestFlow && incoming) {
          if (!isBlocked(userId)) {
            // Build message
            const receiveFriendRequest: ReceiveFriendRequestPayload = {
              friendRequest: {
                friendRequestId,
                timestamp: Date.now(),
                to: getUserIdFromMatrix(ownId),
                from: getUserIdFromMatrix(userId),
                messageBody
              }
            }

            // Send messsage to renderer via rpc
            yield apply(friendRequestModule, friendRequestModule.receiveFriendRequest, [receiveFriendRequest])
          }
        }

        break
      }
      case FriendshipAction.REQUESTED_TO: {
        const request = state.toFriendRequests.find((request) => request.userId === userId)

        if (!request) {
          newState = {
            ...state,
            toFriendRequests: [
              ...state.toFriendRequests,
              { createdAt: Date.now(), userId, friendRequestId, message: messageBody }
            ]
          }
        }

        updateTotalFriendRequestsPayload = {
          ...updateTotalFriendRequestsPayload,
          totalSentRequests: updateTotalFriendRequestsPayload.totalSentRequests + 1
        }

        break
      }
      case FriendshipAction.DELETED: {
        if (state.friends.includes(userId)) {
          const index = state.friends.indexOf(userId)
          const friends = [...state.friends]
          friends.splice(index, 1)
          newState = { ...state, friends }
        }

        totalFriends -= 1
        break
      }
    }

    getUnityInstance().UpdateTotalFriendRequests(updateTotalFriendRequestsPayload)
    getUnityInstance().UpdateTotalFriends({ totalFriends })

    if (newState) {
      yield put(updatePrivateMessagingState(newState))

      if (incoming) {
        yield call(waitForRendererInstance)
      } else {
        yield call(handleOutgoingUpdateFriendshipStatus, payload)
      }

      // TODO!: remove FF validation once the new flow is the only one
      // We only send the UpdateFriendshipStatus message when:
      // + The new friend request flow is disabled
      // + The new friend request flow is enabled and the action is an incoming/outgoing delete
      if (!newFriendRequestFlow || (newFriendRequestFlow && action === FriendshipAction.DELETED)) {
        getUnityInstance().UpdateFriendshipStatus(payload)
      }
    }

    if (!incoming) {
      // refresh self & renderer friends status if update was triggered by renderer
      yield call(refreshFriends)
    }

    // Updates the friendship status of a blocked user to rejected after processing their incoming friend request.
    if (FriendshipAction.REQUESTED_FROM === action && isBlocked(userId) && newFriendRequestFlow && incoming) {
      yield call(handleBlockedUser, userId, messageBody)
    }

    yield call(future.resolve, { userId, error: null })
  } catch (e) {
    if (e instanceof UnknownUsersError) {
      const profile: Avatar | undefined = yield call(ensureFriendProfile, userId)
      const id = profile?.name ? profile.name : `with address '${userId}'`
      showErrorNotification(`User ${id} must log in at least once before befriending them`)
    }

    // in case of any error, re initialize friends, to possibly correct state in both kernel and renderer
    yield call(refreshFriends)

    yield call(future.resolve, { userId, error: FriendshipErrorCode.FEC_UNKNOWN })
  }
}

function getTotalFriendsAndSocialData(rootState: RootState, userId: string) {
  const client = getSocialClient(rootState)
  const rendererModules = getRendererModules(rootState)
  if (!client || !rendererModules) {
    throw new Error('Invalid client or rendererModules')
  }
  return {
    client,
    rendererModules,
    friendsPromise: () => getFriendIds(client),
    newFriendRequestFlow: isNewFriendRequestEnabled(rootState),
    state: getPrivateMessaging(rootState),
    socialData: findPrivateMessagingFriendsByUserId(rootState, userId),
    conversationIdPromise: () => getConversationId(client, getUserIdFromMatrix(userId)),
    updateTotalFriendRequestsPayload: getTotalFriendRequests(rootState),
    totalFriends: getTotalFriends(rootState)
  }
}

function* trackEvents({ payload }: UpdateFriendship) {
  const { action } = payload
  switch (action) {
    case FriendshipAction.APPROVED: {
      trackEvent('Control Friend request approved', {})
      break
    }
    case FriendshipAction.REJECTED: {
      trackEvent('Control Friend request rejected', {})
      break
    }
    case FriendshipAction.CANCELED: {
      trackEvent('Control Friend request cancelled', {})
      break
    }
    case FriendshipAction.REQUESTED_FROM: {
      trackEvent('Control Friend request received', {})
      break
    }
    case FriendshipAction.REQUESTED_TO: {
      trackEvent('Control Friend request sent', {})
      break
    }
    case FriendshipAction.DELETED: {
      trackEvent('Control Friend deleted', {})
      break
    }
  }
}

function showErrorNotification(message: string) {
  getUnityInstance().ShowNotification({
    type: NotificationType.GENERIC,
    message,
    buttonMessage: 'OK',
    timer: 5
  })
}

function* handleOutgoingUpdateFriendshipStatus(update: UpdateFriendship['payload']) {
  const client: SocialAPI | undefined = yield select(getSocialClient)
  const socialData: SocialData = yield select(findPrivateMessagingFriendsByUserId, update.userId)

  if (!client) {
    yield call(update.future.resolve, { userId: update.userId, error: FriendshipErrorCode.FEC_UNKNOWN })
    return
  }

  if (!socialData) {
    logger.error(`could not find social data for`, update.userId)
    yield call(update.future.resolve, { userId: update.userId, error: FriendshipErrorCode.FEC_UNKNOWN })
    return
  }

  const { socialId } = socialData

  try {
    switch (update.action) {
      case FriendshipAction.NONE: {
        // do nothing in this case
        // this action should never happen
        break
      }
      case FriendshipAction.APPROVED: {
        yield client.approveFriendshipRequestFrom(socialId)
        updateUserStatus(client, socialId)
        break
      }
      case FriendshipAction.REJECTED: {
        yield client.rejectFriendshipRequestFrom(socialId)
        break
      }
      case FriendshipAction.CANCELED: {
        yield client.cancelFriendshipRequestTo(socialId)
        break
      }
      case FriendshipAction.REQUESTED_FROM: {
        // do nothing in this case
        break
      }
      case FriendshipAction.REQUESTED_TO: {
        yield client.addAsFriend(socialId, update.messageBody)
        yield call(update.future.resolve, { userId: update.userId, error: null })
        break
      }
      case FriendshipAction.DELETED: {
        yield client.deleteFriendshipWith(socialId)
        break
      }
    }
  } catch (e) {
    logAndTrackError('error while acting user friendship action', e)
  }

  // wait for matrix server to process new status
  yield delay(500)
}

function toSocialData(socialIds: string[]) {
  return socialIds
    .map((socialId) => ({
      userId: parseUserId(socialId),
      socialId
    }))
    .filter(({ userId }) => !!userId) as SocialData[]
}

function logAndTrackError(message: string, e: any) {
  const isSynapseEnabled = getFeatureFlagEnabled(store.getState(), 'use-synapse-server')
  const variant = isSynapseEnabled ? `Synapse` : `Social Service`
  const msg = `${variant} - ${message}`

  logger.error(msg, e)
  trackEvent('error', {
    context: 'kernel#saga',
    message: msg,
    stack: '' + e
  })
}

/**
 * Get the conversation id from the store when possible.
 * If not, then fetch it from matrix and update the private messaging state
 * @param client SocialAPI client
 * @param userId a string with the userId pattern
 */
async function getConversationId(client: SocialAPI, userId: string) {
  let conversationId = findPrivateMessagingFriendsByUserId(store.getState(), userId)?.conversationId

  if (!conversationId) {
    const socialId = getMatrixIdFromUser(userId)
    const conversation: Conversation = await client.createDirectConversation(socialId)

    const socialData: SocialData = {
      userId: userId,
      socialId: socialId,
      conversationId: conversation.id
    }

    updateSocialInfo(socialData)
    conversationId = conversation.id
  }

  return conversationId
}

/**
 * Update the social info from the private messaging state
 * @param socialData the social data to add to the record.
 */
function updateSocialInfo(socialData: SocialData) {
  const friends: FriendsState = getPrivateMessaging(store.getState())

  // add social info
  friends.socialInfo[socialData.socialId] = socialData

  put(
    updatePrivateMessagingState({
      ...friends
    })
  )
}

function* handleLeaveChannel(action: LeaveChannel) {
  try {
    const client = getSocialClient(store.getState())
    if (!client) return

    const channelId = action.payload.channelId
    yield apply(client, client.leaveChannel, [channelId])

    const profile = getCurrentUserProfile(store.getState())
    // if channel is muted, let's reset that config
    if (profile?.muted?.includes(channelId)) {
      store.dispatch(unmutePlayers([channelId]))
    }
  } catch (e) {
    notifyLeaveChannelError(action.payload.channelId, ChannelErrorCode.UNKNOWN)
  }
}

// Join or create channel via command
function* handleJoinOrCreateChannel(action: JoinOrCreateChannel) {
  try {
    const client: SocialAPI | null = getSocialClient(store.getState())
    if (!client) return

    const channelId = action.payload.channelId.toLowerCase()

    const reachedLimit = checkChannelsLimit()
    if (reachedLimit) {
      notifyJoinChannelError(channelId, ChannelErrorCode.LIMIT_EXCEEDED)
      return
    }

    // check if the user has perms to create channels.
    const isAllowed = isAllowedToCreate()
    if (isAllowed) {
      const { created, conversation }: GetOrCreateConversationResponse = yield apply(
        client,
        client.getOrCreateChannel,
        [channelId, []]
      )

      const channel: ChannelInfoPayload = {
        name: channelId,
        channelId: conversation.id,
        unseenMessages: 0,
        lastMessageTimestamp: undefined,
        memberCount: 1,
        description: '',
        joined: true,
        muted: false
      }

      if (created) {
        getUnityInstance().JoinChannelConfirmation({ channelInfoPayload: [channel] })
      } else {
        yield apply(client, client.joinChannel, [conversation.id])
      }
      // if the user does not have perms to create, we check if the channel exists and join if so.
    } else {
      const channelByName = yield apply(client, client.getChannelByName, [channelId])

      if (channelByName) {
        yield apply(client, client.joinChannel, [channelByName.id])
      } else {
        getUnityInstance().AddMessageToChatWindow({
          messageType: ChatMessageType.SYSTEM,
          messageId: uuid(),
          sender: 'Decentraland',
          body: `Ups, sorry! It seems you don't have permissions to create a channel.`,
          timestamp: Date.now()
        })
      }
    }
  } catch (e) {
    if (e instanceof ChannelsError) {
      let errorCode = ChannelErrorCode.UNKNOWN
      if (e.getKind() === ChannelErrorKind.BAD_REGEX) {
        errorCode = ChannelErrorCode.WRONG_FORMAT
      } else if (e.getKind() === ChannelErrorKind.RESERVED_NAME) {
        errorCode = ChannelErrorCode.RESERVED_NAME
      }
      notifyJoinChannelError(action.payload.channelId, errorCode)
    }
  }
}

// Join channel via UI
export async function joinChannel(request: JoinOrCreateChannelPayload) {
  try {
    const client: SocialAPI | null = getSocialClient(store.getState())
    if (!client) return

    const channelId = request.channelId

    const reachedLimit = checkChannelsLimit()
    if (reachedLimit) {
      notifyJoinChannelError(channelId, ChannelErrorCode.LIMIT_EXCEEDED)
      return
    }

    await client.joinChannel(channelId)
  } catch (e) {
    notifyJoinChannelError(request.channelId, ChannelErrorCode.UNKNOWN)
  }
}

// Create channel via UI
export async function createChannel(request: CreateChannelPayload) {
  try {
    const channelId = request.channelId

    const reachedLimit = checkChannelsLimit()
    if (reachedLimit) {
      notifyJoinChannelError(channelId, ChannelErrorCode.LIMIT_EXCEEDED)
      return
    }

    const client: SocialAPI | null = getSocialClient(store.getState())
    if (!client) return

    // create channel
    const { conversation, created } = await client.getOrCreateChannel(channelId, [])

    // if it already exists, we notify an error
    if (!created) {
      notifyJoinChannelError(request.channelId, ChannelErrorCode.ALREADY_EXISTS)
      return
    }

    const channel: ChannelInfoPayload = {
      name: conversation.name ?? request.channelId,
      channelId: conversation.id,
      unseenMessages: 0,
      lastMessageTimestamp: undefined,
      memberCount: 1,
      description: '',
      joined: true,
      muted: false
    }

    getUnityInstance().JoinChannelConfirmation({ channelInfoPayload: [channel] })
  } catch (e) {
    if (e instanceof ChannelsError) {
      let errorCode = ChannelErrorCode.UNKNOWN
      if (e.getKind() === ChannelErrorKind.BAD_REGEX) {
        errorCode = ChannelErrorCode.WRONG_FORMAT
      } else if (e.getKind() === ChannelErrorKind.RESERVED_NAME) {
        errorCode = ChannelErrorCode.RESERVED_NAME
      }
      notifyJoinChannelError(request.channelId, errorCode)
    }
  }
}

// Get unseen messages by channel
export function getUnseenMessagesByChannel() {
  // get conversations messages
  const updateTotalUnseenMessagesByChannelPayload: UpdateTotalUnseenMessagesByChannelPayload =
    getTotalUnseenMessagesByChannel()

  // send total unseen messages by channels to unity
  getUnityInstance().UpdateTotalUnseenMessagesByChannel(updateTotalUnseenMessagesByChannelPayload)
}

// Get user's joined channels
export function getJoinedChannels(request: GetJoinedChannelsPayload) {
  const client = getSocialClient(store.getState())
  if (!client) return []

  // get user joined channels
  const joinedChannels = getChannels(store.getState())

  const conversationsFiltered = joinedChannels.slice(request.skip, request.skip + request.limit)

  const profile = getCurrentUserProfile(store.getState())

  const channelsToReturn: ChannelInfoPayload[] = conversationsFiltered.map((conv) => ({
    name: conv.conversation.name || '',
    channelId: conv.conversation.id,
    unseenMessages: conv.conversation.unreadMessages?.length || 0,
    lastMessageTimestamp: conv.conversation.lastEventTimestamp || undefined,
    memberCount: getOnlineOrJoinedMembersCount(client, conv.conversation),
    description: '',
    joined: true,
    muted: profile?.muted?.includes(conv.conversation.id) ?? false
  }))

  getUnityInstance().UpdateChannelInfo({ channelInfoPayload: channelsToReturn })
}

// Mark channel messages as seen
export async function markAsSeenChannelMessages(request: MarkChannelMessagesAsSeenPayload) {
  const client: SocialAPI | null = getSocialClient(store.getState())
  if (!client) return

  const ownId = client.getUserId()

  // get user's chat unread messages
  const unreadMessages = client.getConversationUnreadMessages(request.channelId).length

  if (unreadMessages > 0) {
    // mark as seen all the messages in the conversation
    await client.markMessagesAsSeen(request.channelId)
  }

  // get total user unread messages
  const friends = await getFriendIds(client)
  const totalUnreadMessages = getTotalUnseenMessages(client, ownId, friends)
  const updateTotalUnseenMessages: UpdateTotalUnseenMessagesPayload = {
    total: totalUnreadMessages
  }

  // get total unseen messages by channel
  const updateTotalUnseenMessagesByChannel: UpdateTotalUnseenMessagesByChannelPayload =
    getTotalUnseenMessagesByChannel()

  getUnityInstance().UpdateTotalUnseenMessagesByChannel(updateTotalUnseenMessagesByChannel)
  getUnityInstance().UpdateTotalUnseenMessages(updateTotalUnseenMessages)
}

// Get channel messages
export async function getChannelMessages(request: GetChannelMessagesPayload) {
  const client: SocialAPI | null = getSocialClient(store.getState())
  if (!client) return

  // get cursor of the conversation located on the given message or at the end of the conversation if there is no given message.
  const messageId: string | undefined = !request.from ? undefined : request.from

  // the message in question is in the middle of a window, so we multiply by two the limit in order to get the required messages.
  let limit = request.limit
  if (messageId !== undefined) {
    limit = limit * 2
  }

  const cursorMessage = await client.getCursorOnMessage(request.channelId, messageId, {
    initialSize: limit,
    limit
  })

  if (!cursorMessage) return

  // get list of messages currently in the window with the oldest event at index 0
  const messages = cursorMessage.getMessages()
  if (messageId !== undefined) {
    // we remove the messages they already have.
    const index = messages.map((messages) => messages.id).indexOf(messageId)
    messages.splice(index)
  }
  const ownId = client.getUserId()

  // deduplicate sender IDs
  const senderIds = Array.from(new Set(messages.map((message) => message.sender)))

  // get members from user IDs
  const members = getMembers(client, senderIds, request.channelId)

  // update catalog with missing users, by using default profiles with name and image url
  sendMissingProfiles(members, ownId)

  const addChatMessages: AddChatMessagesPayload = {
    messages: []
  }

  for (const message of messages) {
    const sender = getUserIdFromMatrix(message.sender)

    addChatMessages.messages.push({
      messageId: message.id,
      messageType: ChatMessageType.PUBLIC,
      timestamp: message.timestamp,
      body: message.text,
      sender,
      senderName: members.find((member) => member.userId === sender)?.name,
      recipient: request.channelId
    })
  }

  getUnityInstance().AddChatMessages(addChatMessages)
}

// Find members that are not added to the catalog. It filters the own profile.
function findMissingMembers(members: ChannelMember[], ownId: string) {
  return members.filter((member) => {
    const localUserId = getUserIdFromMatrix(member.userId)
    return member.userId !== ownId && !isAddedToCatalog(store.getState(), localUserId)
  })
}

function getMembers(client: SocialAPI, userIds: string[], channelId: string) {
  return userIds.map((userId): ChannelMember => {
    const memberInfo = client.getMemberInfo(channelId, userId)
    // ensure member user id is fully qualified, in some cases the userId is just the localpart
    const normalizedUserId = getMatrixIdFromUser(userId)
    return { userId: normalizedUserId, name: memberInfo.displayName ?? '' }
  })
}

// Search channels
export async function searchChannels(request: GetChannelsPayload) {
  const client: SocialAPI | null = getSocialClient(store.getState())
  if (!client) return

  const searchTerm = request.name === '' ? undefined : request.name
  const since: string | undefined = request.since === '' ? undefined : request.since

  // get user joined channelIds
  const joinedChannelIds = getChannels(store.getState()).map((conv) => conv.conversation.id)

  const profile = getCurrentUserProfile(store.getState())

  // search channels
  const { channels, nextBatch } = await client.searchChannel(request.limit, searchTerm, since)

  const channelsToReturn: ChannelInfoPayload[] = channels
    .filter((channel) => channel.name?.includes(searchTerm ?? ''))
    .map((channel) => ({
      channelId: channel.id,
      name: channel.name || '',
      unseenMessages: 0,
      lastMessageTimestamp: undefined,
      memberCount: channel.memberCount,
      description: channel.description || '',
      joined: joinedChannelIds.includes(channel.id),
      muted: profile?.muted?.includes(channel.id) ?? false
    }))

  // sort in descending order by memberCount value
  const channelsSorted = channelsToReturn.sort((a, b) => (a.memberCount > b.memberCount ? -1 : 1))

  const searchResult: ChannelSearchResultsPayload = {
    since: nextBatch === undefined ? null : nextBatch,
    channels: channelsSorted
  }

  getUnityInstance().UpdateChannelSearchResults(searchResult)
}

/**
 * Send join/create channel related error message to unity
 * @param channelId
 * @param errorCode
 */
function notifyJoinChannelError(channelId: string, errorCode: number) {
  const joinChannelError: ChannelErrorPayload = {
    channelId,
    errorCode
  }

  // send error message to unity
  getUnityInstance().JoinChannelError(joinChannelError)
}

/**
 * Send leave channel related error message to unity
 * @param channelId
 * @param errorCode
 */
function notifyLeaveChannelError(channelId: string, errorCode: ChannelErrorCode) {
  const leaveChannelError: ChannelErrorPayload = {
    channelId,
    errorCode
  }
  getUnityInstance().LeaveChannelError(leaveChannelError)
}

/**
 * Send mute/unmute channel related error message to unity
 * @param channelId
 * @param errorCode
 */
function notifyMuteChannelError(channelId: string, errorCode: ChannelErrorCode) {
  const muteChannelError: ChannelErrorPayload = {
    channelId,
    errorCode
  }
  getUnityInstance().MuteChannelError(muteChannelError)
}

/**
 * Get list of total unseen messages by channelId
 */
function getTotalUnseenMessagesByChannel() {
  // get conversations messages
  const conversationsWithMessages = getChannels(store.getState())

  const updateTotalUnseenMessagesByChannelPayload: UpdateTotalUnseenMessagesByChannelPayload = {
    unseenChannelMessages: []
  }

  // it means the user is not joined to any channel or they're joined to channels without messages
  if (conversationsWithMessages.length === 0) {
    return updateTotalUnseenMessagesByChannelPayload
  }

  const client: SocialAPI | null = getSocialClient(store.getState())
  if (!client) {
    return updateTotalUnseenMessagesByChannelPayload
  }

  // get muted channel ids
  const mutedIds = getCurrentUserProfile(store.getState())?.muted

  for (const conv of conversationsWithMessages) {
    // prevent from counting unread messages of muted channels
    updateTotalUnseenMessagesByChannelPayload.unseenChannelMessages.push({
      count: mutedIds?.includes(conv.conversation.id) ? 0 : conv.conversation.unreadMessages?.length || 0,
      channelId: conv.conversation.id
    })
  }

  return updateTotalUnseenMessagesByChannelPayload
}

// Enable / disable channel notifications
export function muteChannel(muteChannel: MuteChannelPayload) {
  const client: SocialAPI | null = getSocialClient(store.getState())
  if (!client) return

  const channelId = muteChannel.channelId

  const channel: Conversation | undefined = client.getChannel(channelId)
  if (!channel) {
    notifyMuteChannelError(channelId, ChannelErrorCode.UNKNOWN)
    return
  }

  // mute / unmute channel
  if (muteChannel.muted) {
    store.dispatch(mutePlayers([channelId]))
  } else {
    store.dispatch(unmutePlayers([channelId]))
  }

  const onlineMembers = getOnlineOrJoinedMembersCount(client, channel)

  const channelInfo: ChannelInfoPayload = {
    name: channel.name ?? '',
    channelId: channel.id,
    unseenMessages: channel.unreadMessages?.length ?? 0,
    lastMessageTimestamp: channel.lastEventTimestamp ?? undefined,
    memberCount: onlineMembers,
    description: '',
    joined: true,
    muted: muteChannel.muted
  }

  // send message to unity
  getUnityInstance().UpdateChannelInfo({ channelInfoPayload: [channelInfo] })
}

/**
 * Get the number of channels the user is joined to and check with a feature flag value if the user has reached the maximum amount allowed.
 * @return `true` if the user has reached the maximum amount allowed | `false` if it has not.
 */
function checkChannelsLimit() {
  const limit = getMaxChannels(store.getState())

  const joinedChannels = getChannels(store.getState()).length

  if (limit > joinedChannels) {
    return false
  }

  return true
}

// Get channel info
export function getChannelInfo(request: GetChannelInfoPayload) {
  const client: SocialAPI | null = getSocialClient(store.getState())
  if (!client) return

  // get notification settings
  const profile = getCurrentUserProfile(store.getState())
  const channels: ChannelInfoPayload[] = []

  for (const channelId of request.channelIds) {
    const channel = client.getChannel(channelId)
    if (!channel) continue

    const muted = profile?.muted?.includes(channelId) ?? false

    const onlineMembers = getOnlineOrJoinedMembersCount(client, channel)

    channels.push({
      name: getNormalizedRoomName(channel.name || ''),
      channelId: channel.id,
      unseenMessages: muted ? 0 : channel.unreadMessages?.length || 0,
      lastMessageTimestamp: channel.lastEventTimestamp || undefined,
      memberCount: onlineMembers,
      description: '',
      joined: true,
      muted
    })
  }
  getUnityInstance().UpdateChannelInfo({ channelInfoPayload: channels })
}

// Get channel members
export async function getChannelMembers(request: GetChannelMembersPayload) {
  const client: SocialAPI | null = getSocialClient(store.getState())
  if (!client) return

  const channel = client.getChannel(request.channelId)
  if (!channel) return

  const channelMembersPayload: UpdateChannelMembersPayload = {
    channelId: request.channelId,
    members: []
  }

  const allMembers = getMembers(client, channel.userIds ?? [], request.channelId).filter(({ name }) => {
    const searchTerm = request.userName.toLocaleLowerCase()
    const lowerCaseName = name.toLocaleLowerCase()
    return lowerCaseName.search(searchTerm) >= 0
  })

  if (allMembers.length === 0) {
    // it means the channel has no members
    getUnityInstance().UpdateChannelMembers(channelMembersPayload)
    return
  }

  // we only notify members who are online if presence is enabled, else every joined member
  const memberIds = allMembers.map((member) => member.userId)
  const onlineOrJoinedMemberIds = getOnlineOrJoinedMembers(memberIds, client)

  // we filter the online members and apply the skip and limit pagination
  const membersProfiles = allMembers
    .filter((member) => onlineOrJoinedMemberIds.includes(member.userId))
    .slice(request.skip, request.skip + request.limit)

  // update catalog with missing users, by using default profiles with name and image url
  const ownId = client.getUserId()
  sendMissingProfiles(membersProfiles, ownId)

  // TODO - should we avoid setting `isOnline` when presence is disabled? - moliva - 2022/11/09
  const membersPayload = membersProfiles.map(
    (member) => ((member.userId = getUserIdFromMatrix(member.userId)), { ...member, isOnline: true })
  )

  // send info to unity
  channelMembersPayload.members.push(...membersPayload)
  getUnityInstance().UpdateChannelMembers(channelMembersPayload)
}

export async function requestFriendship(request: SendFriendRequestPayload) {
  try {
    let userId = request.userId
    let found = false
    const state = store.getState()

    const ownId = getOwnId(state)

    if (!ownId) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_UNKNOWN)
    }

    // Search user profile on server
    if (isAddress(userId)) {
      // Ensure user profile is initialized and send it to renderer
      const avatar = await ensureFriendProfile(userId)
      found = avatar.hasConnectedWeb3 || false
    } else {
      const profileByName = findProfileByName(state, userId)
      if (profileByName) {
        userId = profileByName.userId
        found = true
      }
    }

    // If user profile was not found on the server -> no connected web3, check if it has a name claimed
    if (!found) {
      const net = getSelectedNetwork(state)
      const address = await fetchENSOwner(ethereumConfigurations[net].names, userId)
      if (address) {
        // If an address was found by the name, set it as user id
        userId = address
        found = true
      }
    }

    if (!found) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_NON_EXISTING_USER)
    }

    // Check if the user is trying to send a friend request to themself
    if (getUserIdFromMatrix(ownId) === userId) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_INVALID_REQUEST)
    }

    // Check if the users are already friends or if a friend request has already been sent.
    if (isFriend(store.getState(), userId) || isToPendingRequest(store.getState(), userId)) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_INVALID_REQUEST)
    }

    // Check whether the user has reached the max number of sent requests to a given user
    const maxNumberOfRequests = reachedMaxNumberOfRequests(userId)
    if (maxNumberOfRequests) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_TOO_MANY_REQUESTS_SENT)
    }

    // Check whether there is a remaining cooldown time to send a friend request to a given user.
    const cooldown = hasRemainingCooldown(userId)
    if (cooldown) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_NOT_ENOUGH_TIME_PASSED)
    }

    // Update user data
    store.dispatch(updateUserData(userId.toLowerCase(), getMatrixIdFromUser(userId)))

    // Add as friend
    const response = await UpdateFriendshipAsPromise(
      FriendshipAction.REQUESTED_TO,
      userId.toLowerCase(),
      false,
      request.messageBody
    )

    if (!response.error) {
      const sendFriendRequest: SendFriendRequestReplyOk = {
        friendRequest: {
          friendRequestId: encodeFriendRequestId(ownId, userId, false, FriendshipAction.REQUESTED_TO),
          timestamp: Date.now(),
          from: getUserIdFromMatrix(ownId),
          to: userId,
          messageBody: request.messageBody
        }
      }

      // Update state
      updateFriendsState(userId)

      // Return response
      return buildFriendRequestReply(sendFriendRequest)
    } else {
      // Return error
      return buildFriendRequestErrorResponse(response.error)
    }
  } catch (err) {
    logAndTrackError('Error while sending friend request via rpc', err)

    // Return error
    return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_UNKNOWN)
  }
}

export async function UpdateFriendshipAsPromise(
  action: FriendshipAction,
  userId: string,
  incoming: boolean,
  messageBody?: string
): Promise<{ userId: string; error: FriendshipErrorCode | null }> {
  const fut = future<{ userId: string; error: FriendshipErrorCode | null }>()
  store.dispatch(updateFriendship(action, userId.toLowerCase(), incoming, fut, messageBody))
  return fut
}

export async function cancelFriendRequest(request: CancelFriendRequestPayload) {
  try {
    // Get ownId value
    const ownId = getOwnId(store.getState())
    if (!ownId) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_UNKNOWN)
    }

    // Validate request
    const isValid = validateFriendRequestId(request.friendRequestId, ownId)
    if (!isValid) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_INVALID_REQUEST)
    }

    // Get otherUserId value
    const userId = decodeFriendRequestId(request.friendRequestId, ownId)

    // Search in the store for the message body
    const messageBody = getMessageBody(store.getState(), request.friendRequestId)

    // Update user data
    store.dispatch(updateUserData(userId.toLowerCase(), getMatrixIdFromUser(userId)))

    // Add as friend
    const response = await UpdateFriendshipAsPromise(FriendshipAction.CANCELED, userId.toLowerCase(), false)

    if (!response.error) {
      const cancelFriendRequest: CancelFriendRequestReplyOk = {
        friendRequest: {
          friendRequestId: request.friendRequestId,
          timestamp: Date.now(),
          from: getUserIdFromMatrix(ownId),
          to: userId,
          messageBody
        }
      }

      // Return response
      return buildFriendRequestReply(cancelFriendRequest)
    } else {
      // Return error
      return buildFriendRequestErrorResponse(response.error)
    }
  } catch (err) {
    logAndTrackError('Error while canceling friend request via rpc', err)

    // Return error
    return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_UNKNOWN)
  }
}

export async function rejectFriendRequest(request: RejectFriendRequestPayload) {
  try {
    // Get ownId value
    const ownId = getOwnId(store.getState())
    if (!ownId) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_UNKNOWN)
    }

    // Validate request
    const isValid = validateFriendRequestId(request.friendRequestId, ownId)
    if (!isValid) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_INVALID_REQUEST)
    }

    // Get otherUserId value
    const userId = decodeFriendRequestId(request.friendRequestId, ownId)

    // Search in the store for the message body
    const messageBody = getMessageBody(store.getState(), request.friendRequestId)

    // Update user data
    store.dispatch(updateUserData(userId.toLowerCase(), getMatrixIdFromUser(userId)))

    // Add as friend
    const response = await UpdateFriendshipAsPromise(FriendshipAction.REJECTED, userId.toLowerCase(), false)

    if (!response.error) {
      const rejectFriendRequest: RejectFriendRequestReplyOk = {
        friendRequest: {
          friendRequestId: request.friendRequestId,
          timestamp: Date.now(),
          from: userId,
          to: getUserIdFromMatrix(ownId),
          messageBody
        }
      }

      // Return response
      return buildFriendRequestReply(rejectFriendRequest)
    } else {
      // Return error
      return buildFriendRequestErrorResponse(response.error)
    }
  } catch (err) {
    logAndTrackError('Error while canceling friend request via rpc', err)

    // Return error
    return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_UNKNOWN)
  }
}

export async function acceptFriendRequest(request: AcceptFriendRequestPayload) {
  try {
    // Get ownId value
    const ownId = getOwnId(store.getState())
    if (!ownId) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_UNKNOWN)
    }

    // Validate request
    const isValid = validateFriendRequestId(request.friendRequestId, ownId)
    if (!isValid) {
      return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_INVALID_REQUEST)
    }

    // Get otherUserId value
    const userId = decodeFriendRequestId(request.friendRequestId, ownId)

    // Search in the store for the message body
    const messageBody = getMessageBody(store.getState(), request.friendRequestId)

    // Update user data
    store.dispatch(updateUserData(userId.toLowerCase(), getMatrixIdFromUser(userId)))

    // Add as friend
    const response = await UpdateFriendshipAsPromise(FriendshipAction.APPROVED, userId.toLowerCase(), false)

    if (!response.error) {
      const acceptFriendRequest: AcceptFriendRequestReplyOk = {
        friendRequest: {
          friendRequestId: request.friendRequestId,
          timestamp: Date.now(),
          from: userId,
          to: getUserIdFromMatrix(ownId),
          messageBody
        }
      }

      // Return response
      return buildFriendRequestReply(acceptFriendRequest)
    } else {
      // Return error
      return buildFriendRequestErrorResponse(response.error)
    }
  } catch (err) {
    logAndTrackError('Error while accepting friend request via rpc', err)

    // Return error
    return buildFriendRequestErrorResponse(FriendshipErrorCode.FEC_UNKNOWN)
  }
}

/**
 * TODO: This method should be removed once we implement the correct member resolution in Explorer
 * Checks which members are present in the profile catalog and sends partial profiles for missing users
 * @param members is an array of [member ID, name]
 */
function sendMissingProfiles(members: ChannelMember[], ownId: string) {
  // find missing users
  const missingUsers = findMissingMembers(members, ownId)

  if (missingUsers.length > 0) {
    const missingProfiles = getMissingProfiles(missingUsers)
    getUnityInstance().AddUserProfilesToCatalog({ users: missingProfiles })
  }
}

// TODO: This method should be removed once we implement the correct member resolution in Explorer
function getMissingProfiles(missingUsers: ChannelMember[]): NewProfileForRenderer[] {
  return missingUsers.map((missingUser) => buildMissingProfile(missingUser))
}

// TODO: This method should be removed once we implement the correct member resolution in Explorer
function buildMissingProfile(user: ChannelMember) {
  const localpart = getUserIdFromMatrix(user.userId)
  return defaultProfile({
    userId: localpart,
    name: user.name,
    face256: buildProfilePictureURL(localpart)
  })
}

// TODO: This method should be removed once we implement the correct member resolution in Explorer
function buildProfilePictureURL(userId: string): string {
  const synapseUrl = getSynapseUrl(store.getState())
  return `${synapseUrl}/profile-pictures/${userId}`
}

/**
 * Check with a feature flag value if the user is allowed to create channels.
 * @return `true` if the user is allowed | `false` if it is not.
 */
function isAllowedToCreate() {
  const allowedUsers = getUsersAllowedToCreate(store.getState())
  const currentUserId = getCurrentUserId(store.getState())

  if (!allowedUsers || !currentUserId || allowedUsers.mode !== 0) {
    return false
  }

  if (allowedUsers.allowList.includes(currentUserId)) {
    return true
  }
}

function getOnlineOrJoinedMembers(userIds: string[], client: SocialAPI): string[] {
  const presenceDisabled = getFeatureFlagEnabled(store.getState(), 'matrix_presence_disabled')

  if (presenceDisabled) {
    return userIds
  }

  return getOnlineMembers(userIds, client)
}

/**
 * Filter members online from a given list of user ids.
 * @return `string[]` with the ids of the members who are online.
 */
function getOnlineMembers(userIds: string[], client: SocialAPI): string[] {
  const userStatuses = client.getUserStatuses(...userIds)
  const onlineMembers = userIds.filter((id) => userStatuses.get(id)?.presence === PresenceType.ONLINE)

  return onlineMembers
}

function getOnlineMembersCount(client: SocialAPI, userIds?: string[]): number {
  if (!userIds) return 0

  return getOnlineMembers(userIds, client).length
}

/**
 * Build friend requests error message.
 * @param error - an int representing an error code.
 */
function buildFriendRequestErrorResponse(error: FriendshipErrorCode) {
  return { reply: undefined, error }
}

/**
 * Build friend requests success message.
 * @param reply - a FriendRequestReplyOk kind of type.
 */
function buildFriendRequestReply<T>(reply: NonNullable<T>) {
  return { reply, error: undefined }
}

/**
 * Check whether the user has reached the max number allowed of sent requests to a given user.
 * @param userId - the user to check the number of requests for.
 * @returns true if the user has reached the max number of sent requests, false otherwise
 */
function reachedMaxNumberOfRequests(userId: string) {
  // Get number friend requests sent in the current session to the given user
  const sentRequests = getNumberOfFriendRequests(store.getState())
  const number = sentRequests.get(userId) ?? 0

  // Get the maximum number of requests allowed
  const maxNumber = getAntiSpamLimits(store.getState()).maxNumberRequest

  // Check if the current number of requests is less than the maximum allowed
  return number >= maxNumber
}

/**
 * Check whether there is a remaining cooldown time to send a friend request to a given user.
 * @param userId - the user to check the cooldown time for.
 * @returns true if there is a remaining cooldown time and it hasn't expired yet, false otherwise
 */
function hasRemainingCooldown(userId: string) {
  const currentTime = Date.now()

  // Get the remaining cooldown time for the given user
  const coolDownTimer = getCoolDownOfFriendRequests(store.getState())
  const remainingCooldownTime = coolDownTimer.get(userId)

  // If there is a remaining cooldown time and it hasn't expired yet, return false
  return remainingCooldownTime && currentTime < remainingCooldownTime
}

/**
 * Filters and processes (update their friendship status as rejected) friend requests from blocked users.
 * @param fromFriendRequests - an array of from friend requests.
 */
function handleBlockedUsers(fromFriendRequests: FriendRequest[]) {
  // Get the ids of users who have been blocked
  const blockedIds = fromFriendRequests.filter((fromFriendRequest) => isBlocked(fromFriendRequest.userId))

  // For each blocked user, update their friendship status as rejected
  const promises = blockedIds.map(async (fromFriendRequest) =>
    handleBlockedUser(fromFriendRequest.userId, fromFriendRequest.message)
  )

  // Wait for all Promises to resolve
  return Promise.all(promises)
}

/**
 * Processes (update their friendship status as rejected) a friend request from a blocked user.
 * @param id - the id of the user whose friend request is being processed.
 */
async function handleBlockedUser(id: string, message?: string) {
  // Update their friendship status as rejected
  return await UpdateFriendshipAsPromise(FriendshipAction.REJECTED, id.toLowerCase(), false, message)
}

/**
 * Updates the friends state { numberOfFriendRequests, coolDownOfFriendRequests } in the store with the given user id.
 * @param userId - the id of the user to update the friends state for.
 */
function updateFriendsState(userId: string) {
  // Get the current state from the store
  const state = store.getState()

  const { numberOfFriendRequests, coolDownOfFriendRequests } = getPrivateMessaging(state)

  // Update the number of sent requests and return false
  const number = numberOfFriendRequests.get(userId) ?? 0
  numberOfFriendRequests.set(userId, number + 1)

  // Update the cooldown timer and return false
  const currentTime = Date.now()
  coolDownOfFriendRequests.set(userId, currentTime + COOLDOWN_TIME_MS)

  const newState = { ...state.friends, numberOfFriendRequests, coolDownOfFriendRequests }

  store.dispatch(updatePrivateMessagingState(newState))
}
