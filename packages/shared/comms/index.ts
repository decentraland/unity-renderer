import 'webrtc-adapter'
import { TransportBasedServer } from 'decentraland-rpc/lib/host/TransportBasedServer'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { WebWorkerTransport } from 'decentraland-rpc/lib/common/transports/WebWorker'

import { parcelLimits, ETHEREUM_NETWORK } from 'config'
import { getUserProfile } from './profile'
import {
  getUser,
  setLocalProfile,
  ensureAvatar,
  removeById,
  receiveUserData,
  receiveUserPose,
  getCurrentUser,
  UserInformation,
  getCurrentPeer,
  localProfileUUID,
  Pose
} from '../../dcl/comms/peers'
import { saveToLocalStorage } from 'atomicHelpers/localStorage'
import { getEphemeralKeys } from 'shared/ethereum/EthereumService'
import { chatObservable, ChatEvent } from './chat'
import { positionObserver } from 'shared/world/positionThings'
import { PositionMessage } from './worldcomm_pb'
import { log, error as logError } from 'engine/logger'

const loaderWorkerRaw = require('raw-loader!../../../static/systems/comms.system.js')
const loaderWorkerBLOB = new Blob([loaderWorkerRaw])
const loaderWorkerUrl = URL.createObjectURL(loaderWorkerBLOB)
const worker: Worker = new Worker(loaderWorkerUrl)

class CommunicationServer extends TransportBasedServer {
  public buffer = []
  public rtcConn: RTCPeerConnection | null = null
  public serverId: number = -1
  public requestInfoInterval: any = null

  constructor(transport: ScriptingTransport) {
    super(transport)

    this.on('infoCollected', info => {
      for (let peerInfo of info.peers) {
        const { peerId } = peerInfo

        if (peerInfo.position) {
          receiveUserPose(peerId, { v: peerInfo.position as Pose })
          if (peerInfo.profile) {
            receiveUserData(peerId, peerInfo.profile)
          }
        } else {
          removeById(peerId)
        }
      }
    })

    this.on('onPublicChatReceived', ({ peerId, msgId, text }) => {
      const user = getUser(peerId)

      if (user) {
        const { displayName } = user
        const entry = {
          id: msgId,
          sender: displayName,
          message: text,
          isCommand: false
        }
        chatObservable.notifyObservers({ type: ChatEvent.MESSAGE_RECEIVED, messageEntry: entry })
      }
    })

    this.on('onNewEphemeralKeyRequired', async ({ network }) => {
      const key = await getEphemeralKeys(network, true)
      this.notify('onEphemeralKeyGenerated', { key })
    })

    this.requestInfoInterval = setInterval(() => {
      this.requestInfo()
    }, 100)

    this.on('openWebRtcRequest', async ({ serverId }) => {
      if (this.rtcConn) {
        this.closeWebRtcConnection()
      }
      this.serverId = serverId
      this.rtcConn = new RTCPeerConnection({
        iceServers: [
          {
            urls: 'stun:stun.l.google.com:19302'
          }
        ]
      })

      this.rtcConn.onnegotiationneeded = async () => {
        // NOTE pion doesn't actually support renegotiation yet
        this.closeWebRtcConnection()
      }

      this.rtcConn.onsignalingstatechange = e => log(`signaling state: ${this.rtcConn.signalingState}`)
      this.rtcConn.oniceconnectionstatechange = e => log(`ice connection state: ${this.rtcConn.iceConnectionState}`)

      this.rtcConn.onicecandidate = async event => {
        // NOTE: null candidate means the end of getting candidates
        if (event.candidate === null) {
          const sdp = this.rtcConn.localDescription.sdp
          this.notify('onAnswerGenerated', { sdp: sdp, serverId: this.serverId })
        } else {
          this.notify('onIceCandidate', { sdp: event.candidate.candidate, serverId: this.serverId })
        }
      }

      this.rtcConn.ondatachannel = e => {
        let dc = e.channel

        log('New DataChannel ' + dc.label)
        dc.onclose = () => log('dc has closed')
        dc.onopen = () => log('dc has opened')
        dc.onmessage = e => {
          const data = e.data

          let message
          try {
            message = PositionMessage.deserializeBinary(data)
          } catch (e) {
            logError('cannot deserialize position message (webrtc)', e, data)
          }

          const parcelSize = parcelLimits.parcelSize
          const position = [
            message.getPositionX() * parcelSize,
            message.getPositionY(),
            message.getPositionZ() * parcelSize,
            message.getRotationX(),
            message.getRotationY(),
            message.getRotationZ(),
            message.getRotationW()
          ]

          const msg = { position, alias: message.getAlias(), time: message.getTime(), serverId: this.serverId }
          this.buffer.push(msg)
        }
      }
    })

    this.on('offerReceived', async ({ sdp, serverId }) => {
      log('offer received')
      try {
        await this.rtcConn.setRemoteDescription(new RTCSessionDescription({ type: 'offer', sdp: sdp }))
        const desc = await this.rtcConn.createAnswer()
        await this.rtcConn.setLocalDescription(desc)
      } catch (err) {
        logError(err)
      }
    })

    // NOTE: this actually never happens, because we are not sending an offer, however, I added to the protocol
    // to support other implemenentations, specially if we have to ditch pion
    this.on('answerReceived', async ({ sdp, serverId }) => {
      if (this.serverId === serverId) {
        try {
          await this.rtcConn.setRemoteDescription(sdp)
        } catch (err) {
          logError(err)
        }
      }
    })

    this.on('iceCandidateReceived', async ({ sdp, serverId }) => {
      if (this.serverId === serverId) {
        try {
          await this.rtcConn.addIceCandidate(sdp)
        } catch (err) {
          logError(err)
        }
      }
    })

    this.on('closeWebRtcConnection', ({ serverId }) => {
      if (this.serverId === serverId) {
        this.closeWebRtcConnection()
      }
    })
  }

  closeWebRtcConnection() {
    const serverId = this.serverId
    this.serverId = -1
    this.rtcConn.close()
    this.rtcConn.onsignalingstatechange = null
    this.rtcConn.oniceconnectionstatechange = null
    this.rtcConn.onicecandidate = null
    this.rtcConn.ondatachannel = null
    this.rtcConn = null
    this.notify('onWebRtcConnectionClosed', { serverId })
  }

  enable() {
    super.enable()
  }

  init(peerId: string, network: string, key, user: UserInformation) {
    this.notify('init', {
      peerId,
      profile: {
        displayName: user.displayName,
        publicKey: user.publicKey,
        avatarType: user.avatarType
      },
      network,
      key
    })
  }

  sendPublicChatMessage(id: string, text: string) {
    this.notify('sendPublicChatMessage', { id, text })
  }

  onParcelDataLoaded(data) {
    this.notify('onParcelDataLoaded', { data })
  }

  onProfileUpdate(user: UserInformation) {
    this.notify('onProfileUpdate', {
      profile: {
        displayName: user.displayName,
        publicKey: user.publicKey,
        avatarType: user.avatarType
      }
    })
  }

  onPositionUpdate(
    obj: Readonly<{
      position: BABYLON.Vector3
      rotation: BABYLON.Vector3
      quaternion: BABYLON.Quaternion
    }>
  ) {
    this.notify('onPositionUpdate', {
      position: [
        obj.position.x,
        obj.position.y,
        obj.position.z,
        obj.quaternion.x,
        obj.quaternion.y,
        obj.quaternion.z,
        obj.quaternion.w
      ]
    })
  }

  requestInfo() {
    this.notify('requestInfo', { data: this.buffer })
    this.buffer = []
  }
}

const server = new CommunicationServer(WebWorkerTransport(worker))

export function sendPublicChatMessage(id: string, text: string): void {
  server.sendPublicChatMessage(id, text)
}

export function setCommData(data) {
  server.onParcelDataLoaded(data)
}

/**
 * This function persists the current user data.
 */
export function persistCurrentUser(changes: Partial<UserInformation>): Readonly<UserInformation> {
  const peer = getCurrentPeer()

  Object.assign(peer.user, changes)

  saveToLocalStorage('dcl-profile', peer.user)

  receiveUserData(localProfileUUID, peer.user)

  const user = peer.user
  if (user) {
    server.onProfileUpdate(user)
  }

  return peer.user
}

export async function connect(ethAddress: string, network?: ETHEREUM_NETWORK) {
  const peerId = ethAddress

  setLocalProfile(peerId, {
    ...getUserProfile(),
    publicKey: ethAddress
  })

  ensureAvatar(peerId)

  const user = getCurrentUser()
  if (user) {
    let key = network ? await getEphemeralKeys(network) : null
    server.init(peerId, network, key, user)
  }

  positionObserver.add(p => server.onPositionUpdate(p))
}
