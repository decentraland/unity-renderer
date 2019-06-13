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

export enum MessageType {
  UNKNOWN_MESSAGE_TYPE = 0,
  WELCOME = 1,
  CONNECT = 2,
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
