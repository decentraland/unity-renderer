import { WebWorkerTransport } from 'decentraland-rpc'
import { log, error as logError } from 'engine/logger'
import { Adapter } from './src/adapter'
import {
  Context,
  Position,
  ServerId,
  ClientPositionData,
  UserProfile,
  CommParcelDataPacked,
  init,
  onProfileUpdate,
  onPositionUpdate,
  sendPublicChatMessage,
  setEphemeralKey,
  onCommDataLoaded,
  onInfoRequested
} from './src/client'
import { setInterval } from 'timers'
import { UserData as EphemeralKey } from 'ephemeralkey'
import { ETHEREUM_NETWORK } from 'config'

let gContext: Context | null = null

type InitOptions = {
  peerId: string
  profile: UserProfile
  key: EphemeralKey
  network: ETHEREUM_NETWORK
}

type PositionMessage = {
  position: Position
}

type ProfileMessage = {
  profile: UserProfile
}

type ChatMessage = {
  id: string
  text: string
}

type KeyMessage = {
  key: EphemeralKey
}

type ParcelDataMessage = {
  data: CommParcelDataPacked[]
}

type SdpMessage = {
  serverId: ServerId
  sdp: string
}

type RequestInfoMessage = {
  data: ClientPositionData[]
}

type WebRtcConnectionClosedMessage = {
  serverId: ServerId
}

const connector = new Adapter(WebWorkerTransport(self as any))

{
  connector.on('init', ({ peerId, profile, network, key }: InitOptions) => {
    try {
      log('CommWorker: initialize')
      gContext = init(peerId, connector, profile, network, key)
    } catch (err) {
      logError('CommWorker: onPositionUpdate', err)
    }
  })

  connector.on('onProfileUpdate', ({ profile }: ProfileMessage) => {
    if (gContext) {
      try {
        onProfileUpdate(gContext, profile)
      } catch (err) {
        logError('CommWorker: onProfileUpdate', err)
      }
    }
  })

  connector.on('onPositionUpdate', ({ position }: PositionMessage) => {
    if (gContext) {
      try {
        onPositionUpdate(gContext, position)
      } catch (err) {
        logError('CommWorker: onPositionUpdate', err)
      }
    }
  })

  connector.on('sendPublicChatMessage', async ({ id, text }: ChatMessage) => {
    if (gContext) {
      try {
        sendPublicChatMessage(gContext, id, text)
      } catch (err) {
        logError('CommWorker: sendPublicChatMessage', err)
      }
    }
  })

  connector.on('onEphemeralKeyGenerated', ({ key }: KeyMessage) => {
    if (gContext) {
      try {
        setEphemeralKey(gContext, key)
      } catch (err) {
        logError('CommWorker: onEphemeralKeyGenerated', err)
      }
    }
  })

  connector.on('onParcelDataLoaded', ({ data }: ParcelDataMessage) => {
    if (gContext) {
      try {
        onCommDataLoaded(gContext, data)
      } catch (err) {
        logError('CommWorker: onParcelDataLoaded', err)
      }
    }
  })

  connector.on('onIceCandidate', ({ sdp, serverId }: SdpMessage) => {
    if (gContext) {
      try {
        gContext.clientApi.onIceCandidate(serverId, sdp)
      } catch (err) {
        logError('CommWorker: onIceCandidate', err)
      }
    }
  })

  connector.on('onOfferGenerated', ({ sdp, serverId }: SdpMessage) => {
    if (gContext) {
      try {
        gContext.clientApi.onOfferGenerated(serverId, sdp)
      } catch (err) {
        logError('CommWorker: onOfferGenerated', err)
      }
    }
  })

  connector.on('onAnswerGenerated', ({ sdp, serverId }: SdpMessage) => {
    if (gContext) {
      try {
        gContext.clientApi.onAnswerGenerated(serverId, sdp)
      } catch (err) {
        logError('CommWorker: onAnswerGenerated', err)
      }
    }
  })

  connector.on('onWebRtcConnectionClosed', ({ serverId }: WebRtcConnectionClosedMessage) => {
    if (gContext) {
      try {
        gContext.clientApi.onWebRtcConnectionClosed(serverId)
      } catch (err) {
        logError('CommWorker: onWebRtcConnectionClosed', err)
      }
    }
  })

  connector.on('requestInfo', ({ data }: RequestInfoMessage) => {
    if (gContext) {
      try {
        onInfoRequested(gContext, data)
      } catch (err) {
        logError('CommWorker: requestInfo', err)
      }
    }
  })
}
