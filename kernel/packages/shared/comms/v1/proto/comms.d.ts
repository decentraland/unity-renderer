// package: protocol
// file: comms.proto

import * as jspb from "google-protobuf";

export class AuthData extends jspb.Message {
  getSignature(): string;
  setSignature(value: string): void;

  getIdentity(): string;
  setIdentity(value: string): void;

  getTimestamp(): string;
  setTimestamp(value: string): void;

  getAccessToken(): string;
  setAccessToken(value: string): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): AuthData.AsObject;
  static toObject(includeInstance: boolean, msg: AuthData): AuthData.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: AuthData, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): AuthData;
  static deserializeBinaryFromReader(message: AuthData, reader: jspb.BinaryReader): AuthData;
}

export namespace AuthData {
  export type AsObject = {
    signature: string,
    identity: string,
    timestamp: string,
    accessToken: string,
  }
}

export class DataHeader extends jspb.Message {
  getCategory(): CategoryMap[keyof CategoryMap];
  setCategory(value: CategoryMap[keyof CategoryMap]): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): DataHeader.AsObject;
  static toObject(includeInstance: boolean, msg: DataHeader): DataHeader.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: DataHeader, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): DataHeader;
  static deserializeBinaryFromReader(message: DataHeader, reader: jspb.BinaryReader): DataHeader;
}

export namespace DataHeader {
  export type AsObject = {
    category: CategoryMap[keyof CategoryMap],
  }
}

export class PositionData extends jspb.Message {
  getCategory(): CategoryMap[keyof CategoryMap];
  setCategory(value: CategoryMap[keyof CategoryMap]): void;

  getTime(): number;
  setTime(value: number): void;

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

  getImmediate(): boolean;
  setImmediate(value: boolean): void;

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
    category: CategoryMap[keyof CategoryMap],
    time: number,
    positionX: number,
    positionY: number,
    positionZ: number,
    rotationX: number,
    rotationY: number,
    rotationZ: number,
    rotationW: number,
    immediate: boolean,
  }
}

export class ProfileData extends jspb.Message {
  getCategory(): CategoryMap[keyof CategoryMap];
  setCategory(value: CategoryMap[keyof CategoryMap]): void;

  getTime(): number;
  setTime(value: number): void;

  getProfileVersion(): string;
  setProfileVersion(value: string): void;

  getProfileType(): ProfileData.ProfileTypeMap[keyof ProfileData.ProfileTypeMap];
  setProfileType(value: ProfileData.ProfileTypeMap[keyof ProfileData.ProfileTypeMap]): void;

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
    category: CategoryMap[keyof CategoryMap],
    time: number,
    profileVersion: string,
    profileType: ProfileData.ProfileTypeMap[keyof ProfileData.ProfileTypeMap],
  }

  export interface ProfileTypeMap {
    DEPLOYED: 0;
    LOCAL: 1;
  }

  export const ProfileType: ProfileTypeMap;
}

export class ChatData extends jspb.Message {
  getCategory(): CategoryMap[keyof CategoryMap];
  setCategory(value: CategoryMap[keyof CategoryMap]): void;

  getTime(): number;
  setTime(value: number): void;

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
    category: CategoryMap[keyof CategoryMap],
    time: number,
    messageId: string,
    text: string,
  }
}

export interface CategoryMap {
  UNKNOWN: 0;
  POSITION: 1;
  PROFILE: 2;
  CHAT: 3;
  SCENE_MESSAGE: 4;
}

export const Category: CategoryMap;

