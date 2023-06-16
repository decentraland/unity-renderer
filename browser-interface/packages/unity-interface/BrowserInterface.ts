import { EcsMathReadOnlyQuaternion, EcsMathReadOnlyVector3 } from '@dcl/ecs-math'

import { Authenticator } from '@dcl/crypto'
import { Avatar, generateLazyValidator, JSONSchema, WearableCategory } from '@dcl/schemas'
import { DEBUG, ethereumConfigurations, playerHeight, WORLD_EXPLORER } from 'config'
import { isAddress } from 'eth-connect'
import future, { IFuture } from 'fp-future'
import { getSignedHeaders } from 'lib/decentraland/authentication/signedFetch'
import { arrayCleanup } from 'lib/javascript/arrayCleanup'
import { now } from 'lib/javascript/now'
import { defaultLogger } from 'lib/logger'
import { fetchENSOwner } from 'lib/web3/fetchENSOwner'
import { trackEvent } from 'shared/analytics/trackEvent'
import { setDecentralandTime } from 'shared/apis/host/EnvironmentAPI'
import { reportScenesAroundParcel, reportScenesWorldContext, setHomeScene } from 'shared/atlas/actions'
import { emotesRequest, wearablesRequest } from 'shared/catalogs/actions'
import { EmotesRequestFilters, WearablesRequestFilters } from 'shared/catalogs/types'
import { notifyStatusThroughChat } from 'shared/chat'
import { sendMessage } from 'shared/chat/actions'
import { sendPublicChatMessage } from 'shared/comms'
import { changeRealm } from 'shared/dao'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { getERC20Balance } from 'lib/web3/EthereumService'
import { leaveChannel, updateUserData } from 'shared/friends/actions'
import { ensureFriendProfile } from 'shared/friends/ensureFriendProfile'
import {
  createChannel,
  getChannelInfo,
  getChannelMembers,
  getChannelMessages,
  getFriendRequests,
  getFriends,
  getFriendsWithDirectMessages,
  getJoinedChannels,
  getPrivateMessages,
  getUnseenMessagesByChannel,
  getUnseenMessagesByUser,
  joinChannel,
  markAsSeenChannelMessages,
  markAsSeenPrivateChatMessages,
  muteChannel,
  searchChannels,
  UpdateFriendshipAsPromise
} from 'shared/friends/sagas'
import { areChannelsEnabled, getMatrixIdFromUser } from 'shared/friends/utils'
import { updateStatusMessage } from 'shared/loading/actions'
import { ReportFatalErrorWithUnityPayloadAsync } from 'shared/loading/ReportFatalError'
import { getLastUpdateTime } from 'shared/loading/selectors'
import { AVATAR_LOADING_ERROR } from 'shared/loading/types'
import { renderingActivated, renderingDectivated } from 'shared/loadingScreen/types'
import { globalObservable } from 'shared/observables'
import { denyPortableExperiences, removeScenePortableExperience } from 'shared/portableExperiences/actions'
import { saveProfileDelta, sendProfileToRenderer } from 'shared/profiles/actions'
import { retrieveProfile } from 'shared/profiles/retrieveProfile'
import { findProfileByName } from 'shared/profiles/selectors'
import { ensureRealmAdapter } from 'shared/realm/ensureRealmAdapter'
import { getFetchContentUrlPrefixFromRealmAdapter, isWorldLoaderActive } from 'shared/realm/selectors'
import { setWorldLoadingRadius } from 'shared/scene-loader/actions'
import {logout, redirectToSignUp, signUp, signUpCancel, tosPopupAccepted} from 'shared/session/actions'
import { getPerformanceInfo } from 'shared/session/getPerformanceInfo'
import { getCurrentIdentity, getCurrentUserId, hasWallet } from 'shared/session/selectors'
import { blockPlayers, mutePlayers, unblockPlayers, unmutePlayers } from 'shared/social/actions'
import { setRendererAvatarState } from 'shared/social/avatarTracker'
import { reportHotScenes } from 'shared/social/hotScenes'
import { store } from 'shared/store/isolatedStore'
import {
  AvatarRendererMessage,
  ChatMessage,
  CreateChannelPayload,
  FriendshipAction,
  FriendshipUpdateStatusMessage,
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
  LeaveChannelPayload,
  MarkChannelMessagesAsSeenPayload,
  MarkMessagesAsSeenPayload,
  MuteChannelPayload,
  SetAudioDevicesPayload,
  WorldPosition
} from 'shared/types'
import {
  joinVoiceChat,
  leaveVoiceChat,
  requestToggleVoiceChatRecording,
  requestVoiceChatRecording,
  setAudioDevice,
  setVoiceChatPolicy,
  setVoiceChatVolume
} from 'shared/voiceChat/actions'
import { requestMediaDevice } from 'shared/voiceChat/sagas'
import { rendererSignalSceneReady } from 'shared/world/actions'
import {
  allScenesEvent,
  AllScenesEvents,
  getSceneWorkerBySceneID,
  getSceneWorkerBySceneNumber
} from 'shared/world/parcelSceneManager'
import { receivePositionReport } from 'shared/world/positionThings'
import { TeleportController } from 'shared/world/TeleportController'
import { setAudioStream } from './audioStream'
import { setDelightedSurveyEnabled } from './delightedSurvey'
import { fetchENSOwnerProfile } from './fetchENSOwnerProfile'
import { GIFProcessor } from './gif-processor'
import { getUnityInstance } from './IUnityInterface'

declare const globalThis: { gifProcessor?: GIFProcessor; __debug_wearables: any }
export const futures: Record<string, IFuture<any>> = {}
const TIME_BETWEEN_SCENE_LOADING_UPDATES = 1_000

type UnityEvent = any

type SystemInfoPayload = {
  graphicsDeviceName: string
  graphicsDeviceVersion: string
  graphicsMemorySize: number
  processorType: string
  processorCount: number
  systemMemorySize: number
}

/** Message from renderer sent to save the profile in the catalyst */
export type RendererSaveProfile = {
  avatar: {
    name: string
    bodyShape: string
    skinColor: {
      r: number
      g: number
      b: number
      a: number
    }
    hairColor: {
      r: number
      g: number
      b: number
      a: number
    }
    eyeColor: {
      r: number
      g: number
      b: number
      a: number
    }
    wearables: string[]
    forceRender?: string[]
    emotes: {
      slot: number
      urn: string
    }[]
  }
  face256: string
  body: string
  isSignUpFlow?: boolean
}

const color3Schema: JSONSchema<{ r: number; g: number; b: number; a: number }> = {
  type: 'object',
  required: ['r', 'g', 'b', 'a'],
  properties: {
    r: { type: 'number', nullable: false },
    g: { type: 'number', nullable: false },
    b: { type: 'number', nullable: false },
    a: { type: 'number', nullable: false }
  }
} as any

const emoteSchema: JSONSchema<{ slot: number; urn: string }> = {
  type: 'object',
  required: ['slot', 'urn'],
  properties: {
    slot: { type: 'number', nullable: false },
    urn: { type: 'string', nullable: false }
  }
}

export const rendererSaveProfileSchemaV0: JSONSchema<RendererSaveProfile> = {
  type: 'object',
  required: ['avatar', 'body', 'face256'],
  properties: {
    face256: { type: 'string' },
    body: { type: 'string' },
    isSignUpFlow: { type: 'boolean', nullable: true },
    avatar: {
      type: 'object',
      required: ['bodyShape', 'eyeColor', 'hairColor', 'name', 'skinColor', 'wearables'],
      properties: {
        bodyShape: { type: 'string' },
        name: { type: 'string' },
        eyeColor: color3Schema,
        hairColor: color3Schema,
        skinColor: color3Schema,
        wearables: { type: 'array', items: { type: 'string' } },
        forceRender: { type: 'array', items: { type: 'string' }, nullable: true },
        emotes: { type: 'array', items: emoteSchema }
      }
    }
  }
} as any

export const rendererSaveProfileSchemaV1: JSONSchema<RendererSaveProfile> = {
  type: 'object',
  required: ['avatar', 'body', 'face256'],
  properties: {
    face256: { type: 'string' },
    body: { type: 'string' },
    isSignUpFlow: { type: 'boolean', nullable: true },
    avatar: {
      type: 'object',
      required: ['bodyShape', 'eyeColor', 'hairColor', 'name', 'skinColor', 'wearables'],
      properties: {
        bodyShape: { type: 'string' },
        name: { type: 'string' },
        eyeColor: color3Schema,
        hairColor: color3Schema,
        skinColor: color3Schema,
        wearables: { type: 'array', items: { type: 'string' } },
        forceRender: { type: 'array', items: { type: 'string' }, nullable: true },
        emotes: { type: 'array', items: emoteSchema }
      }
    }
  }
} as any

// This old schema should keep working until ADR74 is merged and renderer is released
const validateRendererSaveProfileV0 = generateLazyValidator<RendererSaveProfile>(rendererSaveProfileSchemaV0)

// This is the new one
const validateRendererSaveProfileV1 = generateLazyValidator<RendererSaveProfile>(rendererSaveProfileSchemaV1)

// the BrowserInterface is a visitor for messages received from Unity
export class BrowserInterface {
  private lastBalanceOfMana: number = -1

  startedFuture = future<void>()
  onUserInteraction = future<void>()

  /**
   * This is the only method that should be called publically in this class.
   * It dispatches "renderer messages" to the correct handlers.
   *
   * It has a fallback that doesn't fail to support future versions of renderers
   * and independant workflows for both teams.
   */
  public handleUnityMessage(type: string, message: any) {
    if (type in this) {
      ;(this as any)[type](message)
    } else {
      if (DEBUG) {
        defaultLogger.info(`Unknown message (did you forget to add ${type} to unity-interface/dcl.ts?)`, message)
      }
    }
  }

  public StartIsolatedMode() {
    defaultLogger.warn('StartIsolatedMode')
  }

  public StopIsolatedMode() {
    defaultLogger.warn('StopIsolatedMode')
  }

  public AllScenesEvent<T extends IEventNames>(data: AllScenesEvents<T>) {
    allScenesEvent(data)
  }

  /** Triggered when the camera moves */
  public ReportPosition(data: {
    position: EcsMathReadOnlyVector3
    rotation: EcsMathReadOnlyQuaternion
    playerHeight?: number
    immediate?: boolean
    cameraRotation?: EcsMathReadOnlyQuaternion
  }) {
    receivePositionReport(
      data.position,
      data.rotation,
      data.cameraRotation || data.rotation,
      data.playerHeight || playerHeight
    )
  }

  public ReportMousePosition(data: { id: string; mousePosition: EcsMathReadOnlyVector3 }) {
    futures[data.id].resolve(data.mousePosition)
  }

  public SceneEvent(data: { sceneId: string; sceneNumber: number; eventType: string; payload: any }) {
    const scene = data.sceneNumber
      ? getSceneWorkerBySceneNumber(data.sceneNumber)
      : getSceneWorkerBySceneID(data.sceneId)

    if (scene) {
      scene.rpcContext.sendSceneEvent(data.eventType as IEventNames, data.payload)

      // Keep backward compatibility with old scenes using deprecated `pointerEvent`
      if (data.eventType === 'actionButtonEvent') {
        const { payload } = data.payload
        // CLICK, PRIMARY or SECONDARY
        if (payload.buttonId >= 0 && payload.buttonId <= 2) {
          scene.rpcContext.sendSceneEvent('pointerEvent', data.payload)
        }
      }
    } else {
      if (data.eventType !== 'metricsUpdate') {
        if (data.sceneId) {
          defaultLogger.error(`SceneEvent: Scene id ${data.sceneId} not found`, data)
        } else {
          defaultLogger.error(`SceneEvent: Scene number ${data.sceneNumber} not found`, data)
        }
      }
    }
  }

  public OpenWebURL(data: { url: string }) {
    globalObservable.emit('openUrl', data)
  }

  /** @deprecated */
  public PerformanceReport(data: Record<string, unknown>) {
    let estimatedAllocatedMemory = 0
    let estimatedTotalMemory = 0
    if (getUnityInstance()?.Module?.asmLibraryArg?._GetDynamicMemorySize) {
      estimatedAllocatedMemory = getUnityInstance().Module.asmLibraryArg._GetDynamicMemorySize()
      estimatedTotalMemory = getUnityInstance().Module.asmLibraryArg._GetTotalMemorySize()
    }
    const perfReport = getPerformanceInfo({ ...(data as any), estimatedAllocatedMemory, estimatedTotalMemory })
    trackEvent('performance report', perfReport)
  }

  /** @deprecated TODO: remove useBinaryTransform after SDK7 is fully in prod */
  public SystemInfoReport(data: SystemInfoPayload & { useBinaryTransform?: boolean }) {
    trackEvent('system info report', data)

    this.startedFuture.resolve()
  }

  public CrashPayloadResponse(data: { payload: any }) {
    getUnityInstance().crashPayloadResponseObservable.notifyObservers(JSON.stringify(data))
  }

  public PreloadFinished(_data: { sceneId: string; sceneNumber: number }) {
    // stub. there is no code about this in unity side yet
  }

  /** @deprecated */
  public Track(data: { name: string; properties: { key: string; value: string }[] | null }) {
    const properties: Record<string, string> = {}
    if (data.properties) {
      for (const property of data.properties) {
        properties[property.key] = property.value
      }
    }

    trackEvent(data.name as UnityEvent, { context: properties.context || 'unity-event', ...properties })
  }

  /** @deprecated */
  public TriggerExpression(data: { id: string; timestamp: number }) {
    allScenesEvent({
      eventType: 'playerExpression',
      payload: {
        expressionId: data.id
      }
    })

    const body = `␐${data.id} ${data.timestamp}`

    sendPublicChatMessage(body)
  }

  public TermsOfServiceResponse(data: {
    sceneId: string
    sceneNumber: number
    accepted: boolean
    dontShowAgain: boolean
  }) {
    if (data.sceneNumber) {
      const sceneId = getSceneWorkerBySceneNumber(data.sceneNumber)?.loadableScene.id
      if (sceneId) {
        data.sceneId = sceneId
      }
    }

    trackEvent('TermsOfServiceResponse', data)
  }

  public MotdConfirmClicked() {
    if (!hasWallet(store.getState())) {
      globalObservable.emit('openUrl', { url: 'https://docs.decentraland.org/get-a-wallet/' })
    }
  }

  public GoTo(data: { x: number; y: number }) {
    notifyStatusThroughChat(`Jumped to ${data.x},${data.y}!`)
    TeleportController.goTo(data.x, data.y).then(
      () => {},
      () => {}
    )
  }

  public GoToMagic() {
    TeleportController.goToCrowd().catch((e) => defaultLogger.error('error goToCrowd', e))
  }

  public GoToCrowd() {
    TeleportController.goToCrowd().catch((e) => defaultLogger.error('error goToCrowd', e))
  }

  public LogOut() {
    store.dispatch(logout())
  }

  public RedirectToSignUp() {
    store.dispatch(redirectToSignUp())
  }

  public SaveUserInterests(interests: string[]) {
    if (!interests) {
      return
    }
    const unique = new Set<string>(interests)

    store.dispatch(saveProfileDelta({ interests: Array.from(unique) }))
  }

  public SaveUserAvatar(changes: RendererSaveProfile) {
    if (validateRendererSaveProfileV1(changes as RendererSaveProfile)) {
      const update: Partial<Avatar> = {
        avatar: {
          bodyShape: changes.avatar.bodyShape,
          eyes: { color: changes.avatar.eyeColor },
          hair: { color: changes.avatar.hairColor },
          skin: { color: changes.avatar.skinColor },
          wearables: changes.avatar.wearables,
          forceRender: (changes.avatar.forceRender ?? []).map((category) => category as WearableCategory),
          snapshots: {
            body: changes.body,
            face256: changes.face256
          },
          emotes: changes.avatar.emotes
        }
      }
      store.dispatch(saveProfileDelta(update))
    } else if (validateRendererSaveProfileV0(changes as RendererSaveProfile)) {
      const update: Partial<Avatar> = {
        avatar: {
          bodyShape: changes.avatar.bodyShape,
          eyes: { color: changes.avatar.eyeColor },
          hair: { color: changes.avatar.hairColor },
          skin: { color: changes.avatar.skinColor },
          wearables: changes.avatar.wearables,
          forceRender: (changes.avatar.forceRender ?? []).map((category) => category as WearableCategory),
          emotes: (changes.avatar.emotes ?? []).map((value, index) => ({ slot: index, urn: value as any as string })),
          snapshots: {
            body: changes.body,
            face256: changes.face256
          }
        }
      }
      store.dispatch(saveProfileDelta(update))
    } else {
      const errors = validateRendererSaveProfileV1.errors ?? validateRendererSaveProfileV0.errors
      defaultLogger.error('error validating schema', errors)
      trackEvent('invalid_schema', {
        schema: 'SaveUserAvatar',
        payload: changes,
        errors: (errors ?? []).map(($) => $.message).join(',')
      })
      defaultLogger.error('Unity sent invalid profile' + JSON.stringify(changes) + ' Errors: ' + JSON.stringify(errors))
    }
  }

  public SendPassport(passport: { name: string; email: string }) {
    store.dispatch(signUp(passport.email, passport.name))
  }

  public RequestOwnProfileUpdate() {
    const userId = getCurrentUserId(store.getState())
    if (userId) {
      store.dispatch(sendProfileToRenderer(userId))
    }
  }

  public SaveUserUnverifiedName(changes: { newUnverifiedName: string }) {
    store.dispatch(saveProfileDelta({ name: changes.newUnverifiedName, hasClaimedName: false }))
  }

  public SaveUserDescription(changes: { description: string }) {
    store.dispatch(saveProfileDelta({ description: changes.description }))
  }

  public GetFriends(getFriendsRequest: GetFriendsPayload) {
    getFriends(getFriendsRequest).catch(defaultLogger.error)
  }

  // @TODO! @deprecated
  public GetFriendRequests(getFriendRequestsPayload: GetFriendRequestsPayload) {
    getFriendRequests(getFriendRequestsPayload).catch((err) => {
      defaultLogger.error('error getFriendRequestsDeprecate', err),
        trackEvent('error', {
          message: `error getting friend requests ` + err.message,
          context: 'kernel#friendsSaga',
          stack: 'getFriendRequests'
        })
    })
  }

  public async MarkMessagesAsSeen(userId: MarkMessagesAsSeenPayload) {
    if (userId.userId === 'nearby') return
    markAsSeenPrivateChatMessages(userId).catch((err) => {
      defaultLogger.error('error markAsSeenPrivateChatMessages', err),
        trackEvent('error', {
          message: `error marking private messages as seen ${userId.userId} ` + err.message,
          context: 'kernel#friendsSaga',
          stack: 'markAsSeenPrivateChatMessages'
        })
    })
  }

  public async GetPrivateMessages(getPrivateMessagesPayload: GetPrivateMessagesPayload) {
    getPrivateMessages(getPrivateMessagesPayload).catch((err) => {
      defaultLogger.error('error getPrivateMessages', err),
        trackEvent('error', {
          message: `error getting private messages ${getPrivateMessagesPayload.userId} ` + err.message,
          context: 'kernel#friendsSaga',
          stack: 'getPrivateMessages'
        })
    })
  }

  public CloseUserAvatar(isSignUpFlow = false) {
    if (isSignUpFlow) {
      store.dispatch(signUpCancel())
    }
  }

  public SaveUserTutorialStep(data: { tutorialStep: number }) {
    store.dispatch(saveProfileDelta({ tutorialStep: data.tutorialStep }))
  }

  public SetInputAudioDevice(data: { deviceId: string }) {
    store.dispatch(setAudioDevice({ inputDeviceId: data.deviceId }))
  }

  public ControlEvent({ eventType, payload }: { eventType: string; payload: any }) {
    switch (eventType) {
      case 'SceneReady': {
        const { sceneId, sceneNumber } = payload
        store.dispatch(rendererSignalSceneReady(sceneId, sceneNumber))
        break
      }
      /** @deprecated #3642 Will be moved to Renderer */
      case 'DeactivateRenderingACK': {
        /**
         * This event is called everytime the renderer deactivates its camera
         */
        store.dispatch(renderingDectivated())
        console.log('DeactivateRenderingACK')
        break
      }
      /** @deprecated #3642 Will be moved to Renderer */
      case 'ActivateRenderingACK': {
        /**
         * This event is called everytime the renderer activates the main camera
         */
        store.dispatch(renderingActivated())
        console.log('ActivateRenderingACK')
        break
      }
      default: {
        defaultLogger.warn(`Unknown event type ${eventType}, ignoring`)
        break
      }
    }
  }

  public SendScreenshot(data: { id: string; encodedTexture: string }) {
    futures[data.id].resolve(data.encodedTexture)
  }

  public ReportBuilderCameraTarget(data: { id: string; cameraTarget: EcsMathReadOnlyVector3 }) {
    futures[data.id].resolve(data.cameraTarget)
  }

  /**
   * @deprecated
   */
  public UserAcceptedCollectibles(_data: { id: string }) {}

  /** @deprecated */
  public SetDelightedSurveyEnabled(data: { enabled: boolean }) {
    setDelightedSurveyEnabled(data.enabled)
  }

  public SetScenesLoadRadius(data: { newRadius: number }) {
    store.dispatch(setWorldLoadingRadius(Math.max(Math.round(data.newRadius), 1)))
  }

  public GetUnseenMessagesByUser() {
    getUnseenMessagesByUser()
  }

  public SetHomeScene(data: { sceneId: string; sceneCoords: string }) {
    if (data.sceneCoords) {
      store.dispatch(setHomeScene(data.sceneCoords))
    } else {
      store.dispatch(setHomeScene(data.sceneId))
    }
  }

  public async RequestAudioDevices() {
    if (!navigator.mediaDevices?.enumerateDevices) {
      defaultLogger.error('enumerateDevices() not supported.')
    } else {
      try {
        await requestMediaDevice()

        // List cameras and microphones.
        const devices = await navigator.mediaDevices.enumerateDevices()

        const filterDevices = (kind: string) => {
          return devices
            .filter((device) => device.kind === kind)
            .map((device) => {
              return { deviceId: device.deviceId, label: device.label }
            })
        }

        const payload: SetAudioDevicesPayload = {
          inputDevices: filterDevices('audioinput'),
          outputDevices: filterDevices('audiooutput')
        }

        getUnityInstance().SetAudioDevices(payload)
      } catch (err: any) {
        defaultLogger.error(`${err.name}: ${err.message}`)
      }
    }
  }

  public GetFriendsWithDirectMessages(getFriendsWithDirectMessagesPayload: GetFriendsWithDirectMessagesPayload) {
    getFriendsWithDirectMessages(getFriendsWithDirectMessagesPayload).catch(defaultLogger.error)
  }

  public ReportScene(data: { sceneId: string; sceneNumber: number }) {
    const sceneId = data.sceneId ?? getSceneWorkerBySceneNumber(data.sceneNumber)?.rpcContext.sceneData.id

    this.OpenWebURL({ url: `https://dcl.gg/report-user-or-scene?scene_or_name=${sceneId}` })
  }

  public ReportPlayer(data: { userId: string }) {
    this.OpenWebURL({
      url: `https://dcl.gg/report-user-or-scene?scene_or_name=${data.userId}`
    })
  }

  public BlockPlayer(data: { userId: string }) {
    store.dispatch(blockPlayers([data.userId]))
  }

  public UnblockPlayer(data: { userId: string }) {
    store.dispatch(unblockPlayers([data.userId]))
  }

  public RequestScenesInfoInArea(data: { parcel: { x: number; y: number }; scenesAround: number }) {
    async function requestMapInfo() {
      const adapter = await ensureRealmAdapter()
      const isWorld = isWorldLoaderActive(adapter)
      if (isWorld) {
        store.dispatch(reportScenesWorldContext(data.parcel, data.scenesAround))
      } else {
        store.dispatch(reportScenesAroundParcel(data.parcel, data.scenesAround))
      }
    }
    requestMapInfo().catch((err) => defaultLogger.log(err))
  }

  public SetAudioStream(data: { url: string; play: boolean; volume: number }) {
    setAudioStream(data.url, data.play, data.volume).catch((err) => defaultLogger.log(err))
  }

  public SendChatMessage(data: { message: ChatMessage }) {
    store.dispatch(sendMessage(data.message))
  }

  public SetVoiceChatRecording(recordingMessage: { recording: boolean }) {
    store.dispatch(requestVoiceChatRecording(recordingMessage.recording))
  }

  public JoinVoiceChat() {
    this.onUserInteraction
      .then(() => {
        store.dispatch(joinVoiceChat())
      })
      .catch(defaultLogger.error)
  }

  public LeaveVoiceChat() {
    store.dispatch(leaveVoiceChat())
  }

  public ToggleVoiceChatRecording() {
    store.dispatch(requestToggleVoiceChatRecording())
  }

  public ApplySettings(settingsMessage: { voiceChatVolume: number; voiceChatAllowCategory: number }) {
    store.dispatch(setVoiceChatVolume(settingsMessage.voiceChatVolume))
    store.dispatch(setVoiceChatPolicy(settingsMessage.voiceChatAllowCategory))
  }

  // @TODO! @deprecated - With the new friend request flow, the only action that will be triggered by this message is FriendshipAction.DELETED.
  public async UpdateFriendshipStatus(message: FriendshipUpdateStatusMessage) {
    try {
      let { userId } = message
      let found = false
      const state = store.getState()

      // TODO - fix this hack: search should come from another message and method should only exec correct updates (userId, action) - moliva - 01/05/2020
      // @TODO! @deprecated - With the new friend request flow, the only action that will be triggered by this message is FriendshipAction.DELETED.
      if (message.action === FriendshipAction.REQUESTED_TO) {
        const avatar = await ensureFriendProfile(userId)

        if (isAddress(userId)) {
          found = avatar.hasConnectedWeb3 || false
        } else {
          const profileByName = findProfileByName(state, userId)
          if (profileByName) {
            userId = profileByName.userId
            found = true
          }
        }
      }

      if (!found) {
        // if user profile was not found on server -> no connected web3, check if it's a claimed name
        const net = getSelectedNetwork(state)
        const address = await fetchENSOwner(ethereumConfigurations[net].names, userId)
        if (address) {
          // if an address was found for the name -> set as user id & add that instead
          userId = address
          found = true
        }
      }

      // @TODO! @deprecated - With the new friend request flow, the only action that will be triggered by this message is FriendshipAction.DELETED.
      if (message.action === FriendshipAction.REQUESTED_TO && !found) {
        // if we still haven't the user by now (meaning the user has never logged and doesn't have a profile in the dao, or the user id is for a non wallet user or name is not correct) -> fail
        getUnityInstance().FriendNotFound(userId)
        return
      }

      store.dispatch(updateUserData(userId.toLowerCase(), getMatrixIdFromUser(userId)))
      await UpdateFriendshipAsPromise(message.action, userId.toLowerCase(), false)
    } catch (error) {
      const message = 'Failed while processing updating friendship status'
      defaultLogger.error(message, error)

      trackEvent('error', {
        context: 'kernel#saga',
        message: message,
        stack: '' + error
      })
    }
  }

  public CreateChannel(createChannelPayload: CreateChannelPayload) {
    if (!areChannelsEnabled()) return
    createChannel(createChannelPayload).catch((err) => {
      defaultLogger.error('error createChannel', err),
        trackEvent('error', {
          message: `error creating channel ${createChannelPayload.channelId} ` + err.message,
          context: 'kernel#friendsSaga',
          stack: 'createChannel'
        })
    })
  }

  public JoinOrCreateChannel(joinOrCreateChannelPayload: JoinOrCreateChannelPayload) {
    if (!areChannelsEnabled()) return
    joinChannel(joinOrCreateChannelPayload).catch((err) => {
      defaultLogger.error('error joinOrCreateChannel', err),
        trackEvent('error', {
          message: `error joining channel ${joinOrCreateChannelPayload.channelId} ` + err.message,
          context: 'kernel#friendsSaga',
          stack: 'joinOrCreateChannel'
        })
    })
  }

  public MarkChannelMessagesAsSeen(markChannelMessagesAsSeenPayload: MarkChannelMessagesAsSeenPayload) {
    if (!areChannelsEnabled()) return
    if (markChannelMessagesAsSeenPayload.channelId === 'nearby') return
    markAsSeenChannelMessages(markChannelMessagesAsSeenPayload).catch((err) => {
      defaultLogger.error('error markAsSeenChannelMessages', err),
        trackEvent('error', {
          message:
            `error marking channel messages as seen ${markChannelMessagesAsSeenPayload.channelId} ` + err.message,
          context: 'kernel#friendsSaga',
          stack: 'markAsSeenChannelMessages'
        })
    })
  }

  public GetChannelMessages(getChannelMessagesPayload: GetChannelMessagesPayload) {
    if (!areChannelsEnabled()) return
    getChannelMessages(getChannelMessagesPayload).catch((err) => {
      defaultLogger.error('error getChannelMessages', err),
        trackEvent('error', {
          message: `error getting channel messages ${getChannelMessagesPayload.channelId} ` + err.message,
          context: 'kernel#friendsSaga',
          stack: 'getChannelMessages'
        })
    })
  }

  public GetChannels(getChannelsPayload: GetChannelsPayload) {
    if (!areChannelsEnabled()) return
    searchChannels(getChannelsPayload).catch((err) => {
      defaultLogger.error('error searchChannels', err),
        trackEvent('error', {
          message: `error searching channels ` + err.message,
          context: 'kernel#friendsSaga',
          stack: 'searchChannels'
        })
    })
  }

  public GetChannelMembers(getChannelMembersPayload: GetChannelMembersPayload) {
    if (!areChannelsEnabled()) return
    getChannelMembers(getChannelMembersPayload).catch((err) => {
      defaultLogger.error('error getChannelMembers', err),
        trackEvent('error', {
          message: `error getChannelMembers ` + err.message,
          context: 'kernel#friendsSaga',
          stack: 'GetChannelMembers'
        })
    })
  }

  public GetUnseenMessagesByChannel() {
    if (!areChannelsEnabled()) return
    getUnseenMessagesByChannel()
  }

  public GetJoinedChannels(getJoinedChannelsPayload: GetJoinedChannelsPayload) {
    if (!areChannelsEnabled()) return
    getJoinedChannels(getJoinedChannelsPayload)
  }

  public LeaveChannel(leaveChannelPayload: LeaveChannelPayload) {
    if (!areChannelsEnabled()) return
    store.dispatch(leaveChannel(leaveChannelPayload.channelId))
  }

  public MuteChannel(muteChannelPayload: MuteChannelPayload) {
    if (!areChannelsEnabled()) return
    muteChannel(muteChannelPayload)
  }

  public GetChannelInfo(getChannelInfoPayload: GetChannelInfoPayload) {
    if (!areChannelsEnabled()) return
    getChannelInfo(getChannelInfoPayload)
  }

  public SearchENSOwner(data: { name: string; maxResults?: number }) {
    async function work() {
      const adapter = await ensureRealmAdapter()
      const fetchContentServerWithPrefix = getFetchContentUrlPrefixFromRealmAdapter(adapter)

      try {
        const profiles = await fetchENSOwnerProfile(data.name, data.maxResults)
        getUnityInstance().SetENSOwnerQueryResult(data.name, profiles, fetchContentServerWithPrefix)
      } catch (error: any) {
        getUnityInstance().SetENSOwnerQueryResult(data.name, undefined, fetchContentServerWithPrefix)
        defaultLogger.error(error)
      }
    }

    work().catch(defaultLogger.error)
  }

  public async JumpIn(data: WorldPosition) {
    const {
      gridPosition: { x, y },
      realm: { serverName }
    } = data

    notifyStatusThroughChat(`Jumping to ${serverName} at ${x},${y}...`)

    changeRealm(serverName).then(
      () => {
        const successMessage = `Welcome to realm ${serverName}!`
        notifyStatusThroughChat(successMessage)
        getUnityInstance().ConnectionToRealmSuccess(data)
        TeleportController.goTo(x, y, successMessage).then(
          () => {},
          () => {}
        )
      },
      (e) => {
        const cause = e === 'realm-full' ? ' The requested realm is full.' : ''
        notifyStatusThroughChat('changerealm: Could not join realm.' + cause)
        getUnityInstance().ConnectionToRealmFailed(data)
        defaultLogger.error(e)
      }
    )
  }

  public async UpdateMemoryUsage() {
    getUnityInstance().SendMemoryUsageToRenderer()
  }

  public ScenesLoadingFeedback(data: { message: string; loadPercentage: number }) {
    const { message, loadPercentage } = data
    const currentTime = now()
    const last = getLastUpdateTime(store.getState())
    const elapsed = currentTime - (last || 0)
    if (elapsed > TIME_BETWEEN_SCENE_LOADING_UPDATES) {
      store.dispatch(updateStatusMessage(message, loadPercentage, currentTime))
    }
  }

  public FetchHotScenes() {
    if (WORLD_EXPLORER) {
      reportHotScenes().catch((e: any) => {
        return defaultLogger.error('FetchHotScenes error', e)
      })
    }
  }

  public SetBaseResolution(data: { baseResolution: number }) {
    getUnityInstance().SetTargetHeight(data.baseResolution)
  }

  async RequestGIFProcessor(data: { imageSource: string; id: string; isWebGL1: boolean }) {
    if (!globalThis.gifProcessor) {
      globalThis.gifProcessor = new GIFProcessor(getUnityInstance().gameInstance, getUnityInstance(), data.isWebGL1)
    }

    globalThis.gifProcessor.ProcessGIF(data)
  }

  public DeleteGIF(data: { value: string }) {
    if (globalThis.gifProcessor) {
      globalThis.gifProcessor.DeleteGIF(data.value)
    }
  }

  public Web3UseResponse(data: { id: string; result: boolean }) {
    if (data.result) {
      futures[data.id].resolve(true)
    } else {
      futures[data.id].reject(new Error('Web3 operation rejected'))
    }
  }

  public FetchBalanceOfMANA() {
    const fn = async () => {
      const identity = getCurrentIdentity(store.getState())

      if (!identity?.hasConnectedWeb3) {
        return
      }
      const net = getSelectedNetwork(store.getState())
      const balance = (await getERC20Balance(identity.address, ethereumConfigurations[net].MANAToken)).toNumber()
      if (this.lastBalanceOfMana !== balance) {
        this.lastBalanceOfMana = balance
        getUnityInstance().UpdateBalanceOfMANA(`${balance}`)
      }
    }

    fn().catch((err) => defaultLogger.error(err))
  }

  public SetMuteUsers(data: { usersId: string[]; mute: boolean }) {
    if (data.mute) {
      store.dispatch(mutePlayers(data.usersId))
    } else {
      store.dispatch(unmutePlayers(data.usersId))
    }
  }

  public async KillPortableExperience(data: { portableExperienceId: string }): Promise<void> {
    store.dispatch(removeScenePortableExperience(data.portableExperienceId))
  }

  public async SetDisabledPortableExperiences(data: { idsToDisable: string[] }): Promise<void> {
    store.dispatch(denyPortableExperiences(data.idsToDisable))
  }

  public RequestBIWCatalogHeader() {
    defaultLogger.warn('RequestBIWCatalogHeader')
  }

  public RequestHeaderForUrl(_data: { method: string; url: string }) {
    defaultLogger.warn('RequestHeaderForUrl')
  }

  public RequestSignedHeaderForBuilder(_data: { method: string; url: string }) {
    defaultLogger.warn('RequestSignedHeaderForBuilder')
  }

  // Note: This message is deprecated and should be deleted in the future.
  //       It is here until the Builder API is stabilized and uses the same signedFetch method as the rest of the platform
  public RequestSignedHeader(data: { method: string; url: string; metadata: Record<string, any> }) {
    const identity = getCurrentIdentity(store.getState())

    const headers: Record<string, string> = identity
      ? getSignedHeaders(data.method, data.url, data.metadata, (_payload) =>
          Authenticator.signPayload(identity, data.url)
        )
      : {}

    getUnityInstance().SendHeaders(data.url, headers)
  }

  public async PublishSceneState(data) {
    defaultLogger.warn('PublishSceneState', data)
  }

  public RequestWearables(data: {
    filters: {
      ownedByUser: string | null
      wearableIds?: string[] | null
      collectionIds?: string[] | null
      thirdPartyId?: string | null
    }
    context?: string
  }) {
    const { filters, context } = data
    const newFilters: WearablesRequestFilters = {
      ownedByUser: filters.ownedByUser ?? undefined,
      thirdPartyId: filters.thirdPartyId ?? undefined,
      wearableIds: arrayCleanup(filters.wearableIds),
      collectionIds: arrayCleanup(filters.collectionIds)
    }
    store.dispatch(wearablesRequest(newFilters, context))
  }

  public RequestEmotes(data: {
    filters: {
      ownedByUser: string | null
      emoteIds?: string[] | null
      collectionIds?: string[] | null
      thirdPartyId?: string | null
    }
    context?: string
  }) {
    const { filters, context } = data
    const newFilters: EmotesRequestFilters = {
      ownedByUser: filters.ownedByUser ?? undefined,
      thirdPartyId: filters.thirdPartyId ?? undefined,
      emoteIds: arrayCleanup(filters.emoteIds),
      collectionIds: arrayCleanup(filters.collectionIds)
    }
    store.dispatch(emotesRequest(newFilters, context))
  }

  public RequestUserProfile(userIdPayload: { value: string }) {
    retrieveProfile(userIdPayload.value, undefined).catch(defaultLogger.error)
  }

  public ReportAvatarFatalError(payload: any) {
    defaultLogger.error(payload)
    ReportFatalErrorWithUnityPayloadAsync(
      new Error(AVATAR_LOADING_ERROR + ' ' + JSON.stringify(payload)),
      'renderer#avatars'
    )
  }

  public UnpublishScene(_data: any) {
    // deprecated
  }

  public async NotifyStatusThroughChat(data: { value: string }) {
    notifyStatusThroughChat(data.value)
  }

  public VideoProgressEvent(videoEvent: {
    componentId: string
    sceneId: string
    sceneNumber: number
    videoTextureId: string
    status: number
    currentOffset: number
    videoLength: number
  }) {
    const scene = videoEvent.sceneNumber
      ? getSceneWorkerBySceneNumber(videoEvent.sceneNumber)
      : getSceneWorkerBySceneID(videoEvent.sceneId)
    if (scene) {
      scene.rpcContext.sendSceneEvent('videoEvent' as IEventNames, {
        componentId: videoEvent.componentId,
        videoClipId: videoEvent.videoTextureId,
        videoStatus: videoEvent.status,
        currentOffset: videoEvent.currentOffset,
        totalVideoLength: videoEvent.videoLength
      })
    } else {
      if (videoEvent.sceneId) defaultLogger.error(`SceneEvent: Scene id ${videoEvent.sceneId} not found`, videoEvent)
      else defaultLogger.error(`SceneEvent: Scene number ${videoEvent.sceneNumber} not found`, videoEvent)
    }
  }

  public ReportAvatarState(data: AvatarRendererMessage) {
    setRendererAvatarState(data)
  }

  public ReportDecentralandTime(data: any) {
    setDecentralandTime(data)
  }

  public ReportLog(data: { type: string; message: string }) {
    const logger = getUnityInstance().logger
    switch (data.type) {
      case 'trace':
        logger.trace(data.message)
        break
      case 'info':
        logger.info(data.message)
        break
      case 'warn':
        logger.warn(data.message)
        break
      case 'error':
        logger.error(data.message)
        break
      default:
        logger.log(data.message)
        break
    }
  }

  //Seamless login, after A/B testing remove this methods and implement a browser-interface<>renderer service
  public ToSPopupAccepted() {
    trackEvent('seamless_login tos accepted', { })
    store.dispatch(tosPopupAccepted())
  }

  public ToSPopupRejected() {
    trackEvent('seamless_login tos rejected', { })
    window.location.href = 'https://decentraland.org'
  }

  public ToSPopupGoToToS() {
    trackEvent('seamless_login go to tos', { })
    globalObservable.emit('openUrl', { url: 'https://decentraland.org/terms' })
  }
}

export const browserInterface: BrowserInterface = new BrowserInterface()
