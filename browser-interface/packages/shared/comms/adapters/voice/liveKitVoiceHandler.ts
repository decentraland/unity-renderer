import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { createLogger } from 'lib/logger'
import {
  LocalAudioTrack,
  ParticipantEvent,
  RemoteAudioTrack,
  RemoteParticipant,
  RemoteTrack,
  RemoteTrackPublication,
  Room,
  RoomEvent,
  Track
} from 'livekit-client'
import { getPeer } from 'shared/comms/peers'
import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { store } from 'shared/store/isolatedStore'
import { shouldPlayVoice } from 'shared/voiceChat/selectors'
import { getSpatialParamsFor } from 'shared/voiceChat/utils'
import { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { GlobalAudioStream } from './loopback'
import { DEBUG_VOICE_CHAT } from 'config'

type ParticipantInfo = {
  participant: RemoteParticipant
  tracks: Map<string, ParticipantTrack>
}

type ParticipantTrack = {
  track: LocalAudioTrack | RemoteAudioTrack
  streamNode: MediaStreamAudioSourceNode
  panNode: PannerNode
}

export function createLiveKitVoiceHandler(room: Room, globalAudioStream: GlobalAudioStream): VoiceHandler {
  const logger = createLogger('🎙 LiveKitVoiceCommunicator: ')

  let recordingListener: ((state: boolean) => void) | undefined
  let errorListener: ((message: string) => void) | undefined

  let globalVolume: number = 1.0
  let validInput = false
  let onUserTalkingCallback: (userId: string, talking: boolean) => void = () => {}

  const participantsInfo = new Map<string, ParticipantInfo>()

  function getParticipantInfo(participant: RemoteParticipant): ParticipantInfo {
    let $: ParticipantInfo | undefined = participantsInfo.get(participant.identity)

    if (!$) {
      $ = {
        participant,
        tracks: new Map()
      }
      participantsInfo.set(participant.identity, $)

      participant.on(ParticipantEvent.IsSpeakingChanged, (talking: boolean) => {
        const audioPublication = participant.getTrack(Track.Source.Microphone)
        if (audioPublication && audioPublication.track) {
          const audioTrack = audioPublication.track as RemoteAudioTrack
          onUserTalkingCallback(participant.identity, audioTrack.isMuted ? false : talking)
        }
      })

      if (DEBUG_VOICE_CHAT) logger.info('Adding participant', participant.identity)
    }

    return $
  }

  function setupAudioTrackForRemoteTrack(track: RemoteAudioTrack): ParticipantTrack {
    if (DEBUG_VOICE_CHAT) logger.info('Adding media track', track.sid)
    const audioContext = globalAudioStream.getAudioContext()
    const streamNode = audioContext.createMediaStreamSource(track.mediaStream!)
    const panNode = audioContext.createPanner()

    streamNode.connect(panNode)
    panNode.connect(globalAudioStream.getGainNode())

    panNode.panningModel = 'equalpower'
    panNode.distanceModel = 'inverse'
    panNode.refDistance = 5
    panNode.maxDistance = 10000
    panNode.coneOuterAngle = 360
    panNode.coneInnerAngle = 180
    panNode.coneOuterGain = 0.9
    panNode.rolloffFactor = 1.0

    return {
      panNode,
      streamNode,
      track
    }
  }

  function handleTrackSubscribed(
    track: RemoteTrack,
    _publication: RemoteTrackPublication,
    participant: RemoteParticipant
  ) {
    if (track.kind !== Track.Kind.Audio) {
      return
    }

    const info = getParticipantInfo(participant)
    const trackId = track.sid
    if (trackId && !info.tracks.has(trackId) && track.kind === Track.Kind.Audio && track.mediaStream) {
      info.tracks.set(trackId, setupAudioTrackForRemoteTrack(track as RemoteAudioTrack))
    }
  }

  function handleTrackUnsubscribed(
    remoteTrack: RemoteTrack,
    _publication: RemoteTrackPublication,
    participant: RemoteParticipant
  ) {
    if (remoteTrack.kind !== Track.Kind.Audio) {
      return
    }

    const info = getParticipantInfo(participant)

    for (const [trackId, track] of info.tracks) {
      if (trackId === remoteTrack.sid) {
        track.panNode.disconnect()
        track.streamNode.disconnect()
        break
      }
    }
  }

  function handleMediaDevicesError() {
    if (errorListener) errorListener('Media Device Error')
  }

  room
    .on(RoomEvent.TrackSubscribed, handleTrackSubscribed)
    .on(RoomEvent.TrackUnsubscribed, handleTrackUnsubscribed)
    .on(RoomEvent.MediaDevicesError, handleMediaDevicesError)

  logger.log('initialized')

  return {
    setRecording(recording) {
      room.localParticipant
        .setMicrophoneEnabled(recording)
        .then(() => {
          if (recordingListener) {
            recordingListener(recording)
          }
        })
        .catch((err) => logger.error('Error: ', err, ', recording=', recording))
    },
    onUserTalking(cb) {
      onUserTalkingCallback = cb
      try {
        if (!room.canPlaybackAudio) {
          room.startAudio().catch(logger.error)
        }

        globalAudioStream.play()
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
    reportPosition(position: rfc4.Position) {
      const spatialParams = getSpatialParamsFor(position)
      const audioContext = globalAudioStream.getAudioContext()
      const listener = audioContext.listener

      if (listener.positionX) {
        listener.positionX.setValueAtTime(spatialParams.position[0], audioContext.currentTime)
        listener.positionY.setValueAtTime(spatialParams.position[1], audioContext.currentTime)
        listener.positionZ.setValueAtTime(spatialParams.position[2], audioContext.currentTime)
      } else {
        listener.setPosition(spatialParams.position[0], spatialParams.position[1], spatialParams.position[2])
      }

      if (listener.forwardX) {
        listener.forwardX.setValueAtTime(spatialParams.orientation[0], audioContext.currentTime)
        listener.forwardY.setValueAtTime(spatialParams.orientation[1], audioContext.currentTime)
        listener.forwardZ.setValueAtTime(spatialParams.orientation[2], audioContext.currentTime)
        listener.upX.setValueAtTime(0, audioContext.currentTime)
        listener.upY.setValueAtTime(1, audioContext.currentTime)
        listener.upZ.setValueAtTime(0, audioContext.currentTime)
      } else {
        listener.setOrientation(
          spatialParams.orientation[0],
          spatialParams.orientation[1],
          spatialParams.orientation[2],
          0,
          1,
          0
        )
      }

      for (const [_, participant] of room.participants) {
        const address = participant.identity
        const peer = getPeer(address)
        const participantInfo = participantsInfo.get(address)

        const state = store.getState()
        const profile = getCurrentUserProfile(state)
        if (profile) {
          const muted = !shouldPlayVoice(state, profile, address)
          const audioPublication = participant.getTrack(Track.Source.Microphone)
          if (audioPublication && audioPublication.track) {
            const audioTrack = audioPublication.track as RemoteAudioTrack
            audioTrack.setMuted(muted)
          }
        }

        if (participantInfo) {
          const spatialParams = peer?.position || position
          for (const [_, { panNode }] of participantInfo.tracks) {
            if (panNode.positionX) {
              panNode.positionX.setValueAtTime(spatialParams.positionX, audioContext.currentTime)
              panNode.positionY.setValueAtTime(spatialParams.positionY, audioContext.currentTime)
              panNode.positionZ.setValueAtTime(spatialParams.positionZ, audioContext.currentTime)
            } else {
              panNode.setPosition(spatialParams.positionX, spatialParams.positionY, spatialParams.positionZ)
            }

            if (panNode.orientationX) {
              panNode.orientationX.setValueAtTime(0, audioContext.currentTime)
              panNode.orientationY.setValueAtTime(0, audioContext.currentTime)
              panNode.orientationZ.setValueAtTime(1, audioContext.currentTime)
            } else {
              panNode.setOrientation(0, 0, 1)
            }
          }
        }
      }
    },
    setVolume: function (volume) {
      globalVolume = volume
      globalAudioStream.setGainVolume(volume)
    },
    setMute: (mute) => {
      globalAudioStream.setGainVolume(mute ? 0 : globalVolume)
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
    async destroy() {}
  }
}
