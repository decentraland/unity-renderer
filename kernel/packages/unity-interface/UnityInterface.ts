import { TeleportController } from 'shared/world/TeleportController'
import { WSS_ENABLED, WORLD_EXPLORER, RESET_TUTORIAL, EDITOR } from 'config'
import { Vector3 } from '../decentraland-ecs/src/decentraland/math'
import { ProfileForRenderer, MinimapSceneInfo } from '../decentraland-ecs/src/decentraland/Types'
import { AirdropInfo } from 'shared/airdrops/interface'
import {
  HUDConfiguration,
  InstancedSpawnPoint,
  LoadableParcelScene,
  Notification,
  ChatMessage,
  HUDElementID,
  FriendsInitializationMessage,
  FriendshipUpdateStatusMessage,
  UpdateUserStatusMessage,
  RenderProfile,
  BuilderConfiguration,
  Wearable,
  KernelConfigForRenderer,
  RealmsInfoForRenderer,
  ContentMapping
} from 'shared/types'
import { nativeMsgBridge } from './nativeMessagesBridge'
import { HotSceneInfo } from 'shared/social/hotScenes'
import { defaultLogger } from 'shared/logger'
import { setDelightedSurveyEnabled } from './delightedSurvey'
import { renderStateObservable } from '../shared/world/worldState'
import { DeploymentResult } from '../shared/apis/SceneStateStorageController/types'
import { ReportRendererInterfaceError } from 'shared/loading/ReportFatalError'
import { QuestForRenderer } from 'dcl-ecs-quests/src/types'

const MINIMAP_CHUNK_SIZE = 100

let _gameInstance: any = null
let originalFillMouseEventData: any

function fillMouseEventDataWrapper(eventStruct: any, e: any, target: any) {
  let widthRatio = _gameInstance.Module.canvas.widthNative / (window.innerWidth * devicePixelRatio)
  let heightRatio = _gameInstance.Module.canvas.heightNative / (window.innerHeight * devicePixelRatio)

  let eWrapper: any = {
    clientX: e.clientX * widthRatio,
    clientY: e.clientY * heightRatio,
    screenX: e.screenX,
    screenY: e.screenY,
    ctrlKey: e.ctrlKey,
    shiftKey: e.shiftKey,
    altKey: e.altKey,
    metaKey: e.metaKey,
    button: e.button,
    buttons: e.buttons,
    movementX: e['movementX'] || e['mozMovementX'] || e['webkitMovementX'],
    movementY: e['movementY'] || e['mozMovementY'] || e['webkitMovementY'],
    type: e.type
  }

  originalFillMouseEventData(eventStruct, eWrapper, target)
}

export let targetHeight: number = 1080

function resizeCanvas(module: any) {
  if (targetHeight > 2000) {
    targetHeight = window.innerHeight * devicePixelRatio
  }

  let desiredHeight = targetHeight

  let ratio = desiredHeight / module.canvas.height
  module.setCanvasSize(module.canvas.width * ratio, module.canvas.height * ratio)
}

export class UnityInterface {
  public debug: boolean = false
  public gameInstance: any
  public Module: any

  public SetTargetHeight(height: number): void {
    if (EDITOR) {
      return
    }

    if (targetHeight === height) {
      return
    }

    if (!this.gameInstance.Module) {
      defaultLogger.log(
        `Can't change base resolution height to ${height}! Are you running explorer in unity editor or native?`
      )
      return
    }

    targetHeight = height
    window.dispatchEvent(new Event('resize'))
  }

  public Init(gameInstance: any): void {
    if (!WSS_ENABLED) {
      nativeMsgBridge.initNativeMessages(gameInstance)
    }

    this.gameInstance = gameInstance
    this.Module = this.gameInstance.Module
    _gameInstance = gameInstance

    if (this.Module) {
      if (EDITOR) {
        const canvas = this.Module.canvas
        canvas.width = canvas.parentElement.clientWidth
        canvas.height = canvas.parentElement.clientHeight
      } else {
        window.addEventListener('resize', this.resizeCanvasDelayed)

        document.addEventListener('visibilitychange', () => {
          if (document.visibilityState === 'visible') resizeCanvas(this.Module)
        })

        this.resizeCanvasDelayed(null)
        this.waitForFillMouseEventData()
      }
    }
  }

  public waitForFillMouseEventData() {
    let DCL = (window as any)['DCL']

    if (DCL.JSEvents !== undefined) {
      originalFillMouseEventData = DCL.JSEvents.fillMouseEventData
      DCL.JSEvents.fillMouseEventData = fillMouseEventDataWrapper
    } else {
      setTimeout(this.waitForFillMouseEventData, 100)
    }
  }

  public SendGenericMessage(object: string, method: string, payload: string) {
    this.SendMessageToUnity(object, method, payload)
  }

  public SetDebug() {
    this.SendMessageToUnity('Main', 'SetDebug')
  }

  public LoadProfile(profile: ProfileForRenderer) {
    this.SendMessageToUnity('Main', 'LoadProfile', JSON.stringify(profile))
  }

  public SetRenderProfile(id: RenderProfile) {
    this.SendMessageToUnity('Main', 'SetRenderProfile', JSON.stringify({ id: id }))
  }

  public DumpScenesLoadInfo() {
    this.SendMessageToUnity('Main', 'DumpScenesLoadInfo')
  }

  public DumpRendererLockersInfo() {
    this.SendMessageToUnity('Main', 'DumpRendererLockersInfo')
  }

  public CreateGlobalScene(data: {
    id: string;
    name: string;
    baseUrl: string,
    contents: Array<ContentMapping>,
    icon?: string,
    isPortableExperience: boolean,
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

    TeleportController.ensureTeleportAnimation()
    this.SendMessageToUnity('CharacterController', 'Teleport', JSON.stringify({ x, y: theY, z }))
    if (cameraTarget || rotateIfTargetIsNotSet) {
      this.SendMessageToUnity('CameraController', 'SetRotation', JSON.stringify({ x, y: theY, z, cameraTarget }))
    }
  }

  /** Tells the engine which scenes to load */

  public LoadParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!')
    }

    this.SendMessageToUnity('Main', 'LoadParcelScenes', JSON.stringify(parcelsToLoad[0]))
  }

  public UpdateParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!')
    }
    this.SendMessageToUnity('Main', 'UpdateParcelScenes', JSON.stringify(parcelsToLoad[0]))
  }

  public UnloadScene(sceneId: string) {
    this.SendMessageToUnity('Main', 'UnloadScene', sceneId)
  }

  public SendSceneMessage(messages: string) {
    this.SendMessageToUnity(`SceneController`, `SendSceneMessage`, messages)
  }

  public SetSceneDebugPanel() {
    this.SendMessageToUnity('Main', 'SetSceneDebugPanel')
  }

  public ShowFPSPanel() {
    this.SendMessageToUnity('Main', 'ShowFPSPanel')
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

  public ActivateRendering() {
    this.SendMessageToUnity('Main', 'ActivateRendering')
  }

  public DeactivateRendering() {
    renderStateObservable.notifyObservers(false)
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

  public AddUserProfileToCatalog(peerProfile: ProfileForRenderer) {
    this.SendMessageToUnity('Main', 'AddUserProfileToCatalog', JSON.stringify(peerProfile))
  }

  public AddWearablesToCatalog(wearables: Wearable[], context?: string) {
    this.SendMessageToUnity('Main', 'AddWearablesToCatalog', JSON.stringify({ wearables, context }))
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

  public TriggerSelfUserExpression(expressionId: string) {
    this.SendMessageToUnity('HUDController', 'TriggerSelfUserExpression', expressionId)
  }

  public UpdateMinimapSceneInformation(info: MinimapSceneInfo[]) {
    for (let i = 0; i < info.length; i += MINIMAP_CHUNK_SIZE) {
      const chunk = info.slice(i, i + MINIMAP_CHUNK_SIZE)
      this.SendMessageToUnity('Main', 'UpdateMinimapSceneInformation', JSON.stringify(chunk))
    }
  }

  public SetTutorialEnabled(fromDeepLink: boolean) {
    this.SendMessageToUnity('TutorialController', 'SetTutorialEnabled', JSON.stringify(fromDeepLink))
  }

  public SetTutorialEnabledForUsersThatAlreadyDidTheTutorial() {
    this.SendMessageToUnity('TutorialController', 'SetTutorialEnabledForUsersThatAlreadyDidTheTutorial')
  }

  public TriggerAirdropDisplay(data: AirdropInfo) {
    // Disabled for security reasons
  }

  public AddMessageToChatWindow(message: ChatMessage) {
    this.SendMessageToUnity('Main', 'AddMessageToChatWindow', JSON.stringify(message))
  }

  public InitializeFriends(initializationMessage: FriendsInitializationMessage) {
    this.SendMessageToUnity('Main', 'InitializeFriends', JSON.stringify(initializationMessage))
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

  public RequestTeleport(teleportData: {}) {
    this.SendMessageToUnity('HUDController', 'RequestTeleport', JSON.stringify(teleportData))
  }

  public UpdateHotScenesList(info: HotSceneInfo[]) {
    const chunks = []

    while (info.length) {
      chunks.push(info.splice(0, MINIMAP_CHUNK_SIZE))
    }

    for (let i = 0; i < chunks.length; i++) {
      const payload = { chunkIndex: i, chunksCount: chunks.length, scenesInfo: chunks[i] }
      this.SendMessageToUnity('Main', 'UpdateHotScenesList', JSON.stringify(payload))
    }
  }

  public SendGIFPointers(id: string, width: number, height: number, pointers: number[], frameDelays: number[]) {
    this.SendMessageToUnity('Main', 'UpdateGIFPointers', JSON.stringify({ id, width, height, pointers, frameDelays }))
  }

  public SendGIFFetchFailure(id: string) {
    this.SendMessageToUnity('Main', 'FailGIFFetch', id)
  }

  public ConfigureEmailPrompt(tutorialStep: number) {
    const emailCompletedFlag = 128
    this.ConfigureHUDElement(HUDElementID.EMAIL_PROMPT, {
      active: (tutorialStep & emailCompletedFlag) === 0,
      visible: false
    })
  }

  public ConfigureTutorial(tutorialStep: number, fromDeepLink: boolean) {
    const tutorialCompletedFlag = 256

    if (WORLD_EXPLORER) {
      if (RESET_TUTORIAL || (tutorialStep & tutorialCompletedFlag) === 0) {
        this.SetTutorialEnabled(fromDeepLink)
      } else {
        this.SetTutorialEnabledForUsersThatAlreadyDidTheTutorial()
        setDelightedSurveyEnabled(true)
      }
    }
  }

  public UpdateBalanceOfMANA(balance: string) {
    this.SendMessageToUnity('HUDController', 'UpdateBalanceOfMANA', balance)
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

  public SetKernelConfiguration(config: KernelConfigForRenderer) {
    this.SendMessageToUnity('Bridges', 'SetKernelConfiguration', JSON.stringify(config))
  }

  public UpdateRealmsInfo(realmsInfo: Partial<RealmsInfoForRenderer>) {
    this.SendMessageToUnity('Bridges', 'UpdateRealmsInfo', JSON.stringify(realmsInfo))
  }

  public SendPublishSceneResult(result: DeploymentResult) {
    this.SendMessageToUnity('Main', 'PublishSceneResult', JSON.stringify(result))
  }

  // *********************************************************************************
  // ************** Quests messages **************
  // *********************************************************************************

  InitQuestsInfo(rendererQuests: QuestForRenderer[]) {
    this.SendMessageToUnity('Main', 'InitializeQuests', JSON.stringify(rendererQuests))
  }

  UpdateQuestProgress(rendererQuest: QuestForRenderer) {
    this.SendMessageToUnity('Main', 'UpdateQuestProgress', JSON.stringify(rendererQuest))
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

  private resizeCanvasDelayed(ev: UIEvent | null) {
    window.setTimeout(() => {
      resizeCanvas(_gameInstance.Module)
    }, 100)
  }

  // NOTE: we override wasm's setThrew function before sending message to unity and restore it to it's
  // original function after message is sent. If an exception is thrown during SendMessage we assume that it's related
  // to the code executed by the SendMessage on unity's side.
  private SendMessageToUnity(object: string, method: string, payload: any = undefined) {
    // "this.Module" is not present when using remote websocket renderer, so we just send the message to unity without doing any override.
    if (!this.Module) {
      this.gameInstance.SendMessage(object, method, payload)
      return
    }

    const originalSetThrew = this.Module['setThrew']
    const unityModule = this.Module
    let isError = false

    function overrideSetThrew() {
      unityModule['setThrew'] = function () {
        isError = true
        return originalSetThrew.apply(this, arguments)
      }
    }

    function restoreSetThrew() {
      unityModule['setThrew'] = originalSetThrew
    }

    overrideSetThrew()
    try {
      this.gameInstance.SendMessage(object, method, payload)
    } finally {
      restoreSetThrew()
    }

    if (isError) {
      const error = `Error while sending Message to Unity. Object: ${object}. Method: ${method}. Payload: ${payload}.`
      defaultLogger.error(error)
      ReportRendererInterfaceError(error, error)
    }
  }
}

export let unityInterface: UnityInterface = new UnityInterface()
