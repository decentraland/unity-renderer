// package: 
// file: comms.proto

import * as jspb from "google-protobuf";

export class CommsMessage extends jspb.Message {
  getTime(): number;
  setTime(value: number): void;

  hasPositionData(): boolean;
  clearPositionData(): void;
  getPositionData(): PositionData | undefined;
  setPositionData(value?: PositionData): void;

  hasProfileData(): boolean;
  clearProfileData(): void;
  getProfileData(): ProfileData | undefined;
  setProfileData(value?: ProfileData): void;

  hasChatData(): boolean;
  clearChatData(): void;
  getChatData(): ChatData | undefined;
  setChatData(value?: ChatData): void;

  hasSceneData(): boolean;
  clearSceneData(): void;
  getSceneData(): SceneData | undefined;
  setSceneData(value?: SceneData): void;

  hasVoiceData(): boolean;
  clearVoiceData(): void;
  getVoiceData(): VoiceData | undefined;
  setVoiceData(value?: VoiceData): void;

  getDataCase(): CommsMessage.DataCase;
  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): CommsMessage.AsObject;
  static toObject(includeInstance: boolean, msg: CommsMessage): CommsMessage.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: CommsMessage, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): CommsMessage;
  static deserializeBinaryFromReader(message: CommsMessage, reader: jspb.BinaryReader): CommsMessage;
}

export namespace CommsMessage {
  export type AsObject = {
    time: number,
    positionData?: PositionData.AsObject,
    profileData?: ProfileData.AsObject,
    chatData?: ChatData.AsObject,
    sceneData?: SceneData.AsObject,
    voiceData?: VoiceData.AsObject,
  }

  export enum DataCase {
    DATA_NOT_SET = 0,
    POSITION_DATA = 2,
    PROFILE_DATA = 3,
    CHAT_DATA = 4,
    SCENE_DATA = 5,
    VOICE_DATA = 6,
  }
}

export class PositionData extends jspb.Message {
  getPositionX(): number;
  setPositionX(value: number): void;

  getPositionY(): number;
  setPositionY(value: number): void;

  getPositionZ(): number;
  setPositionZ(value: number): void;

  getRotationX(): number;
  setRotationX(value: number): void;

  getRotationY(): number;
  setRotationY(value: number): void;

  getRotationZ(): number;
  setRotationZ(value: number): void;

  getRotationW(): number;
  setRotationW(value: number): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): PositionData.AsObject;
  static toObject(includeInstance: boolean, msg: PositionData): PositionData.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: PositionData, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): PositionData;
  static deserializeBinaryFromReader(message: PositionData, reader: jspb.BinaryReader): PositionData;
}

export namespace PositionData {
  export type AsObject = {
    positionX: number,
    positionY: number,
    positionZ: number,
    rotationX: number,
    rotationY: number,
    rotationZ: number,
    rotationW: number,
  }
}

export class ProfileData extends jspb.Message {
  getProfileVersion(): string;
  setProfileVersion(value: string): void;

  getUserId(): string;
  setUserId(value: string): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): ProfileData.AsObject;
  static toObject(includeInstance: boolean, msg: ProfileData): ProfileData.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: ProfileData, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): ProfileData;
  static deserializeBinaryFromReader(message: ProfileData, reader: jspb.BinaryReader): ProfileData;
}

export namespace ProfileData {
  export type AsObject = {
    profileVersion: string,
    userId: string,
  }
}

export class ChatData extends jspb.Message {
  getMessageId(): string;
  setMessageId(value: string): void;

  getText(): string;
  setText(value: string): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): ChatData.AsObject;
  static toObject(includeInstance: boolean, msg: ChatData): ChatData.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: ChatData, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): ChatData;
  static deserializeBinaryFromReader(message: ChatData, reader: jspb.BinaryReader): ChatData;
}

export namespace ChatData {
  export type AsObject = {
    messageId: string,
    text: string,
  }
}

export class SceneData extends jspb.Message {
  getSceneId(): string;
  setSceneId(value: string): void;

  getText(): string;
  setText(value: string): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): SceneData.AsObject;
  static toObject(includeInstance: boolean, msg: SceneData): SceneData.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: SceneData, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): SceneData;
  static deserializeBinaryFromReader(message: SceneData, reader: jspb.BinaryReader): SceneData;
}

export namespace SceneData {
  export type AsObject = {
    sceneId: string,
    text: string,
  }
}

export class VoiceData extends jspb.Message {
  getEncodedSamples(): Uint8Array | string;
  getEncodedSamples_asU8(): Uint8Array;
  getEncodedSamples_asB64(): string;
  setEncodedSamples(value: Uint8Array | string): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): VoiceData.AsObject;
  static toObject(includeInstance: boolean, msg: VoiceData): VoiceData.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: VoiceData, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): VoiceData;
  static deserializeBinaryFromReader(message: VoiceData, reader: jspb.BinaryReader): VoiceData;
}

export namespace VoiceData {
  export type AsObject = {
    encodedSamples: Uint8Array | string,
  }
}

