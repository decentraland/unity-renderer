import { SHOW_FPS_COUNTER } from 'config'
import { SocialAPI } from 'dcl-social-client'
import { parseParcelPosition } from 'lib/decentraland/parcels/parseParcelPosition'
import { worldToGrid } from 'lib/decentraland/parcels/worldToGrid'
import { uuid } from 'lib/javascript/uuid'
import defaultLogger from 'lib/logger'
import { call, put, select, takeEvery } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import { isValidExpression, validExpressions } from 'shared/apis/expressionExplainer'
import { sendPublicChatMessage } from 'shared/comms'
import { getAllPeers } from 'shared/comms/peers'
import { changeRealm } from 'shared/dao'
import { joinOrCreateChannel, leaveChannel, sendChannelMessage } from 'shared/friends/actions'
import { getSocialClient } from 'shared/friends/selectors'
import { areChannelsEnabled } from 'shared/friends/utils'
import { EXPERIENCE_STARTED } from 'shared/loading/types'
import { getExplorerVersion } from 'shared/meta/version'
import {
  findProfileByName,
  getCurrentUserProfile as currentUserProfileSelector,
  getProfile
} from 'shared/profiles/selectors'
import { waitForRendererInstance } from 'shared/renderer/sagas-helper'
import { getRendererModules } from 'shared/renderer/selectors'
import { getCurrentUserId, hasWallet } from 'shared/session/selectors'
import { blockPlayers, mutePlayers, unblockPlayers, unmutePlayers } from 'shared/social/actions'
import { fetchHotScenes } from 'shared/social/hotScenes'
import { store } from 'shared/store/isolatedStore'
import { ChatMessage, ChatMessagePlayerType, ChatMessageType } from 'shared/types'
import { TeleportController } from 'shared/world/TeleportController'
import { PayloadAction } from 'typesafe-actions'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import {
  MessageReceived,
  messageReceived,
  MESSAGE_RECEIVED,
  SendMessage,
  sendPrivateMessage,
  SEND_MESSAGE
} from './actions'
import { notifyStatusThroughChat } from './index'

interface IChatCommand {
  name: string
  description: string
  run: (message: string) => ChatMessage
}

const chatCommands: { [key: string]: IChatCommand } = {}
const excludeList = ['help', 'feelinglonely']
const fpsConfiguration = {
  visible: SHOW_FPS_COUNTER
}

export function* chatSaga(): any {
  initChatCommands()

  yield takeEvery([MESSAGE_RECEIVED, SEND_MESSAGE], trackEvents)

  yield takeEvery(MESSAGE_RECEIVED, handleReceivedMessage)
  yield takeEvery(SEND_MESSAGE, handleSendMessage)

  yield takeEvery(EXPERIENCE_STARTED, showWelcomeMessage)
}

function* showWelcomeMessage() {
  yield put(
    messageReceived({
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      timestamp: Date.now(),
      body: 'Type /help for info about controls'
    })
  )
}

type MessageEvent = typeof MESSAGE_RECEIVED | typeof SEND_MESSAGE

function* trackEvents(action: PayloadAction<MessageEvent, ChatMessage>) {
  const { type, payload } = action
  const isWallet = yield select(hasWallet)
  switch (type) {
    case SEND_MESSAGE: {
      const isChannel = payload.messageType === ChatMessageType.PUBLIC && payload.recipient

      trackEvent('Send chat message', {
        messageId: payload.messageId,
        from: isWallet ? ChatMessagePlayerType.WALLET : ChatMessagePlayerType.GUEST,
        to: payload.messageType === ChatMessageType.PRIVATE ? ChatMessagePlayerType.WALLET : undefined,
        length: payload.body.length,
        messageType: payload.messageType,
        source: isChannel ? 'channel' : undefined
      })
      break
    }
  }
}

function* handleReceivedMessage(action: MessageReceived) {
  yield call(waitForRendererInstance)
  getUnityInstance().AddMessageToChatWindow(action.payload)
}

function* handleSendMessage(action: SendMessage) {
  const { body: message, messageType, recipient } = action.payload

  let entry: ChatMessage | null = null

  // When there is a recipient, it means is a message sent to a channel
  const isChannel = messageType === ChatMessageType.PUBLIC && recipient
  // Check if message is a command
  if (message[0] === '/') {
    entry = handleChatCommand(message)

    if (entry && entry.body.length === 0) {
      // Command is found but has no feedback message
      return
    }

    if (entry && entry.messageType === ChatMessageType.PRIVATE) {
      // Command is found and it is a private message, we've already added the message to the chat window
      return
    }

    // If no such command was found, provide some feedback
    if (!entry) {
      entry = {
        messageType: ChatMessageType.SYSTEM,
        messageId: uuid(),
        sender: 'Decentraland',
        body: `That command doesn’t exist. Type /help for a full list of commands.`,
        timestamp: Date.now()
      }
    }

    yield call(waitForRendererInstance)
    getUnityInstance().AddMessageToChatWindow(entry)
  } else {
    // If the message was not a command ("/cmdname"), then send message through wire
    const currentUserId = yield select(getCurrentUserId)
    if (!currentUserId) {
      defaultLogger.error('Could not get the current user id.')
      trackEvent('error', {
        message: 'error trying to get the current user id.',
        context: 'kernel#chatSaga',
        stack: 'handleSendMessage'
      })
      return
    }

    if (isChannel) {
      entry = {
        messageType: ChatMessageType.PUBLIC,
        messageId: uuid(),
        sender: currentUserId,
        recipient,
        body: message,
        timestamp: Date.now()
      }
      yield put(sendChannelMessage(recipient, entry))
    } else {
      entry = {
        messageType: ChatMessageType.PUBLIC,
        messageId: uuid(),
        timestamp: Date.now(),
        sender: currentUserId,
        body: message
      }
      sendPublicChatMessage(message)

      yield call(waitForRendererInstance)
      getUnityInstance().AddMessageToChatWindow(entry)
    }
  }
}

function handleChatCommand(message: string) {
  const words = message.split(' ')

  const command = words[0].substring(1).trim() // remove the leading '/'

  words.shift() // Remove command from sentence

  const restOfMessage = words.join(' ')

  const cmd = chatCommands[command]

  if (cmd) {
    return cmd.run(restOfMessage)
  }

  return null
}

function addChatCommand(name: string, description: string, fn: (message: string) => ChatMessage): void {
  if (chatCommands[name]) {
    // Chat command already registered
    return
  }

  chatCommands[name] = {
    name,
    description,
    run: (message: string) => fn(message)
  }
}

function initChatCommands() {
  addChatCommand('goto', 'Teleport to another parcel', (message) => {
    const coordinates = parseParcelPosition(message)
    const isValidPosition = isFinite(coordinates.x) && isFinite(coordinates.y)

    let response = ''

    if (isValidPosition) {
      const { x, y } = coordinates
      TeleportController.goTo(x, y).then(
        ({ message }) => notifyStatusThroughChat(message),
        () => {
          // Do nothing. This is handled inside controller
        }
      )
    } else {
      if (message.trim().toLowerCase() === 'random') {
        TeleportController.goToRandom().then(
          ({ message }) => notifyStatusThroughChat(message),
          () => {
            // Do nothing. This is handled inside controller
          }
        )
      } else if (message.trim().toLowerCase() === 'magic' || message.trim().toLowerCase() === 'crowd') {
        TeleportController.goToCrowd().then(
          ({ message }) => notifyStatusThroughChat(message),
          () => {
            // Do nothing. This is handled inside controller
          }
        )
      } else if (message.trim().toLowerCase() === 'home') {
        TeleportController.goToHome().then(
          ({ message }) => notifyStatusThroughChat(message),
          () => {
            // Do nothing. This is handled inside controller
          }
        )
      } else {
        response = 'Could not recognize the coordinates provided. Example usage: /goto 42,42'
      }
    }

    return {
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      sender: 'Decentraland',
      timestamp: Date.now(),
      body: response
    }
  })

  addChatCommand('changerealm', 'Changes communications realms', (message) => {
    const realmString = message.trim()
    const response = ''

    changeRealm(realmString).catch((e) => {
      notifyStatusThroughChat('changerealm: Could not join realm.')
      defaultLogger.error(e)
    })

    return {
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      sender: 'Decentraland',
      timestamp: Date.now(),
      body: response
    }
  })

  addChatCommand('world', 'Goes to a world', (message) => {
    const worldString = `${message.trim()}`
    const response = ''

    changeRealm(worldString).catch((e) => {
      notifyStatusThroughChat('World does not exist.')
      defaultLogger.error(e)
    })

    return {
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      sender: 'Decentraland',
      timestamp: Date.now(),
      body: response
    }
  })

  addChatCommand('players', 'Shows a list of players around you', (_message) => {
    const users = [...getAllPeers().entries()]

    const strings = users
      .filter(([_, value]) => !!(value && value.ethereumAddress))
      .filter(([_, value]) => value.position)
      .map(function ([, value]) {
        // eslint-disable-next-line @typescript-eslint/no-non-null-asserted-optional-chain
        const name = getProfile(store.getState(), value.ethereumAddress!)?.name ?? 'unknown'
        const pos = { x: 0, y: 0 }
        worldToGrid({ x: value.position![0], y: value.position![1], z: value.position![2] }, pos)
        return `  ${name}: ${pos.x}, ${pos.y}`
      })
      .join('\n')

    return {
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      sender: 'Decentraland',
      timestamp: Date.now(),
      body: strings ? `Players around you:\n${strings}` : 'No other players are near to your location'
    }
  })

  addChatCommand('debug', 'Show debug panel', (_message) => getDebugPanelMessage())

  addChatCommand('showfps', 'Show fps panel (deprecated in favor of /debug)', (_message) => getDebugPanelMessage())

  /*
    /detectabs => enable for all shapes
    /detectabs off => disable for all shapes
    /detectabs scene => enable for current scene shapes
    /detectabs scene off => disable for current scene shapes
   */
  addChatCommand('detectabs', 'Paint AB-loaded world objects green and GLTF red', (message) =>
    parseAndSendDetectABMessage(message)
  )

  addChatCommand('getname', 'Gets your username', (_message) => {
    const currentUserProfile = currentUserProfileSelector(store.getState())
    if (!currentUserProfile) throw new Error('profileNotInitialized')
    return {
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      sender: 'Decentraland',
      timestamp: Date.now(),
      body: `Your Display Name is ${currentUserProfile.name}.`
    }
  })

  addChatCommand(
    'emote',
    'Trigger avatar animation named [expression] ("robot", "wave", or "fistpump")',
    (expression) => {
      if (!isValidExpression(expression)) {
        return {
          messageId: uuid(),
          messageType: ChatMessageType.SYSTEM,
          sender: 'Decentraland',
          timestamp: Date.now(),
          body: `Expression ${expression} is not one of ${validExpressions.map((_) => `"${_}"`).join(', ')}`
        }
      }

      const time = Date.now()

      sendPublicChatMessage(`␐${expression} ${time}`)

      getUnityInstance().TriggerSelfUserExpression(expression)
      getRendererModules(store.getState())
        ?.emotes?.triggerSelfUserExpression({ id: expression })
        .catch(defaultLogger.error)

      return {
        messageId: uuid(),
        messageType: ChatMessageType.SYSTEM,
        sender: 'Decentraland',
        timestamp: Date.now(),
        body: ''
      }
    }
  )

  const whisperFn = (expression: string) => {
    const [userName, message] = parseWhisperExpression(expression)

    const currentUserId = getCurrentUserId(store.getState())
    if (!currentUserId) throw new Error('cannotGetCurrentUser')

    const user = findProfileByName(store.getState(), userName)

    if (!user || !user.userId) {
      return {
        messageId: uuid(),
        messageType: ChatMessageType.SYSTEM,
        sender: 'Decentraland',
        timestamp: Date.now(),
        body: `Cannot find user ${userName}`
      }
    }

    const chatMessage = {
      messageId: uuid(),
      messageType: ChatMessageType.PRIVATE,
      sender: currentUserId,
      recipient: user.userId,
      timestamp: Date.now(),
      body: message
    }

    store.dispatch(sendPrivateMessage(user.userId, chatMessage))

    return chatMessage
  }

  addChatCommand('whisper', 'Send a private message to a friend', whisperFn)

  addChatCommand('w', 'Send a private message to a friend', whisperFn)

  function performSocialActionOnPlayer(
    username: string,
    actionBuilder: (usersId: string[]) => { type: string; payload: { playersId: string[] } },
    actionName: 'mute' | 'block' | 'unmute' | 'unblock'
  ) {
    const pastTense: string = actionName === 'mute' || actionName === 'unmute' ? actionName + 'd' : actionName + 'ed'
    const currentUserId = getCurrentUserId(store.getState())
    if (!currentUserId) throw new Error('cannotGetCurrentUser')

    const user = findProfileByName(store.getState(), username)
    if (user && user.userId) {
      // Cannot mute yourself
      if (username === currentUserId) {
        return {
          messageId: uuid(),
          messageType: ChatMessageType.SYSTEM,
          sender: 'Decentraland',
          timestamp: Date.now(),
          body: `You cannot ${actionName} yourself.`
        }
      }

      store.dispatch(actionBuilder([user.userId]))

      return {
        messageId: uuid(),
        messageType: ChatMessageType.SYSTEM,
        sender: 'Decentraland',
        timestamp: Date.now(),
        body: `You ${pastTense} user ${username}.`
      }
    } else {
      return {
        messageId: uuid(),
        messageType: ChatMessageType.SYSTEM,
        sender: 'Decentraland',
        timestamp: Date.now(),
        body: `User not found ${JSON.stringify(username)}.`
      }
    }
  }

  addChatCommand('mute', 'Mute [username]', (message) => {
    return performSocialActionOnPlayer(message, mutePlayers, 'mute')
  })

  addChatCommand('unmute', 'Unmute [username]', (message) => {
    return performSocialActionOnPlayer(message, unmutePlayers, 'unmute')
  })

  addChatCommand('block', 'Block [username]', (message) => {
    return performSocialActionOnPlayer(message, blockPlayers, 'block')
  })

  addChatCommand('unblock', 'Unblock [username]', (message) => {
    return performSocialActionOnPlayer(message, unblockPlayers, 'unblock')
  })

  addChatCommand('help', 'Show a list of commands', (_message) => {
    const excludeListChannels = areChannelsEnabled() ? [] : ['join', 'leave']

    return {
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      sender: 'Decentraland',
      timestamp: Date.now(),
      body:
        `Click on the screen to lock the cursor, later you can unlock it with the [ESC] key.` +
        `\n\nYou can move with the [WASD] keys and jump with the [SPACE] key.` +
        `\n\nYou can toggle the chat with the [ENTER] key.` +
        `\n\nAvailable commands:\n${Object.keys(chatCommands)
          .filter((name) => !excludeList.includes(name))
          .filter((name) => !excludeListChannels.includes(name))
          .map((name) => `\t/${name}: ${chatCommands[name].description}`)
          .concat('\t/help: Show this list of commands')
          .join('\n')}`
    }
  })

  addChatCommand('version', 'Shows application version', (_message) => {
    const explorerVersion = getExplorerVersion()
    return {
      messageId: uuid(),
      sender: 'Decentraland',
      messageType: ChatMessageType.SYSTEM,
      timestamp: Date.now(),
      body: `\nExplorer: ${explorerVersion}\n`
    }
  })

  addChatCommand('feelinglonely', 'Show a list of crowded scenes', (_message) => {
    fetchHotScenes().then(
      ($) => {
        let body = ''
        $.slice(0, 5).forEach((sceneInfo) => {
          const count = sceneInfo.realms.reduce((a, b) => a + b.usersCount, 0)
          body += `${count} ${count > 1 ? 'users' : 'user'} @ ${
            sceneInfo.name.length < 20 ? sceneInfo.name : sceneInfo.name.substring(0, 20) + '...'
          } ${sceneInfo.baseCoords.x},${sceneInfo.baseCoords.y} ${sceneInfo.realms.reduce(
            (a, b) => a + `\n\t realm: ${b.serverName} users: ${b.usersCount}`,
            ''
          )}\n`
        })
        notifyStatusThroughChat(body)
      },
      (e) => {
        defaultLogger.log(e)
        notifyStatusThroughChat('Error looking for other players')
      }
    )
    return {
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      sender: 'Decentraland',
      timestamp: Date.now(),
      body: 'Looking for other players...'
    }
  })

  addChatCommand('join', 'Join or create channel', (channelId) => {
    if (!areChannelsEnabled()) {
      return {
        messageType: ChatMessageType.SYSTEM,
        messageId: uuid(),
        sender: 'Decentraland',
        body: `That command doesn’t exist. Type /help for a full list of commands.`,
        timestamp: Date.now()
      }
    }
    const client: SocialAPI | null = getSocialClient(store.getState())
    if (!client) {
      return {
        messageId: uuid(),
        sender: 'Decentraland',
        messageType: ChatMessageType.SYSTEM,
        timestamp: Date.now(),
        body: 'Error joining/creating channel.'
      }
    }

    // Join or create channel
    store.dispatch(joinOrCreateChannel(channelId, []))

    return {
      messageId: uuid(),
      sender: 'Decentraland',
      messageType: ChatMessageType.SYSTEM,
      timestamp: Date.now(),
      body: `Joining channel ${channelId}`
    }
  })

  addChatCommand('leave', 'Leave channel', (channelId) => {
    if (!areChannelsEnabled()) {
      return {
        messageType: ChatMessageType.SYSTEM,
        messageId: uuid(),
        sender: 'Decentraland',
        body: `That command doesn’t exist. Type /help for a full list of commands.`,
        timestamp: Date.now()
      }
    }
    store.dispatch(leaveChannel(channelId))

    return {
      messageId: uuid(),
      sender: 'Decentraland',
      messageType: ChatMessageType.SYSTEM,
      timestamp: Date.now(),
      body: `Leaving channel`
    }
  })
}

function getDebugPanelMessage() {
  fpsConfiguration.visible = !fpsConfiguration.visible
  fpsConfiguration.visible ? getUnityInstance().ShowFPSPanel() : getUnityInstance().HideFPSPanel()

  return {
    messageId: uuid(),
    sender: 'Decentraland',
    messageType: ChatMessageType.SYSTEM,
    timestamp: Date.now(),
    body: 'Toggling FPS counter'
  }
}

//message can be
//null or empty => enable ABs painting for all scenes
//scene => enable ABs painting for current scene
//off => disable ABs painting for all loaded scenes
//scene off => disable ABs painting for current scene
function parseAndSendDetectABMessage(message: string) {
  let isOn: boolean
  let forCurrentScene: boolean
  const offString: string = 'off'
  const sceneString: string = 'scene'

  if (message && message.includes(' ')) {
    const words: string[] = message.split(' ')
    const firstWord: string = words[0].trim()
    const secondWord: string = words[1].trim()
    forCurrentScene = firstWord === sceneString
    isOn = secondWord !== offString
  } else {
    isOn = message !== offString
    forCurrentScene = message === sceneString
  }

  getUnityInstance().DetectABs({ isOn: isOn, forCurrentScene: forCurrentScene })

  return {
    messageId: uuid(),
    sender: 'Decentraland',
    messageType: ChatMessageType.SYSTEM,
    timestamp: Date.now(),
    body: 'Sending detect ABs message'
  }
}

function parseWhisperExpression(expression: string) {
  const words = expression.split(' ')

  const userName = words[0].trim() // remove the leading '/'

  words.shift() // Remove userName from sentence

  const restOfMessage = words.join(' ')

  return [userName, restOfMessage]
}
