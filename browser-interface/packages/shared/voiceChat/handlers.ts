import { getCurrentUserProfile } from '../profiles/selectors'
import { Package } from '../comms/interface/types'
import { getPeer } from '../comms/peers'
import * as rfc4 from '@dcl/protocol/out-ts/decentraland/kernel/comms/rfc4/comms.gen'
import { store } from '../store/isolatedStore'
import { getVoiceHandler, shouldPlayVoice } from './selectors'
import { voiceChatLogger } from './context'
import { trackEvent } from 'shared/analytics'

// TODO: create a component to emit opus audio in a specific position that can be used
// by the voicechat and by the SDK
export function processVoiceFragment(message: Package<rfc4.Voice>) {
  const state = store.getState()
  const voiceHandler = getVoiceHandler(state)
  const profile = getCurrentUserProfile(state)

  // use getPeer instead of setupPeer to only reproduce voice messages from
  // known avatars
  const peerTrackingInfo = getPeer(message.address)

  if (
    voiceHandler &&
    profile &&
    peerTrackingInfo &&
    peerTrackingInfo.position &&
    shouldPlayVoice(state, profile, peerTrackingInfo.ethereumAddress) &&
    voiceHandler.playEncodedAudio
  ) {
    voiceHandler
      .playEncodedAudio(peerTrackingInfo.ethereumAddress, peerTrackingInfo.position, message.data)
      .catch((e: any) => {
        trackEvent('error', {
          context: 'voice-chat',
          message: e.message,
          stack: ''
        })
        voiceChatLogger.error('Error playing encoded audio!', e)
      })
  }
}
