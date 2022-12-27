/* eslint-disable */
import Long from 'long'
import * as _m0 from 'protobufjs/minimal'

export const protobufPackage = ''

export enum PacketType {
  UKNOWN_PACKET_TYPE = 0,
  MESSAGE = 1,
  PING = 2,
  PONG = 3,
  SUSPEND_RELAY = 4,
  UNRECOGNIZED = -1
}

export function packetTypeFromJSON(object: any): PacketType {
  switch (object) {
    case 0:
    case 'UKNOWN_PACKET_TYPE':
      return PacketType.UKNOWN_PACKET_TYPE
    case 1:
    case 'MESSAGE':
      return PacketType.MESSAGE
    case 2:
    case 'PING':
      return PacketType.PING
    case 3:
    case 'PONG':
      return PacketType.PONG
    case 4:
    case 'SUSPEND_RELAY':
      return PacketType.SUSPEND_RELAY
    case -1:
    case 'UNRECOGNIZED':
    default:
      return PacketType.UNRECOGNIZED
  }
}

export function packetTypeToJSON(object: PacketType): string {
  switch (object) {
    case PacketType.UKNOWN_PACKET_TYPE:
      return 'UKNOWN_PACKET_TYPE'
    case PacketType.MESSAGE:
      return 'MESSAGE'
    case PacketType.PING:
      return 'PING'
    case PacketType.PONG:
      return 'PONG'
    case PacketType.SUSPEND_RELAY:
      return 'SUSPEND_RELAY'
    case PacketType.UNRECOGNIZED:
    default:
      return 'UNRECOGNIZED'
  }
}

export interface MessageData {
  room: string
  dst: Uint8Array[]
  payload: Uint8Array
}

export interface PingData {
  pingId: number
}

export interface PongData {
  pingId: number
}

export interface SuspendRelayData {
  relayedPeers: string[]
  durationMillis: number
}

export interface Packet {
  sequenceId: number
  instanceId: number
  timestamp: number
  src: string
  subtype: string
  /** If negative, it means it is not set. */
  discardOlderThan: number
  optimistic: boolean
  /** If negative, it means it is not set. */
  expireTime: number
  hops: number
  ttl: number
  receivedBy: string[]
  data?:
    | { $case: 'messageData'; messageData: MessageData }
    | { $case: 'pingData'; pingData: PingData }
    | { $case: 'pongData'; pongData: PongData }
    | { $case: 'suspendRelayData'; suspendRelayData: SuspendRelayData }
}

function createBaseMessageData(): MessageData {
  return { room: '', dst: [], payload: new Uint8Array() }
}

export const MessageData = {
  encode(message: MessageData, writer: _m0.Writer = _m0.Writer.create()): _m0.Writer {
    if (message.room !== '') {
      writer.uint32(10).string(message.room)
    }
    for (const v of message.dst) {
      writer.uint32(18).bytes(v!)
    }
    if (message.payload.length !== 0) {
      writer.uint32(26).bytes(message.payload)
    }
    return writer
  },

  decode(input: _m0.Reader | Uint8Array, length?: number): MessageData {
    const reader = input instanceof _m0.Reader ? input : new _m0.Reader(input)
    let end = length === undefined ? reader.len : reader.pos + length
    const message = createBaseMessageData()
    while (reader.pos < end) {
      const tag = reader.uint32()
      switch (tag >>> 3) {
        case 1:
          message.room = reader.string()
          break
        case 2:
          message.dst.push(reader.bytes())
          break
        case 3:
          message.payload = reader.bytes()
          break
        default:
          reader.skipType(tag & 7)
          break
      }
    }
    return message
  },

  fromJSON(object: any): MessageData {
    return {
      room: isSet(object.room) ? String(object.room) : '',
      dst: Array.isArray(object?.dst) ? object.dst.map((e: any) => bytesFromBase64(e)) : [],
      payload: isSet(object.payload) ? bytesFromBase64(object.payload) : new Uint8Array()
    }
  },

  toJSON(message: MessageData): unknown {
    const obj: any = {}
    message.room !== undefined && (obj.room = message.room)
    if (message.dst) {
      obj.dst = message.dst.map((e) => base64FromBytes(e !== undefined ? e : new Uint8Array()))
    } else {
      obj.dst = []
    }
    message.payload !== undefined &&
      (obj.payload = base64FromBytes(message.payload !== undefined ? message.payload : new Uint8Array()))
    return obj
  },

  fromPartial<I extends Exact<DeepPartial<MessageData>, I>>(object: I): MessageData {
    const message = createBaseMessageData()
    message.room = object.room ?? ''
    message.dst = object.dst?.map((e) => e) || []
    message.payload = object.payload ?? new Uint8Array()
    return message
  }
}

function createBasePingData(): PingData {
  return { pingId: 0 }
}

export const PingData = {
  encode(message: PingData, writer: _m0.Writer = _m0.Writer.create()): _m0.Writer {
    if (message.pingId !== 0) {
      writer.uint32(8).uint32(message.pingId)
    }
    return writer
  },

  decode(input: _m0.Reader | Uint8Array, length?: number): PingData {
    const reader = input instanceof _m0.Reader ? input : new _m0.Reader(input)
    let end = length === undefined ? reader.len : reader.pos + length
    const message = createBasePingData()
    while (reader.pos < end) {
      const tag = reader.uint32()
      switch (tag >>> 3) {
        case 1:
          message.pingId = reader.uint32()
          break
        default:
          reader.skipType(tag & 7)
          break
      }
    }
    return message
  },

  fromJSON(object: any): PingData {
    return {
      pingId: isSet(object.pingId) ? Number(object.pingId) : 0
    }
  },

  toJSON(message: PingData): unknown {
    const obj: any = {}
    message.pingId !== undefined && (obj.pingId = Math.round(message.pingId))
    return obj
  },

  fromPartial<I extends Exact<DeepPartial<PingData>, I>>(object: I): PingData {
    const message = createBasePingData()
    message.pingId = object.pingId ?? 0
    return message
  }
}

function createBasePongData(): PongData {
  return { pingId: 0 }
}

export const PongData = {
  encode(message: PongData, writer: _m0.Writer = _m0.Writer.create()): _m0.Writer {
    if (message.pingId !== 0) {
      writer.uint32(8).uint32(message.pingId)
    }
    return writer
  },

  decode(input: _m0.Reader | Uint8Array, length?: number): PongData {
    const reader = input instanceof _m0.Reader ? input : new _m0.Reader(input)
    let end = length === undefined ? reader.len : reader.pos + length
    const message = createBasePongData()
    while (reader.pos < end) {
      const tag = reader.uint32()
      switch (tag >>> 3) {
        case 1:
          message.pingId = reader.uint32()
          break
        default:
          reader.skipType(tag & 7)
          break
      }
    }
    return message
  },

  fromJSON(object: any): PongData {
    return {
      pingId: isSet(object.pingId) ? Number(object.pingId) : 0
    }
  },

  toJSON(message: PongData): unknown {
    const obj: any = {}
    message.pingId !== undefined && (obj.pingId = Math.round(message.pingId))
    return obj
  },

  fromPartial<I extends Exact<DeepPartial<PongData>, I>>(object: I): PongData {
    const message = createBasePongData()
    message.pingId = object.pingId ?? 0
    return message
  }
}

function createBaseSuspendRelayData(): SuspendRelayData {
  return { relayedPeers: [], durationMillis: 0 }
}

export const SuspendRelayData = {
  encode(message: SuspendRelayData, writer: _m0.Writer = _m0.Writer.create()): _m0.Writer {
    for (const v of message.relayedPeers) {
      writer.uint32(10).string(v!)
    }
    if (message.durationMillis !== 0) {
      writer.uint32(16).uint32(message.durationMillis)
    }
    return writer
  },

  decode(input: _m0.Reader | Uint8Array, length?: number): SuspendRelayData {
    const reader = input instanceof _m0.Reader ? input : new _m0.Reader(input)
    let end = length === undefined ? reader.len : reader.pos + length
    const message = createBaseSuspendRelayData()
    while (reader.pos < end) {
      const tag = reader.uint32()
      switch (tag >>> 3) {
        case 1:
          message.relayedPeers.push(reader.string())
          break
        case 2:
          message.durationMillis = reader.uint32()
          break
        default:
          reader.skipType(tag & 7)
          break
      }
    }
    return message
  },

  fromJSON(object: any): SuspendRelayData {
    return {
      relayedPeers: Array.isArray(object?.relayedPeers) ? object.relayedPeers.map((e: any) => String(e)) : [],
      durationMillis: isSet(object.durationMillis) ? Number(object.durationMillis) : 0
    }
  },

  toJSON(message: SuspendRelayData): unknown {
    const obj: any = {}
    if (message.relayedPeers) {
      obj.relayedPeers = message.relayedPeers.map((e) => e)
    } else {
      obj.relayedPeers = []
    }
    message.durationMillis !== undefined && (obj.durationMillis = Math.round(message.durationMillis))
    return obj
  },

  fromPartial<I extends Exact<DeepPartial<SuspendRelayData>, I>>(object: I): SuspendRelayData {
    const message = createBaseSuspendRelayData()
    message.relayedPeers = object.relayedPeers?.map((e) => e) || []
    message.durationMillis = object.durationMillis ?? 0
    return message
  }
}

function createBasePacket(): Packet {
  return {
    sequenceId: 0,
    instanceId: 0,
    timestamp: 0,
    src: '',
    subtype: '',
    discardOlderThan: 0,
    optimistic: false,
    expireTime: 0,
    hops: 0,
    ttl: 0,
    receivedBy: [],
    data: undefined
  }
}

export const Packet = {
  encode(message: Packet, writer: _m0.Writer = _m0.Writer.create()): _m0.Writer {
    if (message.sequenceId !== 0) {
      writer.uint32(8).uint32(message.sequenceId)
    }
    if (message.instanceId !== 0) {
      writer.uint32(16).uint32(message.instanceId)
    }
    if (message.timestamp !== 0) {
      writer.uint32(24).uint64(message.timestamp)
    }
    if (message.src !== '') {
      writer.uint32(34).string(message.src)
    }
    if (message.subtype !== '') {
      writer.uint32(42).string(message.subtype)
    }
    if (message.discardOlderThan !== 0) {
      writer.uint32(48).int32(message.discardOlderThan)
    }
    if (message.optimistic === true) {
      writer.uint32(112).bool(message.optimistic)
    }
    if (message.expireTime !== 0) {
      writer.uint32(56).int32(message.expireTime)
    }
    if (message.hops !== 0) {
      writer.uint32(64).uint32(message.hops)
    }
    if (message.ttl !== 0) {
      writer.uint32(72).uint32(message.ttl)
    }
    for (const v of message.receivedBy) {
      writer.uint32(82).string(v!)
    }
    if (message.data?.$case === 'messageData') {
      MessageData.encode(message.data.messageData, writer.uint32(90).fork()).ldelim()
    }
    if (message.data?.$case === 'pingData') {
      PingData.encode(message.data.pingData, writer.uint32(98).fork()).ldelim()
    }
    if (message.data?.$case === 'pongData') {
      PongData.encode(message.data.pongData, writer.uint32(106).fork()).ldelim()
    }
    if (message.data?.$case === 'suspendRelayData') {
      SuspendRelayData.encode(message.data.suspendRelayData, writer.uint32(122).fork()).ldelim()
    }
    return writer
  },

  decode(input: _m0.Reader | Uint8Array, length?: number): Packet {
    const reader = input instanceof _m0.Reader ? input : new _m0.Reader(input)
    let end = length === undefined ? reader.len : reader.pos + length
    const message = createBasePacket()
    while (reader.pos < end) {
      const tag = reader.uint32()
      switch (tag >>> 3) {
        case 1:
          message.sequenceId = reader.uint32()
          break
        case 2:
          message.instanceId = reader.uint32()
          break
        case 3:
          message.timestamp = longToNumber(reader.uint64() as Long)
          break
        case 4:
          message.src = reader.string()
          break
        case 5:
          message.subtype = reader.string()
          break
        case 6:
          message.discardOlderThan = reader.int32()
          break
        case 14:
          message.optimistic = reader.bool()
          break
        case 7:
          message.expireTime = reader.int32()
          break
        case 8:
          message.hops = reader.uint32()
          break
        case 9:
          message.ttl = reader.uint32()
          break
        case 10:
          message.receivedBy.push(reader.string())
          break
        case 11:
          message.data = { $case: 'messageData', messageData: MessageData.decode(reader, reader.uint32()) }
          break
        case 12:
          message.data = { $case: 'pingData', pingData: PingData.decode(reader, reader.uint32()) }
          break
        case 13:
          message.data = { $case: 'pongData', pongData: PongData.decode(reader, reader.uint32()) }
          break
        case 15:
          message.data = {
            $case: 'suspendRelayData',
            suspendRelayData: SuspendRelayData.decode(reader, reader.uint32())
          }
          break
        default:
          reader.skipType(tag & 7)
          break
      }
    }
    return message
  },

  fromJSON(object: any): Packet {
    return {
      sequenceId: isSet(object.sequenceId) ? Number(object.sequenceId) : 0,
      instanceId: isSet(object.instanceId) ? Number(object.instanceId) : 0,
      timestamp: isSet(object.timestamp) ? Number(object.timestamp) : 0,
      src: isSet(object.src) ? String(object.src) : '',
      subtype: isSet(object.subtype) ? String(object.subtype) : '',
      discardOlderThan: isSet(object.discardOlderThan) ? Number(object.discardOlderThan) : 0,
      optimistic: isSet(object.optimistic) ? Boolean(object.optimistic) : false,
      expireTime: isSet(object.expireTime) ? Number(object.expireTime) : 0,
      hops: isSet(object.hops) ? Number(object.hops) : 0,
      ttl: isSet(object.ttl) ? Number(object.ttl) : 0,
      receivedBy: Array.isArray(object?.receivedBy) ? object.receivedBy.map((e: any) => String(e)) : [],
      data: isSet(object.messageData)
        ? { $case: 'messageData', messageData: MessageData.fromJSON(object.messageData) }
        : isSet(object.pingData)
        ? { $case: 'pingData', pingData: PingData.fromJSON(object.pingData) }
        : isSet(object.pongData)
        ? { $case: 'pongData', pongData: PongData.fromJSON(object.pongData) }
        : isSet(object.suspendRelayData)
        ? { $case: 'suspendRelayData', suspendRelayData: SuspendRelayData.fromJSON(object.suspendRelayData) }
        : undefined
    }
  },

  toJSON(message: Packet): unknown {
    const obj: any = {}
    message.sequenceId !== undefined && (obj.sequenceId = Math.round(message.sequenceId))
    message.instanceId !== undefined && (obj.instanceId = Math.round(message.instanceId))
    message.timestamp !== undefined && (obj.timestamp = Math.round(message.timestamp))
    message.src !== undefined && (obj.src = message.src)
    message.subtype !== undefined && (obj.subtype = message.subtype)
    message.discardOlderThan !== undefined && (obj.discardOlderThan = Math.round(message.discardOlderThan))
    message.optimistic !== undefined && (obj.optimistic = message.optimistic)
    message.expireTime !== undefined && (obj.expireTime = Math.round(message.expireTime))
    message.hops !== undefined && (obj.hops = Math.round(message.hops))
    message.ttl !== undefined && (obj.ttl = Math.round(message.ttl))
    if (message.receivedBy) {
      obj.receivedBy = message.receivedBy.map((e) => e)
    } else {
      obj.receivedBy = []
    }
    message.data?.$case === 'messageData' &&
      (obj.messageData = message.data?.messageData ? MessageData.toJSON(message.data?.messageData) : undefined)
    message.data?.$case === 'pingData' &&
      (obj.pingData = message.data?.pingData ? PingData.toJSON(message.data?.pingData) : undefined)
    message.data?.$case === 'pongData' &&
      (obj.pongData = message.data?.pongData ? PongData.toJSON(message.data?.pongData) : undefined)
    message.data?.$case === 'suspendRelayData' &&
      (obj.suspendRelayData = message.data?.suspendRelayData
        ? SuspendRelayData.toJSON(message.data?.suspendRelayData)
        : undefined)
    return obj
  },

  fromPartial<I extends Exact<DeepPartial<Packet>, I>>(object: I): Packet {
    const message = createBasePacket()
    message.sequenceId = object.sequenceId ?? 0
    message.instanceId = object.instanceId ?? 0
    message.timestamp = object.timestamp ?? 0
    message.src = object.src ?? ''
    message.subtype = object.subtype ?? ''
    message.discardOlderThan = object.discardOlderThan ?? 0
    message.optimistic = object.optimistic ?? false
    message.expireTime = object.expireTime ?? 0
    message.hops = object.hops ?? 0
    message.ttl = object.ttl ?? 0
    message.receivedBy = object.receivedBy?.map((e) => e) || []
    if (
      object.data?.$case === 'messageData' &&
      object.data?.messageData !== undefined &&
      object.data?.messageData !== null
    ) {
      message.data = { $case: 'messageData', messageData: MessageData.fromPartial(object.data.messageData) }
    }
    if (object.data?.$case === 'pingData' && object.data?.pingData !== undefined && object.data?.pingData !== null) {
      message.data = { $case: 'pingData', pingData: PingData.fromPartial(object.data.pingData) }
    }
    if (object.data?.$case === 'pongData' && object.data?.pongData !== undefined && object.data?.pongData !== null) {
      message.data = { $case: 'pongData', pongData: PongData.fromPartial(object.data.pongData) }
    }
    if (
      object.data?.$case === 'suspendRelayData' &&
      object.data?.suspendRelayData !== undefined &&
      object.data?.suspendRelayData !== null
    ) {
      message.data = {
        $case: 'suspendRelayData',
        suspendRelayData: SuspendRelayData.fromPartial(object.data.suspendRelayData)
      }
    }
    return message
  }
}

declare var self: any | undefined
declare var window: any | undefined
declare var global: any | undefined
var globalThis: any = (() => {
  if (typeof globalThis !== 'undefined') return globalThis
  if (typeof self !== 'undefined') return self
  if (typeof window !== 'undefined') return window
  if (typeof global !== 'undefined') return global
  throw 'Unable to locate global object'
})()

const atob: (b64: string) => string =
  globalThis.atob || ((b64) => globalThis.Buffer.from(b64, 'base64').toString('binary'))
function bytesFromBase64(b64: string): Uint8Array {
  const bin = atob(b64)
  const arr = new Uint8Array(bin.length)
  for (let i = 0; i < bin.length; ++i) {
    arr[i] = bin.charCodeAt(i)
  }
  return arr
}

const btoa: (bin: string) => string =
  globalThis.btoa || ((bin) => globalThis.Buffer.from(bin, 'binary').toString('base64'))
function base64FromBytes(arr: Uint8Array): string {
  const bin: string[] = []
  arr.forEach((byte) => {
    bin.push(String.fromCharCode(byte))
  })
  return btoa(bin.join(''))
}

type Builtin = Date | Function | Uint8Array | string | number | boolean | undefined

export type DeepPartial<T> = T extends Builtin
  ? T
  : T extends Array<infer U>
  ? Array<DeepPartial<U>>
  : T extends ReadonlyArray<infer U>
  ? ReadonlyArray<DeepPartial<U>>
  : T extends { $case: string }
  ? { [K in keyof Omit<T, '$case'>]?: DeepPartial<T[K]> } & { $case: T['$case'] }
  : T extends {}
  ? { [K in keyof T]?: DeepPartial<T[K]> }
  : Partial<T>

type KeysOfUnion<T> = T extends T ? keyof T : never
export type Exact<P, I extends P> = P extends Builtin
  ? P
  : P & { [K in keyof P]: Exact<P[K], I[K]> } & Record<Exclude<keyof I, KeysOfUnion<P>>, never>

function longToNumber(long: Long): number {
  if (long.gt(Number.MAX_SAFE_INTEGER)) {
    throw new globalThis.Error('Value is larger than Number.MAX_SAFE_INTEGER')
  }
  return long.toNumber()
}

if (_m0.util.Long !== Long) {
  _m0.util.Long = Long as any
  _m0.configure()
}

function isSet(value: any): boolean {
  return value !== null && value !== undefined
}
