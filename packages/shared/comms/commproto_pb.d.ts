// package:
// file: commproto.proto

import * as jspb from 'google-protobuf'

export class CoordinatorMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): CoordinatorMessage.AsObject
  static toObject(includeInstance: boolean, msg: CoordinatorMessage): CoordinatorMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: CoordinatorMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): CoordinatorMessage
  static deserializeBinaryFromReader(message: CoordinatorMessage, reader: jspb.BinaryReader): CoordinatorMessage
}

export namespace CoordinatorMessage {
  export type AsObject = {
    type: MessageType
  }
}

export class WelcomeMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getAlias(): number
  setAlias(value: number): void

  clearAvailableServersList(): void
  getAvailableServersList(): Array<number>
  setAvailableServersList(value: Array<number>): void
  addAvailableServers(value: number, index?: number): number

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): WelcomeMessage.AsObject
  static toObject(includeInstance: boolean, msg: WelcomeMessage): WelcomeMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: WelcomeMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): WelcomeMessage
  static deserializeBinaryFromReader(message: WelcomeMessage, reader: jspb.BinaryReader): WelcomeMessage
}

export namespace WelcomeMessage {
  export type AsObject = {
    type: MessageType
    alias: number
    availableServersList: Array<number>
  }
}

export class ConnectMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getFromAlias(): number
  setFromAlias(value: number): void

  getToAlias(): number
  setToAlias(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): ConnectMessage.AsObject
  static toObject(includeInstance: boolean, msg: ConnectMessage): ConnectMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: ConnectMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): ConnectMessage
  static deserializeBinaryFromReader(message: ConnectMessage, reader: jspb.BinaryReader): ConnectMessage
}

export namespace ConnectMessage {
  export type AsObject = {
    type: MessageType
    fromAlias: number
    toAlias: number
  }
}

export class WebRtcMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getFromAlias(): number
  setFromAlias(value: number): void

  getToAlias(): number
  setToAlias(value: number): void

  getSdp(): string
  setSdp(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): WebRtcMessage.AsObject
  static toObject(includeInstance: boolean, msg: WebRtcMessage): WebRtcMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: WebRtcMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): WebRtcMessage
  static deserializeBinaryFromReader(message: WebRtcMessage, reader: jspb.BinaryReader): WebRtcMessage
}

export namespace WebRtcMessage {
  export type AsObject = {
    type: MessageType
    fromAlias: number
    toAlias: number
    sdp: string
  }
}

export class WorldCommMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): WorldCommMessage.AsObject
  static toObject(includeInstance: boolean, msg: WorldCommMessage): WorldCommMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: WorldCommMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): WorldCommMessage
  static deserializeBinaryFromReader(message: WorldCommMessage, reader: jspb.BinaryReader): WorldCommMessage
}

export namespace WorldCommMessage {
  export type AsObject = {
    type: MessageType
  }
}

export class PingMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getTime(): number
  setTime(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PingMessage.AsObject
  static toObject(includeInstance: boolean, msg: PingMessage): PingMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PingMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PingMessage
  static deserializeBinaryFromReader(message: PingMessage, reader: jspb.BinaryReader): PingMessage
}

export namespace PingMessage {
  export type AsObject = {
    type: MessageType
    time: number
  }
}

export class TopicSubscriptionMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getFormat(): Format
  setFormat(value: Format): void

  getTopics(): Uint8Array | string
  getTopics_asU8(): Uint8Array
  getTopics_asB64(): string
  setTopics(value: Uint8Array | string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): TopicSubscriptionMessage.AsObject
  static toObject(includeInstance: boolean, msg: TopicSubscriptionMessage): TopicSubscriptionMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: TopicSubscriptionMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): TopicSubscriptionMessage
  static deserializeBinaryFromReader(
    message: TopicSubscriptionMessage,
    reader: jspb.BinaryReader
  ): TopicSubscriptionMessage
}

export namespace TopicSubscriptionMessage {
  export type AsObject = {
    type: MessageType
    format: Format
    topics: Uint8Array | string
  }
}

export class TopicMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getFromAlias(): number
  setFromAlias(value: number): void

  getTopic(): string
  setTopic(value: string): void

  getBody(): Uint8Array | string
  getBody_asU8(): Uint8Array
  getBody_asB64(): string
  setBody(value: Uint8Array | string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): TopicMessage.AsObject
  static toObject(includeInstance: boolean, msg: TopicMessage): TopicMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: TopicMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): TopicMessage
  static deserializeBinaryFromReader(message: TopicMessage, reader: jspb.BinaryReader): TopicMessage
}

export namespace TopicMessage {
  export type AsObject = {
    type: MessageType
    fromAlias: number
    topic: string
    body: Uint8Array | string
  }
}

export class DataMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getFromAlias(): number
  setFromAlias(value: number): void

  getBody(): Uint8Array | string
  getBody_asU8(): Uint8Array
  getBody_asB64(): string
  setBody(value: Uint8Array | string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): DataMessage.AsObject
  static toObject(includeInstance: boolean, msg: DataMessage): DataMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: DataMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): DataMessage
  static deserializeBinaryFromReader(message: DataMessage, reader: jspb.BinaryReader): DataMessage
}

export namespace DataMessage {
  export type AsObject = {
    type: MessageType
    fromAlias: number
    body: Uint8Array | string
  }
}

export class AuthMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getRole(): Role
  setRole(value: Role): void

  getMethod(): string
  setMethod(value: string): void

  getBody(): Uint8Array | string
  getBody_asU8(): Uint8Array
  getBody_asB64(): string
  setBody(value: Uint8Array | string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): AuthMessage.AsObject
  static toObject(includeInstance: boolean, msg: AuthMessage): AuthMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: AuthMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): AuthMessage
  static deserializeBinaryFromReader(message: AuthMessage, reader: jspb.BinaryReader): AuthMessage
}

export namespace AuthMessage {
  export type AsObject = {
    type: MessageType
    role: Role
    method: string
    body: Uint8Array | string
  }
}

export class DataHeader extends jspb.Message {
  getCategory(): Category
  setCategory(value: Category): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): DataHeader.AsObject
  static toObject(includeInstance: boolean, msg: DataHeader): DataHeader.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: DataHeader, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): DataHeader
  static deserializeBinaryFromReader(message: DataHeader, reader: jspb.BinaryReader): DataHeader
}

export namespace DataHeader {
  export type AsObject = {
    category: Category
  }
}

export class PositionData extends jspb.Message {
  getCategory(): Category
  setCategory(value: Category): void

  getTime(): number
  setTime(value: number): void

  getPositionX(): number
  setPositionX(value: number): void

  getPositionY(): number
  setPositionY(value: number): void

  getPositionZ(): number
  setPositionZ(value: number): void

  getRotationX(): number
  setRotationX(value: number): void

  getRotationY(): number
  setRotationY(value: number): void

  getRotationZ(): number
  setRotationZ(value: number): void

  getRotationW(): number
  setRotationW(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PositionData.AsObject
  static toObject(includeInstance: boolean, msg: PositionData): PositionData.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PositionData, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PositionData
  static deserializeBinaryFromReader(message: PositionData, reader: jspb.BinaryReader): PositionData
}

export namespace PositionData {
  export type AsObject = {
    category: Category
    time: number
    positionX: number
    positionY: number
    positionZ: number
    rotationX: number
    rotationY: number
    rotationZ: number
    rotationW: number
  }
}

export class ProfileData extends jspb.Message {
  getCategory(): Category
  setCategory(value: Category): void

  getTime(): number
  setTime(value: number): void

  getAvatarType(): string
  setAvatarType(value: string): void

  getDisplayName(): string
  setDisplayName(value: string): void

  getPublicKey(): string
  setPublicKey(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): ProfileData.AsObject
  static toObject(includeInstance: boolean, msg: ProfileData): ProfileData.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: ProfileData, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): ProfileData
  static deserializeBinaryFromReader(message: ProfileData, reader: jspb.BinaryReader): ProfileData
}

export namespace ProfileData {
  export type AsObject = {
    category: Category
    time: number
    avatarType: string
    displayName: string
    publicKey: string
  }
}

export class ChatData extends jspb.Message {
  getCategory(): Category
  setCategory(value: Category): void

  getTime(): number
  setTime(value: number): void

  getMessageId(): string
  setMessageId(value: string): void

  getText(): string
  setText(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): ChatData.AsObject
  static toObject(includeInstance: boolean, msg: ChatData): ChatData.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: ChatData, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): ChatData
  static deserializeBinaryFromReader(message: ChatData, reader: jspb.BinaryReader): ChatData
}

export namespace ChatData {
  export type AsObject = {
    category: Category
    time: number
    messageId: string
    text: string
  }
}

export enum MessageType {
  UNKNOWN_MESSAGE_TYPE = 0,
  WELCOME = 1,
  CONNECT = 2,
  WEBRTC_SUPPORTED = 3,
  WEBRTC_OFFER = 4,
  WEBRTC_ANSWER = 5,
  WEBRTC_ICE_CANDIDATE = 6,
  PING = 7,
  TOPIC_SUBSCRIPTION = 8,
  TOPIC = 9,
  DATA = 10,
  AUTH = 11
}

export enum Role {
  UNKNOWN_ROLE = 0,
  CLIENT = 1,
  COMMUNICATION_SERVER = 2
}

export enum Format {
  UNKNOWN_FORMAT = 0,
  PLAIN = 1,
  GZIP = 2
}

export enum Category {
  UNKNOWN = 0,
  POSITION = 1,
  PROFILE = 2,
  CHAT = 3
}
