import { ProtocolState } from './types'
import { Context } from '../comms'

export function getCommsContext(state: { protocol: ProtocolState }): Context | undefined {
  return state.protocol.context
}
