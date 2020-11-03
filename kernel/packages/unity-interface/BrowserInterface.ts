import { uuid } from 'decentraland-ecs/src'
import { sendPublicChatMessage } from 'shared/comms'
import { AvatarMessageType } from 'shared/comms/interface/types'
import { avatarMessageObservable } from 'shared/comms/peers'
import { hasConnectedWeb3 } from 'shared/profiles/selectors'
import { TeleportController } from 'shared/world/TeleportController'
import { reportScenesAroundParcel } from 'shared/atlas/actions'
import { playerConfigurations, ethereumConfigurations, decentralandConfigurations } from 'config'
import { ReadOnlyQuaternion, ReadOnlyVector3, Vector3, Quaternion } from '../decentraland-ecs/src/decentraland/math'
import { IEventNames } from '../decentraland-ecs/src/decentraland/Types'
import { sceneLifeCycleObservable } from '../decentraland-loader/lifecycle/controllers/scene'
import { queueTrackingEvent } from 'shared/analytics'
import { aborted } from 'shared/loading/ReportFatalError'
import { defaultLogger } from 'shared/logger'
import { saveProfileRequest } from 'shared/profiles/actions'
import { Avatar } from 'shared/profiles/types'
import { getPerformanceInfo } from 'shared/session/getPerformanceInfo'
import { ChatMessage, FriendshipUpdateStatusMessage, FriendshipAction, WorldPosition } from 'shared/types'
import { getSceneWorkerBySceneID } from 'shared/world/parcelSceneManager'
import { positionObservable } from 'shared/world/positionThings'
import { worldRunningObservable } from 'shared/world/worldState'
import { sendMessage } from 'shared/chat/actions'
import { updateUserData, updateFriendship } from 'shared/friends/actions'
import { changeRealm, catalystRealmConnected, candidatesFetched } from 'shared/dao'
import { notifyStatusThroughChat } from 'shared/comms/chat'
import { getAppNetwork, fetchOwner } from 'shared/web3'
import { updateStatusMessage } from 'shared/loading/actions'
import { blockPlayers, mutePlayers, unblockPlayers, unmutePlayers } from 'shared/social/actions'
import { UnityParcelScene } from './UnityParcelScene'
import { setAudioStream } from './audioStream'
import { logout } from 'shared/session/actions'
import { getIdentity, hasWallet } from 'shared/session'
import { StoreContainer } from 'shared/store/rootTypes'
import { unityInterface } from './UnityInterface'
import { setDelightedSurveyEnabled } from './delightedSurvey'
import { IFuture } from 'fp-future'
import { reportHotScenes } from 'shared/social/hotScenes'

import { GIFProcessor } from 'gif-processor/processor'
import { setVoiceChatRecording, setVoicePolicy, setVoiceVolume, toggleVoiceChatRecording } from 'shared/comms/actions'
import { getERC20Balance } from 'shared/ethereum/EthereumService'
import { getCurrentUserId } from 'shared/session/selectors'
import { ensureFriendProfile } from 'shared/friends/ensureFriendProfile'

declare const DCL: any

declare const globalThis: StoreContainer
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
      const parcelScene = scene.parcelScene as UnityParcelScene
      parcelScene.emit(data.eventType as IEventNames, data.payload)
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

  public PerformanceReport(samples: string) {
    const perfReport = getPerformanceInfo(samples)
    queueTrackingEvent('performance report', perfReport)
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

  public SaveUserInterests(interests: string[]) {
    if (!interests) {
      return
    }
    const unique = new Set<string>(interests)

    globalThis.globalStore.dispatch(saveProfileRequest({ interests: Array.from(unique) }))
  }

  public SaveUserAvatar(changes: { face: string; face128: string; face256: string; body: string; avatar: Avatar }) {
    const { face, face128, face256, body, avatar } = changes
    const update = { avatar: { ...avatar, snapshots: { face, face128, face256, body } } }
    globalThis.globalStore.dispatch(saveProfileRequest(update))
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
          worldRunningObservable.notifyObservers(true)
        }
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
      if (hasWallet()) {
        window.analytics.identify(userId, { email: data.userEmail })
      } else {
        window.analytics.identify({ email: data.userEmail })
      }
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

  public ApplySettings(settingsMessage: { voiceChatVolume: number, voiceChatAllowCategory: number }) {
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
      const address = await fetchOwner(ethereumConfigurations[net].names, userId)
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
    reportHotScenes().catch((e: any) => {
      return defaultLogger.error('FetchHotScenes error', e)
    })
  }

  public SetBaseResolution(data: { baseResolution: number }) {
    unityInterface.SetTargetHeight(data.baseResolution)
  }

  async RequestGIFProcessor(data: { imageSource: string; id: string; isWebGL1: boolean }) {
    const isSupported =
      // tslint:disable-next-line
      typeof OffscreenCanvas !== 'undefined' && typeof OffscreenCanvasRenderingContext2D === 'function'

    if (!isSupported) {
      unityInterface.RejectGIFProcessingRequest()
      return
    }

    if (!DCL.gifProcessor) {
      DCL.gifProcessor = new GIFProcessor(unityInterface.gameInstance, unityInterface, data.isWebGL1)
    }

    DCL.gifProcessor.ProcessGIF(data)
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
}

export let browserInterface: BrowserInterface = new BrowserInterface()
