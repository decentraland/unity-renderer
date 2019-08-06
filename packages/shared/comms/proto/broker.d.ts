// package: protocol
// file: broker.proto

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

  getData(): Uint8Array | string
  getData_asU8(): Uint8Array
  getData_asB64(): string
  setData(value: Uint8Array | string): void

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
    data: Uint8Array | string
  }
}

export class MessageHeader extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): MessageHeader.AsObject
  static toObject(includeInstance: boolean, msg: MessageHeader): MessageHeader.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: MessageHeader, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): MessageHeader
  static deserializeBinaryFromReader(message: MessageHeader, reader: jspb.BinaryReader): MessageHeader
}

export namespace MessageHeader {
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

export class SubscriptionMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getFormat(): Format
  setFormat(value: Format): void

  getTopics(): Uint8Array | string
  getTopics_asU8(): Uint8Array
  getTopics_asB64(): string
  setTopics(value: Uint8Array | string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): SubscriptionMessage.AsObject
  static toObject(includeInstance: boolean, msg: SubscriptionMessage): SubscriptionMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: SubscriptionMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): SubscriptionMessage
  static deserializeBinaryFromReader(message: SubscriptionMessage, reader: jspb.BinaryReader): SubscriptionMessage
}

export namespace SubscriptionMessage {
  export type AsObject = {
    type: MessageType
    format: Format
    topics: Uint8Array | string
  }
}

export class AuthMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getRole(): Role
  setRole(value: Role): void

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
    body: Uint8Array | string
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

export class TopicFWMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getFromAlias(): number
  setFromAlias(value: number): void

  getBody(): Uint8Array | string
  getBody_asU8(): Uint8Array
  getBody_asB64(): string
  setBody(value: Uint8Array | string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): TopicFWMessage.AsObject
  static toObject(includeInstance: boolean, msg: TopicFWMessage): TopicFWMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: TopicFWMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): TopicFWMessage
  static deserializeBinaryFromReader(message: TopicFWMessage, reader: jspb.BinaryReader): TopicFWMessage
}

export namespace TopicFWMessage {
  export type AsObject = {
    type: MessageType
    fromAlias: number
    body: Uint8Array | string
  }
}

export class TopicIdentityMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getFromAlias(): number
  setFromAlias(value: number): void

  getTopic(): string
  setTopic(value: string): void

  getIdentity(): Uint8Array | string
  getIdentity_asU8(): Uint8Array
  getIdentity_asB64(): string
  setIdentity(value: Uint8Array | string): void

  getRole(): Role
  setRole(value: Role): void

  getBody(): Uint8Array | string
  getBody_asU8(): Uint8Array
  getBody_asB64(): string
  setBody(value: Uint8Array | string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): TopicIdentityMessage.AsObject
  static toObject(includeInstance: boolean, msg: TopicIdentityMessage): TopicIdentityMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: TopicIdentityMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): TopicIdentityMessage
  static deserializeBinaryFromReader(message: TopicIdentityMessage, reader: jspb.BinaryReader): TopicIdentityMessage
}

export namespace TopicIdentityMessage {
  export type AsObject = {
    type: MessageType
    fromAlias: number
    topic: string
    identity: Uint8Array | string
    role: Role
    body: Uint8Array | string
  }
}

export class TopicIdentityFWMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getFromAlias(): number
  setFromAlias(value: number): void

  getIdentity(): Uint8Array | string
  getIdentity_asU8(): Uint8Array
  getIdentity_asB64(): string
  setIdentity(value: Uint8Array | string): void

  getRole(): Role
  setRole(value: Role): void

  getBody(): Uint8Array | string
  getBody_asU8(): Uint8Array
  getBody_asB64(): string
  setBody(value: Uint8Array | string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): TopicIdentityFWMessage.AsObject
  static toObject(includeInstance: boolean, msg: TopicIdentityFWMessage): TopicIdentityFWMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: TopicIdentityFWMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): TopicIdentityFWMessage
  static deserializeBinaryFromReader(message: TopicIdentityFWMessage, reader: jspb.BinaryReader): TopicIdentityFWMessage
}

export namespace TopicIdentityFWMessage {
  export type AsObject = {
    type: MessageType
    fromAlias: number
    identity: Uint8Array | string
    role: Role
    body: Uint8Array | string
  }
}

export enum MessageType {
  UNKNOWN_MESSAGE_TYPE = 0,
  WELCOME = 1,
  CONNECT = 2,
  WEBRTC_OFFER = 3,
  WEBRTC_ANSWER = 4,
  WEBRTC_ICE_CANDIDATE = 5,
  PING = 6,
  SUBSCRIPTION = 7,
  AUTH = 8,
  TOPIC = 9,
  TOPIC_FW = 10,
  TOPIC_IDENTITY = 11,
  TOPIC_IDENTITY_FW = 12
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
