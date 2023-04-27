/* eslint-disable @typescript-eslint/ban-types */
import type { EcsMathReadOnlyVector3 } from '@dcl/ecs-math'
import type { QuestForRenderer } from '@dcl/ecs-quests/@dcl/types'
import type { UnityGame } from 'unity-interface/loader'
import type { Observable } from 'mz-observable'
import type {
  RenderProfile,
  InstancedSpawnPoint,
  WearableV2,
  HUDElementID,
  HUDConfiguration,
  RealmsInfoForRenderer,
  BuilderConfiguration,
  ChatMessage,
  FriendshipUpdateStatusMessage,
  FriendsInitializationMessage,
  TutorialInitializationMessage,
  Notification,
  UpdateUserStatusMessage,
  WorldPosition,
  AddFriendsPayload,
  AddFriendRequestsPayload,
  UpdateTotalUnseenMessagesPayload,
  UpdateUserUnseenMessagesPayload,
  AddChatMessagesPayload,
  UpdateTotalUnseenMessagesByUserPayload,
  AddFriendsWithDirectMessagesPayload,
  UpdateTotalFriendRequestsPayload,
  FriendsInitializeChatPayload,
  UpdateTotalFriendsPayload,
  UpdateTotalUnseenMessagesByChannelPayload,
  ChannelErrorPayload,
  ChannelInfoPayloads,
  UpdateChannelMembersPayload,
  ChannelSearchResultsPayload,
  SetAudioDevicesPayload
} from 'shared/types'
import type { FeatureFlag } from 'shared/meta/types'
import type { IFuture } from 'fp-future'
import type { Avatar, ContentMapping } from '@dcl/schemas'
import type { ILogger } from 'lib/logger'
import type {
  AddUserProfilesToCatalogPayload,
  NewProfileForRenderer
} from 'lib/decentraland/profiles/transformations/types'
import type { Emote } from 'shared/catalogs/types'
import { AboutResponse } from 'shared/protocol/decentraland/realm/about.gen'

export type RealmInfo = {
  serverName: string
  layer?: string
  usersCount: number
  usersMax: number
  userParcels: { x: number; y: number }[]
}

export type HotSceneInfo = {
  id: string
  name: string
  creator: string
  description: string
  thumbnail: string
  baseCoords: { x: number; y: number }
  parcels: { x: number; y: number }[]
  usersTotalCount: number
  realms: RealmInfo[]
}

export type MinimapSceneInfo = {
  name: string
  owner: string
  description: string
  previewImageUrl: string | undefined
  type: number
  parcels: {
    x: number
    y: number
  }[]
  isPOI: boolean
}

let instance: IUnityInterface | null = null

export function setUnityInstance(_instance: IUnityInterface) {
  instance = _instance
}

export function getUnityInstance(): IUnityInterface {
  if (!instance) throw new Error('unityInstance not initialized yet')
  return instance
}

export interface IUnityInterface {
  gameInstance: UnityGame
  Module: any
  crashPayloadResponseObservable: Observable<string>
  logger: ILogger
  SetTargetHeight(height: number): void
  Init(gameInstance: UnityGame): void
  SendGenericMessage(object: string, method: string, payload: string): void
  SetDebug(): void
  LoadProfile(profile: NewProfileForRenderer): void
  UpdateHomeScene(sceneId: string): void
  SetRenderProfile(id: RenderProfile): void
  CreateGlobalScene(data: {
    id: string
    name: string
    baseUrl: string
    contents: Array<ContentMapping>
    icon?: string
    isPortableExperience: boolean
    sceneNumber: number
    sdk7: boolean
  }): void

  /** Sends the camera position & target to the engine */

  Teleport(
    {
      position: { x, y, z },
      cameraTarget
    }: InstancedSpawnPoint,
    rotateIfTargetIsNotSet?: boolean
  ): void

  SendSceneMessage(messages: string): void
  /** @deprecated send it with the kernelConfigForRenderer instead. */
  SetSceneDebugPanel(): void
  ShowFPSPanel(): void
  HideFPSPanel(): void
  DetectABs(data: { isOn: boolean; forCurrentScene: boolean }): void
  SetEngineDebugPanel(): void
  SetDisableAssetBundles(): void
  CrashPayloadRequest(): Promise<string>

  /** @deprecated #3642 Kernel will no longer control Loading Screen */
  ActivateRendering(): void

  /** @deprecated #3642 Not used */
  DeactivateRendering(): void
  ReportFocusOn(): void
  ReportFocusOff(): void
  UnlockCursor(): void
  SetCursorState(locked: boolean): void
  SetBuilderReady(): void
  AddUserProfilesToCatalog(payload: AddUserProfilesToCatalogPayload): void
  AddUserProfileToCatalog(peerProfile: NewProfileForRenderer): void
  AddWearablesToCatalog(wearables: WearableV2[], context?: string): void
  AddEmotesToCatalog(emotes: Emote[], context?: string): void
  WearablesRequestFailed(error: string, context: string | undefined): void
  RemoveWearablesFromCatalog(wearableIds: string[]): void
  ClearWearableCatalog(): void
  ShowNotification(notification: Notification): void
  ConfigureHUDElement(hudElementId: HUDElementID, configuration: HUDConfiguration, extraPayload?: any): void
  ShowWelcomeNotification(): void

  /** @deprecated */
  TriggerSelfUserExpression(expressionId: string): void

  UpdateMinimapSceneInformation(info: MinimapSceneInfo[]): void
  UpdateMinimapSceneInformationFromAWorld(info: MinimapSceneInfo[]): void
  SetTutorialEnabled(tutorialConfig: TutorialInitializationMessage): void
  SetTutorialEnabledForUsersThatAlreadyDidTheTutorial(tutorialConfig: TutorialInitializationMessage): void
  AddMessageToChatWindow(message: ChatMessage): void
  AddChatMessages(addChatMessagesPayload: AddChatMessagesPayload): void

  // *********************************************************************************
  // ************** Chat messages **************
  // *********************************************************************************

  InitializeFriends(initializationMessage: FriendsInitializationMessage): void
  InitializeChat(initializationMessage: FriendsInitializeChatPayload): void
  UpdateFriendshipStatus(updateMessage: FriendshipUpdateStatusMessage): void
  UpdateUserPresence(status: UpdateUserStatusMessage): void
  FriendNotFound(queryString: string): void
  AddFriends(addFriendsPayload: AddFriendsPayload): void
  // @TODO! @deprecated
  AddFriendRequests(addFriendRequestsPayload: AddFriendRequestsPayload): void
  UpdateTotalUnseenMessages(updateTotalUnseenMessagesPayload: UpdateTotalUnseenMessagesPayload): void
  UpdateUserUnseenMessages(updateUserUnseenMessagesPayload: UpdateUserUnseenMessagesPayload): void
  UpdateTotalUnseenMessagesByUser(updateTotalUnseenMessagesByUserPayload: UpdateTotalUnseenMessagesByUserPayload): void
  AddFriendsWithDirectMessages(addFriendsWithDirectMessagesPayload: AddFriendsWithDirectMessagesPayload): void
  UpdateTotalFriendRequests(updateTotalFriendRequestsPayload: UpdateTotalFriendRequestsPayload): void
  UpdateTotalFriends(updateTotalFriendsPayload: UpdateTotalFriendsPayload): void

  // *********************************************************************************
  // ************** Channels **************
  // *********************************************************************************

  JoinChannelConfirmation(channelsInfoPayload: ChannelInfoPayloads): void
  JoinChannelError(joinChannelErrorPayload: ChannelErrorPayload): void
  UpdateTotalUnseenMessagesByChannel(
    updateTotalUnseenMessagesByChannelPayload: UpdateTotalUnseenMessagesByChannelPayload
  ): void
  UpdateChannelInfo(channelsInfoPayload: ChannelInfoPayloads): void
  UpdateChannelSearchResults(channelSearchResultsPayload: ChannelSearchResultsPayload): void
  LeaveChannelError(leaveChannelErrorPayload: ChannelErrorPayload): void
  MuteChannelError(muteChannelErrorPayload: ChannelErrorPayload): void
  UpdateChannelMembers(updateChannelMembersPayload: UpdateChannelMembersPayload): void

  RequestTeleport(teleportData: {}): void
  UpdateHotScenesList(info: HotSceneInfo[]): void
  ConnectionToRealmSuccess(successData: WorldPosition): void
  ConnectionToRealmFailed(failedData: WorldPosition): void
  SendGIFPointers(id: string, width: number, height: number, pointers: number[], frameDelays: number[]): void
  SendGIFFetchFailure(id: string): void
  ConfigureTutorial(tutorialStep: number, tutorialConfig: TutorialInitializationMessage): void
  UpdateBalanceOfMANA(balance: string): void
  RequestWeb3ApiUse(requestType: string, payload: any): IFuture<boolean>
  SetPlayerTalking(talking: boolean): void
  ShowAvatarEditorInSignIn(): void
  SetUserTalking(userId: string, talking: boolean): void
  SetUsersMuted(usersId: string[], muted: boolean): void
  SetVoiceChatEnabledByScene(enabled: boolean): void
  SetVoiceChatStatus(status: { isConnected: boolean }): void
  SetKernelConfiguration(config: any): void
  SetFeatureFlagsConfiguration(config: FeatureFlag): void
  UpdateRealmsInfo(realmsInfo: Partial<RealmsInfoForRenderer>): void
  SetENSOwnerQueryResult(searchInput: string, profiles: Avatar[] | undefined, contentServerBaseUrl: string): void
  SendHeaders(endpoint: string, headers: Record<string, string>): void
  SendMemoryUsageToRenderer(): void
  UpdateRealmAbout(configurations: AboutResponse): void

  // *********************************************************************************
  // ************** Builder in world messages **************
  // *********************************************************************************

  SendPublishSceneResult(): void
  SendBuilderProjectInfo(projectName: string, projectDescription: string, isNewEmptyProject: boolean): void
  SendSceneAssets(): void
  SendUnpublishSceneResult(): void

  //Note: This message is deprecated and should be deleted in the future.
  //      We are maintaining it for backward compatibility we can safely delete if we are further than 2/03/2022
  SendBuilderCatalogHeaders(headers: Record<string, string>): void

  // *********************************************************************************
  // ************** Quests messages **************
  // *********************************************************************************

  InitQuestsInfo(rendererQuests: QuestForRenderer[]): void
  UpdateQuestProgress(rendererQuest: QuestForRenderer): void

  // *********************************************************************************
  // ************** Builder messages **************
  // *********************************************************************************

  SendBuilderMessage(method: string, payload: string): void
  SelectGizmoBuilder(type: string): void
  ResetBuilderObject(): void
  SetCameraZoomDeltaBuilder(delta: number): void
  GetCameraTargetBuilder(futureId: string): void
  SetPlayModeBuilder(on: string): void
  PreloadFileBuilder(url: string): void
  GetMousePositionBuilder(x: string, y: string, id: string): void
  TakeScreenshotBuilder(id: string): void
  SetCameraPositionBuilder(position: EcsMathReadOnlyVector3): void
  SetCameraRotationBuilder(aplha: number, beta: number): void
  ResetCameraZoomBuilder(): void
  SetBuilderGridResolution(position: number, rotation: number, scale: number): void
  SetBuilderSelectedEntities(entities: string[]): void
  ResetBuilderScene(): void
  OnBuilderKeyDown(key: string): void
  SetBuilderConfiguration(config: BuilderConfiguration): void
  SendMessageToUnity(object: string, method: string, payload?: any): void

  SetAudioDevices(devices: SetAudioDevicesPayload): void
}
