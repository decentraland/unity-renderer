import { ByteBuffer } from '../ByteBuffer'
import { CrdtMessageType, CrdtMessageHeader, CRDT_MESSAGE_HEADER_LENGTH } from './types'

/**
 * @public
 */
export namespace CrdtMessageProtocol {
  /**
   * Validate if the message incoming is completed
   * @param buf - ByteBuffer
   */
  export function validate(buf: ByteBuffer) {
    const rem = buf.remainingBytes()
    if (rem < CRDT_MESSAGE_HEADER_LENGTH) {
      return false
    }

    const messageLength = buf.getUint32(buf.currentReadOffset())
    if (rem < messageLength) {
      return false
    }

    return true
  }

  /**
   * Get the current header, consuming the bytes involved.
   * @param buf - ByteBuffer
   * @returns header or null if there is no validated message
   */
  export function readHeader(buf: ByteBuffer): CrdtMessageHeader | null {
    if (!validate(buf)) {
      return null
    }

    return {
      length: buf.readUint32(),
      type: buf.readUint32() as CrdtMessageType
    }
  }

  /**
   * Get the current header, without consuming the bytes involved.
   * @param buf - ByteBuffer
   * @returns header or null if there is no validated message
   */
  export function getHeader(buf: ByteBuffer): CrdtMessageHeader | null {
    if (!validate(buf)) {
      return null
    }

    const currentOffset = buf.currentReadOffset()
    return {
      length: buf.getUint32(currentOffset),
      type: buf.getUint32(currentOffset + 4) as CrdtMessageType
    }
  }

  /**
   * Consume the incoming message without processing it.
   * @param buf - ByteBuffer
   * @returns true in case of success or false if there is no valid message.
   */
  export function consumeMessage(buf: ByteBuffer): boolean {
    const header = getHeader(buf)
    if (!header) {
      return false
    }

    buf.incrementReadOffset(header.length)
    return true
  }
}
