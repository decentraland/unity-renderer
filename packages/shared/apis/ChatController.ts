// tslint:disable:prefer-function-over-method
import { registerAPI, exposeMethod, APIOptions } from 'decentraland-rpc/lib/host'

import { persistCurrentUser, sendPublicChatMessage } from 'shared/comms'
import { chatObservable, ChatEvent } from 'shared/comms/chat'
import { ExposableAPI } from 'shared/apis/ExposableAPI'
import { IChatCommand, MessageEntry } from 'shared/types'
import { EngineAPI } from 'shared/apis/EngineAPI'
import { teleportObservable } from 'shared/world/positionThings'
import {
  getCurrentUser,
  findPeerByName,
  getPeer,
  addToBlockedUsers,
  addToMutedUsers,
  getBlockedUsers,
  removeFromBlockedUsers,
  removeFromMutedUsers
} from 'shared/comms/peers'
import { uuid } from 'atomicHelpers/math'

export const positionRegex = new RegExp('^(-?[0-9]+) *, *(-?[0-9]+)$')

export interface IChatController {
  /**
   * Send the chat message
   * @param messageEntry
   */
  send(message: string): Promise<MessageEntry>
  /**
   * Return list of chat commands
   */
  getChatCommands(): Promise<{ [key: string]: IChatCommand }>
}

@registerAPI('ChatController')
export class ChatController extends ExposableAPI implements IChatController {
  private chatCommands: { [key: string]: IChatCommand } = {}

  constructor(options: APIOptions) {
    super(options)
    this.initChatCommands()

    const engineAPI = options.getAPIInstance(EngineAPI)

    chatObservable.add((event: any) => {
      if (event.type === ChatEvent.MESSAGE_RECEIVED || event.type === ChatEvent.MESSAGE_SENT) {
        const { type, ...data } = event
        engineAPI.sendSubscriptionEvent(event.type, data)
      }
    })
  }

  @exposeMethod
  async send(message: string): Promise<MessageEntry> {
    let entry = this.handleChatCommand(message)
    // If the message looks like a command but no command was found, provide some feedback
    if (!entry && message[0] === '/') {
      entry = {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message: `That command doesn’t exist. Type /help for a full list of commands.`
      }
    } else {
      // If the message was not a command ("/cmdname"), then send message through wire
      const currentUser = getCurrentUser()
      if (!currentUser) throw new Error('cannotGetCurrentUser')
      const newEntry = (entry = {
        id: uuid(),
        isCommand: false,
        sender: currentUser.displayName || currentUser.publicKey || 'unknown',
        message
      })
      sendPublicChatMessage(newEntry.id, newEntry.message)
    }

    return entry
  }

  @exposeMethod
  async getChatCommands() {
    return this.chatCommands
  }

  // @internal
  handleChatCommand(message: string) {
    const words = message.split(' ')
    const command = words[0].substring(1)
    // Remove command from sentence
    words.shift()
    const restOfMessage = words.join(' ')

    const cmd = this.chatCommands[command]
    if (cmd) {
      return cmd.run(restOfMessage)
    }

    return null
  }

  // @internal
  addChatCommand(name: string, description: string, fn: (message: string) => MessageEntry): void {
    if (this.chatCommands[name]) {
      // Chat command already registered
      return
    }
    this.chatCommands[name] = {
      name,
      description,
      run: message => fn(message)
    }
  }

  // @internal
  initChatCommands() {
    this.addChatCommand('setavatar', 'Sets avatar model', message => {
      const avatarType = message
        .split(' ')
        .join('-')
        .toLowerCase()

      persistCurrentUser({ avatarType: avatarType })

      return {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Avatar type changed to ${message}.`
      }
    })

    this.addChatCommand('setname', 'Sets your username', message => {
      const avatarAttrs = persistCurrentUser({ displayName: message })

      return {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Display Name was changed to ${avatarAttrs.displayName}.`
      }
    })

    this.addChatCommand('goto', 'Teleport to another parcel', message => {
      const coordinates = positionRegex.exec(message)
      if (!coordinates) {
        return {
          id: uuid(),
          isCommand: true,
          sender: 'Decentraland',
          message: 'Could not recognize the coordinates provided. Example usage: /goto 42,42'
        }
      }
      const x = coordinates[1]
      const y = coordinates[2]
      teleportObservable.notifyObservers({ x: parseInt(x, 10), y: parseInt(y, 10) })

      return {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Teleporting to ${x}, ${y}...`
      }
    })

    this.addChatCommand('getname', 'Gets your username', message => {
      const avatarAttrs = getCurrentUser()
      if (!avatarAttrs) throw new Error('cannotGetCurrentUser')
      return {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Your Display Name is ${avatarAttrs.displayName}.`
      }
    })

    this.addChatCommand('block', 'Block [username]', message => {
      const username = message
      const currentUser = getCurrentUser()

      if (!currentUser) throw new Error('cannotGetCurrentUser')

      const user = findPeerByName(username)

      if (user && user.publicKey) {
        // Cannot block yourself
        if (user.publicKey === currentUser.publicKey) {
          return { id: uuid(), isCommand: true, sender: 'Decentraland', message: `You cannot block yourself.` }
        }

        addToBlockedUsers(user.publicKey)
        addToMutedUsers(user.publicKey)

        return {
          id: uuid(),
          isCommand: true,
          sender: 'Decentraland',
          message: `You blocked user ${JSON.stringify(username)}.`
        }
      } else {
        return {
          id: uuid(),
          isCommand: true,
          sender: 'Decentraland',
          message: `User not found ${JSON.stringify(username)}.`
        }
      }
    })

    this.addChatCommand('unblock', 'Unblock [username]', message => {
      const username = message

      const user = findPeerByName(username)

      if (user && user.publicKey) {
        removeFromBlockedUsers(user.publicKey)
        // TODO: Remove this literal mute, muting shold happen automatticaly with block
        removeFromMutedUsers(user.publicKey)

        return { id: uuid(), isCommand: true, sender: 'Decentraland', message: `You unblocked user ${username}.` }
      } else {
        return {
          id: uuid(),
          isCommand: true,
          sender: 'Decentraland',
          message: `User not found ${JSON.stringify(username)}.`
        }
      }
    })

    this.addChatCommand('blocked', 'Show a list of blocked users', message => {
      const users = getBlockedUsers()
      if (!users || users.size === 0) {
        return {
          id: uuid(),
          sender: 'Decentraland',
          isCommand: true,
          message: `No blocked users. Use the '/block [username]' command to block someone.`
        }
      }

      return {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message: `These are the users you are blocking:\n${[...users]
          .map(user => {
            const profile = getPeer(user)
            return `\t${profile && profile.user ? `${profile.user.displayName}: ${user}` : user}\n`
          })
          .join('\n')}`
      }
    })

    this.addChatCommand('mute', 'Mute [username]', message => {
      const username = message
      const currentUser = getCurrentUser()
      if (!currentUser) throw new Error('cannotGetCurrentUser')

      const user = findPeerByName(username)
      if (user && user.publicKey) {
        // Cannot mute yourself
        if (username === currentUser.displayName) {
          return { id: uuid(), isCommand: true, sender: 'Decentraland', message: `You cannot mute yourself.` }
        }

        addToMutedUsers(user.publicKey)

        return { id: uuid(), isCommand: true, sender: 'Decentraland', message: `You muted user ${username}.` }
      } else {
        return {
          id: uuid(),
          isCommand: true,
          sender: 'Decentraland',
          message: `User not found ${JSON.stringify(username)}.`
        }
      }
    })

    this.addChatCommand('unmute', 'Unmute [username]', message => {
      const username = message
      const currentUser = getCurrentUser()
      if (!currentUser) throw new Error('cannotGetCurrentUser')

      const user = findPeerByName(username)

      if (user && user.publicKey) {
        // Cannot unmute or mute yourself
        if (username === currentUser.displayName) {
          return { id: uuid(), isCommand: true, sender: 'Decentraland', message: `You cannot mute or unmute yourself.` }
        }

        removeFromMutedUsers(user.publicKey)

        return { id: uuid(), isCommand: true, sender: 'Decentraland', message: `You unmuted user ${username}.` }
      } else {
        return {
          id: uuid(),
          isCommand: true,
          sender: 'Decentraland',
          message: `User not found ${JSON.stringify(username)}.`
        }
      }
    })

    this.addChatCommand('setstatus', 'Sets your status', message => {
      const avatarAttrs = persistCurrentUser({ status: message })

      return {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Your status was changed to ${avatarAttrs.status}.`
      }
    })

    this.addChatCommand('getstatus', 'Gets your status', message => {
      const currentUser = getCurrentUser()
      if (!currentUser) throw new Error('cannotGetCurrentUser')

      return { id: uuid(), isCommand: true, sender: 'Decentraland', message: `Your status is ${currentUser.status}.` }
    })

    this.addChatCommand('help', 'Show a list of commands', message => {
      return {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Available commands:\n${Object.keys(this.chatCommands)
          .filter(name => name !== 'help')
          .map(name => `\t'${name}': ${this.chatCommands[name].description}`)
          .concat('\thelp: Show this list of commands')
          .join('\n')}`
      }
    })
  }
}
