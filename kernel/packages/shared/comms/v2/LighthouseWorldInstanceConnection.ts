import { WorldInstanceConnection } from '../interface/index'
import { Stats } from '../debug'
import { Package, BusMessage, ChatMessage, ProfileVersion, UserInformation } from '../interface/types'
import { Position, positionHash } from '../interface/utils'
import { Peer } from 'decentraland-katalyst-peer'
import { createLogger } from 'shared/logger'
import { PeerMessageTypes } from 'decentraland-katalyst-peer/src/messageTypes'

const NOOP = () => {
  // do nothing
}

const logger = createLogger('Lighthouse: ')

export class LighthouseWorldInstanceConnection implements WorldInstanceConnection {
  stats: Stats | null = null

  sceneMessageHandler: (alias: string, data: Package<BusMessage>) => void = NOOP
  chatHandler: (alias: string, data: Package<ChatMessage>) => void = NOOP
  profileHandler: (alias: string, identity: string, data: Package<ProfileVersion>) => void = NOOP
  positionHandler: (alias: string, data: Package<Position>) => void = NOOP

  isAuthenticated: boolean = true // TODO - remove this

  ping: number = -1

  constructor(private peer: Peer) {
    logger.info(`connected peer as `, peer.nickname)
    peer.callback = (sender, room, payload) => {
      switch (payload.type) {
        case 'profile': {
          this.profileHandler(sender, payload.data.user, payload)
          break
        }
        case 'chat': {
          this.chatHandler(sender, payload)
          break
        }
        case 'position': {
          this.positionHandler(sender, payload)
          break
        }
        default: {
          logger.warn(`message with unknown type received ${payload.type}`)
          break
        }
      }
    }
  }

  printDebugInformation() {
    // TODO - implement this - moliva - 20/12/2019
  }

  close() {
    const rooms = this.peer.currentRooms
    return Promise.all(
      rooms.map(room => this.peer.leaveRoom(room.id).catch(e => logger.trace(`error while leaving room ${room.id}`, e)))
    )
  }

  async sendInitialMessage(userInfo: Partial<UserInformation>) {
    const topic = userInfo.userId!

    await this.peer.sendMessage(topic, {
      type: 'profile',
      time: Date.now(),
      data: { version: userInfo.version, user: userInfo.userId }
    })
  }

  async sendProfileMessage(currentPosition: Position, userInfo: UserInformation) {
    const topic = positionHash(currentPosition)

    await this.peer.sendMessage(
      topic,
      {
        type: 'profile',
        time: Date.now(),
        data: { version: userInfo.version, user: userInfo.userId }
      },
      PeerMessageTypes.unreliable
    )
  }

  async sendPositionMessage(p: Position) {
    const topic = positionHash(p)

    await this.peer.sendMessage(
      topic,
      {
        type: 'position',
        time: Date.now(),
        data: [p[0], p[1], p[2], p[3], p[4], p[5], p[6]]
      },
      PeerMessageTypes.unreliable
    )
  }

  async sendParcelUpdateMessage(currentPosition: Position, p: Position) {
    const topic = positionHash(currentPosition)

    await this.peer.sendMessage(
      topic,
      {
        type: 'position',
        time: Date.now(),
        data: [p[0], p[1], p[2], p[3], p[4], p[5], p[6]]
      },
      PeerMessageTypes.unreliable
    )
  }

  async sendParcelSceneCommsMessage(sceneId: string, message: string) {
    const topic = sceneId

    await this.peer.sendMessage(topic, {
      type: 'chat',
      time: Date.now(),
      data: { id: sceneId, text: message }
    })
  }

  async sendChatMessage(currentPosition: Position, messageId: string, text: string) {
    const topic = positionHash(currentPosition)

    await this.peer.sendMessage(topic, {
      type: 'chat',
      time: Date.now(),
      data: { id: messageId, text }
    })
  }

  async updateSubscriptions(rooms: string[]) {
    const currentRooms = this.peer.currentRooms
    const joining = rooms.map(room => {
      if (!currentRooms.some(current => current.id === room)) {
        return this.peer.joinRoom(room)
      } else {
        return Promise.resolve()
      }
    })
    const leaving = currentRooms.map(current => {
      if (!rooms.some(room => current.id === room)) {
        return this.peer.leaveRoom(current.id)
      } else {
        return Promise.resolve()
      }
    })
    return Promise.all([...joining, ...leaving]).then(NOOP)
  }
}
