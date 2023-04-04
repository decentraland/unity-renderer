import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { isChrome } from 'lib/browser/isChrome'
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
import Html from './Html'
import { startLoopback } from './loopback'
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

export const createLiveKitVoiceHandler = async (room: Room): Promise<VoiceHandler> => {
  const logger = createLogger('🎙 LiveKitVoiceCommunicator: ')

  const parentElement = Html.loopbackAudioElement()

  let recordingListener: ((state: boolean) => void) | undefined
  let errorListener: ((message: string) => void) | undefined
  let globalVolume: number = 1.0
  let globalMuted: boolean = false
  let validInput = false
  let onUserTalkingCallback: (userId: string, talking: boolean) => void = () => {}

  const participantsInfo = new Map<string, ParticipantInfo>()
  const audioContext = new AudioContext()
  const destination = audioContext.createMediaStreamDestination()
  const destinationStream = isChrome() ? await startLoopback(destination.stream) : destination.stream

  const gainNode = audioContext.createGain()
  gainNode.connect(destination)
  gainNode.gain.value = 1

  if (parentElement) {
    parentElement.srcObject = destinationStream
  }

  function getGlobalVolume(): number {
    return globalMuted ? 0.0 : globalVolume
  }

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

  // this function sets up the local tracking of the remote participant
  // and refreshes the audio nodes based on the most up-to-date audio tracks
  function setupTracksForParticipant(participant: RemoteParticipant) {
    const info = getParticipantInfo(participant)

    // first remove extra tracks
    for (const [trackId, track] of info.tracks) {
      if (!participant.audioTracks.has(trackId)) {
        disconnectParticipantTrack(track)
        info.tracks.delete(trackId)
      }
    }

    // and subscribe to new ones
    for (const [, track] of participant.audioTracks) {
      if (track.audioTrack?.kind === Track.Kind.Audio) {
        subscribeParticipantTrack(participant, track.audioTrack as any)
      }
    }
  }

  function subscribeParticipantTrack(participant: RemoteParticipant, track: RemoteAudioTrack) {
    const info = getParticipantInfo(participant)
    const trackId = track.sid
    if (trackId && !info.tracks.has(trackId) && track.kind === Track.Kind.Audio && track.mediaStream) {
      info.tracks.set(trackId, setupAudioTrackForRemoteTrack(track))
    }
  }

  function setupAudioTrackForRemoteTrack(track: RemoteAudioTrack): ParticipantTrack {
    if (DEBUG_VOICE_CHAT) logger.info('Adding media track', track.sid)
    const streamNode = audioContext.createMediaStreamSource(track.mediaStream!)
    const panNode = audioContext.createPanner()

    streamNode.connect(panNode)
    panNode.connect(gainNode)

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

  function disconnectParticipantTrack(participantTrack: ParticipantTrack) {
    if (DEBUG_VOICE_CHAT) logger.info('Disconnecting media track', participantTrack.track.sid)
    participantTrack.panNode.disconnect()
    participantTrack.streamNode.disconnect()
    participantTrack.track.stop()
    participantTrack.track.detach()
  }

  function handleTrackSubscribed(
    track: RemoteTrack,
    publication: RemoteTrackPublication,
    participant: RemoteParticipant
  ) {
    if (track.kind === Track.Kind.Audio) {
      subscribeParticipantTrack(participant, track as RemoteAudioTrack)
    }
  }
  function handleTrackUnsubscribed(
    track: RemoteTrack,
    publication: RemoteTrackPublication,
    participant: RemoteParticipant
  ) {
    setupTracksForParticipant(participant)
  }

  async function handleDisconnect() {
    if (DEBUG_VOICE_CHAT) logger.log('Handler Disconnect')

    room
      .off(RoomEvent.Disconnected, handleDisconnect)
      .off(RoomEvent.TrackSubscribed, handleTrackSubscribed)
      .off(RoomEvent.TrackUnsubscribed, handleTrackUnsubscribed)
      .off(RoomEvent.TrackPublished, handleTrackPublished)
      .off(RoomEvent.TrackUnpublished, handleTrackUnpublished)
      .off(RoomEvent.MediaDevicesError, handleMediaDevicesError)
      .off(RoomEvent.ParticipantConnected, addParticipant)
      .off(RoomEvent.ParticipantDisconnected, removeParticipant)

    for (const [userId] of participantsInfo) {
      removeParticipantById(userId)
    }
    participantsInfo.clear()

    gainNode.disconnect()
    try {
      await audioContext.close()
    } catch (err) {}
  }

  function handleMediaDevicesError() {
    if (errorListener) errorListener('Media Device Error')
  }

  function addParticipant(participant: RemoteParticipant) {
    setupTracksForParticipant(participant)
  }

  function removeParticipantById(userId: string) {
    // remove tracks from all attached elements
    const participantInfo = participantsInfo.get(userId)
    if (participantInfo) {
      if (DEBUG_VOICE_CHAT) logger.info('Removing participant', userId)
      for (const [trackId, participantTrack] of participantInfo.tracks) {
        try {
          disconnectParticipantTrack(participantTrack)
          participantInfo.tracks.delete(trackId)
        } catch (err: any) {
          logger.error(err)
        }
      }
      participantsInfo.delete(userId)
    }
  }

  function removeParticipant(participant: RemoteParticipant) {
    removeParticipantById(participant.identity)
  }

  function handleTrackPublished(trackPublication: RemoteTrackPublication, remoteParticipant: RemoteParticipant) {
    if (trackPublication.audioTrack) {
      subscribeParticipantTrack(remoteParticipant, trackPublication.audioTrack as RemoteAudioTrack)
    }
  }

  function handleTrackUnpublished(trackPublication: RemoteTrackPublication, remoteParticipant: RemoteParticipant) {
    setupTracksForParticipant(remoteParticipant)
  }

  function reconnectAllParticipants() {
    // remove all participants
    for (const [identity] of participantsInfo) {
      removeParticipantById(identity)
    }

    // add existing participants
    for (const [_, participant] of room.participants) {
      addParticipant(participant)
    }
  }

  function updateParticipantsVolume() {
    gainNode.gain.value = getGlobalVolume()
  }

  room
    .on(RoomEvent.Disconnected, handleDisconnect)
    .on(RoomEvent.TrackSubscribed, handleTrackSubscribed)
    .on(RoomEvent.TrackUnsubscribed, handleTrackUnsubscribed)
    .on(RoomEvent.MediaDevicesError, handleMediaDevicesError)
    .on(RoomEvent.ParticipantConnected, addParticipant)
    .on(RoomEvent.ParticipantDisconnected, removeParticipant)
    .on(RoomEvent.Reconnected, function (..._args) {
      reconnectAllParticipants()
    })

  if (audioContext.state !== 'running') await audioContext.resume()

  logger.log('initialized')

  reconnectAllParticipants()

  return {
    setRecording(recording) {
      room.localParticipant
        .setMicrophoneEnabled(recording)
        .then(() => {
          if (recordingListener) recordingListener(recording)
        })
        .catch((err) => logger.error('Error: ', err, ', recording=', recording))
    },
    onUserTalking(cb) {
      onUserTalkingCallback = cb
      try {
        if (!room.canPlaybackAudio) {
          room.startAudio().catch(logger.error)
        }

        parentElement?.play().catch(logger.log)
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
      updateParticipantsVolume()
    },
    setMute: (mute) => {
      globalMuted = mute
      updateParticipantsVolume()
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
      await Promise.allSettled([
        room.localParticipant.unpublishTracks(
          Array.from(room.localParticipant.audioTracks.values())
            .map(($) => $.audioTrack!)
            .filter(Boolean)
        ),
        handleDisconnect()
      ])
    }
  }
}
