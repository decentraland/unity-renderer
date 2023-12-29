import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { createLogger } from 'lib/logger'
import {
  ParticipantEvent,
  RemoteParticipant,
  RemoteTrack,
  RemoteTrackPublication,
  Room,
  RoomEvent,
  Track
} from 'livekit-client'
import { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { loopbackAudioElement } from './loopback'

export function createLiveKitVoiceHandler(room: Room): VoiceHandler {
  const logger = createLogger('🎙 LiveKitVoiceCommunicator: ')

  // const globalAudioStream = await getGlobalAudioStream()

  let recordingListener: ((state: boolean) => void) | undefined
  let errorListener: ((message: string) => void) | undefined
  let onUserTalkingCallback: ((userId: string, talking: boolean) => void) | undefined = undefined

  // let globalVolume: number = 1.0
  let validInput = false

  function handleTrackSubscribed(
    track: RemoteTrack,
    _publication: RemoteTrackPublication,
    participant: RemoteParticipant
  ) {
    if (track.kind !== Track.Kind.Audio) {
      return
    }

    participant.on(ParticipantEvent.IsSpeakingChanged, (talking: boolean) => {
      if (onUserTalkingCallback) {
        onUserTalkingCallback(participant.identity, talking)
      }
    })

    const element = track.attach()
    loopbackAudioElement().appendChild(element)
  }

  function handleTrackUnsubscribed(
    track: RemoteTrack,
    _publication: RemoteTrackPublication,
    _participant: RemoteParticipant
  ) {
    if (track.kind !== Track.Kind.Audio) {
      return
    }

    track.detach()
  }

  room
    .on(RoomEvent.TrackSubscribed, handleTrackSubscribed)
    .on(RoomEvent.TrackUnsubscribed, handleTrackUnsubscribed)
    .on(RoomEvent.MediaDevicesError, () => {
      if (errorListener) {
        errorListener('Media Device Error')
      }
    })

  logger.log(`initialized ${room.name}`)

  return {
    async setRecording(recording) {
      try {
        await room.localParticipant.setMicrophoneEnabled(recording)
        if (recordingListener) {
          recordingListener(recording)
        }
      } catch (err) {
        logger.error('Error: ', err, ', recording=', recording)
        if (recordingListener) {
          recordingListener(false)
        }
      }
    },
    onUserTalking(cb) {
      onUserTalkingCallback = cb
      try {
        if (!room.canPlaybackAudio) {
          room.startAudio().catch(logger.error)
        }

        // globalAudioStream.play()
      } catch (err: any) {
        logger.error(err)
      }
    },
    onRecording(cb) {
      recordingListener = cb
    },
    onError(cb) {
      errorListener = cb
    },
    reportPosition(_position: rfc4.Position) {},
    setVolume: function (volume) {
      // TODO
      // globalVolume = volume
      // globalAudioStream.setGainVolume(volume)
    },
    setMute: (mute) => {
      // TODO
      // globalAudioStream.setGainVolume(mute ? 0 : globalVolume)
    },
    setInputStream: async (localStream) => {
      try {
        await room.switchActiveDevice('audioinput', localStream.id)
        validInput = true
      } catch (e) {
        validInput = false
        if (errorListener) errorListener('setInputStream catch' + JSON.stringify(e))
      }
    },
    hasInput: () => {
      return validInput
    },
    async destroy() {
      onUserTalkingCallback = undefined
      recordingListener = undefined
      errorListener = undefined
    }
  }
}
