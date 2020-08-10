import { uuid } from 'decentraland-ecs/src'
import { persistCurrentUser, sendPublicChatMessage } from 'shared/comms'
import { AvatarMessageType } from 'shared/comms/interface/types'
import { avatarMessageObservable, getUserProfile } from 'shared/comms/peers'
import { getProfile, hasConnectedWeb3 } from 'shared/profiles/selectors'
import { TeleportController } from 'shared/world/TeleportController'
import { reportScenesAroundParcel } from 'shared/atlas/actions'
import { playerConfigurations, ethereumConfigurations } from 'config'
import { ReadOnlyQuaternion, ReadOnlyVector3, Vector3, Quaternion } from '../decentraland-ecs/src/decentraland/math'
import { IEventNames } from '../decentraland-ecs/src/decentraland/Types'
import { sceneLifeCycleObservable } from '../decentraland-loader/lifecycle/controllers/scene'
import { queueTrackingEvent } from 'shared/analytics'
import { aborted } from 'shared/loading/ReportFatalError'
import { defaultLogger } from 'shared/logger'
import { saveProfileRequest } from 'shared/profiles/actions'
import { Avatar, Profile } from 'shared/profiles/types'
import { getPerformanceInfo } from 'shared/session/getPerformanceInfo'
import { ChatMessage, FriendshipUpdateStatusMessage, FriendshipAction, WorldPosition } from 'shared/types'
import { getSceneWorkerBySceneID } from 'shared/world/parcelSceneManager'
import { positionObservable } from 'shared/world/positionThings'
import { worldRunningObservable } from 'shared/world/worldState'
import { profileToRendererFormat } from 'shared/profiles/transformations/profileToRendererFormat'
import { sendMessage } from 'shared/chat/actions'
import { updateUserData, updateFriendship } from 'shared/friends/actions'
import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'
import { changeRealm, catalystRealmConnected, candidatesFetched } from 'shared/dao'
import { notifyStatusThroughChat } from 'shared/comms/chat'
import { getAppNetwork, fetchOwner } from 'shared/web3'
import { updateStatusMessage } from 'shared/loading/actions'
import { UnityParcelScene } from './UnityParcelScene'
import { setAudioStream } from './audioStream'
import { logout } from 'shared/session/actions'
import { getIdentity, hasWallet } from 'shared/session'
import { StoreContainer } from 'shared/store/rootTypes'
import { unityInterface } from './UnityInterface'
import { IFuture } from 'fp-future'
import { reportHotScenes } from 'shared/social/hotScenes'

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
  mousePosition: Vector3.Zero()
}

export class BrowserInterface {
  /** Triggered when the camera moves */
  public ReportPosition(data: { position: ReadOnlyVector3; rotation: ReadOnlyQuaternion; playerHeight?: number }) {
    positionEvent.position.set(data.position.x, data.position.y, data.position.z)
    positionEvent.quaternion.set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w)
    positionEvent.rotation.copyFrom(positionEvent.quaternion.eulerAngles)
    positionEvent.playerHeight = data.playerHeight || playerConfigurations.height
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

  public SaveUserAvatar(changes: { face: string; face128: string; face256: string; body: string; avatar: Avatar }) {
    const { face, face128, face256, body, avatar } = changes
    const profile: Profile = getUserProfile().profile as Profile
    const updated = { ...profile, avatar: { ...avatar, snapshots: { face, face128, face256, body } } }
    globalThis.globalStore.dispatch(saveProfileRequest(updated))
  }

  public SaveUserTutorialStep(data: { tutorialStep: number }) {
    const profile: Profile = getUserProfile().profile as Profile
    profile.tutorialStep = data.tutorialStep
    globalThis.globalStore.dispatch(saveProfileRequest(profile))

    persistCurrentUser({
      version: profile.version,
      profile: profileToRendererFormat(profile, getIdentity())
    })
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

  public EditAvatarClicked() {
    // We used to call delightedSurvey() here
  }

  public ReportScene(sceneId: string) {
    this.OpenWebURL({ url: `https://decentralandofficial.typeform.com/to/KzaUxh?sceneId=${sceneId}` })
  }

  public ReportPlayer(username: string) {
    this.OpenWebURL({ url: `https://decentralandofficial.typeform.com/to/owLkla?username=${username}` })
  }

  public BlockPlayer(data: { userId: string }) {
    const profile = getProfile(globalThis.globalStore.getState(), getIdentity().address)

    if (profile) {
      let blocked: string[] = [data.userId]

      if (profile.blocked) {
        for (let blockedUser of profile.blocked) {
          if (blockedUser === data.userId) {
            return
          }
        }

        // Merge the existing array and any previously blocked users
        blocked = [...profile.blocked, ...blocked]
      }

      globalThis.globalStore.dispatch(saveProfileRequest({ ...profile, blocked }))
    }
  }

  public UnblockPlayer(data: { userId: string }) {
    const profile = getProfile(globalThis.globalStore.getState(), getIdentity().address)

    if (profile) {
      const blocked = profile.blocked ? profile.blocked.filter((id) => id !== data.userId) : []
      globalThis.globalStore.dispatch(saveProfileRequest({ ...profile, blocked }))
    }
  }

  public ReportUserEmail(data: { userEmail: string }) {
    const profile = getUserProfile().profile
    if (profile) {
      if (hasWallet()) {
        window.analytics.identify(profile.userId, { email: data.userEmail })
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

  public async UpdateFriendshipStatus(message: FriendshipUpdateStatusMessage) {
    let { userId, action } = message

    // TODO - fix this hack: search should come from another message and method should only exec correct updates (userId, action) - moliva - 01/05/2020
    let found = false
    if (action === FriendshipAction.REQUESTED_TO) {
      await ProfileAsPromise(userId) // ensure profile
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
}

export let browserInterface: BrowserInterface = new BrowserInterface()
