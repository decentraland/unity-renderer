import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { Package } from 'shared/comms/interface/types'
import { getPeer } from 'shared/comms/peers'
import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { store } from 'shared/store/isolatedStore'
import { getVoiceHandler, shouldPlayVoice } from './selectors'
import { voiceChatLogger } from './logger'
import { trackEvent } from 'shared/analytics/trackEvent'

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
