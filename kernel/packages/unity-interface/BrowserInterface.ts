import { uuid } from 'decentraland-ecs/src'
import { sendPublicChatMessage } from 'shared/comms'
import { AvatarMessageType } from 'shared/comms/interface/types'
import { avatarMessageObservable } from 'shared/comms/peers'
import { hasConnectedWeb3 } from 'shared/profiles/selectors'
import { TeleportController } from 'shared/world/TeleportController'
import { reportScenesAroundParcel } from 'shared/atlas/actions'
import { decentralandConfigurations, ethereumConfigurations, playerConfigurations, WORLD_EXPLORER } from 'config'
import { Quaternion, ReadOnlyQuaternion, ReadOnlyVector3, Vector3 } from '../decentraland-ecs/src/decentraland/math'
import { IEventNames } from '../decentraland-ecs/src/decentraland/Types'
import { sceneLifeCycleObservable } from '../decentraland-loader/lifecycle/controllers/scene'
import { identifyEmail, queueTrackingEvent } from 'shared/analytics'
import { aborted } from 'shared/loading/ReportFatalError'
import { defaultLogger } from 'shared/logger'
import { profileRequest, saveProfileRequest } from 'shared/profiles/actions'
import { Avatar, ProfileType } from 'shared/profiles/types'
import {
  ChatMessage,
  FriendshipUpdateStatusMessage,
  FriendshipAction,
  WorldPosition,
  LoadableParcelScene
} from 'shared/types'
import { getSceneWorkerBySceneID, setNewParcelScene, stopParcelSceneWorker } from 'shared/world/parcelSceneManager'
import { getPerformanceInfo, getRawPerformanceInfo } from 'shared/session/getPerformanceInfo'
import { positionObservable } from 'shared/world/positionThings'
import { renderStateObservable } from 'shared/world/worldState'
import { sendMessage } from 'shared/chat/actions'
import { updateFriendship, updateUserData } from 'shared/friends/actions'
import { candidatesFetched, catalystRealmConnected, changeRealm } from 'shared/dao'
import { notifyStatusThroughChat } from 'shared/comms/chat'
import { fetchENSOwner, getAppNetwork } from 'shared/web3'
import { updateStatusMessage } from 'shared/loading/actions'
import { blockPlayers, mutePlayers, unblockPlayers, unmutePlayers } from 'shared/social/actions'
import { setAudioStream } from './audioStream'
import { changeSignUpStage, logout, redirectToSignUp, signUpCancel, signUpSetProfile } from 'shared/session/actions'
import { getIdentity, hasWallet } from 'shared/session'
import { StoreContainer } from 'shared/store/rootTypes'
import { unityInterface } from './UnityInterface'
import { setDelightedSurveyEnabled } from './delightedSurvey'
import { IFuture } from 'fp-future'
import { reportHotScenes } from 'shared/social/hotScenes'
import { GIFProcessor } from 'gif-processor/processor'
import { setVoiceChatRecording, setVoicePolicy, setVoiceVolume, toggleVoiceChatRecording } from 'shared/comms/actions'
import { getERC20Balance } from 'shared/ethereum/EthereumService'
import { StatefulWorker } from 'shared/world/StatefulWorker'
import { getCurrentUserId } from 'shared/session/selectors'
import { ensureFriendProfile } from 'shared/friends/ensureFriendProfile'
import Html from 'shared/Html'
import { reloadScene } from 'decentraland-loader/lifecycle/utils/reloadScene'
import { isGuest } from '../shared/ethereum/provider'
import { killPortableExperienceScene } from './portableExperiencesUtils'
import { wearablesRequest } from 'shared/catalogs/actions'
import { WearablesRequestFilters } from 'shared/catalogs/types'
import { fetchENSOwnerProfile } from './fetchENSOwnerProfile'
import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'
import { profileToRendererFormat } from 'shared/profiles/transformations/profileToRendererFormat'

declare const globalThis: StoreContainer & { gifProcessor?: GIFProcessor }
export let futures: Record<string, IFuture<any>> = {}

// ** TODO - move to friends related file - moliva - 15/07/2020
function toSocialId(userId: string) {
  const domain = globalThis.globalStore.getState().friends.client?.getDomain()
  return `@${userId.toLowerCase()}:${domain}`
}

const positionEvent = {
  position: Vector3.Zero(),
  quaternion: Quaternion.Identity,
  rotation: Vector3.Zero(),
  playerHeight: playerConfigurations.height,
  mousePosition: Vector3.Zero(),
  immediate: false // By default the renderer lerps avatars position
}

export class BrowserInterface {
  private lastBalanceOfMana: number = -1

  // visitor pattern? anyone?
  /**
   * This is the only method that should be called publically in this class.
   * It dispatches "renderer messages" to the correct handlers.
   *
   * It has a fallback that doesn't fail to support future versions of renderers
   * and independant workflows for both teams.
   */
  public handleUnityMessage(type: string, message: any) {
    if (type in this) {
      // tslint:disable-next-line:semicolon
      ;(this as any)[type](message)
    } else {
      defaultLogger.info(`Unknown message (did you forget to add ${type} to unity-interface/dcl.ts?)`, message)
    }
  }

  /** Triggered when the camera moves */
  public ReportPosition(data: {
    position: ReadOnlyVector3
    rotation: ReadOnlyQuaternion
    playerHeight?: number
    immediate?: boolean
  }) {
    positionEvent.position.set(data.position.x, data.position.y, data.position.z)
    positionEvent.quaternion.set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w)
    positionEvent.rotation.copyFrom(positionEvent.quaternion.eulerAngles)
    positionEvent.playerHeight = data.playerHeight || playerConfigurations.height

    // By default the renderer lerps avatars position
    positionEvent.immediate = false

    if (data.immediate !== undefined) {
      positionEvent.immediate = data.immediate
    }

    positionObservable.notifyObservers(positionEvent)
  }

  public ReportMousePosition(data: { id: string; mousePosition: ReadOnlyVector3 }) {
    positionEvent.mousePosition.set(data.mousePosition.x, data.mousePosition.y, data.mousePosition.z)
    positionObservable.notifyObservers(positionEvent)
    futures[data.id].resolve(data.mousePosition)
  }

  public SceneEvent(data: { sceneId: string; eventType: string; payload: any }) {
    const scene = getSceneWorkerBySceneID(data.sceneId)
    if (scene) {
      scene.emit(data.eventType as IEventNames, data.payload)
    } else {
      if (data.eventType !== 'metricsUpdate') {
        defaultLogger.error(`SceneEvent: Scene ${data.sceneId} not found`, data)
      }
    }
  }

  public OpenWebURL(data: { url: string }) {
    const newWindow: any = window.open(data.url, '_blank', 'noopener,noreferrer')
    if (newWindow != null) newWindow.opener = null
  }

  public PerformanceHiccupReport(data: { hiccupsInThousandFrames: number; hiccupsTime: number; totalTime: number }) {
    queueTrackingEvent('hiccup report', data)
  }

  public PerformanceReport(data: { samples: string; fpsIsCapped: boolean }) {
    const perfReport = getPerformanceInfo(data)
    queueTrackingEvent('performance report', perfReport)

    const rawPerfReport = getRawPerformanceInfo(data)
    queueTrackingEvent('raw perf report', rawPerfReport)
  }

  public PreloadFinished(data: { sceneId: string }) {
    // stub. there is no code about this in unity side yet
  }

  public Track(data: { name: string; properties: { key: string; value: string }[] | null }) {
    const properties: Record<string, string> = {}
    if (data.properties) {
      for (const property of data.properties) {
        properties[property.key] = property.value
      }
    }

    queueTrackingEvent(data.name, properties)
  }

  public TriggerExpression(data: { id: string; timestamp: number }) {
    avatarMessageObservable.notifyObservers({
      type: AvatarMessageType.USER_EXPRESSION,
      uuid: uuid(),
      expressionId: data.id,
      timestamp: data.timestamp
    })
    const messageId = uuid()
    const body = `␐${data.id} ${data.timestamp}`

    sendPublicChatMessage(messageId, body)
  }

  public TermsOfServiceResponse(sceneId: string, accepted: boolean, dontShowAgain: boolean) {
    // TODO
  }

  public MotdConfirmClicked() {
    if (hasWallet()) {
      TeleportController.goToNext()
    } else {
      window.open('https://docs.decentraland.org/get-a-wallet/', '_blank')
    }
  }

  public GoTo(data: { x: number; y: number }) {
    TeleportController.goTo(data.x, data.y)
  }

  public GoToMagic() {
    TeleportController.goToMagic()
  }

  public GoToCrowd() {
    TeleportController.goToCrowd().catch((e) => defaultLogger.error('error goToCrowd', e))
  }

  public LogOut() {
    globalThis.globalStore.dispatch(logout())
  }

  public RedirectToSignUp() {
    globalThis.globalStore.dispatch(redirectToSignUp())
  }

  public SaveUserInterests(interests: string[]) {
    if (!interests) {
      return
    }
    const unique = new Set<string>(interests)

    globalThis.globalStore.dispatch(saveProfileRequest({ interests: Array.from(unique) }))
  }

  public SaveUserAvatar(changes: {
    face: string
    face128: string
    face256: string
    body: string
    avatar: Avatar
    isSignUpFlow?: boolean
  }) {
    const { face, face128, face256, body, avatar } = changes
    const update = { avatar: { ...avatar, snapshots: { face, face128, face256, body } } }
    if (!changes.isSignUpFlow) {
      globalThis.globalStore.dispatch(saveProfileRequest(update))
    } else {
      globalThis.globalStore.dispatch(signUpSetProfile(update))
      globalThis.globalStore.dispatch(changeSignUpStage('passport'))
      Html.switchGameContainer(false)
      unityInterface.DeactivateRendering()
    }
  }

  public RequestOwnProfileUpdate() {
    const userId = getCurrentUserId(globalThis.globalStore.getState())
    if (!isGuest() && userId) {
      globalThis.globalStore.dispatch(profileRequest(userId))
    }
  }

  public SaveUserUnverifiedName(changes: { newUnverifiedName: string }) {
    globalThis.globalStore.dispatch(saveProfileRequest({ unclaimedName: changes.newUnverifiedName }))
  }

  public CloseUserAvatar(isSignUpFlow = false) {
    if (isSignUpFlow) {
      unityInterface.DeactivateRendering()
      globalThis.globalStore.dispatch(signUpCancel())
    }
  }

  public SaveUserTutorialStep(data: { tutorialStep: number }) {
    const update = { tutorialStep: data.tutorialStep }
    globalThis.globalStore.dispatch(saveProfileRequest(update))
  }

  public ControlEvent({ eventType, payload }: { eventType: string; payload: any }) {
    switch (eventType) {
      case 'SceneReady': {
        const { sceneId } = payload
        sceneLifeCycleObservable.notifyObservers({ sceneId, status: 'ready' })
        break
      }
      case 'ActivateRenderingACK': {
        if (!aborted) {
          renderStateObservable.notifyObservers(true)
        }
        break
      }
      case 'StartStatefulMode': {
        const { sceneId } = payload
        const worker = getSceneWorkerBySceneID(sceneId)!
        unityInterface.UnloadScene(sceneId) // Maybe unity should do it by itself?
        const parcelScene = worker.getParcelScene()
        stopParcelSceneWorker(worker)
        const data = parcelScene.data.data as LoadableParcelScene
        unityInterface.LoadParcelScenes([data]) // Maybe unity should do it by itself?
        setNewParcelScene(sceneId, new StatefulWorker(parcelScene))
        break
      }
      case 'StopStatefulMode': {
        const { sceneId } = payload
        reloadScene(sceneId).catch((error) => defaultLogger.warn(`Failed to stop stateful mode`, error))
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

  public ReportBuilderCameraTarget(data: { id: string; cameraTarget: ReadOnlyVector3 }) {
    futures[data.id].resolve(data.cameraTarget)
  }

  public UserAcceptedCollectibles(data: { id: string }) {
    // Here, we should have "airdropObservable.notifyObservers(data.id)".
    // It's disabled because of security reasons.
  }

  public SetDelightedSurveyEnabled(data: { enabled: boolean }) {
    setDelightedSurveyEnabled(data.enabled)
  }

  public ReportScene(sceneId: string) {
    this.OpenWebURL({ url: `https://decentralandofficial.typeform.com/to/KzaUxh?sceneId=${sceneId}` })
  }

  public ReportPlayer(username: string) {
    this.OpenWebURL({ url: `https://decentralandofficial.typeform.com/to/owLkla?username=${username}` })
  }

  public BlockPlayer(data: { userId: string }) {
    globalThis.globalStore.dispatch(blockPlayers([data.userId]))
  }

  public UnblockPlayer(data: { userId: string }) {
    globalThis.globalStore.dispatch(unblockPlayers([data.userId]))
  }

  public ReportUserEmail(data: { userEmail: string }) {
    const userId = getCurrentUserId(globalThis.globalStore.getState())
    if (userId) {
      identifyEmail(data.userEmail, hasWallet() ? userId : undefined)
    }
  }

  public RequestScenesInfoInArea(data: { parcel: { x: number; y: number }; scenesAround: number }) {
    globalThis.globalStore.dispatch(reportScenesAroundParcel(data.parcel, data.scenesAround))
  }

  public SetAudioStream(data: { url: string; play: boolean; volume: number }) {
    setAudioStream(data.url, data.play, data.volume).catch((err) => defaultLogger.log(err))
  }

  public SendChatMessage(data: { message: ChatMessage }) {
    globalThis.globalStore.dispatch(sendMessage(data.message))
  }

  public SetVoiceChatRecording(recordingMessage: { recording: boolean }) {
    globalThis.globalStore.dispatch(setVoiceChatRecording(recordingMessage.recording))
  }

  public ToggleVoiceChatRecording() {
    globalThis.globalStore.dispatch(toggleVoiceChatRecording())
  }

  public ApplySettings(settingsMessage: { voiceChatVolume: number; voiceChatAllowCategory: number }) {
    globalThis.globalStore.dispatch(setVoiceVolume(settingsMessage.voiceChatVolume))
    globalThis.globalStore.dispatch(setVoicePolicy(settingsMessage.voiceChatAllowCategory))
  }

  public async UpdateFriendshipStatus(message: FriendshipUpdateStatusMessage) {
    let { userId, action } = message

    // TODO - fix this hack: search should come from another message and method should only exec correct updates (userId, action) - moliva - 01/05/2020
    let found = false
    if (action === FriendshipAction.REQUESTED_TO) {
      await ensureFriendProfile(userId)
      found = hasConnectedWeb3(globalThis.globalStore.getState(), userId)
    }

    if (!found) {
      // if user profile was not found on server -> no connected web3, check if it's a claimed name
      const net = await getAppNetwork()
      const address = await fetchENSOwner(ethereumConfigurations[net].names, userId)
      if (address) {
        // if an address was found for the name -> set as user id & add that instead
        userId = address
        found = true
      }
    }

    if (action === FriendshipAction.REQUESTED_TO && !found) {
      // if we still haven't the user by now (meaning the user has never logged and doesn't have a profile in the dao, or the user id is for a non wallet user or name is not correct) -> fail
      // tslint:disable-next-line
      unityInterface.FriendNotFound(userId)
      return
    }

    globalThis.globalStore.dispatch(updateUserData(userId.toLowerCase(), toSocialId(userId)))
    globalThis.globalStore.dispatch(updateFriendship(action, userId.toLowerCase(), false))
  }

  public SearchENSOwner(data: { name: string; maxResults?: number }) {
    const profilesPromise = fetchENSOwnerProfile(data.name, data.maxResults)

    profilesPromise
      .then((profiles) => {
        unityInterface.SetENSOwnerQueryResult(data.name, profiles)
      })
      .catch((error) => {
        unityInterface.SetENSOwnerQueryResult(data.name, undefined)
        defaultLogger.error(error)
      })
  }

  public async JumpIn(data: WorldPosition) {
    const {
      gridPosition: { x, y },
      realm: { serverName, layer }
    } = data

    const realmString = serverName + '-' + layer

    notifyStatusThroughChat(`Jumping to ${realmString} at ${x},${y}...`)

    const future = candidatesFetched()
    if (future.isPending) {
      notifyStatusThroughChat(`Waiting while realms are initialized, this may take a while...`)
    }

    await future

    const realm = changeRealm(realmString)

    if (realm) {
      catalystRealmConnected().then(
        () => {
          TeleportController.goTo(x, y, `Jumped to ${x},${y} in realm ${realmString}!`)
        },
        (e) => {
          const cause = e === 'realm-full' ? ' The requested realm is full.' : ''
          notifyStatusThroughChat('Could not join realm.' + cause)

          defaultLogger.error('Error joining realm', e)
        }
      )
    } else {
      notifyStatusThroughChat(`Couldn't find realm ${realmString}`)
    }
  }

  public ScenesLoadingFeedback(data: { message: string; loadPercentage: number }) {
    const { message, loadPercentage } = data
    globalThis.globalStore.dispatch(updateStatusMessage(message, loadPercentage))
  }

  public FetchHotScenes() {
    if (WORLD_EXPLORER) {
      reportHotScenes().catch((e: any) => {
        return defaultLogger.error('FetchHotScenes error', e)
      })
    }
  }

  public SetBaseResolution(data: { baseResolution: number }) {
    unityInterface.SetTargetHeight(data.baseResolution)
  }

  async RequestGIFProcessor(data: { imageSource: string; id: string; isWebGL1: boolean }) {
    if (!globalThis.gifProcessor) {
      globalThis.gifProcessor = new GIFProcessor(unityInterface.gameInstance, unityInterface, data.isWebGL1)
    }

    globalThis.gifProcessor.ProcessGIF(data)
  }

  public DeleteGIF(data: { value: string }) {
    if (globalThis.gifProcessor) {
      globalThis.gifProcessor.DeleteGIF(data.value)
    }
  }

  public async FetchBalanceOfMANA() {
    const identity = getIdentity()

    if (!identity?.hasConnectedWeb3) {
      return
    }

    const balance = (await getERC20Balance(identity.address, decentralandConfigurations.paymentTokens.MANA)).toNumber()
    if (this.lastBalanceOfMana !== balance) {
      this.lastBalanceOfMana = balance
      unityInterface.UpdateBalanceOfMANA(`${balance}`)
    }
  }

  public SetMuteUsers(data: { usersId: string[]; mute: boolean }) {
    if (data.mute) {
      globalThis.globalStore.dispatch(mutePlayers(data.usersId))
    } else {
      globalThis.globalStore.dispatch(unmutePlayers(data.usersId))
    }
  }

  public async KillPortableExperience(data: { portableExperienceId: string }): Promise<void> {
    await killPortableExperienceScene(data.portableExperienceId)
  }

  public RequestWearables(data: {
    filters: {
      ownedByUser: string | null
      wearableIds?: string[] | null
      collectionIds?: string[] | null
    }
    context?: string
  }) {
    const { filters, context } = data
    const newFilters: WearablesRequestFilters = {
      ownedByUser: filters.ownedByUser ?? undefined,
      wearableIds: this.arrayCleanup(filters.wearableIds),
      collectionIds: this.arrayCleanup(filters.collectionIds)
    }
    globalThis.globalStore.dispatch(wearablesRequest(newFilters, context))
  }

  public RequestUserProfile(userIdPayload: { value: string }) {
    ProfileAsPromise(userIdPayload.value, undefined, ProfileType.DEPLOYED)
      .then((profile) => unityInterface.AddUserProfileToCatalog(profileToRendererFormat(profile)))
      .catch((error) => defaultLogger.error(`error fetching profile ${userIdPayload.value} ${error}`))
  }

  private arrayCleanup<T>(array: T[] | null | undefined): T[] | undefined {
    return !array || array.length === 0 ? undefined : array
  }
}

export let browserInterface: BrowserInterface = new BrowserInterface()
