import {
  CreateEntityPayload,
  RemoveEntityPayload,
  EntityAction,
  QueryPayload,
  SetEntityParentPayload,
  OpenNFTDialogPayload,
  ComponentUpdatedPayload,
  ComponentRemovedPayload,
  ComponentDisposedPayload,
  ComponentCreatedPayload,
  AttachEntityComponentPayload,
  UpdateEntityComponentPayload,
  EntityActionType,
  QueryType
} from 'shared/types'
import type { UnityGame } from 'unity-interface/loader'
import { incrementMessageFromKernelToRendererNative } from 'shared/session/getPerformanceInfo'
import { _INTERNAL_WEB_TRANSPORT_ALLOC_SIZE } from 'renderer-protocol/transports/webTransport'

enum RaycastQueryType {
  NONE,
  HIT_FIRST = 1,
  HIT_ALL = 2,
  HIT_FIRST_AVATAR = 3,
  HIT_ALL_AVATARS = 4
}

function queryTypeToId(type: QueryType): number {
  switch (type) {
    case 'HitFirst':
      return RaycastQueryType.HIT_FIRST
    case 'HitAll':
      return RaycastQueryType.HIT_ALL
    case 'HitFirstAvatar':
      return RaycastQueryType.HIT_FIRST_AVATAR
    case 'HitAllAvatars':
      return RaycastQueryType.HIT_ALL_AVATARS
    default:
      return RaycastQueryType.NONE
  }
}

export class NativeMessagesBridge {
  private callCreateEntity!: () => void
  private callRemoveEntity!: () => void
  private callSceneReady!: () => void

  private callSetTag!: (tag: string) => void
  private callSetSceneId!: (sceneId: string) => void
  private callSetSceneNumber!: (sceneNumber: number) => void
  private callSetEntityId!: (entityId: string) => void

  private callSetEntityParent!: (parentId: string) => void

  private callEntityComponentCreateOrUpdate!: (classId: number, json: string) => void
  private callEntityComponentDestroy!: (name: string) => void

  private callSharedComponentCreate!: (classId: number, id: string) => void
  private callSharedComponentAttach!: (id: string) => void
  private callSharedComponentUpdate!: (id: string, json: string) => void
  private callSharedComponentDispose!: (id: string) => void

  private callOpenNftDialog!: (contactAddress: string, comment: string, tokenId: string) => void
  private callOpenExternalUrl!: (url: string) => void
  private callQuery!: (payload: number) => void

  private callLoadParcelScene!: (scene: never) => void
  private callUpdateParcelScene!: (scene: never) => void
  private callUnloadParcelScene!: (sceneId: string) => void

  private callSdk6BinaryMessage!: (ptr: number, length: number) => void

  private currentSceneId: string = ''
  private currentTag: string = ''
  private currentEntityId: string = ''

  private unityModule: any

  private queryMemBlockPtr: number = 0
  private binarySdk6MessageMemBlockPtr: number = 0
  private currentSceneNumber: number = 0

  public initNativeMessages(gameInstance: UnityGame) {
    this.unityModule = gameInstance.Module

    if (!this.unityModule) {
      console.error('Unity module not found! Are you in WSS mode?')
      return
    }

    const QUERY_MEM_SIZE = 40
    this.queryMemBlockPtr = this.unityModule._malloc(QUERY_MEM_SIZE)

    const SDK6_BINARY_MSG_MEM_SIZE = _INTERNAL_WEB_TRANSPORT_ALLOC_SIZE
    this.binarySdk6MessageMemBlockPtr = this.unityModule._malloc(SDK6_BINARY_MSG_MEM_SIZE)

    this.callSetEntityId = this.unityModule.cwrap('call_SetEntityId', null, ['string'])
    this.callSetSceneId = this.unityModule.cwrap('call_SetSceneId', null, ['string'])
    this.callSetSceneNumber = this.unityModule.cwrap('call_SetSceneNumber', null, ['number'])
    this.callSetTag = this.unityModule.cwrap('call_SetTag', null, ['string'])

    this.callSetEntityParent = this.unityModule.cwrap('call_SetEntityParent', null, ['string'])

    this.callEntityComponentCreateOrUpdate = this.unityModule.cwrap('call_EntityComponentCreateOrUpdate', null, [
      'number',
      'string'
    ])

    this.callEntityComponentDestroy = this.unityModule.cwrap('call_EntityComponentDestroy', null, ['string'])

    this.callSharedComponentCreate = this.unityModule.cwrap('call_SharedComponentCreate', null, ['number', 'string'])
    this.callSharedComponentAttach = this.unityModule.cwrap('call_SharedComponentAttach', null, ['string', 'string'])
    this.callSharedComponentUpdate = this.unityModule.cwrap('call_SharedComponentUpdate', null, ['string', 'string'])
    this.callSharedComponentDispose = this.unityModule.cwrap('call_SharedComponentDispose', null, ['string'])

    this.callOpenNftDialog = this.unityModule.cwrap('call_OpenNftDialog', null, ['string', 'string', 'string'])
    this.callOpenExternalUrl = this.unityModule.cwrap('call_OpenExternalUrl', null, ['string'])
    this.callQuery = this.unityModule.cwrap('call_Query', null, ['number'])

    this.callCreateEntity = this.unityModule.cwrap('call_CreateEntity', null, [])
    this.callRemoveEntity = this.unityModule.cwrap('call_RemoveEntity', null, [])
    this.callSceneReady = this.unityModule.cwrap('call_SceneReady', null, [])
    // This bind is used in webTransport.ts
    // this.callBinaryMessage = this.unityModule.cwrap('call_BinaryMessage', null, ['number', 'number', 'string'])
    this.callSdk6BinaryMessage = this.unityModule.cwrap('call_Sdk6BinaryMessage', null, ['number', 'number', 'string'])
  }

  public optimizeSendMessage() {
    // no-op
  }

  public isMethodSupported(_method: EntityActionType): boolean {
    return true
  }

  setSceneId(sceneId: string) {
    if (sceneId !== this.currentSceneId) {
      this.callSetSceneId(sceneId)
    }
    this.currentSceneId = sceneId
  }

  setSceneNumber(sceneNumber: number) {
    if (sceneNumber !== this.currentSceneNumber) {
      this.callSetSceneNumber(sceneNumber)
    }
    this.currentSceneNumber = sceneNumber
  }

  setEntityId(entityId: string) {
    if (entityId !== this.currentEntityId) {
      this.callSetEntityId(entityId)
    }
    this.currentEntityId = entityId
  }

  setTag(tag: string) {
    if (tag !== this.currentTag) {
      this.callSetTag(tag)
    }
    this.currentTag = tag
  }

  createEntity(payload: CreateEntityPayload) {
    this.setEntityId(payload.id)
    this.callCreateEntity()
  }

  removeEntity(payload: RemoveEntityPayload) {
    this.setEntityId(payload.id)
    this.callRemoveEntity()
  }

  sceneReady() {
    this.callSceneReady()
  }

  setEntityParent(payload: SetEntityParentPayload) {
    this.setEntityId(payload.entityId)
    this.callSetEntityParent(payload.parentId)
  }

  openNftDialog(payload: OpenNFTDialogPayload) {
    this.callOpenNftDialog(payload.assetContractAddress, payload.comment ?? '', payload.tokenId)
  }

  openExternalUrl(url: any) {
    this.callOpenExternalUrl(url)
  }

  query(queryPayload: QueryPayload) {
    let alignedPtr = this.queryMemBlockPtr >> 2

    const raycastType = queryTypeToId(queryPayload.payload.queryType)
    const id = queryPayload.payload.queryId

    alignedPtr++ // Skip first byte because the only query type is raycast. Not needed.
    this.unityModule.HEAP32[alignedPtr++] = id
    this.unityModule.HEAP32[alignedPtr++] = raycastType
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.origin.x
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.origin.y
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.origin.z
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.direction.x
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.direction.y
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.direction.z
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.distance

    this.callQuery(this.queryMemBlockPtr)
  }

  sharedComponentUpdate(payload: ComponentUpdatedPayload) {
    this.callSharedComponentUpdate(payload.id, payload.json)
  }

  entityComponentRemove(payload: ComponentRemovedPayload) {
    this.setEntityId(payload.entityId)
    this.callEntityComponentDestroy(payload.name)
  }

  sharedComponentDispose(payload: ComponentDisposedPayload) {
    this.callSharedComponentDispose(payload.id)
  }

  sharedComponentCreate(payload: ComponentCreatedPayload) {
    this.callSharedComponentCreate(payload.classId, payload.id)
  }

  sharedComponentAttach(payload: AttachEntityComponentPayload) {
    this.setEntityId(payload.entityId)
    this.callSharedComponentAttach(payload.id)
  }

  entityComponentCreateOrUpdate(payload: UpdateEntityComponentPayload) {
    this.setEntityId(payload.entityId)
    this.callEntityComponentCreateOrUpdate(payload.classId, payload.json)
  }

  public loadParcelScene(loadableParcelScene: never) {
    this.callLoadParcelScene(loadableParcelScene)
  }

  public updateParcelScene(loadableParcelScene: never) {
    this.callUpdateParcelScene(loadableParcelScene)
  }

  public unloadParcelScene(sceneId: string) {
    this.callUnloadParcelScene(sceneId)
  }

  public SendNativeMessage(parcelSceneId: string, sceneNumber: number, action: EntityAction): void {
    this.setSceneId(parcelSceneId)
    this.setSceneNumber(sceneNumber)

    // increment counter of messages sent
    incrementMessageFromKernelToRendererNative()

    if (action.tag !== undefined) {
      this.setTag(action.tag)
    } else {
      this.setTag('')
    }

    switch (action.type) {
      case 'CreateEntity':
        this.createEntity(action.payload)
        break
      case 'RemoveEntity':
        this.removeEntity(action.payload)
        break
      case 'InitMessagesFinished':
        this.sceneReady()
        break
      case 'SetEntityParent':
        this.setEntityParent(action.payload)
        break
      case 'UpdateEntityComponent':
        this.entityComponentCreateOrUpdate(action.payload)
        break
      case 'ComponentRemoved':
        this.entityComponentRemove(action.payload)
        break
      case 'AttachEntityComponent':
        this.sharedComponentAttach(action.payload)
        break
      case 'ComponentCreated':
        this.sharedComponentCreate(action.payload)
        break
      case 'ComponentDisposed':
        this.sharedComponentDispose(action.payload)
        break
      case 'ComponentUpdated':
        this.sharedComponentUpdate(action.payload)
        break
      case 'Query':
        this.query(action.payload)
        break
      case 'OpenExternalUrl':
        this.openExternalUrl(action.payload)
        break
      case 'OpenNFTDialog':
        this.openNftDialog(action.payload)
        break
    }
  }

  public sdk6BinaryMessage(sceneNumber: number, message: Uint8Array, messageLength: number) {
    const ptr = this.binarySdk6MessageMemBlockPtr
    this.unityModule.HEAPU8.set(message, ptr)
    this.setSceneNumber(sceneNumber)
    this.callSdk6BinaryMessage(ptr, messageLength)
  }
}

export const nativeMsgBridge: NativeMessagesBridge = new NativeMessagesBridge()
