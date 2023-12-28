import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { createLogger } from 'lib/logger'
import {
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

type ParticipantInfo = {
  tracks: Map<string, ParticipantTrack>
}

type ParticipantTrack = {
  streamNode: MediaStreamAudioSourceNode
  panNode: PannerNode
}

export function createLiveKitVoiceHandler(room: Room, globalAudioStream: GlobalAudioStream): VoiceHandler {
  const logger = createLogger('🎙 LiveKitVoiceCommunicator: ')

  let recordingListener: ((state: boolean) => void) | undefined
  let errorListener: ((message: string) => void) | undefined
  let onUserTalkingCallback: ((userId: string, talking: boolean) => void) | undefined = undefined

  let globalVolume: number = 1.0
  let validInput = false
  const participantsInfo = new Map<string, ParticipantInfo>()

  function handleTrackSubscribed(
    track: RemoteTrack,
    _publication: RemoteTrackPublication,
    participant: RemoteParticipant
  ) {
    if (track.kind !== Track.Kind.Audio || !track.sid) {
      return
    }

    participant.on(ParticipantEvent.IsSpeakingChanged, (talking: boolean) => {
      if (onUserTalkingCallback) {
        onUserTalkingCallback(participant.identity, talking)
      }
    })

    let info = participantsInfo.get(participant.identity)
    if (!info) {
      info = { tracks: new Map<string, ParticipantTrack>() }
      participantsInfo.set(participant.identity, info)
    }

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

    info.tracks.set(track.sid, { panNode, streamNode })
  }

  function handleTrackUnsubscribed(
    remoteTrack: RemoteTrack,
    _publication: RemoteTrackPublication,
    participant: RemoteParticipant
  ) {
    if (remoteTrack.kind !== Track.Kind.Audio || !remoteTrack.sid) {
      return
    }

    const info = participantsInfo.get(participant.identity)
    if (!info) {
      return
    }

    const track = info.tracks.get(remoteTrack.sid)
    if (track) {
      track.panNode.disconnect()
      track.streamNode.disconnect()
    }

    info.tracks.delete(remoteTrack.sid)
  }

  function handleParticipantDisconnected(p: RemoteParticipant) {
    const info = participantsInfo.get(p.identity)
    if (!info) {
      return
    }

    for (const track of info.tracks.values()) {
      track.panNode.disconnect()
      track.streamNode.disconnect()
    }

    participantsInfo.delete(p.identity)
  }

  room
    .on(RoomEvent.TrackSubscribed, handleTrackSubscribed)
    .on(RoomEvent.TrackUnsubscribed, handleTrackUnsubscribed)
    .on(RoomEvent.MediaDevicesError, () => {
      if (errorListener) {
        errorListener('Media Device Error')
      }
    })
    .on(RoomEvent.ParticipantDisconnected, handleParticipantDisconnected)

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
          console.log('START AUDIO')
          room.startAudio().catch(logger.error)
        } else {
          console.log('ELSE')
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

      for (const participant of room.participants.values()) {
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
          for (const { panNode } of participantInfo.tracks.values()) {
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
    async destroy() {
      onUserTalkingCallback = undefined
      recordingListener = undefined
      errorListener = undefined
    }
  }
}
