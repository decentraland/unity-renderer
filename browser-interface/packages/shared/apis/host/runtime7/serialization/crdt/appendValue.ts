import { CrdtMessageProtocol } from './crdtMessageProtocol'
import { Entity } from '../../engine/entity'
import { ByteBuffer } from '../ByteBuffer'
import { AppendValueMessage, CrdtMessageType, CRDT_MESSAGE_HEADER_LENGTH } from './types'

/**
 * @public
 */
export namespace AppendValueOperation {
  export const MESSAGE_HEADER_LENGTH = 16

  /**
   * Call this function for an optimal writing data passing the ByteBuffer
   *  already allocated
   */
  export function write(entity: Entity, timestamp: number, componentId: number, data: Uint8Array, buf: ByteBuffer) {
    // reserve the beginning
    const startMessageOffset = buf.incrementWriteOffset(CRDT_MESSAGE_HEADER_LENGTH + MESSAGE_HEADER_LENGTH)

    // write body
    buf.writeBuffer(data, false)
    const messageLength = buf.currentWriteOffset() - startMessageOffset

    // Write CrdtMessage header
    buf.setUint32(startMessageOffset, messageLength)
    buf.setUint32(startMessageOffset + 4, CrdtMessageType.APPEND_VALUE)

    // Write ComponentOperation header
    buf.setUint32(startMessageOffset + 8, entity as number)
    buf.setUint32(startMessageOffset + 12, componentId)
    buf.setUint32(startMessageOffset + 16, timestamp)
    const newLocal = messageLength - MESSAGE_HEADER_LENGTH - CRDT_MESSAGE_HEADER_LENGTH
    buf.setUint32(startMessageOffset + 20, newLocal)
  }

  export function read(buf: ByteBuffer): AppendValueMessage | null {
    const header = CrdtMessageProtocol.readHeader(buf)

    /* istanbul ignore if */
    if (!header) {
      return null
    }

    /* istanbul ignore if */
    if (header.type !== CrdtMessageType.APPEND_VALUE) {
      throw new Error('AppendValueOperation tried to read another message type.')
    }

    return {
      ...header,
      entityId: buf.readUint32() as Entity,
      componentId: buf.readUint32(),
      timestamp: buf.readUint32(),
      data: buf.readBuffer()
    }
  }
}
