import mitt from 'mitt'
import { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { CommsAdapterEvents, MinimumCommunicationsAdapter, SendHints } from './types'

export class OfflineAdapter implements MinimumCommunicationsAdapter {
  events = mitt<CommsAdapterEvents>()

  constructor() {}
  async getVoiceHandler(): Promise<VoiceHandler | undefined> {
    return undefined
  }
  async disconnect(_error?: Error | undefined): Promise<void> {}
  send(_data: Uint8Array, _hints: SendHints): void {}
  async connect(): Promise<void> {}
  async getParticipants() {
    return []
  }
}
