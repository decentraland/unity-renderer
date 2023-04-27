import { Vector3 } from '@dcl/ecs-math'
import { QuestForRenderer } from '@dcl/ecs-quests/@dcl/types'
import { Avatar, ContentMapping } from '@dcl/schemas'
import type { UnityGame } from 'unity-interface/loader'
import { RENDERER_WS, RESET_TUTORIAL, WORLD_EXPLORER, WSS_ENABLED } from 'config'
import future, { IFuture } from 'fp-future'
import { profileToRendererFormat } from 'lib/decentraland/profiles/transformations/profileToRendererFormat'
import { AddUserProfilesToCatalogPayload, NewProfileForRenderer } from 'lib/decentraland/profiles/transformations/types'
import { stringify } from 'lib/javascript/stringify'
import { uniqBy } from 'lib/javascript/uniqBy'
import { uuid } from 'lib/javascript/uuid'
import { createUnityLogger, ILogger } from 'lib/logger'
import { Observable } from 'mz-observable'
import { trackEvent } from 'shared/analytics/trackEvent'
import { Emote, WearableV2 } from 'shared/catalogs/types'
import { FeatureFlag } from 'shared/meta/types'
import { incrementCounter } from 'shared/analytics/occurences'
import { getProvider } from 'shared/session/index'
import {
  AddChatMessagesPayload,
  AddFriendRequestsPayload,
  AddFriendsPayload,
  AddFriendsWithDirectMessagesPayload,
  BuilderConfiguration,
  ChannelErrorPayload,
  ChannelInfoPayloads,
  ChannelSearchResultsPayload,
  ChatMessage,
  FriendshipUpdateStatusMessage,
  FriendsInitializationMessage,
  FriendsInitializeChatPayload,
  HeaderRequest,
  HUDConfiguration,
  HUDElementID,
  InstancedSpawnPoint,
  Notification,
  NotificationType,
  RealmsInfoForRenderer,
  RenderProfile,
  SetAudioDevicesPayload,
  TutorialInitializationMessage,
  UpdateChannelMembersPayload,
  UpdateTotalFriendRequestsPayload,
  UpdateTotalFriendsPayload,
  UpdateTotalUnseenMessagesByChannelPayload,
  UpdateTotalUnseenMessagesByUserPayload,
  UpdateTotalUnseenMessagesPayload,
  UpdateUserStatusMessage,
  UpdateUserUnseenMessagesPayload,
  WorldPosition
} from 'shared/types'
import { futures } from './BrowserInterface'
import { setDelightedSurveyEnabled } from './delightedSurvey'
import { HotSceneInfo, IUnityInterface, MinimapSceneInfo, setUnityInstance } from './IUnityInterface'
import { nativeMsgBridge } from './nativeMessagesBridge'
import { AboutResponse } from 'shared/protocol/decentraland/realm/about.gen'
import { isWorldLoaderActive } from "../shared/realm/selectors"
import { ensureRealmAdapter } from "../shared/realm/ensureRealmAdapter"

const MINIMAP_CHUNK_SIZE = 100

export const originalPixelRatio: number = devicePixelRatio
devicePixelRatio = 1

const unityLogger: ILogger = createUnityLogger()

export class UnityInterface implements IUnityInterface {
  public logger = unityLogger
  public gameInstance!: UnityGame
  public Module: any
  public crashPayloadResponseObservable: Observable<string> = new Observable<string>()

  public SetTargetHeight(height: number): void {
    // above 2000 is assumed "Match display", below that it is "Normal"
    // as defined in https://rfc.decentraland.org/adr/ADR-83
    if (height >= 2000) {
      devicePixelRatio = originalPixelRatio
    } else {
      devicePixelRatio = 1
    }
  }

  public Init(gameInstance: UnityGame): void {
    if (!WSS_ENABLED) {
      nativeMsgBridge.initNativeMessages(gameInstance)
    }

    this.gameInstance = gameInstance
    this.Module = this.gameInstance.Module
  }

  public SendGenericMessage(object: string, method: string, payload: string) {
    this.SendMessageToUnity(object, method, payload)
  }

  public SetDebug() {
    this.SendMessageToUnity('Main', 'SetDebug')
  }

  public LoadProfile(profile: NewProfileForRenderer) {
    this.SendMessageToUnity('Main', 'LoadProfile', JSON.stringify(profile))
  }

  public UpdateHomeScene(sceneCoords: string) {
    this.SendMessageToUnity('Main', 'UpdateHomeScene', sceneCoords)
  }

  public SetRenderProfile(id: RenderProfile) {
    this.SendMessageToUnity('Main', 'SetRenderProfile', JSON.stringify({ id: id }))
  }

  public SetAudioDevices(devices: SetAudioDevicesPayload) {
    this.SendMessageToUnity('Bridges', 'SetAudioDevices', JSON.stringify(devices))
  }

  public CreateGlobalScene(data: {
    id: string
    name: string
    baseUrl: string
    contents: Array<ContentMapping>
    icon?: string
    isPortableExperience: boolean
    sceneNumber: number
    sdk7: boolean
  }) {
    /**
     * UI Scenes are scenes that does not check any limit or boundary. The
     * position is fixed at 0,0 and they are universe-wide. An example of this
     * kind of scenes is the Avatar scene. All the avatars are just GLTFs in
     * a scene.
     */
    this.SendMessageToUnity('Main', 'CreateGlobalScene', JSON.stringify(data))
  }

  /** Sends the camera position & target to the engine */

  public Teleport(
    { position: { x, y, z }, cameraTarget }: InstancedSpawnPoint,
    rotateIfTargetIsNotSet: boolean = true
  ) {
    const theY = y <= 0 ? 2 : y

    this.SendMessageToUnity('CharacterController', 'Teleport', JSON.stringify({ x, y: theY, z }))
    if (cameraTarget || rotateIfTargetIsNotSet) {
      this.SendMessageToUnity('CameraController', 'SetRotation', JSON.stringify({ x, y: theY, z, cameraTarget }))
    }
  }

  public SendSceneMessage(messages: string) {
    this.SendMessageToUnity(`SceneController`, `SendSceneMessage`, messages)
  }

  /** @deprecated send it with the kernelConfigForRenderer instead. */
  public SetSceneDebugPanel() {
    this.SendMessageToUnity('Main', 'SetSceneDebugPanel')
  }

  public ShowFPSPanel() {
    this.SendMessageToUnity('Main', 'ShowFPSPanel')
  }

  public DetectABs(data: { isOn: boolean; forCurrentScene: boolean }) {
    this.SendMessageToUnity('Bridges', 'DetectABs', JSON.stringify(data))
  }

  public HideFPSPanel() {
    this.SendMessageToUnity('Main', 'HideFPSPanel')
  }

  public SetEngineDebugPanel() {
    this.SendMessageToUnity('Main', 'SetEngineDebugPanel')
  }

  public SetDisableAssetBundles() {
    this.SendMessageToUnity('Main', 'SetDisableAssetBundles')
  }

  public async CrashPayloadRequest(): Promise<string> {
    // Over wasm this should come back on the same call stack frame because
    // the response comes within the CrashPayloadRequest method body.

    // For websocket this should take more frames, so we need promises.
    const promise = new Promise<string>((resolve, reject) => {
      const crashListener = this.crashPayloadResponseObservable.addOnce((payload) => {
        resolve(payload)
      })

      setTimeout(() => {
        this.crashPayloadResponseObservable.remove(crashListener)
        reject()
      }, 2000)

      this.SendMessageToUnity('Main', 'CrashPayloadRequest')
    })

    return promise
  }

  public ActivateRendering() {
    this.SendMessageToUnity('Main', 'ActivateRendering')
  }

  public SendMemoryUsageToRenderer() {
    const memory = (performance as any).memory
    const jsHeapSizeLimit = memory?.jsHeapSizeLimit
    const totalJSHeapSize = memory?.totalJSHeapSize
    const usedJSHeapSize = memory?.usedJSHeapSize

    this.SendMessageToUnity(
      'Main',
      'SetMemoryUsage',
      JSON.stringify({ jsHeapSizeLimit, totalJSHeapSize, usedJSHeapSize })
    )
  }

  public UpdateRealmAbout(configurations: AboutResponse) {
    this.SendMessageToUnity('Bridges', 'SetRealmAbout', JSON.stringify(configurations))
  }

  public DeactivateRendering() {
    this.SendMessageToUnity('Main', 'DeactivateRendering')
  }

  public ReportFocusOn() {
    this.SendMessageToUnity('Bridges', 'ReportFocusOn')
  }

  public ReportFocusOff() {
    this.SendMessageToUnity('Bridges', 'ReportFocusOff')
  }

  public UnlockCursor() {
    this.SetCursorState(false)
  }

  public SetCursorState(locked: boolean) {
    this.SendMessageToUnity('Bridges', 'UnlockCursorBrowser', locked ? 1 : 0)
  }

  public SetBuilderReady() {
    this.SendMessageToUnity('Main', 'BuilderReady')
  }

  public AddUserProfileToCatalog(peerProfile: NewProfileForRenderer) {
    this.SendMessageToUnity('Main', 'AddUserProfileToCatalog', JSON.stringify(peerProfile))
  }

  public AddWearablesToCatalog(wearables: WearableV2[], context?: string) {
    //We are manipulating this method since we currently cannot process large string in UnityWebGL.
    //This is a mitigation while we implement proper pagination
    if (RENDERER_WS) {
      //If we are in desktop, we can send the message normally
      this.SendMessageToUnity('Main', 'AddWearablesToCatalog', JSON.stringify({ wearables, context }))
    } else {
      //First, we remove the duplicate wearables entries.
      wearables = uniqBy(wearables, 'id')

      //Then, we map to a string array to find the limit of wearables we can add
      const wearablesStringArray: string[] = wearables.map(stringify)

      //Theoretical limit is at 1306299. Added a smaller value to keep it safe
      const SAFE_THRESHOLD = 1300000
      let counter = 0
      let totalLength = 0
      while (counter < wearablesStringArray.length && totalLength < SAFE_THRESHOLD) {
        // accumulate while size is lower than threshold
        totalLength += wearablesStringArray[counter].length
        counter++
      }

      const payload =
        '{"wearables": [' +
        wearablesStringArray.slice(0, counter).join(',') +
        '], "context":"' +
        context?.toString() +
        '"}'
      //We send to Unity the resultant values analyzed
      this.SendMessageToUnity('Main', 'AddWearablesToCatalog', payload)

      //If counter is less than length, then the wearables have been truncated and we need to warn the user
      if (counter < wearablesStringArray.length) {
        this.ShowNotification({
          type: NotificationType.GENERIC,
          message:
            "Your wearables list couldn't be fully loaded, some assets might not load, bear with us while we solve the issue",
          buttonMessage: 'OK',
          timer: 10
        })
      }
    }
  }

  public AddEmotesToCatalog(emotes: Emote[], context?: string) {
    this.SendMessageToUnity('Bridges', 'AddEmotesToCatalog', JSON.stringify({ emotes, context }))
  }

  public WearablesRequestFailed(error: string, context: string | undefined) {
    this.SendMessageToUnity('Main', 'WearablesRequestFailed', JSON.stringify({ error, context }))
  }

  public RemoveWearablesFromCatalog(wearableIds: string[]) {
    this.SendMessageToUnity('Main', 'RemoveWearablesFromCatalog', JSON.stringify(wearableIds))
  }

  public ClearWearableCatalog() {
    this.SendMessageToUnity('Main', 'ClearWearableCatalog')
  }

  public ShowNotification(notification: Notification) {
    this.SendMessageToUnity('HUDController', 'ShowNotificationFromJson', JSON.stringify(notification))
  }

  public ConfigureHUDElement(
    hudElementId: HUDElementID,
    configuration: HUDConfiguration,
    extraPayload: any | null = null
  ) {
    this.SendMessageToUnity(
      'HUDController',
      `ConfigureHUDElement`,
      JSON.stringify({
        hudElementId: hudElementId,
        configuration: configuration,
        extraPayload: extraPayload ? JSON.stringify(extraPayload) : null
      })
    )
  }

  public ShowWelcomeNotification() {
    this.SendMessageToUnity('HUDController', 'ShowWelcomeNotification')
  }

  /** @deprecated */
  public TriggerSelfUserExpression(expressionId: string) {
    this.SendMessageToUnity('HUDController', 'TriggerSelfUserExpression', expressionId)
  }

  public async UpdateMinimapSceneInformation(info: MinimapSceneInfo[])
  {
    const adapter = await ensureRealmAdapter()
    const isWorldScene = isWorldLoaderActive(adapter)
    const payload = JSON.stringify({ isWorldScene, scenesInfo: info })

    this.SendMessageToUnity('Main', 'UpdateMinimapSceneInformation', payload)
  }

  public UpdateMinimapSceneInformationFromAWorld(info: MinimapSceneInfo[])
  {
    this.SendMessageToUnity('Main', 'UpdateMinimapSceneInformation', JSON.stringify(info))
    const isWorldScene = false
    const payload = JSON.stringify({ isWorldScene, scenesInfo: info })

    this.SendMessageToUnity('Main', 'UpdateMinimapSceneInformation', payload)
  }

  public SetTutorialEnabled(tutorialConfig: TutorialInitializationMessage) {
    this.SendMessageToUnity('TutorialController', 'SetTutorialEnabled', JSON.stringify(tutorialConfig))
  }

  public SetTutorialEnabledForUsersThatAlreadyDidTheTutorial(tutorialConfig: TutorialInitializationMessage) {
    this.SendMessageToUnity(
      'TutorialController',
      'SetTutorialEnabledForUsersThatAlreadyDidTheTutorial',
      JSON.stringify(tutorialConfig)
    )
  }

  public AddMessageToChatWindow(message: ChatMessage) {
    if (message.body.length > 1000) {
      trackEvent('long_chat_message_ignored', { message: message.body, sender: message.sender })
      return
    }
    this.SendMessageToUnity('Bridges', 'AddMessageToChatWindow', JSON.stringify(message))
  }

  public AddChatMessages(addChatMessagesPayload: AddChatMessagesPayload): void {
    this.SendMessageToUnity('Bridges', 'AddChatMessages', JSON.stringify(addChatMessagesPayload))
  }

  public InitializeFriends(initializationMessage: FriendsInitializationMessage) {
    this.SendMessageToUnity('Main', 'InitializeFriends', JSON.stringify(initializationMessage))
  }

  public InitializeChat(initializationMessage: FriendsInitializeChatPayload): void {
    this.SendMessageToUnity('Bridges', 'InitializeChat', JSON.stringify(initializationMessage))
  }

  public AddUserProfilesToCatalog(payload: AddUserProfilesToCatalogPayload): void {
    this.SendMessageToUnity('Main', 'AddUserProfilesToCatalog', JSON.stringify(payload))
  }

  public AddFriends(addFriendsPayload: AddFriendsPayload): void {
    this.SendMessageToUnity('Main', 'AddFriends', JSON.stringify(addFriendsPayload))
  }

  // @TODO! @deprecated
  public AddFriendRequests(addFriendRequestsPayload: AddFriendRequestsPayload): void {
    this.SendMessageToUnity('Main', 'AddFriendRequests', JSON.stringify(addFriendRequestsPayload))
  }

  public UpdateTotalUnseenMessagesByUser(
    updateTotalUnseenMessagesByUserPayload: UpdateTotalUnseenMessagesByUserPayload
  ): void {
    this.SendMessageToUnity(
      'Bridges',
      'UpdateTotalUnseenMessagesByUser',
      JSON.stringify(updateTotalUnseenMessagesByUserPayload)
    )
  }

  public AddFriendsWithDirectMessages(addFriendsWithDirectMessagesPayload: AddFriendsWithDirectMessagesPayload): void {
    this.SendMessageToUnity('Main', 'AddFriendsWithDirectMessages', JSON.stringify(addFriendsWithDirectMessagesPayload))
  }

  public UpdateTotalFriendRequests(updateTotalFriendRequestsPayload: UpdateTotalFriendRequestsPayload): void {
    this.SendMessageToUnity('Main', 'UpdateTotalFriendRequests', JSON.stringify(updateTotalFriendRequestsPayload))
  }

  public UpdateTotalFriends(updateTotalFriendPayload: UpdateTotalFriendsPayload): void {
    this.SendMessageToUnity('Main', 'UpdateTotalFriends', JSON.stringify(updateTotalFriendPayload))
  }

  public UpdateTotalUnseenMessages(updateTotalUnseenMessagesPayload: UpdateTotalUnseenMessagesPayload): void {
    this.SendMessageToUnity('Bridges', 'UpdateTotalUnseenMessages', JSON.stringify(updateTotalUnseenMessagesPayload))
  }

  public UpdateUserUnseenMessages(updateUserUnseenMessagesPayload: UpdateUserUnseenMessagesPayload): void {
    this.SendMessageToUnity('Bridges', 'UpdateUserUnseenMessages', JSON.stringify(updateUserUnseenMessagesPayload))
  }

  public UpdateFriendshipStatus(updateMessage: FriendshipUpdateStatusMessage) {
    this.SendMessageToUnity('Main', 'UpdateFriendshipStatus', JSON.stringify(updateMessage))
  }

  public UpdateUserPresence(status: UpdateUserStatusMessage) {
    this.SendMessageToUnity('Main', 'UpdateUserPresence', JSON.stringify(status))
  }

  public FriendNotFound(queryString: string) {
    this.SendMessageToUnity('Main', 'FriendNotFound', JSON.stringify(queryString))
  }

  public JoinChannelConfirmation(channelInfoPayload: ChannelInfoPayloads) {
    this.SendMessageToUnity('Bridges', 'JoinChannelConfirmation', JSON.stringify(channelInfoPayload))
  }

  public JoinChannelError(joinChannelErrorPayload: ChannelErrorPayload) {
    this.SendMessageToUnity('Bridges', 'JoinChannelError', JSON.stringify(joinChannelErrorPayload))
  }

  public UpdateTotalUnseenMessagesByChannel(
    updateTotalUnseenMessagesByChannelPayload: UpdateTotalUnseenMessagesByChannelPayload
  ) {
    this.SendMessageToUnity(
      'Bridges',
      'UpdateTotalUnseenMessagesByChannel',
      JSON.stringify(updateTotalUnseenMessagesByChannelPayload)
    )
  }

  public UpdateChannelInfo(channelInfoPayload: ChannelInfoPayloads) {
    this.SendMessageToUnity('Bridges', 'UpdateChannelInfo', JSON.stringify(channelInfoPayload))
  }

  public UpdateChannelSearchResults(channelSearchResultsPayload: ChannelSearchResultsPayload) {
    this.SendMessageToUnity('Bridges', 'UpdateChannelSearchResults', JSON.stringify(channelSearchResultsPayload))
  }

  public LeaveChannelError(leaveChannelErrorPayload: ChannelErrorPayload) {
    this.SendMessageToUnity('Bridges', 'LeaveChannelError', JSON.stringify(leaveChannelErrorPayload))
  }

  public MuteChannelError(muteChannelErrorPayload: ChannelErrorPayload) {
    this.SendMessageToUnity('Bridges', 'MuteChannelError', JSON.stringify(muteChannelErrorPayload))
  }

  public UpdateChannelMembers(updateChannelMembersPayload: UpdateChannelMembersPayload) {
    this.SendMessageToUnity('Bridges', 'UpdateChannelMembers', JSON.stringify(updateChannelMembersPayload))
  }

  // eslint-disable-next-line @typescript-eslint/ban-types
  public RequestTeleport(teleportData: {}) {
    this.SendMessageToUnity('HUDController', 'RequestTeleport', JSON.stringify(teleportData))
  }

  public UpdateHotScenesList(info: HotSceneInfo[]) {
    const chunks: any[] = []

    while (info.length) {
      chunks.push(info.splice(0, MINIMAP_CHUNK_SIZE))
    }

    for (let i = 0; i < chunks.length; i++) {
      const payload = { chunkIndex: i, chunksCount: chunks.length, scenesInfo: chunks[i] }
      this.SendMessageToUnity('Main', 'UpdateHotScenesList', JSON.stringify(payload))
    }
  }

  public ConnectionToRealmSuccess(successData: WorldPosition) {
    this.SendMessageToUnity('Bridges', 'ConnectionToRealmSuccess', JSON.stringify(successData))
  }

  public ConnectionToRealmFailed(failedData: WorldPosition) {
    this.SendMessageToUnity('Bridges', 'ConnectionToRealmFailed', JSON.stringify(failedData))
  }

  public SendGIFPointers(id: string, width: number, height: number, pointers: number[], frameDelays: number[]) {
    this.SendMessageToUnity('Main', 'UpdateGIFPointers', JSON.stringify({ id, width, height, pointers, frameDelays }))
  }

  public SendGIFFetchFailure(id: string) {
    this.SendMessageToUnity('Main', 'FailGIFFetch', id)
  }

  public ConfigureTutorial(tutorialStep: number, tutorialConfig: TutorialInitializationMessage) {
    const tutorialCompletedFlag = 256

    if (WORLD_EXPLORER) {
      if (RESET_TUTORIAL || (tutorialStep & tutorialCompletedFlag) === 0) {
        this.SetTutorialEnabled(tutorialConfig)
      } else {
        this.SetTutorialEnabledForUsersThatAlreadyDidTheTutorial(tutorialConfig)
        setDelightedSurveyEnabled(true)
      }
    }
  }

  public UpdateBalanceOfMANA(balance: string) {
    this.SendMessageToUnity('HUDController', 'UpdateBalanceOfMANA', balance)
  }

  public RequestWeb3ApiUse(requestType: string, payload: any): IFuture<boolean> {
    const isWalletConnect = (getProvider() as any).wc !== undefined

    const id = uuid()
    futures[id] = future()

    if (!isWalletConnect) {
      futures[id].resolve(true)
    } else {
      this.SendMessageToUnity('Bridges', 'RequestWeb3ApiUse', JSON.stringify({ id, requestType, payload }))
    }

    return futures[id]
  }

  public SetPlayerTalking(talking: boolean) {
    this.SendMessageToUnity('HUDController', 'SetPlayerTalking', JSON.stringify(talking))
  }

  public ShowAvatarEditorInSignIn() {
    this.SendMessageToUnity('HUDController', 'ShowAvatarEditorInSignUp')
    this.SendMessageToUnity('Main', 'ForceActivateRendering')
  }

  public SetUserTalking(userId: string, talking: boolean) {
    this.SendMessageToUnity('HUDController', 'SetUserTalking', JSON.stringify({ userId: userId, talking: talking }))
  }

  public SetUsersMuted(usersId: string[], muted: boolean) {
    this.SendMessageToUnity('HUDController', 'SetUsersMuted', JSON.stringify({ usersId: usersId, muted: muted }))
  }

  public SetVoiceChatEnabledByScene(enabled: boolean) {
    this.SendMessageToUnity('HUDController', 'SetVoiceChatEnabledByScene', enabled ? 1 : 0)
  }

  public SetVoiceChatStatus(status: { isConnected: boolean }): void {
    this.SendMessageToUnity('VoiceChatController', 'VoiceChatStatus', JSON.stringify(status))
  }

  public SetKernelConfiguration(config: any) {
    this.SendMessageToUnity('Bridges', 'SetKernelConfiguration', JSON.stringify(config))
  }

  public SetFeatureFlagsConfiguration(config: FeatureFlag) {
    this.SendMessageToUnity('Bridges', 'SetFeatureFlagConfiguration', JSON.stringify(config))
  }

  public UpdateRealmsInfo(realmsInfo: Partial<RealmsInfoForRenderer>) {
    this.SendMessageToUnity('Bridges', 'UpdateRealmsInfo', JSON.stringify(realmsInfo))
  }

  public SendPublishSceneResult() {
    this.logger.warn('SendPublishSceneResult')
  }

  public SendBuilderProjectInfo(projectName: string, projectDescription: string, isNewEmptyProject: boolean) {
    this.SendMessageToUnity(
      'Main',
      'BuilderProjectInfo',
      JSON.stringify({ title: projectName, description: projectDescription, isNewEmptyProject: isNewEmptyProject })
    )
  }

  // Note: This message is deprecated and should be deleted in the future.
  //       We are maintaining it for backward compatibility  we can safely delete if we are further than 2/03/2022
  public SendBuilderCatalogHeaders(headers: Record<string, string>) {
    this.SendMessageToUnity('Main', 'BuilderInWorldCatalogHeaders', JSON.stringify(headers))
  }

  public SendHeaders(endpoint: string, headers: Record<string, string>) {
    const request: HeaderRequest = {
      endpoint: endpoint,
      headers: headers
    }
    this.SendMessageToUnity('Main', 'RequestedHeaders', JSON.stringify(request))
  }

  public SendSceneAssets() {
    this.logger.warn('SendSceneAssets')
  }

  public SetENSOwnerQueryResult(searchInput: string, profiles: Avatar[] | undefined, contentServerBaseUrl: string) {
    if (!profiles) {
      this.SendMessageToUnity('Bridges', 'SetENSOwnerQueryResult', JSON.stringify({ searchInput, success: false }))
      return
    }
    // TODO: why do we send the whole profile while asking for the ENS???
    const profilesForRenderer: NewProfileForRenderer[] = []
    for (const profile of profiles) {
      profilesForRenderer.push(
        profileToRendererFormat(profile, { address: profile.userId, baseUrl: contentServerBaseUrl })
      )
    }
    this.SendMessageToUnity(
      'Bridges',
      'SetENSOwnerQueryResult',
      JSON.stringify({ searchInput, success: true, profiles: profilesForRenderer })
    )
  }

  public SendUnpublishSceneResult() {
    this.logger.warn('SendUnpublishSceneResult')
  }

  // *********************************************************************************
  // ************** Quests messages **************
  // *********************************************************************************

  InitQuestsInfo(rendererQuests: QuestForRenderer[]) {
    this.SendMessageToUnity('Bridges', 'InitializeQuests', JSON.stringify(rendererQuests))
  }

  UpdateQuestProgress(rendererQuest: QuestForRenderer) {
    this.SendMessageToUnity('Bridges', 'UpdateQuestProgress', JSON.stringify(rendererQuest))
  }

  // *********************************************************************************
  // ************** Builder messages **************
  // *********************************************************************************
  // @internal

  public SendBuilderMessage(method: string, payload: string = '') {
    this.SendMessageToUnity(`BuilderController`, method, payload)
  }

  public SelectGizmoBuilder(type: string) {
    this.SendBuilderMessage('SelectGizmo', type)
  }

  public ResetBuilderObject() {
    this.SendBuilderMessage('ResetObject')
  }

  public SetCameraZoomDeltaBuilder(delta: number) {
    this.SendBuilderMessage('ZoomDelta', delta.toString())
  }

  public GetCameraTargetBuilder(futureId: string) {
    this.SendBuilderMessage('GetCameraTargetBuilder', futureId)
  }

  public SetPlayModeBuilder(on: string) {
    this.SendBuilderMessage('SetPlayMode', on)
  }

  public PreloadFileBuilder(url: string) {
    this.SendBuilderMessage('PreloadFile', url)
  }

  public GetMousePositionBuilder(x: string, y: string, id: string) {
    this.SendBuilderMessage('GetMousePosition', `{"x":"${x}", "y": "${y}", "id": "${id}" }`)
  }

  public TakeScreenshotBuilder(id: string) {
    this.SendBuilderMessage('TakeScreenshot', id)
  }

  public SetCameraPositionBuilder(position: Vector3) {
    this.SendBuilderMessage('SetBuilderCameraPosition', position.x + ',' + position.y + ',' + position.z)
  }

  public SetCameraRotationBuilder(aplha: number, beta: number) {
    this.SendBuilderMessage('SetBuilderCameraRotation', aplha + ',' + beta)
  }

  public ResetCameraZoomBuilder() {
    this.SendBuilderMessage('ResetBuilderCameraZoom')
  }

  public SetBuilderGridResolution(position: number, rotation: number, scale: number) {
    this.SendBuilderMessage(
      'SetGridResolution',
      JSON.stringify({ position: position, rotation: rotation, scale: scale })
    )
  }

  public SetBuilderSelectedEntities(entities: string[]) {
    this.SendBuilderMessage('SetSelectedEntities', JSON.stringify({ entities: entities }))
  }

  public ResetBuilderScene() {
    this.SendBuilderMessage('ResetBuilderScene')
  }

  public OnBuilderKeyDown(key: string) {
    this.SendBuilderMessage('OnBuilderKeyDown', key)
  }

  public SetBuilderConfiguration(config: BuilderConfiguration) {
    this.SendBuilderMessage('SetBuilderConfiguration', JSON.stringify(config))
  }

  public SendMessageToUnity(object: string, method: string, payload: any = undefined) {
    try {
      this.gameInstance.SendMessage(object, method, payload)
      incrementCounter(method as any)
    } catch (e: any) {
      incrementCounter(`setThrew:${method}`)
      unityLogger.error(
        `Error on "${method}" from kernel to unity-renderer, with args (${payload}). Reported message is: "${e.message}", stack trace:\n${e.stack}`
      )
    }
  }
}

setUnityInstance(new UnityInterface())
