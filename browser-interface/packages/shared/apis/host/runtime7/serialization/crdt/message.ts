import { CrdtMessageProtocol } from './crdtMessageProtocol'
import { ByteBuffer } from '../ByteBuffer'
import { CrdtMessageType, CrdtMessage } from './types'
import { PutComponentOperation } from './putComponent'
import { DeleteComponent } from './deleteComponent'
import { DeleteEntity } from './deleteEntity'
import { AppendValueOperation } from './appendValue'

export function readMessage(buf: ByteBuffer): CrdtMessage | null {
  const header = CrdtMessageProtocol.getHeader(buf)
  if (!header) return null

  if (header.type === CrdtMessageType.PUT_COMPONENT) {
    return PutComponentOperation.read(buf)
  } else if (header.type === CrdtMessageType.DELETE_COMPONENT) {
    return DeleteComponent.read(buf)
  } else if (header.type === CrdtMessageType.APPEND_VALUE) {
    return AppendValueOperation.read(buf)
  } else if (header.type === CrdtMessageType.DELETE_ENTITY) {
    return DeleteEntity.read(buf)
  }

  return null
}
