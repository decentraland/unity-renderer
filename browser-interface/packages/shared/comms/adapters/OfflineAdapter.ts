import mitt from 'mitt'
import { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { CommsAdapterEvents, MinimumCommunicationsAdapter, SendHints } from './types'
import { createOpusVoiceHandler } from './voice/opusVoiceHandler'

export class OfflineAdapter implements MinimumCommunicationsAdapter {
  events = mitt<CommsAdapterEvents>()

  constructor() {}
  async createVoiceHandler(): Promise<VoiceHandler> {
    return createOpusVoiceHandler()
  }
  async disconnect(_error?: Error | undefined): Promise<void> {}
  send(_data: Uint8Array, _hints: SendHints): void {}
  async connect(): Promise<void> {}
}
