import {
  FriendRequestInfo,
  FriendshipErrorCode
} from 'shared/protocol/decentraland/renderer/common/friend_request_common.gen'
import {
  FriendshipStatus,
  GetFriendshipStatusRequest
} from 'shared/protocol/decentraland/renderer/kernel_services/friends_kernel.gen'
import {
  CancelFriendRequestPayload,
  GetFriendRequestsReplyOk,
  SendFriendRequestPayload
} from 'shared/protocol/decentraland/renderer/kernel_services/friend_request_kernel.gen'
import {
  Conversation,
  ConversationType,
  CurrentUserStatus,
  MessageStatus,
  PresenceType,
  SocialAPI,
  TextMessage
} from 'dcl-social-client'
import { profileToRendererFormat } from 'lib/decentraland/profiles/transformations/profileToRendererFormat'
import { AddUserProfilesToCatalogPayload } from 'lib/decentraland/profiles/transformations/types'
import { StoreEnhancer } from 'redux'
import { expectSaga } from 'redux-saga-test-plan'
import { select } from 'redux-saga/effects'
import { setMatrixClient } from 'shared/friends/actions'
import * as friendsSagas from 'shared/friends/sagas'
import * as friendsSelectors from 'shared/friends/selectors'
import { FriendRequest, FriendsState } from 'shared/friends/types'
import { encodeFriendRequestId } from 'shared/friends/utils'
import { ProfileState, ProfileUserInfo } from 'shared/profiles/types'
import { store } from 'shared/store/isolatedStore'
import { reducers } from 'shared/store/rootReducer'
import { RootState } from 'shared/store/rootTypes'
import { buildStore } from 'shared/store/store'
import {
  AddChatMessagesPayload,
  AddFriendsWithDirectMessagesPayload,
  ChatMessageType,
  FriendshipAction,
  GetFriendRequestsPayload,
  GetFriendsPayload,
  GetFriendsWithDirectMessagesPayload,
  GetPrivateMessagesPayload,
  PresenceStatus
} from 'shared/types'
import sinon, { assert } from 'sinon'
import { getUnityInstance, setUnityInstance } from 'unity-interface/IUnityInterface'
import { GetMutualFriendsRequest } from 'shared/protocol/decentraland/renderer/kernel_services/mutual_friends_kernel.gen'

function getMockedAvatar(userId: string, name: string): ProfileUserInfo {
  return {
    data: {
      avatar: {
        snapshots: {
          face256: '',
          body: ''
        },
        eyes: { color: '' },
        hair: { color: '' },
        skin: { color: '' }
      } as any,
      description: '',
      ethAddress: userId,
      hasClaimedName: false,
      name,
      tutorialStep: 1,
      userId,
      version: 1
    },
    status: 'ok'
  }
}

const textMessages: TextMessage[] = [
  {
    id: '1',
    timestamp: Date.now(),
    text: 'Hi there, how are you?',
    sender: '0xa2',
    status: MessageStatus.READ
  },
  {
    id: '2',
    timestamp: Date.now(),
    text: 'Hi, it is all good',
    sender: '0xa3',
    status: MessageStatus.READ
  }
]

const friendIds = ['0xa1', '0xb1', '0xc1', '0xd1']
const mutualFriendsIds = ['0x99', '0x98', '0x97']

const fromFriendRequest: FriendRequest = {
  friendRequestId: encodeFriendRequestId('ownId', '0xa1', true, FriendshipAction.REQUESTED_FROM),
  userId: '0xz1',
  createdAt: 123123132
}

const toFriendRequest: FriendRequest = {
  friendRequestId: encodeFriendRequestId('ownId', '0xa1', false, FriendshipAction.REQUESTED_TO),
  userId: '0xz2',
  createdAt: 123123132
}

const numberOfFriendRequestsEntries = [
  ['0xd9', 11],
  ['0xd7', 1]
] as const
const numberOfFriendRequests: Map<string, number> = new Map(numberOfFriendRequestsEntries)

const coolDownOfFriendRequestsEntries = [['0xd7', Date.now() + 10000]] as const
const coolDownOfFriendRequests: Map<string, number> = new Map(coolDownOfFriendRequestsEntries)

const lastStatusOfFriendsEntries = [
  [
    '@0xa1:decentraland.org',
    {
      realm: {
        layer: '',
        serverName: 'serverTest'
      },
      position: { x: 0, y: 1 },
      presence: PresenceType.ONLINE,
      lastActiveAgo: 1
    }
  ]
] as const
const lastStatusOfFriends = new Map<string, CurrentUserStatus>(lastStatusOfFriendsEntries)

const profilesFromStore = [
  getMockedAvatar('0xa1', 'john'), // It's friend
  getMockedAvatar('0xa2', 'mike'), // We use this one as getOwnId / getUserId
  getMockedAvatar('0xc1', 'agus'), // It's friend
  getMockedAvatar('0xd1', 'boris'), // It's friend
  getMockedAvatar('0xd9', 'juli'), // It's not friend and it's not pending
  getMockedAvatar('0xd7', 'martha') // It's not friend and it's not pending
]

const getMockedConversation = (userIds: string[]): Conversation => ({
  type: ConversationType.DIRECT,
  id: userIds.join('-'),
  userIds,
  lastEventTimestamp: Date.now(),
  hasMessages: true
})

const allCurrentConversations: Array<{ conversation: Conversation; unreadMessages: boolean }> = [
  {
    conversation: getMockedConversation([profilesFromStore[0].data.userId, profilesFromStore[1].data.userId]),
    unreadMessages: false
  },
  {
    conversation: getMockedConversation([profilesFromStore[0].data.userId, profilesFromStore[2].data.userId]),
    unreadMessages: false
  },
  {
    conversation: getMockedConversation([profilesFromStore[0].data.userId, profilesFromStore[3].data.userId]),
    unreadMessages: false
  }
]

const stubClient = {
  getAllCurrentConversations: () => allCurrentConversations,
  getAllCurrentFriendsConversations: () => allCurrentConversations,
  getCursorOnMessage: () => Promise.resolve({ getMessages: () => textMessages }),
  getUserId: () => '0xa2',
  createDirectConversation: () => allCurrentConversations[0].conversation,
  getUserStatuses: (...friendIds: string[]) => {
    const m = new Map()
    for (const id of friendIds) {
      const status = lastStatusOfFriends.get(id)
      if (status) {
        m.set(id, status)
      }
    }
    return m
  },
  getDomain: () => 'decentraland.org',
  setStatus: () => Promise.resolve(),
  getOwnId: () => '0xa2',
  getMessageBody: () => undefined,
  getMutualFriends: () => mutualFriendsIds
} as unknown as SocialAPI

const friendsFromStore: FriendsState = {
  client: stubClient,
  socialInfo: {},
  friends: friendIds,
  fromFriendRequests: [fromFriendRequest],
  toFriendRequests: [toFriendRequest],
  lastStatusOfFriends: new Map(),
  numberOfFriendRequests: new Map(),
  coolDownOfFriendRequests: new Map()
}

const FETCH_CONTENT_SERVER = 'base-url'

function mockStoreCalls(
  fakeLastStatusOfFriends?: Map<string, CurrentUserStatus>,
  fakeNumberOfFriendRequests?: Map<string, number>,
  fakeCoolDownOfFriendRequests?: Map<string, number>
): StoreEnhancer<any, RootState> {
  // here we list all the functions that should be invoked by this tests
  setUnityInstance({
    AddUserProfilesToCatalog() {},
    AddFriends() {},
    UpdateUserPresence() {},
    AddFriendsWithDirectMessages() {},
    AddFriendRequests() {},
    AddChatMessages() {},
    UpdateChannelInfo() {},
    UpdateTotalUnseenMessagesByChannel() {},
    UpdateChannelSearchResults() {}
  } as any)

  const userInfo: ProfileState['userInfo'] = {}

  for (const profile of profilesFromStore) {
    userInfo[profile.data.userId.toLowerCase()] = profile
  }

  return (createStore) => (reducer, initialState: any) => {
    // set the initial state for our selectors
    const $: RootState = initialState || reducers(undefined, {} as any)
    const state: RootState = {
      ...$,
      realm: {
        ...$.realm,
        realmAdapter: {
          services: { legacy: { fetchContentServer: FETCH_CONTENT_SERVER } }
        } as any
      },
      friends: {
        ...friendsFromStore,
        lastStatusOfFriends: fakeLastStatusOfFriends || friendsFromStore.lastStatusOfFriends,
        numberOfFriendRequests: fakeNumberOfFriendRequests || friendsFromStore.numberOfFriendRequests,
        coolDownOfFriendRequests: fakeCoolDownOfFriendRequests || friendsFromStore.coolDownOfFriendRequests
      },
      sceneLoader: {
        ...$.sceneLoader,
        parcelPosition: { x: 1, y: 2 }
      },
      profiles: {
        userInfo,
        lastSentProfileVersion: new Map()
      }
    }

    return createStore(reducer, state as any)
  }
}

describe('Friends sagas', () => {
  sinon.mock()

  describe('Get friends', () => {
    beforeEach(() => {
      const { store } = buildStore(mockStoreCalls())
      globalThis.globalStore = store
    })

    afterEach(() => {
      sinon.restore()
      sinon.reset()
    })

    describe("When there's a filter by id", () => {
      it('Should filter the responses to have only the ones that include the userId and have the full friends length as total', async () => {
        const unityInstance = getUnityInstance()
        const unityMock = sinon.mock(unityInstance)
        const request: GetFriendsPayload = {
          limit: 1000,
          skip: 0,
          userNameOrId: '0xa'
        }
        const expectedFriends: AddUserProfilesToCatalogPayload = {
          users: [
            profileToRendererFormat(profilesFromStore[0].data, { baseUrl: FETCH_CONTENT_SERVER }),
            profileToRendererFormat(profilesFromStore[1].data, { baseUrl: FETCH_CONTENT_SERVER })
          ]
        }
        const addedFriends = {
          friends: expectedFriends.users.map((friend) => friend.userId),
          totalFriends: profilesFromStore.length
        }

        sinon.stub(unityInstance, 'UpdateUserPresence').callsFake(() => {}) // friendsSagas.getFriends update user presence internally
        unityMock.expects('AddUserProfilesToCatalog').once().calledWithMatch(expectedFriends)
        unityMock.expects('AddFriends').once().calledWithMatch(addedFriends)
        await friendsSagas.getFriends(request)
        unityMock.verify()
      })
    })

    describe("When there's a filter by name", () => {
      it('Should filter the responses to have only the ones that include the user name and have the full friends length as total', async () => {
        const request2: GetFriendsPayload = {
          limit: 1000,
          skip: 0,
          userNameOrId: 'MiKe'
        }
        const expectedFriends: AddUserProfilesToCatalogPayload = {
          users: [profileToRendererFormat(profilesFromStore[1].data, { baseUrl: FETCH_CONTENT_SERVER })]
        }
        const addedFriends = {
          friends: expectedFriends.users.map((friend) => friend.userId),
          totalFriends: profilesFromStore.length
        }
        sinon.mock(getUnityInstance()).expects('AddUserProfilesToCatalog').once().calledWithMatch(expectedFriends)
        sinon.mock(getUnityInstance()).expects('AddFriends').once().calledWithMatch(addedFriends)
        await friendsSagas.getFriends(request2)
        sinon.mock(getUnityInstance()).verify()
      })
    })

    describe("When there's a skip", () => {
      it('Should filter the responses to skip the expected amount', async () => {
        const request2: GetFriendsPayload = {
          limit: 1000,
          skip: 1
        }
        const expectedFriends: AddUserProfilesToCatalogPayload = {
          users: profilesFromStore
            .slice(1)
            .map((profile) => profileToRendererFormat(profile.data, { baseUrl: FETCH_CONTENT_SERVER }))
        }
        const addedFriends = {
          friends: expectedFriends.users.map((friend) => friend.userId),
          totalFriends: 4
        }
        sinon.mock(getUnityInstance()).expects('AddUserProfilesToCatalog').once().calledWithMatch(expectedFriends)
        sinon.mock(getUnityInstance()).expects('AddFriends').once().calledWithMatch(addedFriends)
        await friendsSagas.getFriends(request2)
        sinon.mock(getUnityInstance()).verify()
      })
    })
  })

  // @TODO! @deprecated
  describe('Get friend requests', () => {
    beforeEach(() => {
      const { store } = buildStore(mockStoreCalls())
      globalThis.globalStore = store
    })

    afterEach(() => {
      sinon.restore()
      sinon.reset()
    })

    describe("When there're sent and received friend requests", () => {
      it('Should call unity with the declared parameters', async () => {
        const request: GetFriendRequestsPayload = {
          sentLimit: 10,
          sentSkip: 0,
          receivedLimit: 10,
          receivedSkip: 0
        }

        const addedFriendRequests = {
          requestedTo: friendsFromStore.toFriendRequests.map((friend) => friend.userId),
          requestedFrom: friendsFromStore.fromFriendRequests.map((friend) => friend.userId),
          totalReceivedFriendRequests: friendsFromStore.fromFriendRequests.length,
          totalSentFriendRequests: friendsFromStore.toFriendRequests.length
        }

        const expectedFriends: AddUserProfilesToCatalogPayload = {
          users: [
            profileToRendererFormat(profilesFromStore[0].data, { baseUrl: FETCH_CONTENT_SERVER }),
            profileToRendererFormat(profilesFromStore[1].data, { baseUrl: FETCH_CONTENT_SERVER })
          ]
        }

        sinon.mock(getUnityInstance()).expects('AddUserProfilesToCatalog').once().calledWithMatch(expectedFriends)
        sinon.mock(getUnityInstance()).expects('AddFriendRequests').once().calledWithMatch(addedFriendRequests)
        await friendsSagas.getFriendRequests(request)
        sinon.mock(getUnityInstance()).verify()
      })
    })

    describe("When there're friend requests, but there's also a skip", () => {
      it('Should filter the requests to skip the expected amount', async () => {
        const request: GetFriendRequestsPayload = {
          sentLimit: 10,
          sentSkip: 5,
          receivedLimit: 10,
          receivedSkip: 5
        }

        const addedFriendRequests = {
          requestedTo: friendsFromStore.toFriendRequests.slice(5).map((friend) => friend.userId),
          requestedFrom: friendsFromStore.fromFriendRequests.slice(5).map((friend) => friend.userId),
          totalReceivedFriendRequests: friendsFromStore.fromFriendRequests.length,
          totalSentFriendRequests: friendsFromStore.toFriendRequests.length
        }

        const expectedFriends: AddUserProfilesToCatalogPayload = {
          users: profilesFromStore
            .slice(5)
            .map((profile) => profileToRendererFormat(profile.data, { baseUrl: FETCH_CONTENT_SERVER }))
        }

        sinon.mock(getUnityInstance()).expects('AddUserProfilesToCatalog').once().calledWithMatch(expectedFriends)
        sinon.mock(getUnityInstance()).expects('AddFriendRequests').once().calledWithMatch(addedFriendRequests)
        await friendsSagas.getFriendRequests(request)
        sinon.mock(getUnityInstance()).verify()
      })
    })
  })

  describe('Get friend requests via protocol', () => {
    beforeEach(() => {
      const { store } = buildStore(mockStoreCalls())
      globalThis.globalStore = store
    })

    afterEach(() => {
      sinon.restore()
      sinon.reset()
    })

    describe("When there're sent and received friend requests and there is no exception", () => {
      it('Should return an undefined error and GetFriendRequestsReplyOk reply', async () => {
        const request: GetFriendRequestsPayload = {
          sentLimit: 10,
          sentSkip: 0,
          receivedLimit: 10,
          receivedSkip: 0
        }

        const friendRequests: GetFriendRequestsReplyOk = {
          requestedTo: friendsFromStore.toFriendRequests.map((friend) => getFriendRequestInfo(friend, false)),
          requestedFrom: friendsFromStore.fromFriendRequests.map((friend) => getFriendRequestInfo(friend, true)),
          totalReceivedFriendRequests: friendsFromStore.fromFriendRequests.length,
          totalSentFriendRequests: friendsFromStore.toFriendRequests.length
        }

        const expectedResponse = {
          reply: friendRequests,
          error: undefined
        }

        const expectedFriends: AddUserProfilesToCatalogPayload = {
          users: [
            profileToRendererFormat(profilesFromStore[0].data, { baseUrl: FETCH_CONTENT_SERVER }),
            profileToRendererFormat(profilesFromStore[1].data, { baseUrl: FETCH_CONTENT_SERVER })
          ]
        }

        sinon.mock(getUnityInstance()).expects('AddUserProfilesToCatalog').once().calledWithMatch(expectedFriends)
        const response = await friendsSagas.getFriendRequestsProtocol(request)
        assert.match(response, expectedResponse)
        sinon.mock(getUnityInstance()).verify()
      })
    })
  })

  describe('Get mutual friends', () => {
    beforeEach(() => {
      const { store } = buildStore(mockStoreCalls())
      globalThis.globalStore = store
    })

    afterEach(() => {
      sinon.restore()
      sinon.reset()
    })

    describe("When there're mutual friends between the authenticated user and the user specified in the request", () => {
      it('Should return an array with the addresses of the friends', async () => {
        const request: GetMutualFriendsRequest = {
          userId: '0xaa'
        }

        const expectedResponse = mutualFriendsIds

        const response = await friendsSagas.getMutualFriends(request)
        assert.match(response, expectedResponse)
      })
    })
  })

  describe('Get friends with direct messages', () => {
    describe("when there's a client", () => {
      beforeEach(() => {
        const { store } = buildStore(mockStoreCalls())
        globalThis.globalStore = store
      })

      afterEach(() => {
        sinon.restore()
        sinon.reset()
      })

      it('Should send unity the expected profiles and the expected friend conversations', async () => {
        const unityInstance = getUnityInstance()
        const unityMock = sinon.mock(unityInstance)
        const request: GetFriendsWithDirectMessagesPayload = {
          limit: 1000,
          skip: 0,
          userNameOrId: '0xa' // this will only bring the friend 0xa2
        }
        const expectedFriends: AddUserProfilesToCatalogPayload = {
          users: [profileToRendererFormat(profilesFromStore[1].data, { baseUrl: FETCH_CONTENT_SERVER })]
        }

        const expectedAddFriendsWithDirectMessagesPayload: AddFriendsWithDirectMessagesPayload = {
          currentFriendsWithDirectMessages: [
            {
              lastMessageTimestamp: allCurrentConversations[0].conversation.lastEventTimestamp!,
              userId: profilesFromStore[1].data.userId
            }
          ],
          totalFriendsWithDirectMessages: allCurrentConversations.length
        }

        sinon.stub(unityInstance, 'UpdateUserPresence').callsFake(() => {}) // friendsSagas.getFriendsWithDirectMessages update user presence internally
        unityMock.expects('AddUserProfilesToCatalog').once().calledWithMatch(expectedFriends)
        unityMock
          .expects('AddFriendsWithDirectMessages')
          .once()
          .calledWithMatch(expectedAddFriendsWithDirectMessagesPayload)
        await friendsSagas.getFriendsWithDirectMessages(request)
        unityMock.verify()
      })
    })
  })

  describe('Get private messages from specific chat', () => {
    describe('When a private chat is opened', () => {
      beforeEach(() => {
        const { store } = buildStore()
        globalThis.globalStore = store

        mockStoreCalls()
      })

      afterEach(() => {
        sinon.restore()
        sinon.reset()
      })

      it('Should call unity with the expected private messages', async () => {
        const request: GetPrivateMessagesPayload = {
          userId: '0xa3',
          limit: 10,
          fromMessageId: ''
        }

        // parse messages
        const addChatMessagesPayload: AddChatMessagesPayload = {
          messages: textMessages.map((message) => ({
            messageId: message.id,
            messageType: ChatMessageType.PRIVATE,
            timestamp: message.timestamp,
            body: message.text,
            sender: message.sender === '0xa2' ? '0xa2' : request.userId,
            recipient: message.sender === '0xa2' ? request.userId : '0xa2'
          }))
        }

        sinon.mock(getUnityInstance()).expects('AddChatMessages').once().calledWithMatch(addChatMessagesPayload)
        await friendsSagas.getPrivateMessages(request)
        sinon.mock(getUnityInstance()).verify()
      })
    })
  })

  describe('Update friends status', () => {
    afterEach(() => {
      sinon.restore()
      sinon.reset()
    })

    it("Should send status when it's not stored in the redux state yet", async () => {
      // restore statuses
      const { store } = buildStore(mockStoreCalls(new Map()))
      globalThis.globalStore = store

      const unityMock = sinon.mock(getUnityInstance())
      unityMock.expects('UpdateUserPresence').once().calledWithMatch({
        userId: '0xa1',
        realm: lastStatusOfFriendsEntries[0][1].realm,
        position: lastStatusOfFriendsEntries[0][1].position,
        presence: PresenceStatus.ONLINE
      })
      await expectSaga(friendsSagas.initializeStatusUpdateInterval)
        .provide([
          [
            select(friendsSagas.getStatusUpdateIntervalInfo),
            {
              client: stubClient,
              realmConnectionString: 'realm-test',
              position: { x: 1, y: 5 },
              rawFriends: friendIds
            }
          ]
        ])
        .dispatch(setMatrixClient(stubClient))
        .silentRun(0) // due to initializeStatusUpdateInterval saga is a while(true) gen
      unityMock.verify()
    })

    it("Should send status when it's stored but the new one is different", async () => {
      const { store } = buildStore(mockStoreCalls(lastStatusOfFriends))
      globalThis.globalStore = store

      const client: SocialAPI = {
        ...stubClient,
        getUserStatuses: (...friendIds: string[]) => {
          const m = new Map()
          for (const id of friendIds) {
            const status = lastStatusOfFriends.get(id)
            if (status) {
              m.set(id, { ...status, position: { x: 100, y: 200 } }) // new status. different from the mocked ones in "entries" constant
            }
          }
          return m
        }
      }
      const unityMock = sinon.mock(getUnityInstance())
      unityMock
        .expects('UpdateUserPresence')
        .once()
        .calledWithMatch({
          userId: '0xa1',
          realm: lastStatusOfFriendsEntries[0][1].realm,
          position: { x: 100, y: 200 },
          presence: PresenceStatus.ONLINE
        })
      await expectSaga(friendsSagas.initializeStatusUpdateInterval)
        .provide([
          [
            select(friendsSagas.getStatusUpdateIntervalInfo),
            {
              client: client,
              realmConnectionString: 'realm-test',
              position: { x: 1, y: 5 },
              rawFriends: friendIds
            }
          ]
        ])
        .dispatch(setMatrixClient(client))
        .silentRun(0)
      unityMock.verify()
    })

    it("Should not send status when it's equal to the last sent", async () => {
      const { store } = buildStore(mockStoreCalls(lastStatusOfFriends))
      globalThis.globalStore = store

      const unityMock = sinon.mock(getUnityInstance())
      unityMock.expects('UpdateUserPresence').never()
      await expectSaga(friendsSagas.initializeStatusUpdateInterval)
        .provide([
          [
            select(friendsSagas.getStatusUpdateIntervalInfo),
            {
              client: stubClient,
              realmConnectionString: 'some-realm',
              position: { x: 1, y: 5 },
              rawFriends: friendIds
            }
          ]
        ])
        .dispatch(setMatrixClient(stubClient))
        .silentRun(0)
      unityMock.verify()
    })
  })

  describe('Get friendship status', () => {
    beforeEach(() => {
      const { store } = buildStore(mockStoreCalls())
      globalThis.globalStore = store
    })

    afterEach(() => {
      sinon.restore()
      sinon.reset()
    })

    context('When the given user id is not a friend and it is not a pending request', () => {
      it('Should return FriendshipStatus.NONE', () => {
        const request: GetFriendshipStatusRequest = {
          userId: 'some_user_id'
        }

        const expectedResponse = FriendshipStatus.NONE

        const response = friendsSagas.getFriendshipStatus(request)
        assert.match(response, expectedResponse)
      })
    })

    context('When the given user id is a friend', () => {
      it('Should return FriendshipStatus.APPROVED', () => {
        const request: GetFriendshipStatusRequest = {
          userId: '0xa1'
        }

        const expectedResponse = FriendshipStatus.APPROVED

        const response = friendsSagas.getFriendshipStatus(request)
        assert.match(response, expectedResponse)
      })
    })

    context('When the given user id is a to pending request', () => {
      it('Should return FriendshipStatus.REQUESTED_TO', () => {
        const request: GetFriendshipStatusRequest = {
          userId: '0xz2'
        }

        const expectedResponse = FriendshipStatus.REQUESTED_TO

        const response = friendsSagas.getFriendshipStatus(request)
        assert.match(response, expectedResponse)
      })
    })
  })

  describe('Send friend request via protocol', () => {
    beforeEach(() => {
      const { store } = buildStore(mockStoreCalls(undefined, numberOfFriendRequests, coolDownOfFriendRequests))
      globalThis.globalStore = store
    })

    afterEach(() => {
      sinon.restore()
      sinon.reset()
    })

    context('When the user tries to send a friend request to themself', () => {
      it('Should return FriendshipStatus.FEC_INVALID_REQUEST', async () => {
        const request: SendFriendRequestPayload = {
          userId: friendsSelectors.getOwnId(store.getState())!,
          messageBody: 'u r so cool'
        }

        const expectedResponse = {
          reply: undefined,
          error: FriendshipErrorCode.FEC_INVALID_REQUEST
        }

        const response = await friendsSagas.requestFriendship(request)
        assert.match(response, expectedResponse)
      })
    })

    context('When the user sends a friend request to a user who is a friend already', () => {
      it('Should return FriendshipStatus.FEC_INVALID_REQUEST', async () => {
        const request: SendFriendRequestPayload = {
          userId: friendIds[0],
          messageBody: 'u r so cool'
        }

        const expectedResponse = {
          reply: undefined,
          error: FriendshipErrorCode.FEC_INVALID_REQUEST
        }

        const response = await friendsSagas.requestFriendship(request)
        assert.match(response, expectedResponse)
      })
    })

    context('When the user sends a friend request to a user, but it`s reached the max number of sent requests', () => {
      it('Should return FriendshipStatus.FEC_TOO_MANY_REQUESTS_SENT', async () => {
        const request: SendFriendRequestPayload = {
          userId: '0xd9',
          messageBody: 'u r so cool'
        }

        const expectedResponse = {
          reply: undefined,
          error: FriendshipErrorCode.FEC_TOO_MANY_REQUESTS_SENT
        }

        const response = await friendsSagas.requestFriendship(request)
        assert.match(response, expectedResponse)
      })
    })

    context('When the user sends a friend request to a user, but it is spamming', () => {
      it('Should return FriendshipStatus.FEC_NOT_ENOUGH_TIME_PASSED', async () => {
        const request: SendFriendRequestPayload = {
          userId: '0xd7',
          messageBody: 'u r so cool'
        }

        const expectedResponse = {
          reply: undefined,
          error: FriendshipErrorCode.FEC_NOT_ENOUGH_TIME_PASSED
        }

        const response = await friendsSagas.requestFriendship(request)
        assert.match(response, expectedResponse)
      })
    })
  })

  describe('Cancel friend requests via protocol', () => {
    beforeEach(() => {
      const { store } = buildStore(mockStoreCalls())
      globalThis.globalStore = store
    })

    afterEach(() => {
      sinon.restore()
      sinon.reset()
    })

    context('When a user cancels a sent friend request, but the ownId is not part of the friendRequestId.', () => {
      it('Should return FEC_INVALID_REQUEST error', async () => {
        const request: CancelFriendRequestPayload = {
          friendRequestId: `not_matching_userId`
        }

        const expectedResponse = {
          reply: undefined,
          error: FriendshipErrorCode.FEC_INVALID_REQUEST
        }

        const response = await friendsSagas.cancelFriendRequest(request)
        assert.match(response, expectedResponse)
      })
    })
  })
})

/**
 * Helper func to map FriendRequest to FriendRequestInfo
 */
function getFriendRequestInfo(friend: FriendRequest, incoming: boolean) {
  const friendRequest: FriendRequestInfo = incoming
    ? {
        friendRequestId: friend.friendRequestId,
        timestamp: friend.createdAt,
        from: friend.userId,
        to: stubClient.getUserId(),
        messageBody: friend.message
      }
    : {
        friendRequestId: friend.friendRequestId,
        timestamp: friend.createdAt,
        from: stubClient.getUserId(),
        to: friend.userId,
        messageBody: friend.message
      }

  return friendRequest
}
