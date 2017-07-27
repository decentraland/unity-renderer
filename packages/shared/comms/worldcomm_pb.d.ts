// package:
// file: worldcomm.proto

import * as jspb from 'google-protobuf'

export class GenericMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getTime(): number
  setTime(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): GenericMessage.AsObject
  static toObject(includeInstance: boolean, msg: GenericMessage): GenericMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: GenericMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): GenericMessage
  static deserializeBinaryFromReader(message: GenericMessage, reader: jspb.BinaryReader): GenericMessage
}

export namespace GenericMessage {
  export type AsObject = {
    type: MessageType
    time: number
  }
}

export class PositionMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

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

  getAlias(): number
  setAlias(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): PositionMessage.AsObject
  static toObject(includeInstance: boolean, msg: PositionMessage): PositionMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: PositionMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): PositionMessage
  static deserializeBinaryFromReader(message: PositionMessage, reader: jspb.BinaryReader): PositionMessage
}

export namespace PositionMessage {
  export type AsObject = {
    type: MessageType
    time: number
    positionX: number
    positionY: number
    positionZ: number
    rotationX: number
    rotationY: number
    rotationZ: number
    rotationW: number
    alias: number
  }
}

export class ProfileMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getTime(): number
  setTime(value: number): void

  getPositionX(): number
  setPositionX(value: number): void

  getPositionZ(): number
  setPositionZ(value: number): void

  getAvatarType(): string
  setAvatarType(value: string): void

  getDisplayName(): string
  setDisplayName(value: string): void

  getPeerId(): string
  setPeerId(value: string): void

  getAlias(): number
  setAlias(value: number): void

  getPublicKey(): string
  setPublicKey(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): ProfileMessage.AsObject
  static toObject(includeInstance: boolean, msg: ProfileMessage): ProfileMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: ProfileMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): ProfileMessage
  static deserializeBinaryFromReader(message: ProfileMessage, reader: jspb.BinaryReader): ProfileMessage
}

export namespace ProfileMessage {
  export type AsObject = {
    type: MessageType
    time: number
    positionX: number
    positionZ: number
    avatarType: string
    displayName: string
    peerId: string
    alias: number
    publicKey: string
  }
}

export class ChatMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getTime(): number
  setTime(value: number): void

  getMessageId(): string
  setMessageId(value: string): void

  getPositionX(): number
  setPositionX(value: number): void

  getPositionZ(): number
  setPositionZ(value: number): void

  getText(): string
  setText(value: string): void

  getAlias(): number
  setAlias(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): ChatMessage.AsObject
  static toObject(includeInstance: boolean, msg: ChatMessage): ChatMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: ChatMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): ChatMessage
  static deserializeBinaryFromReader(message: ChatMessage, reader: jspb.BinaryReader): ChatMessage
}

export namespace ChatMessage {
  export type AsObject = {
    type: MessageType
    time: number
    messageId: string
    positionX: number
    positionZ: number
    text: string
    alias: number
  }
}

export class ClientDisconnectedFromServerMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getTime(): number
  setTime(value: number): void

  getAlias(): number
  setAlias(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): ClientDisconnectedFromServerMessage.AsObject
  static toObject(
    includeInstance: boolean,
    msg: ClientDisconnectedFromServerMessage
  ): ClientDisconnectedFromServerMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: ClientDisconnectedFromServerMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): ClientDisconnectedFromServerMessage
  static deserializeBinaryFromReader(
    message: ClientDisconnectedFromServerMessage,
    reader: jspb.BinaryReader
  ): ClientDisconnectedFromServerMessage
}

export namespace ClientDisconnectedFromServerMessage {
  export type AsObject = {
    type: MessageType
    time: number
    alias: number
  }
}

export class ClockSkewMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getTime(): number
  setTime(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): ClockSkewMessage.AsObject
  static toObject(includeInstance: boolean, msg: ClockSkewMessage): ClockSkewMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: ClockSkewMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): ClockSkewMessage
  static deserializeBinaryFromReader(message: ClockSkewMessage, reader: jspb.BinaryReader): ClockSkewMessage
}

export namespace ClockSkewMessage {
  export type AsObject = {
    type: MessageType
    time: number
  }
}

export class FlowStatusMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getTime(): number
  setTime(value: number): void

  getFlowStatus(): FlowStatus
  setFlowStatus(value: FlowStatus): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): FlowStatusMessage.AsObject
  static toObject(includeInstance: boolean, msg: FlowStatusMessage): FlowStatusMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: FlowStatusMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): FlowStatusMessage
  static deserializeBinaryFromReader(message: FlowStatusMessage, reader: jspb.BinaryReader): FlowStatusMessage
}

export namespace FlowStatusMessage {
  export type AsObject = {
    type: MessageType
    time: number
    flowStatus: FlowStatus
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

export class WebRtcSupportedMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getTime(): number
  setTime(value: number): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): WebRtcSupportedMessage.AsObject
  static toObject(includeInstance: boolean, msg: WebRtcSupportedMessage): WebRtcSupportedMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: WebRtcSupportedMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): WebRtcSupportedMessage
  static deserializeBinaryFromReader(message: WebRtcSupportedMessage, reader: jspb.BinaryReader): WebRtcSupportedMessage
}

export namespace WebRtcSupportedMessage {
  export type AsObject = {
    type: MessageType
    time: number
  }
}

export class WebRtcOfferMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getTime(): number
  setTime(value: number): void

  getSdp(): string
  setSdp(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): WebRtcOfferMessage.AsObject
  static toObject(includeInstance: boolean, msg: WebRtcOfferMessage): WebRtcOfferMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: WebRtcOfferMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): WebRtcOfferMessage
  static deserializeBinaryFromReader(message: WebRtcOfferMessage, reader: jspb.BinaryReader): WebRtcOfferMessage
}

export namespace WebRtcOfferMessage {
  export type AsObject = {
    type: MessageType
    time: number
    sdp: string
  }
}

export class WebRtcAnswerMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getTime(): number
  setTime(value: number): void

  getSdp(): string
  setSdp(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): WebRtcAnswerMessage.AsObject
  static toObject(includeInstance: boolean, msg: WebRtcAnswerMessage): WebRtcAnswerMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: WebRtcAnswerMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): WebRtcAnswerMessage
  static deserializeBinaryFromReader(message: WebRtcAnswerMessage, reader: jspb.BinaryReader): WebRtcAnswerMessage
}

export namespace WebRtcAnswerMessage {
  export type AsObject = {
    type: MessageType
    time: number
    sdp: string
  }
}

export class WebRtcIceCandidateMessage extends jspb.Message {
  getType(): MessageType
  setType(value: MessageType): void

  getTime(): number
  setTime(value: number): void

  getSdp(): string
  setSdp(value: string): void

  serializeBinary(): Uint8Array
  toObject(includeInstance?: boolean): WebRtcIceCandidateMessage.AsObject
  static toObject(includeInstance: boolean, msg: WebRtcIceCandidateMessage): WebRtcIceCandidateMessage.AsObject
  static extensions: { [key: number]: jspb.ExtensionFieldInfo<jspb.Message> }
  static extensionsBinary: { [key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message> }
  static serializeBinaryToWriter(message: WebRtcIceCandidateMessage, writer: jspb.BinaryWriter): void
  static deserializeBinary(bytes: Uint8Array): WebRtcIceCandidateMessage
  static deserializeBinaryFromReader(
    message: WebRtcIceCandidateMessage,
    reader: jspb.BinaryReader
  ): WebRtcIceCandidateMessage
}

export namespace WebRtcIceCandidateMessage {
  export type AsObject = {
    type: MessageType
    time: number
    sdp: string
  }
}

export enum MessageType {
  UNKNOWN_MESSAGE_TYPE = 0,
  POSITION = 2,
  CHAT = 3,
  CLIENT_DISCONNECTED_FROM_SERVER = 4,
  PROFILE = 5,
  CLOCK_SKEW_DETECTED = 6,
  FLOW_STATUS = 7,
  PING = 8,
  WEBRTC_SUPPORTED = 9,
  WEBRTC_OFFER = 10,
  WEBRTC_ANSWER = 11,
  WEBRTC_ICE_CANDIDATE = 12
}

export enum FlowStatus {
  UNKNOWN_STATUS = 0,
  CLOSE = 1,
  OPEN = 100,
  OPEN_WEBRTC_PREFERRED = 101
}
