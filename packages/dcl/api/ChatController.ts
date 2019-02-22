// tslint:disable:prefer-function-over-method
import { registerAPI, exposeMethod, APIOptions } from 'decentraland-rpc/lib/host'
import { v4 } from 'uuid'

import { avatarTypes } from 'dcl/entities/utils/avatarHelpers'
import { persistCurrentUser, sendPublicChatMessage } from 'shared/comms'
import {
  addToBlockedUsers,
  addToMutedUsers,
  removeFromBlockedUsers,
  removeFromMutedUsers,
  getBlockedUsers,
  getMutedUsers
} from 'shared/comms/profile'
import { getCurrentUser, muteUsers, hideBlockedUsers, findPeerByName, getPeer } from 'dcl/comms/peers'
import { chatObservable, ChatEvent } from 'shared/comms/chat'
import { ExposableAPI } from '../../shared/apis/ExposableAPI'
import { IChatCommand, MessageEntry } from 'shared/types'
import { EngineAPI } from 'shared/apis/EngineAPI'
import { teleportObserver } from 'shared/world/positionThings'

export const positionRegex = new RegExp('^(-?[0-9]+) *, *(-?[0-9]+)$')

export interface IChatController {
  /**
   * Send the chat message
   * @param messageEntry
   */
  send(messageEntry: MessageEntry): Promise<boolean | MessageEntry>
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
  async send(messageEntry: MessageEntry): Promise<boolean | MessageEntry> {
    let cmd = this.handleChatCommand(messageEntry)
    // If the message looks like a command but no command was found, provide some feedback
    if (!cmd && messageEntry.message[0] === '/') {
      cmd = {
        id: v4(),
        isCommand: true,
        sender: 'Decentraland',
        message: `That command doesn’t exist. Type /help for a full list of commands.`
      }
    }
    // If the message was not a command ("/cmdname"), then send message through wire
    if (!cmd) {
      sendPublicChatMessage(messageEntry.id, messageEntry.message)
    }

    return cmd
  }

  @exposeMethod
  async getChatCommands() {
    return this.chatCommands
  }

  // @internal
  handleChatCommand(messageEntry: MessageEntry) {
    const words = messageEntry.message.split(' ')
    const command = words[0].substring(1)
    // Remove command from sentence
    words.shift()
    const restOfMessage = words.join(' ')

    const cmd = this.chatCommands[command]
    if (cmd) {
      return cmd.run(restOfMessage)
    }

    return false
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

      // Stop if user tries to set unsupported avatar type
      if (!avatarTypes.has(avatarType)) {
        return {
          id: v4(),
          isCommand: true,
          sender: 'Decentraland',
          message: `Avatar type "${message}" is not supported! (use fox, round robot or square robot instead)`
        }
      }

      persistCurrentUser({ avatarType: avatarType })

      return {
        id: v4(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Avatar type changed to ${message}.`
      }
    })

    this.addChatCommand('setname', 'Sets your username', message => {
      const avatarAttrs = persistCurrentUser({ displayName: message })

      return {
        id: v4(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Display Name was changed to ${avatarAttrs.displayName}.`
      }
    })

    this.addChatCommand('goto', 'Teleport to another parcel', message => {
      const coordinates = positionRegex.exec(message)
      if (!coordinates) {
        return { id: v4(), isCommand: true, sender: 'Decentraland', message: 'Could not recognize the coordinates provided. Example usage: /goto 42,42' }
      }
      const x = coordinates[1]
      const y = coordinates[2]
      teleportObserver.notifyObservers({ x: parseInt(x, 10), y: parseInt(y, 10) })

      return {
        id: v4(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Teleporting to ${x}, ${y}...`
      }
    })

    this.addChatCommand('getname', 'Gets your username', message => {
      const avatarAttrs = getCurrentUser()

      return {
        id: v4(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Your Display Name is ${avatarAttrs.displayName}.`
      }
    })

    this.addChatCommand('block', 'Block [username]', message => {
      const username = message
      // Cannot block yourself
      if (username === getCurrentUser().displayName) {
        return { id: v4(), isCommand: true, sender: 'Decentraland', message: `You cannot block yourself.` }
      }

      const user = findPeerByName(username)

      addToBlockedUsers(user.publicKey)
      addToMutedUsers(user.publicKey)

      hideBlockedUsers(getBlockedUsers())
      muteUsers(getMutedUsers())

      return { id: v4(), isCommand: true, sender: 'Decentraland', message: `You blocked user ${username}.` }
    })

    this.addChatCommand('unblock', 'Unblock [username]', message => {
      const username = message

      const user = findPeerByName(username)

      removeFromBlockedUsers(user.publicKey)
      // TODO: Remove this literal mute, muting shold happen automatticaly with block
      removeFromMutedUsers(user.publicKey)
      hideBlockedUsers(getBlockedUsers())
      muteUsers(getMutedUsers())

      return { id: v4(), isCommand: true, sender: 'Decentraland', message: `You unblocked user ${username}.` }
    })

    this.addChatCommand('blocked', 'Show a list of blocked users', message => {
      const users = getBlockedUsers()
      if (!users || users.size === 0) {
        return {
          id: v4(),
          sender: 'Decentraland',
          isCommand: true,
          message: `No blocked users. Use the '/block [username]' command to block someone.`
        }
      }

      return {
        id: v4(),
        isCommand: true,
        sender: 'Decentraland',
        message: `These are the users you are blocking:\n${[...users]
          .map(user => {
            const profile = getPeer(user)
            return `\t${profile ? `${profile.user.displayName}: ${user}` : user}\n`
          })
          .join('\n')}`
      }
    })

    this.addChatCommand('mute', 'Mute [username]', message => {
      const username = message
      // Cannot mute yourself
      if (username === getCurrentUser().displayName) {
        return { id: v4(), isCommand: true, sender: 'Decentraland', message: `You cannot mute yourself.` }
      }

      const user = findPeerByName(username)

      addToMutedUsers(user.publicKey)
      muteUsers(getMutedUsers())

      return { id: v4(), isCommand: true, sender: 'Decentraland', message: `You muted user ${username}.` }
    })

    this.addChatCommand('unmute', 'Unmute [username]', message => {
      const username = message
      // Cannot unmute or mute yourself
      if (username === getCurrentUser().displayName) {
        return { id: v4(), isCommand: true, sender: 'Decentraland', message: `You cannot mute or unmute yourself.` }
      }

      const user = findPeerByName(username)

      removeFromMutedUsers(user.publicKey)

      muteUsers(getMutedUsers())

      return { id: v4(), isCommand: true, sender: 'Decentraland', message: `You unmuted user ${username}.` }
    })

    this.addChatCommand('setstatus', 'Sets your status', message => {
      const avatarAttrs = persistCurrentUser({ status: message })

      return {
        id: v4(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Your status was changed to ${avatarAttrs.status}.`
      }
    })

    this.addChatCommand('getstatus', 'Gets your status', message => {
      const avatarAttrs = getCurrentUser()

      return { id: v4(), isCommand: true, sender: 'Decentraland', message: `Your status is ${avatarAttrs.status}.` }
    })

    this.addChatCommand('help', 'Show a list of commands', message => {
      return {
        id: v4(),
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
