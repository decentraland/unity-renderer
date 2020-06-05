// package: engineinterface
// file: engineinterface.proto

import * as jspb from 'google-protobuf'
import * as google_protobuf_empty_pb from 'google-protobuf/google/protobuf/empty_pb'

export class PB_CreateEntity extends jspb.Message {
  getId(): string
  setId(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_CreateEntity.AsObject
  static toObject(includeInstance: boolean, msg: PB_CreateEntity): PB_CreateEntity.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_CreateEntity, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_CreateEntity
  static deserializeBinaryFromReader(message: PB_CreateEntity, reader: jspb.BinaryReader): PB_CreateEntity
}

export namespace PB_CreateEntity {
  export type AsObject = {
    id: string
  }
}

export class PB_RemoveEntity extends jspb.Message {
  getId(): string
  setId(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_RemoveEntity.AsObject
  static toObject(includeInstance: boolean, msg: PB_RemoveEntity): PB_RemoveEntity.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_RemoveEntity, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_RemoveEntity
  static deserializeBinaryFromReader(message: PB_RemoveEntity, reader: jspb.BinaryReader): PB_RemoveEntity
}

export namespace PB_RemoveEntity {
  export type AsObject = {
    id: string
  }
}

export class PB_SetEntityParent extends jspb.Message {
  getEntityid(): string
  setEntityid(value: string): void

  getParentid(): string
  setParentid(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_SetEntityParent.AsObject
  static toObject(includeInstance: boolean, msg: PB_SetEntityParent): PB_SetEntityParent.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_SetEntityParent, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_SetEntityParent
  static deserializeBinaryFromReader(message: PB_SetEntityParent, reader: jspb.BinaryReader): PB_SetEntityParent
}

export namespace PB_SetEntityParent {
  export type AsObject = {
    entityid: string
    parentid: string
  }
}

export class PB_ComponentRemoved extends jspb.Message {
  getEntityid(): string
  setEntityid(value: string): void

  getName(): string
  setName(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_ComponentRemoved.AsObject
  static toObject(includeInstance: boolean, msg: PB_ComponentRemoved): PB_ComponentRemoved.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_ComponentRemoved, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_ComponentRemoved
  static deserializeBinaryFromReader(message: PB_ComponentRemoved, reader: jspb.BinaryReader): PB_ComponentRemoved
}

export namespace PB_ComponentRemoved {
  export type AsObject = {
    entityid: string
    name: string
  }
}

export class PB_Component extends jspb.Message {
  hasTransform(): boolean
  clearTransform(): void
  getTransform(): PB_Transform | undefined
  setTransform(value?: PB_Transform): void

  hasUuidcallback(): boolean
  clearUuidcallback(): void
  getUuidcallback(): PB_UUIDCallback | undefined
  setUuidcallback(value?: PB_UUIDCallback): void

  hasBox(): boolean
  clearBox(): void
  getBox(): PB_BoxShape | undefined
  setBox(value?: PB_BoxShape): void

  hasSphere(): boolean
  clearSphere(): void
  getSphere(): PB_SphereShape | undefined
  setSphere(value?: PB_SphereShape): void

  hasPlane(): boolean
  clearPlane(): void
  getPlane(): PB_PlaneShape | undefined
  setPlane(value?: PB_PlaneShape): void

  hasCone(): boolean
  clearCone(): void
  getCone(): PB_ConeShape | undefined
  setCone(value?: PB_ConeShape): void

  hasCylinder(): boolean
  clearCylinder(): void
  getCylinder(): PB_CylinderShape | undefined
  setCylinder(value?: PB_CylinderShape): void

  hasText(): boolean
  clearText(): void
  getText(): PB_TextShape | undefined
  setText(value?: PB_TextShape): void

  hasNft(): boolean
  clearNft(): void
  getNft(): PB_NFTShape | undefined
  setNft(value?: PB_NFTShape): void

  hasContainerrect(): boolean
  clearContainerrect(): void
  getContainerrect(): PB_UIContainerRect | undefined
  setContainerrect(value?: PB_UIContainerRect): void

  hasContainerstack(): boolean
  clearContainerstack(): void
  getContainerstack(): PB_UIContainerStack | undefined
  setContainerstack(value?: PB_UIContainerStack): void

  hasUitextshape(): boolean
  clearUitextshape(): void
  getUitextshape(): PB_UITextShape | undefined
  setUitextshape(value?: PB_UITextShape): void

  hasUiinputtextshape(): boolean
  clearUiinputtextshape(): void
  getUiinputtextshape(): PB_UIInputText | undefined
  setUiinputtextshape(value?: PB_UIInputText): void

  hasUiimageshape(): boolean
  clearUiimageshape(): void
  getUiimageshape(): PB_UIImage | undefined
  setUiimageshape(value?: PB_UIImage): void

  hasCircle(): boolean
  clearCircle(): void
  getCircle(): PB_CircleShape | undefined
  setCircle(value?: PB_CircleShape): void

  hasBillboard(): boolean
  clearBillboard(): void
  getBillboard(): PB_Billboard | undefined
  setBillboard(value?: PB_Billboard): void

  hasGltf(): boolean
  clearGltf(): void
  getGltf(): PB_GLTFShape | undefined
  setGltf(value?: PB_GLTFShape): void

  hasObj(): boolean
  clearObj(): void
  getObj(): PB_OBJShape | undefined
  setObj(value?: PB_OBJShape): void

  hasAvatar(): boolean
  clearAvatar(): void
  getAvatar(): PB_AvatarShape | undefined
  setAvatar(value?: PB_AvatarShape): void

  hasBasicmaterial(): boolean
  clearBasicmaterial(): void
  getBasicmaterial(): PB_BasicMaterial | undefined
  setBasicmaterial(value?: PB_BasicMaterial): void

  hasTexture(): boolean
  clearTexture(): void
  getTexture(): PB_Texture | undefined
  setTexture(value?: PB_Texture): void

  hasAudioclip(): boolean
  clearAudioclip(): void
  getAudioclip(): PB_AudioClip | undefined
  setAudioclip(value?: PB_AudioClip): void

  hasAudiosource(): boolean
  clearAudiosource(): void
  getAudiosource(): PB_AudioSource | undefined
  setAudiosource(value?: PB_AudioSource): void

  getModelCase(): PB_Component.ModelCase
  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Component.AsObject
  static toObject(includeInstance: boolean, msg: PB_Component): PB_Component.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Component, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Component
  static deserializeBinaryFromReader(message: PB_Component, reader: jspb.BinaryReader): PB_Component
}

export namespace PB_Component {
  export type AsObject = {
    transform?: PB_Transform.AsObject
    uuidcallback?: PB_UUIDCallback.AsObject
    box?: PB_BoxShape.AsObject
    sphere?: PB_SphereShape.AsObject
    plane?: PB_PlaneShape.AsObject
    cone?: PB_ConeShape.AsObject
    cylinder?: PB_CylinderShape.AsObject
    text?: PB_TextShape.AsObject
    nft?: PB_NFTShape.AsObject
    containerrect?: PB_UIContainerRect.AsObject
    containerstack?: PB_UIContainerStack.AsObject
    uitextshape?: PB_UITextShape.AsObject
    uiinputtextshape?: PB_UIInputText.AsObject
    uiimageshape?: PB_UIImage.AsObject
    circle?: PB_CircleShape.AsObject
    billboard?: PB_Billboard.AsObject
    gltf?: PB_GLTFShape.AsObject
    obj?: PB_OBJShape.AsObject
    avatar?: PB_AvatarShape.AsObject
    basicmaterial?: PB_BasicMaterial.AsObject
    texture?: PB_Texture.AsObject
    audioclip?: PB_AudioClip.AsObject
    audiosource?: PB_AudioSource.AsObject
  }

  export enum ModelCase {
    MODEL_NOT_SET = 0,
    TRANSFORM = 1,
    UUIDCALLBACK = 8,
    BOX = 16,
    SPHERE = 17,
    PLANE = 18,
    CONE = 19,
    CYLINDER = 20,
    TEXT = 21,
    NFT = 22,
    CONTAINERRECT = 25,
    CONTAINERSTACK = 26,
    UITEXTSHAPE = 27,
    UIINPUTTEXTSHAPE = 28,
    UIIMAGESHAPE = 29,
    CIRCLE = 31,
    BILLBOARD = 32,
    GLTF = 54,
    OBJ = 55,
    AVATAR = 56,
    BASICMATERIAL = 64,
    TEXTURE = 68,
    AUDIOCLIP = 200,
    AUDIOSOURCE = 201
  }
}

export class PB_Color4 extends jspb.Message {
  getR(): number
  setR(value: number): void

  getG(): number
  setG(value: number): void

  getB(): number
  setB(value: number): void

  getA(): number
  setA(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Color4.AsObject
  static toObject(includeInstance: boolean, msg: PB_Color4): PB_Color4.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Color4, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Color4
  static deserializeBinaryFromReader(message: PB_Color4, reader: jspb.BinaryReader): PB_Color4
}

export namespace PB_Color4 {
  export type AsObject = {
    r: number
    g: number
    b: number
    a: number
  }
}

export class PB_Color3 extends jspb.Message {
  getR(): number
  setR(value: number): void

  getG(): number
  setG(value: number): void

  getB(): number
  setB(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Color3.AsObject
  static toObject(includeInstance: boolean, msg: PB_Color3): PB_Color3.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Color3, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Color3
  static deserializeBinaryFromReader(message: PB_Color3, reader: jspb.BinaryReader): PB_Color3
}

export namespace PB_Color3 {
  export type AsObject = {
    r: number
    g: number
    b: number
  }
}

export class PB_TextShapeModel extends jspb.Message {
  getBillboard(): boolean
  setBillboard(value: boolean): void

  getValue(): string
  setValue(value: string): void

  hasColor(): boolean
  clearColor(): void
  getColor(): PB_Color3 | undefined
  setColor(value?: PB_Color3): void

  getOpacity(): number
  setOpacity(value: number): void

  getFontsize(): number
  setFontsize(value: number): void

  getFontautosize(): boolean
  setFontautosize(value: boolean): void

  getFontweight(): string
  setFontweight(value: string): void

  getHtextalign(): string
  setHtextalign(value: string): void

  getVtextalign(): string
  setVtextalign(value: string): void

  getWidth(): number
  setWidth(value: number): void

  getHeight(): number
  setHeight(value: number): void

  getAdaptwidth(): boolean
  setAdaptwidth(value: boolean): void

  getAdaptheight(): boolean
  setAdaptheight(value: boolean): void

  getPaddingtop(): number
  setPaddingtop(value: number): void

  getPaddingright(): number
  setPaddingright(value: number): void

  getPaddingbottom(): number
  setPaddingbottom(value: number): void

  getPaddingleft(): number
  setPaddingleft(value: number): void

  getLinespacing(): number
  setLinespacing(value: number): void

  getLinecount(): number
  setLinecount(value: number): void

  getTextwrapping(): boolean
  setTextwrapping(value: boolean): void

  getShadowblur(): number
  setShadowblur(value: number): void

  getShadowoffsetx(): number
  setShadowoffsetx(value: number): void

  getShadowoffsety(): number
  setShadowoffsety(value: number): void

  hasShadowcolor(): boolean
  clearShadowcolor(): void
  getShadowcolor(): PB_Color3 | undefined
  setShadowcolor(value?: PB_Color3): void

  getOutlinewidth(): number
  setOutlinewidth(value: number): void

  hasOutlinecolor(): boolean
  clearOutlinecolor(): void
  getOutlinecolor(): PB_Color3 | undefined
  setOutlinecolor(value?: PB_Color3): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_TextShapeModel.AsObject
  static toObject(includeInstance: boolean, msg: PB_TextShapeModel): PB_TextShapeModel.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_TextShapeModel, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_TextShapeModel
  static deserializeBinaryFromReader(message: PB_TextShapeModel, reader: jspb.BinaryReader): PB_TextShapeModel
}

export namespace PB_TextShapeModel {
  export type AsObject = {
    billboard: boolean
    value: string
    color?: PB_Color3.AsObject
    opacity: number
    fontsize: number
    fontautosize: boolean
    fontweight: string
    htextalign: string
    vtextalign: string
    width: number
    height: number
    adaptwidth: boolean
    adaptheight: boolean
    paddingtop: number
    paddingright: number
    paddingbottom: number
    paddingleft: number
    linespacing: number
    linecount: number
    textwrapping: boolean
    shadowblur: number
    shadowoffsetx: number
    shadowoffsety: number
    shadowcolor?: PB_Color3.AsObject
    outlinewidth: number
    outlinecolor?: PB_Color3.AsObject
  }
}

export class PB_Vector3 extends jspb.Message {
  getX(): number
  setX(value: number): void

  getY(): number
  setY(value: number): void

  getZ(): number
  setZ(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Vector3.AsObject
  static toObject(includeInstance: boolean, msg: PB_Vector3): PB_Vector3.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Vector3, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Vector3
  static deserializeBinaryFromReader(message: PB_Vector3, reader: jspb.BinaryReader): PB_Vector3
}

export namespace PB_Vector3 {
  export type AsObject = {
    x: number
    y: number
    z: number
  }
}

export class PB_Quaternion extends jspb.Message {
  getX(): number
  setX(value: number): void

  getY(): number
  setY(value: number): void

  getZ(): number
  setZ(value: number): void

  getW(): number
  setW(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Quaternion.AsObject
  static toObject(includeInstance: boolean, msg: PB_Quaternion): PB_Quaternion.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Quaternion, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Quaternion
  static deserializeBinaryFromReader(message: PB_Quaternion, reader: jspb.BinaryReader): PB_Quaternion
}

export namespace PB_Quaternion {
  export type AsObject = {
    x: number
    y: number
    z: number
    w: number
  }
}

export class PB_Transform extends jspb.Message {
  hasPosition(): boolean
  clearPosition(): void
  getPosition(): PB_Vector3 | undefined
  setPosition(value?: PB_Vector3): void

  hasRotation(): boolean
  clearRotation(): void
  getRotation(): PB_Quaternion | undefined
  setRotation(value?: PB_Quaternion): void

  hasScale(): boolean
  clearScale(): void
  getScale(): PB_Vector3 | undefined
  setScale(value?: PB_Vector3): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Transform.AsObject
  static toObject(includeInstance: boolean, msg: PB_Transform): PB_Transform.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Transform, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Transform
  static deserializeBinaryFromReader(message: PB_Transform, reader: jspb.BinaryReader): PB_Transform
}

export namespace PB_Transform {
  export type AsObject = {
    position?: PB_Vector3.AsObject
    rotation?: PB_Quaternion.AsObject
    scale?: PB_Vector3.AsObject
  }
}

export class PB_UpdateEntityComponent extends jspb.Message {
  getEntityid(): string
  setEntityid(value: string): void

  getClassid(): number
  setClassid(value: number): void

  getName(): string
  setName(value: string): void

  getData(): string
  setData(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UpdateEntityComponent.AsObject
  static toObject(includeInstance: boolean, msg: PB_UpdateEntityComponent): PB_UpdateEntityComponent.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UpdateEntityComponent, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UpdateEntityComponent
  static deserializeBinaryFromReader(
    message: PB_UpdateEntityComponent,
    reader: jspb.BinaryReader
  ): PB_UpdateEntityComponent
}

export namespace PB_UpdateEntityComponent {
  export type AsObject = {
    entityid: string
    classid: number
    name: string
    data: string
  }
}

export class PB_ComponentCreated extends jspb.Message {
  getId(): string
  setId(value: string): void

  getClassid(): number
  setClassid(value: number): void

  getName(): string
  setName(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_ComponentCreated.AsObject
  static toObject(includeInstance: boolean, msg: PB_ComponentCreated): PB_ComponentCreated.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_ComponentCreated, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_ComponentCreated
  static deserializeBinaryFromReader(message: PB_ComponentCreated, reader: jspb.BinaryReader): PB_ComponentCreated
}

export namespace PB_ComponentCreated {
  export type AsObject = {
    id: string
    classid: number
    name: string
  }
}

export class PB_AttachEntityComponent extends jspb.Message {
  getEntityid(): string
  setEntityid(value: string): void

  getName(): string
  setName(value: string): void

  getId(): string
  setId(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_AttachEntityComponent.AsObject
  static toObject(includeInstance: boolean, msg: PB_AttachEntityComponent): PB_AttachEntityComponent.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_AttachEntityComponent, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_AttachEntityComponent
  static deserializeBinaryFromReader(
    message: PB_AttachEntityComponent,
    reader: jspb.BinaryReader
  ): PB_AttachEntityComponent
}

export namespace PB_AttachEntityComponent {
  export type AsObject = {
    entityid: string
    name: string
    id: string
  }
}

export class PB_ComponentDisposed extends jspb.Message {
  getId(): string
  setId(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_ComponentDisposed.AsObject
  static toObject(includeInstance: boolean, msg: PB_ComponentDisposed): PB_ComponentDisposed.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_ComponentDisposed, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_ComponentDisposed
  static deserializeBinaryFromReader(message: PB_ComponentDisposed, reader: jspb.BinaryReader): PB_ComponentDisposed
}

export namespace PB_ComponentDisposed {
  export type AsObject = {
    id: string
  }
}

export class PB_ComponentUpdated extends jspb.Message {
  getId(): string
  setId(value: string): void

  getJson(): string
  setJson(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_ComponentUpdated.AsObject
  static toObject(includeInstance: boolean, msg: PB_ComponentUpdated): PB_ComponentUpdated.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_ComponentUpdated, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_ComponentUpdated
  static deserializeBinaryFromReader(message: PB_ComponentUpdated, reader: jspb.BinaryReader): PB_ComponentUpdated
}

export namespace PB_ComponentUpdated {
  export type AsObject = {
    id: string
    json: string
  }
}

export class PB_Ray extends jspb.Message {
  hasOrigin(): boolean
  clearOrigin(): void
  getOrigin(): PB_Vector3 | undefined
  setOrigin(value?: PB_Vector3): void

  hasDirection(): boolean
  clearDirection(): void
  getDirection(): PB_Vector3 | undefined
  setDirection(value?: PB_Vector3): void

  getDistance(): number
  setDistance(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Ray.AsObject
  static toObject(includeInstance: boolean, msg: PB_Ray): PB_Ray.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Ray, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Ray
  static deserializeBinaryFromReader(message: PB_Ray, reader: jspb.BinaryReader): PB_Ray
}

export namespace PB_Ray {
  export type AsObject = {
    origin?: PB_Vector3.AsObject
    direction?: PB_Vector3.AsObject
    distance: number
  }
}

export class PB_RayQuery extends jspb.Message {
  getQueryid(): string
  setQueryid(value: string): void

  getQuerytype(): string
  setQuerytype(value: string): void

  hasRay(): boolean
  clearRay(): void
  getRay(): PB_Ray | undefined
  setRay(value?: PB_Ray): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_RayQuery.AsObject
  static toObject(includeInstance: boolean, msg: PB_RayQuery): PB_RayQuery.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_RayQuery, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_RayQuery
  static deserializeBinaryFromReader(message: PB_RayQuery, reader: jspb.BinaryReader): PB_RayQuery
}

export namespace PB_RayQuery {
  export type AsObject = {
    queryid: string
    querytype: string
    ray?: PB_Ray.AsObject
  }
}

export class PB_Query extends jspb.Message {
  getQueryid(): string
  setQueryid(value: string): void

  getPayload(): string
  setPayload(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Query.AsObject
  static toObject(includeInstance: boolean, msg: PB_Query): PB_Query.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Query, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Query
  static deserializeBinaryFromReader(message: PB_Query, reader: jspb.BinaryReader): PB_Query
}

export namespace PB_Query {
  export type AsObject = {
    queryid: string
    payload: string
  }
}

export class PB_SendSceneMessage extends jspb.Message {
  getSceneid(): string
  setSceneid(value: string): void

  getTag(): string
  setTag(value: string): void

  hasCreateentity(): boolean
  clearCreateentity(): void
  getCreateentity(): PB_CreateEntity | undefined
  setCreateentity(value?: PB_CreateEntity): void

  hasRemoveentity(): boolean
  clearRemoveentity(): void
  getRemoveentity(): PB_RemoveEntity | undefined
  setRemoveentity(value?: PB_RemoveEntity): void

  hasSetentityparent(): boolean
  clearSetentityparent(): void
  getSetentityparent(): PB_SetEntityParent | undefined
  setSetentityparent(value?: PB_SetEntityParent): void

  hasUpdateentitycomponent(): boolean
  clearUpdateentitycomponent(): void
  getUpdateentitycomponent(): PB_UpdateEntityComponent | undefined
  setUpdateentitycomponent(value?: PB_UpdateEntityComponent): void

  hasAttachentitycomponent(): boolean
  clearAttachentitycomponent(): void
  getAttachentitycomponent(): PB_AttachEntityComponent | undefined
  setAttachentitycomponent(value?: PB_AttachEntityComponent): void

  hasComponentcreated(): boolean
  clearComponentcreated(): void
  getComponentcreated(): PB_ComponentCreated | undefined
  setComponentcreated(value?: PB_ComponentCreated): void

  hasComponentdisposed(): boolean
  clearComponentdisposed(): void
  getComponentdisposed(): PB_ComponentDisposed | undefined
  setComponentdisposed(value?: PB_ComponentDisposed): void

  hasComponentremoved(): boolean
  clearComponentremoved(): void
  getComponentremoved(): PB_ComponentRemoved | undefined
  setComponentremoved(value?: PB_ComponentRemoved): void

  hasComponentupdated(): boolean
  clearComponentupdated(): void
  getComponentupdated(): PB_ComponentUpdated | undefined
  setComponentupdated(value?: PB_ComponentUpdated): void

  hasQuery(): boolean
  clearQuery(): void
  getQuery(): PB_Query | undefined
  setQuery(value?: PB_Query): void

  hasScenestarted(): boolean
  clearScenestarted(): void
  getScenestarted(): google_protobuf_empty_pb.Empty | undefined
  setScenestarted(value?: google_protobuf_empty_pb.Empty): void

  hasOpenexternalurl(): boolean
  clearOpenexternalurl(): void
  getOpenexternalurl(): PB_OpenExternalUrl | undefined
  setOpenexternalurl(value?: PB_OpenExternalUrl): void

  hasOpennftdialog(): boolean
  clearOpennftdialog(): void
  getOpennftdialog(): PB_OpenNFTDialog | undefined
  setOpennftdialog(value?: PB_OpenNFTDialog): void

  getPayloadCase(): PB_SendSceneMessage.PayloadCase
  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_SendSceneMessage.AsObject
  static toObject(includeInstance: boolean, msg: PB_SendSceneMessage): PB_SendSceneMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_SendSceneMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_SendSceneMessage
  static deserializeBinaryFromReader(message: PB_SendSceneMessage, reader: jspb.BinaryReader): PB_SendSceneMessage
}

export namespace PB_SendSceneMessage {
  export type AsObject = {
    sceneid: string
    tag: string
    createentity?: PB_CreateEntity.AsObject
    removeentity?: PB_RemoveEntity.AsObject
    setentityparent?: PB_SetEntityParent.AsObject
    updateentitycomponent?: PB_UpdateEntityComponent.AsObject
    attachentitycomponent?: PB_AttachEntityComponent.AsObject
    componentcreated?: PB_ComponentCreated.AsObject
    componentdisposed?: PB_ComponentDisposed.AsObject
    componentremoved?: PB_ComponentRemoved.AsObject
    componentupdated?: PB_ComponentUpdated.AsObject
    query?: PB_Query.AsObject
    scenestarted?: google_protobuf_empty_pb.Empty.AsObject
    openexternalurl?: PB_OpenExternalUrl.AsObject
    opennftdialog?: PB_OpenNFTDialog.AsObject
  }

  export enum PayloadCase {
    PAYLOAD_NOT_SET = 0,
    CREATEENTITY = 3,
    REMOVEENTITY = 4,
    SETENTITYPARENT = 5,
    UPDATEENTITYCOMPONENT = 6,
    ATTACHENTITYCOMPONENT = 7,
    COMPONENTCREATED = 8,
    COMPONENTDISPOSED = 9,
    COMPONENTREMOVED = 10,
    COMPONENTUPDATED = 11,
    QUERY = 12,
    SCENESTARTED = 13,
    OPENEXTERNALURL = 14,
    OPENNFTDIALOG = 15
  }
}

export class PB_SetPosition extends jspb.Message {
  getX(): number
  setX(value: number): void

  getY(): number
  setY(value: number): void

  getZ(): number
  setZ(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_SetPosition.AsObject
  static toObject(includeInstance: boolean, msg: PB_SetPosition): PB_SetPosition.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_SetPosition, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_SetPosition
  static deserializeBinaryFromReader(message: PB_SetPosition, reader: jspb.BinaryReader): PB_SetPosition
}

export namespace PB_SetPosition {
  export type AsObject = {
    x: number
    y: number
    z: number
  }
}

export class PB_ContentMapping extends jspb.Message {
  getFile(): string
  setFile(value: string): void

  getHash(): string
  setHash(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_ContentMapping.AsObject
  static toObject(includeInstance: boolean, msg: PB_ContentMapping): PB_ContentMapping.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_ContentMapping, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_ContentMapping
  static deserializeBinaryFromReader(message: PB_ContentMapping, reader: jspb.BinaryReader): PB_ContentMapping
}

export namespace PB_ContentMapping {
  export type AsObject = {
    file: string
    hash: string
  }
}

export class PB_Position extends jspb.Message {
  getX(): number
  setX(value: number): void

  getY(): number
  setY(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Position.AsObject
  static toObject(includeInstance: boolean, msg: PB_Position): PB_Position.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Position, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Position
  static deserializeBinaryFromReader(message: PB_Position, reader: jspb.BinaryReader): PB_Position
}

export namespace PB_Position {
  export type AsObject = {
    x: number
    y: number
  }
}

export class PB_LoadParcelScenes extends jspb.Message {
  getId(): string
  setId(value: string): void

  hasBaseposition(): boolean
  clearBaseposition(): void
  getBaseposition(): PB_Position | undefined
  setBaseposition(value?: PB_Position): void

  clearParcelsList(): void
  getParcelsList(): Array<PB_Position>
  setParcelsList(value: Array<PB_Position>): void
  addParcels(value?: PB_Position, index?: number): PB_Position

  clearContentsList(): void
  getContentsList(): Array<PB_ContentMapping>
  setContentsList(value: Array<PB_ContentMapping>): void
  addContents(value?: PB_ContentMapping, index?: number): PB_ContentMapping

  getBaseurl(): string
  setBaseurl(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_LoadParcelScenes.AsObject
  static toObject(includeInstance: boolean, msg: PB_LoadParcelScenes): PB_LoadParcelScenes.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_LoadParcelScenes, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_LoadParcelScenes
  static deserializeBinaryFromReader(message: PB_LoadParcelScenes, reader: jspb.BinaryReader): PB_LoadParcelScenes
}

export namespace PB_LoadParcelScenes {
  export type AsObject = {
    id: string
    baseposition?: PB_Position.AsObject
    parcelsList: Array<PB_Position.AsObject>
    contentsList: Array<PB_ContentMapping.AsObject>
    baseurl: string
  }
}

export class PB_CreateUIScene extends jspb.Message {
  getId(): string
  setId(value: string): void

  getBaseurl(): string
  setBaseurl(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_CreateUIScene.AsObject
  static toObject(includeInstance: boolean, msg: PB_CreateUIScene): PB_CreateUIScene.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_CreateUIScene, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_CreateUIScene
  static deserializeBinaryFromReader(message: PB_CreateUIScene, reader: jspb.BinaryReader): PB_CreateUIScene
}

export namespace PB_CreateUIScene {
  export type AsObject = {
    id: string
    baseurl: string
  }
}

export class PB_UnloadScene extends jspb.Message {
  getSceneid(): string
  setSceneid(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UnloadScene.AsObject
  static toObject(includeInstance: boolean, msg: PB_UnloadScene): PB_UnloadScene.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UnloadScene, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UnloadScene
  static deserializeBinaryFromReader(message: PB_UnloadScene, reader: jspb.BinaryReader): PB_UnloadScene
}

export namespace PB_UnloadScene {
  export type AsObject = {
    sceneid: string
  }
}

export class PB_DclMessage extends jspb.Message {
  hasSetdebug(): boolean
  clearSetdebug(): void
  getSetdebug(): google_protobuf_empty_pb.Empty | undefined
  setSetdebug(value?: google_protobuf_empty_pb.Empty): void

  hasSetscenedebugpanel(): boolean
  clearSetscenedebugpanel(): void
  getSetscenedebugpanel(): google_protobuf_empty_pb.Empty | undefined
  setSetscenedebugpanel(value?: google_protobuf_empty_pb.Empty): void

  hasSetenginedebugpanel(): boolean
  clearSetenginedebugpanel(): void
  getSetenginedebugpanel(): google_protobuf_empty_pb.Empty | undefined
  setSetenginedebugpanel(value?: google_protobuf_empty_pb.Empty): void

  hasSendscenemessage(): boolean
  clearSendscenemessage(): void
  getSendscenemessage(): PB_SendSceneMessage | undefined
  setSendscenemessage(value?: PB_SendSceneMessage): void

  hasLoadparcelscenes(): boolean
  clearLoadparcelscenes(): void
  getLoadparcelscenes(): PB_LoadParcelScenes | undefined
  setLoadparcelscenes(value?: PB_LoadParcelScenes): void

  hasUnloadscene(): boolean
  clearUnloadscene(): void
  getUnloadscene(): PB_UnloadScene | undefined
  setUnloadscene(value?: PB_UnloadScene): void

  hasSetposition(): boolean
  clearSetposition(): void
  getSetposition(): PB_SetPosition | undefined
  setSetposition(value?: PB_SetPosition): void

  hasReset(): boolean
  clearReset(): void
  getReset(): google_protobuf_empty_pb.Empty | undefined
  setReset(value?: google_protobuf_empty_pb.Empty): void

  hasCreateuiscene(): boolean
  clearCreateuiscene(): void
  getCreateuiscene(): PB_CreateUIScene | undefined
  setCreateuiscene(value?: PB_CreateUIScene): void

  getMessageCase(): PB_DclMessage.MessageCase
  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_DclMessage.AsObject
  static toObject(includeInstance: boolean, msg: PB_DclMessage): PB_DclMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_DclMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_DclMessage
  static deserializeBinaryFromReader(message: PB_DclMessage, reader: jspb.BinaryReader): PB_DclMessage
}

export namespace PB_DclMessage {
  export type AsObject = {
    setdebug?: google_protobuf_empty_pb.Empty.AsObject
    setscenedebugpanel?: google_protobuf_empty_pb.Empty.AsObject
    setenginedebugpanel?: google_protobuf_empty_pb.Empty.AsObject
    sendscenemessage?: PB_SendSceneMessage.AsObject
    loadparcelscenes?: PB_LoadParcelScenes.AsObject
    unloadscene?: PB_UnloadScene.AsObject
    setposition?: PB_SetPosition.AsObject
    reset?: google_protobuf_empty_pb.Empty.AsObject
    createuiscene?: PB_CreateUIScene.AsObject
  }

  export enum MessageCase {
    MESSAGE_NOT_SET = 0,
    SETDEBUG = 1,
    SETSCENEDEBUGPANEL = 2,
    SETENGINEDEBUGPANEL = 3,
    SENDSCENEMESSAGE = 4,
    LOADPARCELSCENES = 5,
    UNLOADSCENE = 6,
    SETPOSITION = 7,
    RESET = 8,
    CREATEUISCENE = 9
  }
}

export class PB_AnimationState extends jspb.Message {
  getClip(): string
  setClip(value: string): void

  getLooping(): boolean
  setLooping(value: boolean): void

  getWeight(): number
  setWeight(value: number): void

  getPlaying(): boolean
  setPlaying(value: boolean): void

  getShouldreset(): boolean
  setShouldreset(value: boolean): void

  getSpeed(): number
  setSpeed(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_AnimationState.AsObject
  static toObject(includeInstance: boolean, msg: PB_AnimationState): PB_AnimationState.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_AnimationState, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_AnimationState
  static deserializeBinaryFromReader(message: PB_AnimationState, reader: jspb.BinaryReader): PB_AnimationState
}

export namespace PB_AnimationState {
  export type AsObject = {
    clip: string
    looping: boolean
    weight: number
    playing: boolean
    shouldreset: boolean
    speed: number
  }
}

export class PB_Animator extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Animator.AsObject
  static toObject(includeInstance: boolean, msg: PB_Animator): PB_Animator.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Animator, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Animator
  static deserializeBinaryFromReader(message: PB_Animator, reader: jspb.BinaryReader): PB_Animator
}

export namespace PB_Animator {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
  }
}

export class PB_AudioClip extends jspb.Message {
  getUrl(): string
  setUrl(value: string): void

  getLoop(): boolean
  setLoop(value: boolean): void

  getVolume(): number
  setVolume(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_AudioClip.AsObject
  static toObject(includeInstance: boolean, msg: PB_AudioClip): PB_AudioClip.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_AudioClip, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_AudioClip
  static deserializeBinaryFromReader(message: PB_AudioClip, reader: jspb.BinaryReader): PB_AudioClip
}

export namespace PB_AudioClip {
  export type AsObject = {
    url: string
    loop: boolean
    volume: number
  }
}

export class PB_AudioSource extends jspb.Message {
  hasAudioclip(): boolean
  clearAudioclip(): void
  getAudioclip(): PB_AudioClip | undefined
  setAudioclip(value?: PB_AudioClip): void

  getAudioclipid(): string
  setAudioclipid(value: string): void

  getLoop(): boolean
  setLoop(value: boolean): void

  getVolume(): number
  setVolume(value: number): void

  getPlaying(): boolean
  setPlaying(value: boolean): void

  getPitch(): number
  setPitch(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_AudioSource.AsObject
  static toObject(includeInstance: boolean, msg: PB_AudioSource): PB_AudioSource.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_AudioSource, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_AudioSource
  static deserializeBinaryFromReader(message: PB_AudioSource, reader: jspb.BinaryReader): PB_AudioSource
}

export namespace PB_AudioSource {
  export type AsObject = {
    audioclip?: PB_AudioClip.AsObject
    audioclipid: string
    loop: boolean
    volume: number
    playing: boolean
    pitch: number
  }
}

export class PB_AvatarShape extends jspb.Message {
  getId(): string
  setId(value: string): void

  getBaseurl(): string
  setBaseurl(value: string): void

  getName(): string
  setName(value: string): void

  hasBodyshape(): boolean
  clearBodyshape(): void
  getBodyshape(): PB_Wearable | undefined
  setBodyshape(value?: PB_Wearable): void

  clearWearablesList(): void
  getWearablesList(): Array<PB_Wearable>
  setWearablesList(value: Array<PB_Wearable>): void
  addWearables(value?: PB_Wearable, index?: number): PB_Wearable

  hasSkin(): boolean
  clearSkin(): void
  getSkin(): PB_Skin | undefined
  setSkin(value?: PB_Skin): void

  hasHair(): boolean
  clearHair(): void
  getHair(): PB_Hair | undefined
  setHair(value?: PB_Hair): void

  hasEyes(): boolean
  clearEyes(): void
  getEyes(): PB_Eyes | undefined
  setEyes(value?: PB_Eyes): void

  hasEyebrows(): boolean
  clearEyebrows(): void
  getEyebrows(): PB_Face | undefined
  setEyebrows(value?: PB_Face): void

  hasMouth(): boolean
  clearMouth(): void
  getMouth(): PB_Face | undefined
  setMouth(value?: PB_Face): void

  getUsedummymodel(): boolean
  setUsedummymodel(value: boolean): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_AvatarShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_AvatarShape): PB_AvatarShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_AvatarShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_AvatarShape
  static deserializeBinaryFromReader(message: PB_AvatarShape, reader: jspb.BinaryReader): PB_AvatarShape
}

export namespace PB_AvatarShape {
  export type AsObject = {
    id: string
    baseurl: string
    name: string
    bodyshape?: PB_Wearable.AsObject
    wearablesList: Array<PB_Wearable.AsObject>
    skin?: PB_Skin.AsObject
    hair?: PB_Hair.AsObject
    eyes?: PB_Eyes.AsObject
    eyebrows?: PB_Face.AsObject
    mouth?: PB_Face.AsObject
    usedummymodel: boolean
  }
}

export class PB_Wearable extends jspb.Message {
  getCategody(): string
  setCategody(value: string): void

  getContentname(): string
  setContentname(value: string): void

  clearContentsList(): void
  getContentsList(): Array<PB_ContentMapping>
  setContentsList(value: Array<PB_ContentMapping>): void
  addContents(value?: PB_ContentMapping, index?: number): PB_ContentMapping

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Wearable.AsObject
  static toObject(includeInstance: boolean, msg: PB_Wearable): PB_Wearable.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Wearable, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Wearable
  static deserializeBinaryFromReader(message: PB_Wearable, reader: jspb.BinaryReader): PB_Wearable
}

export namespace PB_Wearable {
  export type AsObject = {
    categody: string
    contentname: string
    contentsList: Array<PB_ContentMapping.AsObject>
  }
}

export class PB_Face extends jspb.Message {
  getTexture(): string
  setTexture(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Face.AsObject
  static toObject(includeInstance: boolean, msg: PB_Face): PB_Face.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Face, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Face
  static deserializeBinaryFromReader(message: PB_Face, reader: jspb.BinaryReader): PB_Face
}

export namespace PB_Face {
  export type AsObject = {
    texture: string
  }
}

export class PB_Eyes extends jspb.Message {
  getTexture(): string
  setTexture(value: string): void

  getMask(): string
  setMask(value: string): void

  hasColor(): boolean
  clearColor(): void
  getColor(): PB_Color4 | undefined
  setColor(value?: PB_Color4): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Eyes.AsObject
  static toObject(includeInstance: boolean, msg: PB_Eyes): PB_Eyes.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Eyes, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Eyes
  static deserializeBinaryFromReader(message: PB_Eyes, reader: jspb.BinaryReader): PB_Eyes
}

export namespace PB_Eyes {
  export type AsObject = {
    texture: string
    mask: string
    color?: PB_Color4.AsObject
  }
}

export class PB_Hair extends jspb.Message {
  hasColor(): boolean
  clearColor(): void
  getColor(): PB_Color4 | undefined
  setColor(value?: PB_Color4): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Hair.AsObject
  static toObject(includeInstance: boolean, msg: PB_Hair): PB_Hair.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Hair, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Hair
  static deserializeBinaryFromReader(message: PB_Hair, reader: jspb.BinaryReader): PB_Hair
}

export namespace PB_Hair {
  export type AsObject = {
    color?: PB_Color4.AsObject
  }
}

export class PB_Skin extends jspb.Message {
  hasColor(): boolean
  clearColor(): void
  getColor(): PB_Color4 | undefined
  setColor(value?: PB_Color4): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Skin.AsObject
  static toObject(includeInstance: boolean, msg: PB_Skin): PB_Skin.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Skin, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Skin
  static deserializeBinaryFromReader(message: PB_Skin, reader: jspb.BinaryReader): PB_Skin
}

export namespace PB_Skin {
  export type AsObject = {
    color?: PB_Color4.AsObject
  }
}

export class PB_BasicMaterial extends jspb.Message {
  hasTexture(): boolean
  clearTexture(): void
  getTexture(): PB_Texture | undefined
  setTexture(value?: PB_Texture): void

  getAlphatest(): number
  setAlphatest(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_BasicMaterial.AsObject
  static toObject(includeInstance: boolean, msg: PB_BasicMaterial): PB_BasicMaterial.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_BasicMaterial, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_BasicMaterial
  static deserializeBinaryFromReader(message: PB_BasicMaterial, reader: jspb.BinaryReader): PB_BasicMaterial
}

export namespace PB_BasicMaterial {
  export type AsObject = {
    texture?: PB_Texture.AsObject
    alphatest: number
  }
}

export class PB_Billboard extends jspb.Message {
  getX(): boolean
  setX(value: boolean): void

  getY(): boolean
  setY(value: boolean): void

  getZ(): boolean
  setZ(value: boolean): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Billboard.AsObject
  static toObject(includeInstance: boolean, msg: PB_Billboard): PB_Billboard.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Billboard, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Billboard
  static deserializeBinaryFromReader(message: PB_Billboard, reader: jspb.BinaryReader): PB_Billboard
}

export namespace PB_Billboard {
  export type AsObject = {
    x: boolean
    y: boolean
    z: boolean
  }
}

export class PB_BoxShape extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_BoxShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_BoxShape): PB_BoxShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_BoxShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_BoxShape
  static deserializeBinaryFromReader(message: PB_BoxShape, reader: jspb.BinaryReader): PB_BoxShape
}

export namespace PB_BoxShape {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
  }
}

export class PB_CircleShape extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getSegments(): number
  setSegments(value: number): void

  getArc(): number
  setArc(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_CircleShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_CircleShape): PB_CircleShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_CircleShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_CircleShape
  static deserializeBinaryFromReader(message: PB_CircleShape, reader: jspb.BinaryReader): PB_CircleShape
}

export namespace PB_CircleShape {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
    segments: number
    arc: number
  }
}

export class PB_ConeShape extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getRadiustop(): number
  setRadiustop(value: number): void

  getRadiusbottom(): number
  setRadiusbottom(value: number): void

  getSegmentsheight(): number
  setSegmentsheight(value: number): void

  getSegmentsradial(): number
  setSegmentsradial(value: number): void

  getOpenended(): boolean
  setOpenended(value: boolean): void

  getRadius(): number
  setRadius(value: number): void

  getArc(): number
  setArc(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_ConeShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_ConeShape): PB_ConeShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_ConeShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_ConeShape
  static deserializeBinaryFromReader(message: PB_ConeShape, reader: jspb.BinaryReader): PB_ConeShape
}

export namespace PB_ConeShape {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
    radiustop: number
    radiusbottom: number
    segmentsheight: number
    segmentsradial: number
    openended: boolean
    radius: number
    arc: number
  }
}

export class PB_CylinderShape extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getRadiustop(): number
  setRadiustop(value: number): void

  getRadiusbottom(): number
  setRadiusbottom(value: number): void

  getSegmentsheight(): number
  setSegmentsheight(value: number): void

  getSegmentsradial(): number
  setSegmentsradial(value: number): void

  getOpenended(): boolean
  setOpenended(value: boolean): void

  getRadius(): number
  setRadius(value: number): void

  getArc(): number
  setArc(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_CylinderShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_CylinderShape): PB_CylinderShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_CylinderShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_CylinderShape
  static deserializeBinaryFromReader(message: PB_CylinderShape, reader: jspb.BinaryReader): PB_CylinderShape
}

export namespace PB_CylinderShape {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
    radiustop: number
    radiusbottom: number
    segmentsheight: number
    segmentsradial: number
    openended: boolean
    radius: number
    arc: number
  }
}

export class PB_GlobalPointerDown extends jspb.Message {
  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_GlobalPointerDown.AsObject
  static toObject(includeInstance: boolean, msg: PB_GlobalPointerDown): PB_GlobalPointerDown.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_GlobalPointerDown, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_GlobalPointerDown
  static deserializeBinaryFromReader(message: PB_GlobalPointerDown, reader: jspb.BinaryReader): PB_GlobalPointerDown
}

export namespace PB_GlobalPointerDown {
  export type AsObject = {}
}

export class PB_GlobalPointerUp extends jspb.Message {
  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_GlobalPointerUp.AsObject
  static toObject(includeInstance: boolean, msg: PB_GlobalPointerUp): PB_GlobalPointerUp.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_GlobalPointerUp, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_GlobalPointerUp
  static deserializeBinaryFromReader(message: PB_GlobalPointerUp, reader: jspb.BinaryReader): PB_GlobalPointerUp
}

export namespace PB_GlobalPointerUp {
  export type AsObject = {}
}

export class PB_GLTFShape extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getSrc(): string
  setSrc(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_GLTFShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_GLTFShape): PB_GLTFShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_GLTFShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_GLTFShape
  static deserializeBinaryFromReader(message: PB_GLTFShape, reader: jspb.BinaryReader): PB_GLTFShape
}

export namespace PB_GLTFShape {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
    src: string
  }
}

export class PB_Material extends jspb.Message {
  getAlpha(): number
  setAlpha(value: number): void

  hasAlbedocolor(): boolean
  clearAlbedocolor(): void
  getAlbedocolor(): PB_Color3 | undefined
  setAlbedocolor(value?: PB_Color3): void

  hasEmissivecolor(): boolean
  clearEmissivecolor(): void
  getEmissivecolor(): PB_Color3 | undefined
  setEmissivecolor(value?: PB_Color3): void

  getMetallic(): number
  setMetallic(value: number): void

  getRoughness(): number
  setRoughness(value: number): void

  hasAmbientcolor(): boolean
  clearAmbientcolor(): void
  getAmbientcolor(): PB_Color3 | undefined
  setAmbientcolor(value?: PB_Color3): void

  hasReflectioncolor(): boolean
  clearReflectioncolor(): void
  getReflectioncolor(): PB_Color3 | undefined
  setReflectioncolor(value?: PB_Color3): void

  hasReflectivitycolor(): boolean
  clearReflectivitycolor(): void
  getReflectivitycolor(): PB_Color3 | undefined
  setReflectivitycolor(value?: PB_Color3): void

  getDirectintensity(): number
  setDirectintensity(value: number): void

  getMicrosurface(): number
  setMicrosurface(value: number): void

  getEmissiveintensity(): number
  setEmissiveintensity(value: number): void

  getEnvironmentintensity(): number
  setEnvironmentintensity(value: number): void

  getSpecularintensity(): number
  setSpecularintensity(value: number): void

  hasAlbedotexture(): boolean
  clearAlbedotexture(): void
  getAlbedotexture(): PB_Texture | undefined
  setAlbedotexture(value?: PB_Texture): void

  hasAlphatexture(): boolean
  clearAlphatexture(): void
  getAlphatexture(): PB_Texture | undefined
  setAlphatexture(value?: PB_Texture): void

  hasEmissivetexture(): boolean
  clearEmissivetexture(): void
  getEmissivetexture(): PB_Texture | undefined
  setEmissivetexture(value?: PB_Texture): void

  hasBumptexture(): boolean
  clearBumptexture(): void
  getBumptexture(): PB_Texture | undefined
  setBumptexture(value?: PB_Texture): void

  hasRefractiontexture(): boolean
  clearRefractiontexture(): void
  getRefractiontexture(): PB_Texture | undefined
  setRefractiontexture(value?: PB_Texture): void

  getDisablelighting(): boolean
  setDisablelighting(value: boolean): void

  getTransparencymode(): number
  setTransparencymode(value: number): void

  getHasalpha(): boolean
  setHasalpha(value: boolean): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Material.AsObject
  static toObject(includeInstance: boolean, msg: PB_Material): PB_Material.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Material, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Material
  static deserializeBinaryFromReader(message: PB_Material, reader: jspb.BinaryReader): PB_Material
}

export namespace PB_Material {
  export type AsObject = {
    alpha: number
    albedocolor?: PB_Color3.AsObject
    emissivecolor?: PB_Color3.AsObject
    metallic: number
    roughness: number
    ambientcolor?: PB_Color3.AsObject
    reflectioncolor?: PB_Color3.AsObject
    reflectivitycolor?: PB_Color3.AsObject
    directintensity: number
    microsurface: number
    emissiveintensity: number
    environmentintensity: number
    specularintensity: number
    albedotexture?: PB_Texture.AsObject
    alphatexture?: PB_Texture.AsObject
    emissivetexture?: PB_Texture.AsObject
    bumptexture?: PB_Texture.AsObject
    refractiontexture?: PB_Texture.AsObject
    disablelighting: boolean
    transparencymode: number
    hasalpha: boolean
  }
}

export class PB_NFTShape extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getSrc(): string
  setSrc(value: string): void

  hasColor(): boolean
  clearColor(): void
  getColor(): PB_Color3 | undefined
  setColor(value?: PB_Color3): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_NFTShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_NFTShape): PB_NFTShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_NFTShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_NFTShape
  static deserializeBinaryFromReader(message: PB_NFTShape, reader: jspb.BinaryReader): PB_NFTShape
}

export namespace PB_NFTShape {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
    src: string
    color?: PB_Color3.AsObject
  }
}

export class PB_OBJShape extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getSrc(): string
  setSrc(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_OBJShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_OBJShape): PB_OBJShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_OBJShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_OBJShape
  static deserializeBinaryFromReader(message: PB_OBJShape, reader: jspb.BinaryReader): PB_OBJShape
}

export namespace PB_OBJShape {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
    src: string
  }
}

export class PB_PlaneShape extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getWidth(): number
  setWidth(value: number): void

  getHeight(): number
  setHeight(value: number): void

  clearUvsList(): void
  getUvsList(): Array<number>
  setUvsList(value: Array<number>): void
  addUvs(value: number, index?: number): number

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_PlaneShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_PlaneShape): PB_PlaneShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_PlaneShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_PlaneShape
  static deserializeBinaryFromReader(message: PB_PlaneShape, reader: jspb.BinaryReader): PB_PlaneShape
}

export namespace PB_PlaneShape {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
    width: number
    height: number
    uvsList: Array<number>
  }
}

export class PB_Shape extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Shape.AsObject
  static toObject(includeInstance: boolean, msg: PB_Shape): PB_Shape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Shape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Shape
  static deserializeBinaryFromReader(message: PB_Shape, reader: jspb.BinaryReader): PB_Shape
}

export namespace PB_Shape {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
  }
}

export class PB_SphereShape extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_SphereShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_SphereShape): PB_SphereShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_SphereShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_SphereShape
  static deserializeBinaryFromReader(message: PB_SphereShape, reader: jspb.BinaryReader): PB_SphereShape
}

export namespace PB_SphereShape {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
  }
}

export class PB_TextShape extends jspb.Message {
  getWithcollisions(): boolean
  setWithcollisions(value: boolean): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getOutlinewidth(): number
  setOutlinewidth(value: number): void

  hasOutlinecolor(): boolean
  clearOutlinecolor(): void
  getOutlinecolor(): PB_Color3 | undefined
  setOutlinecolor(value?: PB_Color3): void

  hasColor(): boolean
  clearColor(): void
  getColor(): PB_Color3 | undefined
  setColor(value?: PB_Color3): void

  getFontsize(): number
  setFontsize(value: number): void

  getFontweight(): string
  setFontweight(value: string): void

  getOpacity(): number
  setOpacity(value: number): void

  getValue(): string
  setValue(value: string): void

  getLinespacing(): string
  setLinespacing(value: string): void

  getLinecount(): number
  setLinecount(value: number): void

  getResizetofit(): boolean
  setResizetofit(value: boolean): void

  getTextwrapping(): boolean
  setTextwrapping(value: boolean): void

  getShadowblur(): number
  setShadowblur(value: number): void

  getShadowoffsetx(): number
  setShadowoffsetx(value: number): void

  getShadowoffsety(): number
  setShadowoffsety(value: number): void

  hasShadowcolor(): boolean
  clearShadowcolor(): void
  getShadowcolor(): PB_Color3 | undefined
  setShadowcolor(value?: PB_Color3): void

  getZindex(): number
  setZindex(value: number): void

  getHtextalign(): string
  setHtextalign(value: string): void

  getVtextalign(): string
  setVtextalign(value: string): void

  getWidth(): number
  setWidth(value: number): void

  getHeight(): number
  setHeight(value: number): void

  getPaddingtop(): number
  setPaddingtop(value: number): void

  getPaddingright(): number
  setPaddingright(value: number): void

  getPaddingbottom(): number
  setPaddingbottom(value: number): void

  getPaddingleft(): number
  setPaddingleft(value: number): void

  getIspickable(): boolean
  setIspickable(value: boolean): void

  getBillboard(): boolean
  setBillboard(value: boolean): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_TextShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_TextShape): PB_TextShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_TextShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_TextShape
  static deserializeBinaryFromReader(message: PB_TextShape, reader: jspb.BinaryReader): PB_TextShape
}

export namespace PB_TextShape {
  export type AsObject = {
    withcollisions: boolean
    visible: boolean
    outlinewidth: number
    outlinecolor?: PB_Color3.AsObject
    color?: PB_Color3.AsObject
    fontsize: number
    fontweight: string
    opacity: number
    value: string
    linespacing: string
    linecount: number
    resizetofit: boolean
    textwrapping: boolean
    shadowblur: number
    shadowoffsetx: number
    shadowoffsety: number
    shadowcolor?: PB_Color3.AsObject
    zindex: number
    htextalign: string
    vtextalign: string
    width: number
    height: number
    paddingtop: number
    paddingright: number
    paddingbottom: number
    paddingleft: number
    ispickable: boolean
    billboard: boolean
  }
}

export class PB_Texture extends jspb.Message {
  getSrc(): string
  setSrc(value: string): void

  getSamplingmode(): number
  setSamplingmode(value: number): void

  getWrap(): number
  setWrap(value: number): void

  getHasalpha(): boolean
  setHasalpha(value: boolean): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_Texture.AsObject
  static toObject(includeInstance: boolean, msg: PB_Texture): PB_Texture.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_Texture, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_Texture
  static deserializeBinaryFromReader(message: PB_Texture, reader: jspb.BinaryReader): PB_Texture
}

export namespace PB_Texture {
  export type AsObject = {
    src: string
    samplingmode: number
    wrap: number
    hasalpha: boolean
  }
}

export class PB_UIButton extends jspb.Message {
  getName(): string
  setName(value: string): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getOpacity(): number
  setOpacity(value: number): void

  getHalign(): string
  setHalign(value: string): void

  getValign(): string
  setValign(value: string): void

  getWidth(): string
  setWidth(value: string): void

  getHeight(): string
  setHeight(value: string): void

  getPositionx(): string
  setPositionx(value: string): void

  getPositiony(): string
  setPositiony(value: string): void

  getIspointerblocker(): boolean
  setIspointerblocker(value: boolean): void

  hasParent(): boolean
  clearParent(): void
  getParent(): PB_UIShape | undefined
  setParent(value?: PB_UIShape): void

  getFontsize(): number
  setFontsize(value: number): void

  getFontweight(): string
  setFontweight(value: string): void

  getThickness(): number
  setThickness(value: number): void

  getCornerradius(): number
  setCornerradius(value: number): void

  hasColor(): boolean
  clearColor(): void
  getColor(): PB_Color4 | undefined
  setColor(value?: PB_Color4): void

  hasBackground(): boolean
  clearBackground(): void
  getBackground(): PB_Color4 | undefined
  setBackground(value?: PB_Color4): void

  getPaddingtop(): number
  setPaddingtop(value: number): void

  getPaddingright(): number
  setPaddingright(value: number): void

  getPaddingbottom(): number
  setPaddingbottom(value: number): void

  getPaddingleft(): number
  setPaddingleft(value: number): void

  getShadowblur(): number
  setShadowblur(value: number): void

  getShadowoffsetx(): number
  setShadowoffsetx(value: number): void

  getShadowoffsety(): number
  setShadowoffsety(value: number): void

  hasShadowcolor(): boolean
  clearShadowcolor(): void
  getShadowcolor(): PB_Color4 | undefined
  setShadowcolor(value?: PB_Color4): void

  getText(): string
  setText(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UIButton.AsObject
  static toObject(includeInstance: boolean, msg: PB_UIButton): PB_UIButton.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UIButton, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UIButton
  static deserializeBinaryFromReader(message: PB_UIButton, reader: jspb.BinaryReader): PB_UIButton
}

export namespace PB_UIButton {
  export type AsObject = {
    name: string
    visible: boolean
    opacity: number
    halign: string
    valign: string
    width: string
    height: string
    positionx: string
    positiony: string
    ispointerblocker: boolean
    parent?: PB_UIShape.AsObject
    fontsize: number
    fontweight: string
    thickness: number
    cornerradius: number
    color?: PB_Color4.AsObject
    background?: PB_Color4.AsObject
    paddingtop: number
    paddingright: number
    paddingbottom: number
    paddingleft: number
    shadowblur: number
    shadowoffsetx: number
    shadowoffsety: number
    shadowcolor?: PB_Color4.AsObject
    text: string
  }
}

export class PB_UICanvas extends jspb.Message {
  getName(): string
  setName(value: string): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getOpacity(): number
  setOpacity(value: number): void

  getHalign(): string
  setHalign(value: string): void

  getValign(): string
  setValign(value: string): void

  getWidth(): string
  setWidth(value: string): void

  getHeight(): string
  setHeight(value: string): void

  getPositionx(): string
  setPositionx(value: string): void

  getPositiony(): string
  setPositiony(value: string): void

  getIspointerblocker(): boolean
  setIspointerblocker(value: boolean): void

  hasParent(): boolean
  clearParent(): void
  getParent(): PB_UIShape | undefined
  setParent(value?: PB_UIShape): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UICanvas.AsObject
  static toObject(includeInstance: boolean, msg: PB_UICanvas): PB_UICanvas.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UICanvas, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UICanvas
  static deserializeBinaryFromReader(message: PB_UICanvas, reader: jspb.BinaryReader): PB_UICanvas
}

export namespace PB_UICanvas {
  export type AsObject = {
    name: string
    visible: boolean
    opacity: number
    halign: string
    valign: string
    width: string
    height: string
    positionx: string
    positiony: string
    ispointerblocker: boolean
    parent?: PB_UIShape.AsObject
  }
}

export class PB_UIContainerRect extends jspb.Message {
  getName(): string
  setName(value: string): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getOpacity(): number
  setOpacity(value: number): void

  getHalign(): string
  setHalign(value: string): void

  getValign(): string
  setValign(value: string): void

  getWidth(): string
  setWidth(value: string): void

  getHeight(): string
  setHeight(value: string): void

  getPositionx(): string
  setPositionx(value: string): void

  getPositiony(): string
  setPositiony(value: string): void

  getIspointerblocker(): boolean
  setIspointerblocker(value: boolean): void

  hasParent(): boolean
  clearParent(): void
  getParent(): PB_UIShape | undefined
  setParent(value?: PB_UIShape): void

  getAdaptwidth(): boolean
  setAdaptwidth(value: boolean): void

  getAdaptheight(): boolean
  setAdaptheight(value: boolean): void

  getThickness(): number
  setThickness(value: number): void

  hasColor(): boolean
  clearColor(): void
  getColor(): PB_Color4 | undefined
  setColor(value?: PB_Color4): void

  getAlignmentusessize(): boolean
  setAlignmentusessize(value: boolean): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UIContainerRect.AsObject
  static toObject(includeInstance: boolean, msg: PB_UIContainerRect): PB_UIContainerRect.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UIContainerRect, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UIContainerRect
  static deserializeBinaryFromReader(message: PB_UIContainerRect, reader: jspb.BinaryReader): PB_UIContainerRect
}

export namespace PB_UIContainerRect {
  export type AsObject = {
    name: string
    visible: boolean
    opacity: number
    halign: string
    valign: string
    width: string
    height: string
    positionx: string
    positiony: string
    ispointerblocker: boolean
    parent?: PB_UIShape.AsObject
    adaptwidth: boolean
    adaptheight: boolean
    thickness: number
    color?: PB_Color4.AsObject
    alignmentusessize: boolean
  }
}

export class PB_UIContainerStack extends jspb.Message {
  getName(): string
  setName(value: string): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getOpacity(): number
  setOpacity(value: number): void

  getHalign(): string
  setHalign(value: string): void

  getValign(): string
  setValign(value: string): void

  getWidth(): string
  setWidth(value: string): void

  getHeight(): string
  setHeight(value: string): void

  getPositionx(): string
  setPositionx(value: string): void

  getPositiony(): string
  setPositiony(value: string): void

  getIspointerblocker(): boolean
  setIspointerblocker(value: boolean): void

  hasParent(): boolean
  clearParent(): void
  getParent(): PB_UIShape | undefined
  setParent(value?: PB_UIShape): void

  getAdaptwidth(): boolean
  setAdaptwidth(value: boolean): void

  getAdaptheight(): boolean
  setAdaptheight(value: boolean): void

  hasColor(): boolean
  clearColor(): void
  getColor(): PB_Color4 | undefined
  setColor(value?: PB_Color4): void

  getStackorientation(): PB_UIStackOrientationMap[keyof PB_UIStackOrientationMap]
  setStackorientation(value: PB_UIStackOrientationMap[keyof PB_UIStackOrientationMap]): void

  getSpacing(): number
  setSpacing(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UIContainerStack.AsObject
  static toObject(includeInstance: boolean, msg: PB_UIContainerStack): PB_UIContainerStack.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UIContainerStack, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UIContainerStack
  static deserializeBinaryFromReader(message: PB_UIContainerStack, reader: jspb.BinaryReader): PB_UIContainerStack
}

export namespace PB_UIContainerStack {
  export type AsObject = {
    name: string
    visible: boolean
    opacity: number
    halign: string
    valign: string
    width: string
    height: string
    positionx: string
    positiony: string
    ispointerblocker: boolean
    parent?: PB_UIShape.AsObject
    adaptwidth: boolean
    adaptheight: boolean
    color?: PB_Color4.AsObject
    stackorientation: PB_UIStackOrientationMap[keyof PB_UIStackOrientationMap]
    spacing: number
  }
}

export class PB_UIImage extends jspb.Message {
  getName(): string
  setName(value: string): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getOpacity(): number
  setOpacity(value: number): void

  getHalign(): string
  setHalign(value: string): void

  getValign(): string
  setValign(value: string): void

  getWidth(): string
  setWidth(value: string): void

  getHeight(): string
  setHeight(value: string): void

  getPositionx(): string
  setPositionx(value: string): void

  getPositiony(): string
  setPositiony(value: string): void

  getIspointerblocker(): boolean
  setIspointerblocker(value: boolean): void

  hasParent(): boolean
  clearParent(): void
  getParent(): PB_UIShape | undefined
  setParent(value?: PB_UIShape): void

  getSourceleft(): number
  setSourceleft(value: number): void

  getSourcetop(): number
  setSourcetop(value: number): void

  getSourcewidth(): number
  setSourcewidth(value: number): void

  getSourceheight(): number
  setSourceheight(value: number): void

  hasSource(): boolean
  clearSource(): void
  getSource(): PB_Texture | undefined
  setSource(value?: PB_Texture): void

  getPaddingtop(): number
  setPaddingtop(value: number): void

  getPaddingright(): number
  setPaddingright(value: number): void

  getPaddingbottom(): number
  setPaddingbottom(value: number): void

  getPaddingleft(): number
  setPaddingleft(value: number): void

  getSizeinpixels(): boolean
  setSizeinpixels(value: boolean): void

  hasOnclick(): boolean
  clearOnclick(): void
  getOnclick(): PB_UUIDCallback | undefined
  setOnclick(value?: PB_UUIDCallback): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UIImage.AsObject
  static toObject(includeInstance: boolean, msg: PB_UIImage): PB_UIImage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UIImage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UIImage
  static deserializeBinaryFromReader(message: PB_UIImage, reader: jspb.BinaryReader): PB_UIImage
}

export namespace PB_UIImage {
  export type AsObject = {
    name: string
    visible: boolean
    opacity: number
    halign: string
    valign: string
    width: string
    height: string
    positionx: string
    positiony: string
    ispointerblocker: boolean
    parent?: PB_UIShape.AsObject
    sourceleft: number
    sourcetop: number
    sourcewidth: number
    sourceheight: number
    source?: PB_Texture.AsObject
    paddingtop: number
    paddingright: number
    paddingbottom: number
    paddingleft: number
    sizeinpixels: boolean
    onclick?: PB_UUIDCallback.AsObject
  }
}

export class PB_UUIDCallback extends jspb.Message {
  getType(): string
  setType(value: string): void

  getUuid(): string
  setUuid(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UUIDCallback.AsObject
  static toObject(includeInstance: boolean, msg: PB_UUIDCallback): PB_UUIDCallback.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UUIDCallback, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UUIDCallback
  static deserializeBinaryFromReader(message: PB_UUIDCallback, reader: jspb.BinaryReader): PB_UUIDCallback
}

export namespace PB_UUIDCallback {
  export type AsObject = {
    type: string
    uuid: string
  }
}

export class PB_UIInputText extends jspb.Message {
  getName(): string
  setName(value: string): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getOpacity(): number
  setOpacity(value: number): void

  getHalign(): string
  setHalign(value: string): void

  getValign(): string
  setValign(value: string): void

  getWidth(): string
  setWidth(value: string): void

  getHeight(): string
  setHeight(value: string): void

  getPositionx(): string
  setPositionx(value: string): void

  getPositiony(): string
  setPositiony(value: string): void

  getIspointerblocker(): boolean
  setIspointerblocker(value: boolean): void

  hasParent(): boolean
  clearParent(): void
  getParent(): PB_UIShape | undefined
  setParent(value?: PB_UIShape): void

  getOutlinewidth(): number
  setOutlinewidth(value: number): void

  hasOutlinecolor(): boolean
  clearOutlinecolor(): void
  getOutlinecolor(): PB_Color4 | undefined
  setOutlinecolor(value?: PB_Color4): void

  hasColor(): boolean
  clearColor(): void
  getColor(): PB_Color4 | undefined
  setColor(value?: PB_Color4): void

  getThickness(): number
  setThickness(value: number): void

  getFontsize(): number
  setFontsize(value: number): void

  getFontweight(): string
  setFontweight(value: string): void

  getValue(): string
  setValue(value: string): void

  hasPlaceholdercolor(): boolean
  clearPlaceholdercolor(): void
  getPlaceholdercolor(): PB_Color4 | undefined
  setPlaceholdercolor(value?: PB_Color4): void

  getPlaceholder(): string
  setPlaceholder(value: string): void

  getMargin(): number
  setMargin(value: number): void

  getMaxwidth(): number
  setMaxwidth(value: number): void

  getHtextalign(): string
  setHtextalign(value: string): void

  getVtextalign(): string
  setVtextalign(value: string): void

  getAutostretchwidth(): boolean
  setAutostretchwidth(value: boolean): void

  hasBackground(): boolean
  clearBackground(): void
  getBackground(): PB_Color4 | undefined
  setBackground(value?: PB_Color4): void

  hasFocusedbackground(): boolean
  clearFocusedbackground(): void
  getFocusedbackground(): PB_Color4 | undefined
  setFocusedbackground(value?: PB_Color4): void

  getTextwrapping(): boolean
  setTextwrapping(value: boolean): void

  getShadowblur(): number
  setShadowblur(value: number): void

  getShadowoffsetx(): number
  setShadowoffsetx(value: number): void

  getShadowoffsety(): number
  setShadowoffsety(value: number): void

  hasShadowcolor(): boolean
  clearShadowcolor(): void
  getShadowcolor(): PB_Color4 | undefined
  setShadowcolor(value?: PB_Color4): void

  getPaddingtop(): number
  setPaddingtop(value: number): void

  getPaddingright(): number
  setPaddingright(value: number): void

  getPaddingbottom(): number
  setPaddingbottom(value: number): void

  getPaddingleft(): number
  setPaddingleft(value: number): void

  hasOntextsubmit(): boolean
  clearOntextsubmit(): void
  getOntextsubmit(): PB_UUIDCallback | undefined
  setOntextsubmit(value?: PB_UUIDCallback): void

  hasOnchanged(): boolean
  clearOnchanged(): void
  getOnchanged(): PB_UUIDCallback | undefined
  setOnchanged(value?: PB_UUIDCallback): void

  hasOnfocus(): boolean
  clearOnfocus(): void
  getOnfocus(): PB_UUIDCallback | undefined
  setOnfocus(value?: PB_UUIDCallback): void

  hasOnblur(): boolean
  clearOnblur(): void
  getOnblur(): PB_UUIDCallback | undefined
  setOnblur(value?: PB_UUIDCallback): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UIInputText.AsObject
  static toObject(includeInstance: boolean, msg: PB_UIInputText): PB_UIInputText.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UIInputText, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UIInputText
  static deserializeBinaryFromReader(message: PB_UIInputText, reader: jspb.BinaryReader): PB_UIInputText
}

export namespace PB_UIInputText {
  export type AsObject = {
    name: string
    visible: boolean
    opacity: number
    halign: string
    valign: string
    width: string
    height: string
    positionx: string
    positiony: string
    ispointerblocker: boolean
    parent?: PB_UIShape.AsObject
    outlinewidth: number
    outlinecolor?: PB_Color4.AsObject
    color?: PB_Color4.AsObject
    thickness: number
    fontsize: number
    fontweight: string
    value: string
    placeholdercolor?: PB_Color4.AsObject
    placeholder: string
    margin: number
    maxwidth: number
    htextalign: string
    vtextalign: string
    autostretchwidth: boolean
    background?: PB_Color4.AsObject
    focusedbackground?: PB_Color4.AsObject
    textwrapping: boolean
    shadowblur: number
    shadowoffsetx: number
    shadowoffsety: number
    shadowcolor?: PB_Color4.AsObject
    paddingtop: number
    paddingright: number
    paddingbottom: number
    paddingleft: number
    ontextsubmit?: PB_UUIDCallback.AsObject
    onchanged?: PB_UUIDCallback.AsObject
    onfocus?: PB_UUIDCallback.AsObject
    onblur?: PB_UUIDCallback.AsObject
  }
}

export class PB_UIScrollRect extends jspb.Message {
  getName(): string
  setName(value: string): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getOpacity(): number
  setOpacity(value: number): void

  getHalign(): string
  setHalign(value: string): void

  getValign(): string
  setValign(value: string): void

  getWidth(): string
  setWidth(value: string): void

  getHeight(): string
  setHeight(value: string): void

  getPositionx(): string
  setPositionx(value: string): void

  getPositiony(): string
  setPositiony(value: string): void

  getIspointerblocker(): boolean
  setIspointerblocker(value: boolean): void

  hasParent(): boolean
  clearParent(): void
  getParent(): PB_UIShape | undefined
  setParent(value?: PB_UIShape): void

  getValuex(): number
  setValuex(value: number): void

  getValuey(): number
  setValuey(value: number): void

  hasBordercolor(): boolean
  clearBordercolor(): void
  getBordercolor(): PB_Color4 | undefined
  setBordercolor(value?: PB_Color4): void

  hasBackgroundcolor(): boolean
  clearBackgroundcolor(): void
  getBackgroundcolor(): PB_Color4 | undefined
  setBackgroundcolor(value?: PB_Color4): void

  getIshorizontal(): boolean
  setIshorizontal(value: boolean): void

  getIsvertical(): boolean
  setIsvertical(value: boolean): void

  getPaddingtop(): number
  setPaddingtop(value: number): void

  getPaddingright(): number
  setPaddingright(value: number): void

  getPaddingbottom(): number
  setPaddingbottom(value: number): void

  getPaddingleft(): number
  setPaddingleft(value: number): void

  hasOnchanged(): boolean
  clearOnchanged(): void
  getOnchanged(): PB_UUIDCallback | undefined
  setOnchanged(value?: PB_UUIDCallback): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UIScrollRect.AsObject
  static toObject(includeInstance: boolean, msg: PB_UIScrollRect): PB_UIScrollRect.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UIScrollRect, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UIScrollRect
  static deserializeBinaryFromReader(message: PB_UIScrollRect, reader: jspb.BinaryReader): PB_UIScrollRect
}

export namespace PB_UIScrollRect {
  export type AsObject = {
    name: string
    visible: boolean
    opacity: number
    halign: string
    valign: string
    width: string
    height: string
    positionx: string
    positiony: string
    ispointerblocker: boolean
    parent?: PB_UIShape.AsObject
    valuex: number
    valuey: number
    bordercolor?: PB_Color4.AsObject
    backgroundcolor?: PB_Color4.AsObject
    ishorizontal: boolean
    isvertical: boolean
    paddingtop: number
    paddingright: number
    paddingbottom: number
    paddingleft: number
    onchanged?: PB_UUIDCallback.AsObject
  }
}

export class PB_UIShape extends jspb.Message {
  getName(): string
  setName(value: string): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getOpacity(): number
  setOpacity(value: number): void

  getHalign(): string
  setHalign(value: string): void

  getValign(): string
  setValign(value: string): void

  getWidth(): string
  setWidth(value: string): void

  getHeight(): string
  setHeight(value: string): void

  getPositionx(): string
  setPositionx(value: string): void

  getPositiony(): string
  setPositiony(value: string): void

  getIspointerblocker(): boolean
  setIspointerblocker(value: boolean): void

  hasParent(): boolean
  clearParent(): void
  getParent(): PB_UIShape | undefined
  setParent(value?: PB_UIShape): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UIShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_UIShape): PB_UIShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UIShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UIShape
  static deserializeBinaryFromReader(message: PB_UIShape, reader: jspb.BinaryReader): PB_UIShape
}

export namespace PB_UIShape {
  export type AsObject = {
    name: string
    visible: boolean
    opacity: number
    halign: string
    valign: string
    width: string
    height: string
    positionx: string
    positiony: string
    ispointerblocker: boolean
    parent?: PB_UIShape.AsObject
  }
}

export class PB_UITextShape extends jspb.Message {
  getName(): string
  setName(value: string): void

  getVisible(): boolean
  setVisible(value: boolean): void

  getOpacity(): number
  setOpacity(value: number): void

  getHalign(): string
  setHalign(value: string): void

  getValign(): string
  setValign(value: string): void

  getWidth(): string
  setWidth(value: string): void

  getHeight(): string
  setHeight(value: string): void

  getPositionx(): string
  setPositionx(value: string): void

  getPositiony(): string
  setPositiony(value: string): void

  getIspointerblocker(): boolean
  setIspointerblocker(value: boolean): void

  hasParent(): boolean
  clearParent(): void
  getParent(): PB_UIShape | undefined
  setParent(value?: PB_UIShape): void

  getOutlinewidth(): number
  setOutlinewidth(value: number): void

  hasOutlinecolor(): boolean
  clearOutlinecolor(): void
  getOutlinecolor(): PB_Color4 | undefined
  setOutlinecolor(value?: PB_Color4): void

  hasColor(): boolean
  clearColor(): void
  getColor(): PB_Color4 | undefined
  setColor(value?: PB_Color4): void

  getFontsize(): number
  setFontsize(value: number): void

  getFontautosize(): boolean
  setFontautosize(value: boolean): void

  getFontweight(): string
  setFontweight(value: string): void

  getValue(): string
  setValue(value: string): void

  getLinespacing(): number
  setLinespacing(value: number): void

  getLinecount(): number
  setLinecount(value: number): void

  getAdaptwidth(): boolean
  setAdaptwidth(value: boolean): void

  getAdaptheight(): boolean
  setAdaptheight(value: boolean): void

  getTextwrapping(): boolean
  setTextwrapping(value: boolean): void

  getShadowblur(): number
  setShadowblur(value: number): void

  getShadowoffsetx(): number
  setShadowoffsetx(value: number): void

  getShadowoffsety(): number
  setShadowoffsety(value: number): void

  hasShadowcolor(): boolean
  clearShadowcolor(): void
  getShadowcolor(): PB_Color4 | undefined
  setShadowcolor(value?: PB_Color4): void

  getHtextalign(): string
  setHtextalign(value: string): void

  getVtextalign(): string
  setVtextalign(value: string): void

  getPaddingtop(): number
  setPaddingtop(value: number): void

  getPaddingright(): number
  setPaddingright(value: number): void

  getPaddingbottom(): number
  setPaddingbottom(value: number): void

  getPaddingleft(): number
  setPaddingleft(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_UITextShape.AsObject
  static toObject(includeInstance: boolean, msg: PB_UITextShape): PB_UITextShape.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_UITextShape, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_UITextShape
  static deserializeBinaryFromReader(message: PB_UITextShape, reader: jspb.BinaryReader): PB_UITextShape
}

export namespace PB_UITextShape {
  export type AsObject = {
    name: string
    visible: boolean
    opacity: number
    halign: string
    valign: string
    width: string
    height: string
    positionx: string
    positiony: string
    ispointerblocker: boolean
    parent?: PB_UIShape.AsObject
    outlinewidth: number
    outlinecolor?: PB_Color4.AsObject
    color?: PB_Color4.AsObject
    fontsize: number
    fontautosize: boolean
    fontweight: string
    value: string
    linespacing: number
    linecount: number
    adaptwidth: boolean
    adaptheight: boolean
    textwrapping: boolean
    shadowblur: number
    shadowoffsetx: number
    shadowoffsety: number
    shadowcolor?: PB_Color4.AsObject
    htextalign: string
    vtextalign: string
    paddingtop: number
    paddingright: number
    paddingbottom: number
    paddingleft: number
  }
}

export class PB_OpenExternalUrl extends jspb.Message {
  getUrl(): string
  setUrl(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_OpenExternalUrl.AsObject
  static toObject(includeInstance: boolean, msg: PB_OpenExternalUrl): PB_OpenExternalUrl.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_OpenExternalUrl, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_OpenExternalUrl
  static deserializeBinaryFromReader(message: PB_OpenExternalUrl, reader: jspb.BinaryReader): PB_OpenExternalUrl
}

export namespace PB_OpenExternalUrl {
  export type AsObject = {
    url: string
  }
}

export class PB_OpenNFTDialog extends jspb.Message {
  getAssetcontractaddress(): string
  setAssetcontractaddress(value: string): void

  getTokenid(): string
  setTokenid(value: string): void

  getComment(): string
  setComment(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PB_OpenNFTDialog.AsObject
  static toObject(includeInstance: boolean, msg: PB_OpenNFTDialog): PB_OpenNFTDialog.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PB_OpenNFTDialog, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PB_OpenNFTDialog
  static deserializeBinaryFromReader(message: PB_OpenNFTDialog, reader: jspb.BinaryReader): PB_OpenNFTDialog
}

export namespace PB_OpenNFTDialog {
  export type AsObject = {
    assetcontractaddress: string
    tokenid: string
    comment: string
  }
}

export interface PB_UIStackOrientationMap {
  VERTICAL: 0
  HORIZONTAL: 1
}

export const PB_UIStackOrientation: PB_UIStackOrientationMap
