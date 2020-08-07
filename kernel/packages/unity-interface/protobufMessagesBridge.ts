import { PB_CreateEntity, PB_RemoveEntity, PB_UpdateEntityComponent, PB_AttachEntityComponent, PB_ComponentRemoved, PB_SetEntityParent, PB_Query, PB_RayQuery, PB_Ray, PB_Vector3, PB_ComponentCreated, PB_ComponentDisposed, PB_ComponentUpdated, PB_OpenExternalUrl, PB_OpenNFTDialog, PB_SendSceneMessage } from '../shared/proto/engineinterface_pb'
import { CreateEntityPayload, RemoveEntityPayload, SetEntityParentPayload, ComponentRemovedPayload, ComponentCreatedPayload, QueryPayload, ComponentDisposedPayload, ComponentUpdatedPayload, OpenNFTDialogPayload, UpdateEntityComponentPayload, AttachEntityComponentPayload } from '../shared/types'
import { Empty } from 'google-protobuf/google/protobuf/empty_pb'

export class ProtobufMessagesBridge {
  // protobuf message instances
  createEntity: PB_CreateEntity = new PB_CreateEntity()
  removeEntity: PB_RemoveEntity = new PB_RemoveEntity()
  updateEntityComponent: PB_UpdateEntityComponent = new PB_UpdateEntityComponent()
  attachEntity: PB_AttachEntityComponent = new PB_AttachEntityComponent()
  removeEntityComponent: PB_ComponentRemoved = new PB_ComponentRemoved()
  setEntityParent: PB_SetEntityParent = new PB_SetEntityParent()
  query: PB_Query = new PB_Query()
  rayQuery: PB_RayQuery = new PB_RayQuery()
  ray: PB_Ray = new PB_Ray()
  origin: PB_Vector3 = new PB_Vector3()
  direction: PB_Vector3 = new PB_Vector3()
  componentCreated: PB_ComponentCreated = new PB_ComponentCreated()
  componentDisposed: PB_ComponentDisposed = new PB_ComponentDisposed()
  componentUpdated: PB_ComponentUpdated = new PB_ComponentUpdated()
  openExternalUrl: PB_OpenExternalUrl = new PB_OpenExternalUrl()
  openNFTDialog: PB_OpenNFTDialog = new PB_OpenNFTDialog()

  encodeSceneMessage(parcelSceneId: string, method: string, payload: any, tag: string = ''): string {
    let message: PB_SendSceneMessage = new PB_SendSceneMessage()
    message.setSceneid(parcelSceneId)
    message.setTag(tag)

    switch (method) {
      case 'CreateEntity':
        message.setCreateentity(this.encodeCreateEntity(payload))
        break
      case 'RemoveEntity':
        message.setRemoveentity(this.encodeRemoveEntity(payload))
        break
      case 'UpdateEntityComponent':
        message.setUpdateentitycomponent(this.encodeUpdateEntityComponent(payload))
        break
      case 'AttachEntityComponent':
        message.setAttachentitycomponent(this.encodeAttachEntityComponent(payload))
        break
      case 'ComponentRemoved':
        message.setComponentremoved(this.encodeComponentRemoved(payload))
        break
      case 'SetEntityParent':
        message.setSetentityparent(this.encodeSetEntityParent(payload))
        break
      case 'Query':
        message.setQuery(this.encodeQuery(payload))
        break
      case 'ComponentCreated':
        message.setComponentcreated(this.encodeComponentCreated(payload))
        break
      case 'ComponentDisposed':
        message.setComponentdisposed(this.encodeComponentDisposed(payload))
        break
      case 'ComponentUpdated':
        message.setComponentupdated(this.encodeComponentUpdated(payload))
        break
      case 'InitMessagesFinished':
        message.setScenestarted(new Empty()) // don't know if this is necessary
        break
      case 'OpenExternalUrl':
        message.setOpenexternalurl(this.encodeOpenExternalUrl(payload))
        break
      case 'OpenNFTDialog':
        message.setOpennftdialog(this.encodeOpenNFTDialog(payload))
        break
    }

    let arrayBuffer: Uint8Array = message.serializeBinary()
    return btoa(String.fromCharCode(...arrayBuffer))
  }

  encodeCreateEntity(createEntityPayload: CreateEntityPayload): PB_CreateEntity {
    this.createEntity.setId(createEntityPayload.id)
    return this.createEntity
  }

  encodeRemoveEntity(removeEntityPayload: RemoveEntityPayload): PB_RemoveEntity {
    this.removeEntity.setId(removeEntityPayload.id)
    return this.removeEntity
  }

  encodeUpdateEntityComponent(updateEntityComponentPayload: UpdateEntityComponentPayload): PB_UpdateEntityComponent {
    this.updateEntityComponent.setClassid(updateEntityComponentPayload.classId)
    this.updateEntityComponent.setEntityid(updateEntityComponentPayload.entityId)
    this.updateEntityComponent.setData(updateEntityComponentPayload.json)
    return this.updateEntityComponent
  }

  encodeAttachEntityComponent(attachEntityPayload: AttachEntityComponentPayload): PB_AttachEntityComponent {
    this.attachEntity.setEntityid(attachEntityPayload.entityId)
    this.attachEntity.setName(attachEntityPayload.name)
    this.attachEntity.setId(attachEntityPayload.id)
    return this.attachEntity
  }

  encodeComponentRemoved(removeEntityComponentPayload: ComponentRemovedPayload): PB_ComponentRemoved {
    this.removeEntityComponent.setEntityid(removeEntityComponentPayload.entityId)
    this.removeEntityComponent.setName(removeEntityComponentPayload.name)
    return this.removeEntityComponent
  }

  encodeSetEntityParent(setEntityParentPayload: SetEntityParentPayload): PB_SetEntityParent {
    this.setEntityParent.setEntityid(setEntityParentPayload.entityId)
    this.setEntityParent.setParentid(setEntityParentPayload.parentId)
    return this.setEntityParent
  }

  encodeQuery(queryPayload: QueryPayload): PB_Query {
    this.origin.setX(queryPayload.payload.ray.origin.x)
    this.origin.setY(queryPayload.payload.ray.origin.y)
    this.origin.setZ(queryPayload.payload.ray.origin.z)
    this.direction.setX(queryPayload.payload.ray.direction.x)
    this.direction.setY(queryPayload.payload.ray.direction.y)
    this.direction.setZ(queryPayload.payload.ray.direction.z)
    this.ray.setOrigin(this.origin)
    this.ray.setDirection(this.direction)
    this.ray.setDistance(queryPayload.payload.ray.distance)
    this.rayQuery.setRay(this.ray)
    this.rayQuery.setQueryid(queryPayload.payload.queryId)
    this.rayQuery.setQuerytype(queryPayload.payload.queryType)
    this.query.setQueryid(queryPayload.queryId)
    let arrayBuffer: Uint8Array = this.rayQuery.serializeBinary()
    let base64: string = btoa(String.fromCharCode(...arrayBuffer))
    this.query.setPayload(base64)
    return this.query
  }

  encodeComponentCreated(componentCreatedPayload: ComponentCreatedPayload): PB_ComponentCreated {
    this.componentCreated.setId(componentCreatedPayload.id)
    this.componentCreated.setClassid(componentCreatedPayload.classId)
    this.componentCreated.setName(componentCreatedPayload.name)
    return this.componentCreated
  }

  encodeComponentDisposed(componentDisposedPayload: ComponentDisposedPayload) {
    this.componentDisposed.setId(componentDisposedPayload.id)
    return this.componentDisposed
  }

  encodeComponentUpdated(componentUpdatedPayload: ComponentUpdatedPayload): PB_ComponentUpdated {
    this.componentUpdated.setId(componentUpdatedPayload.id)
    this.componentUpdated.setJson(componentUpdatedPayload.json)
    return this.componentUpdated
  }

  encodeOpenExternalUrl(url: any): PB_OpenExternalUrl {
    this.openExternalUrl.setUrl(url)
    return this.openExternalUrl
  }

  encodeOpenNFTDialog(nftDialogPayload: OpenNFTDialogPayload): PB_OpenNFTDialog {
    this.openNFTDialog.setAssetcontractaddress(nftDialogPayload.assetContractAddress)
    this.openNFTDialog.setTokenid(nftDialogPayload.tokenId)
    this.openNFTDialog.setComment(nftDialogPayload.comment ? nftDialogPayload.comment : '')
    return this.openNFTDialog
  }
}

export const protobufMsgBridge: ProtobufMessagesBridge = new ProtobufMessagesBridge()
