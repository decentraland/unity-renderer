import { takeEvery, put, select } from 'redux-saga/effects'
import { PayloadAction } from 'typesafe-actions'
import { Vector3Component } from 'atomicHelpers/landHelpers'
import { UnityInterfaceContainer } from 'unity-interface/dcl'
import {
  MESSAGE_RECEIVED,
  MessageReceived,
  messageReceived,
  SEND_MESSAGE,
  SendMessage,
  sendPrivateMessage
} from './actions'
import { uuid } from 'atomicHelpers/math'
import { ChatMessageType, ChatMessage } from 'shared/types'
import { EXPERIENCE_STARTED } from 'shared/loading/types'
import { queueTrackingEvent } from 'shared/analytics'
import { sendPublicChatMessage } from 'shared/comms'
import { peerMap, avatarMessageObservable } from 'shared/comms/peers'
import { parseParcelPosition, worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { TeleportController } from 'shared/world/TeleportController'
import { notifyStatusThroughChat } from 'shared/comms/chat'
import defaultLogger from 'shared/logger'
import { catalystRealmConnected, changeRealm, changeToCrowdedRealm } from 'shared/dao'
import { isValidExpression, expressionExplainer, validExpressions } from 'shared/apis/expressionExplainer'
import { RootState, StoreContainer } from 'shared/store/rootTypes'
import { SHOW_FPS_COUNTER } from 'config'
import { AvatarMessage, AvatarMessageType } from 'shared/comms/interface/types'
import { sampleDropData } from 'shared/airdrops/sampleDrop'
import { findProfileByName, getCurrentUserProfile, getProfile } from 'shared/profiles/selectors'
import { isFriend } from 'shared/friends/selectors'
import { fetchHotScenes } from 'shared/social/hotScenes'
import { getCurrentUserId } from 'shared/session/selectors'
import { blockPlayers, mutePlayers, unblockPlayers, unmutePlayers } from 'shared/social/actions'

declare const globalThis: UnityInterfaceContainer & StoreContainer

interface IChatCommand {
  name: string
  description: string
  run: (message: string) => ChatMessage
}

const chatCommands: { [key: string]: IChatCommand } = {}
const excludeList = ['help', 'airdrop', 'feelinglonely']
const fpsConfiguration = {
  visible: SHOW_FPS_COUNTER
}

const userPose: { [key: string]: Vector3Component } = {}
avatarMessageObservable.add((pose: AvatarMessage) => {
  if (pose.type === AvatarMessageType.USER_POSE) {
    userPose[pose.uuid] = { x: pose.pose[0], y: pose.pose[1], z: pose.pose[2] }
  }
  if (pose.type === AvatarMessageType.USER_REMOVED) {
    delete userPose[pose.uuid]
  }
})

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
  switch (type) {
    case MESSAGE_RECEIVED: {
      queueTrackingEvent('Chat message received', { length: payload.body.length })
      break
    }
    case SEND_MESSAGE: {
      const { messageId, body } = payload
      queueTrackingEvent('Send chat message', {
        messageId,
        length: body.length
      })
      break
    }
  }
}

function* handleReceivedMessage(action: MessageReceived) {
  globalThis.unityInterface.AddMessageToChatWindow(action.payload)
}

function* handleSendMessage(action: SendMessage) {
  const { body: message } = action.payload

  let entry: ChatMessage | null = null

  // Check if message is a command
  if (message[0] === '/') {
    entry = handleChatCommand(message)

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
  } else {
    // If the message was not a command ("/cmdname"), then send message through wire
    const currentUserId = yield select(getCurrentUserId)
    if (!currentUserId) throw new Error('cannotGetCurrentUser')

    entry = {
      messageType: ChatMessageType.PUBLIC,
      messageId: uuid(),
      timestamp: Date.now(),
      sender: currentUserId,
      body: message
    }

    sendPublicChatMessage(entry.messageId, entry.body)
  }

  globalThis.unityInterface.AddMessageToChatWindow(entry)
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
    const isValid = isFinite(coordinates.x) && isFinite(coordinates.y)

    let response = ''

    if (!isValid) {
      if (message.trim().toLowerCase() === 'magic') {
        response = TeleportController.goToMagic().message
      } else if (message.trim().toLowerCase() === 'random') {
        response = TeleportController.goToRandom().message
      } else if (message.trim().toLowerCase() === 'next') {
        response = TeleportController.goToNext().message
      } else if (message.trim().toLowerCase() === 'crowd') {
        response = `Teleporting to a crowd of people in current realm...`

        TeleportController.goToCrowd().then(
          ({ message, success }) => notifyStatusThroughChat(message),
          () => {
            // Do nothing. This is handled inside controller
          }
        )
      } else {
        response = 'Could not recognize the coordinates provided. Example usage: /goto 42,42'
      }
    } else {
      const { x, y } = coordinates
      response = TeleportController.goTo(x, y).message
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
    let response = ''

    if (realmString === 'crowd') {
      response = `Changing to realm that is crowded nearby...`

      changeToCrowdedRealm().then(
        ([changed, realm]) => {
          if (changed) {
            notifyStatusThroughChat(
              `Found a crowded realm to join. Welcome to the realm ${realm.catalystName}-${realm.layer}!`
            )
          } else {
            notifyStatusThroughChat(`Already on most crowded realm for location. Nothing changed.`)
          }
        },
        (e) => {
          const cause = e === 'realm-full' ? ' The requested realm is full.' : ''
          notifyStatusThroughChat('Could not join realm.' + cause)
          defaultLogger.error(`Error joining crowded realm ${realmString}`, e)
        }
      )
    } else {
      const realm = changeRealm(realmString)

      if (realm) {
        response = `Changing to Realm ${realm.catalystName}-${realm.layer}...`
        // TODO: This status should be shown in the chat window
        catalystRealmConnected().then(
          () =>
            notifyStatusThroughChat(
              `Changed realm successfuly. Welcome to the realm ${realm.catalystName}-${realm.layer}!`
            ),
          (e) => {
            const cause = e === 'realm-full' ? ' The requested realm is full.' : ''
            notifyStatusThroughChat('Could not join realm.' + cause)
            defaultLogger.error('Error joining realm', e)
          }
        )
      } else {
        response = `Couldn't find realm ${realmString}`
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

  addChatCommand('players', 'Shows a list of players around you', (message) => {
    const users = [...peerMap.entries()]

    const strings = users
      .filter(([_, value]) => !!(value && value.user && value.user.userId))
      .filter(([uuid]) => userPose[uuid])
      .map(function ([uuid, value]) {
        const name = getProfile(getGlobalState(), value.user?.userId!)?.name ?? 'unknown'
        const pos = { x: 0, y: 0 }
        worldToGrid(userPose[uuid], pos)
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

  addChatCommand('showfps', 'Show FPS counter', (message) => {
    fpsConfiguration.visible = !fpsConfiguration.visible
    const unityWindow: any = window
    fpsConfiguration.visible ? unityWindow.unityInterface.ShowFPSPanel() : unityWindow.unityInterface.HideFPSPanel()

    return {
      messageId: uuid(),
      sender: 'Decentraland',
      messageType: ChatMessageType.SYSTEM,
      timestamp: Date.now(),
      body: 'Toggling FPS counter'
    }
  })

  addChatCommand('getname', 'Gets your username', (message) => {
    const currentUserProfile = getCurrentUserProfile(getGlobalState())
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

      sendPublicChatMessage(uuid(), `␐${expression} ${time}`)

      globalThis.unityInterface.TriggerSelfUserExpression(expression)

      return {
        messageId: uuid(),
        messageType: ChatMessageType.SYSTEM,
        sender: 'Decentraland',
        timestamp: Date.now(),
        body: `You start ${expressionExplainer[expression]}`
      }
    }
  )

  let whisperFn = (expression: string) => {
    const [userName, message] = parseWhisperExpression(expression)

    const currentUserId = getCurrentUserId(getGlobalState())
    if (!currentUserId) throw new Error('cannotGetCurrentUser')

    const user = findProfileByName(getGlobalState(), userName)

    if (!user || !user.userId) {
      return {
        messageId: uuid(),
        messageType: ChatMessageType.SYSTEM,
        sender: 'Decentraland',
        timestamp: Date.now(),
        body: `Cannot find user ${userName}`
      }
    }

    const _isFriend: ReturnType<typeof isFriend> = isFriend(globalThis.globalStore.getState(), user.userId)
    if (!_isFriend) {
      return {
        messageId: uuid(),
        messageType: ChatMessageType.SYSTEM,
        sender: 'Decentraland',
        timestamp: Date.now(),
        body: `Trying to send a message to a non friend ${userName}`
      }
    }

    globalThis.globalStore.dispatch(sendPrivateMessage(user.userId, message))

    return {
      messageId: uuid(),
      messageType: ChatMessageType.PRIVATE,
      sender: currentUserId,
      recipient: user.userId,
      timestamp: Date.now(),
      body: message
    }
  }

  addChatCommand('whisper', 'Send a private message to a friend', whisperFn)

  addChatCommand('w', 'Send a private message to a friend', whisperFn)

  addChatCommand('airdrop', 'fake an airdrop', () => {
    const unityWindow: any = window
    unityWindow.unityInterface.TriggerAirdropDisplay(sampleDropData)
    return {
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      sender: 'Decentraland',
      timestamp: Date.now(),
      body: 'Faking airdrop...'
    }
  })

  function performSocialActionOnPlayer(
    username: string,
    actionBuilder: (usersId: string[]) => { type: string; payload: { playersId: string[] } },
    actionName: 'mute' | 'block' | 'unmute' | 'unblock'
  ) {
    let pastTense: string = actionName === 'mute' || actionName === 'unmute' ? actionName + 'd' : actionName + 'ed'
    const currentUserId = getCurrentUserId(getGlobalState())
    if (!currentUserId) throw new Error('cannotGetCurrentUser')

    const user = findProfileByName(getGlobalState(), username)
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

      globalThis.globalStore.dispatch(actionBuilder([user.userId]))

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

  addChatCommand('help', 'Show a list of commands', (message) => {
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
          .map((name) => `\t/${name}: ${chatCommands[name].description}`)
          .concat('\t/help: Show this list of commands')
          .join('\n')}`
    }
  })

  addChatCommand('feelinglonely', 'Show a list of crowded scenes', (message) => {
    fetchHotScenes().then(
      ($) => {
        let body = ''
        $.slice(0, 5).forEach((sceneInfo) => {
          const count = sceneInfo.realms.reduce((a, b) => a + b.usersCount, 0)
          body += `${count} ${count > 1 ? 'users' : 'user'} @ ${
            sceneInfo.name.length < 20 ? sceneInfo.name : sceneInfo.name.substring(0, 20) + '...'
          } ${sceneInfo.baseCoords.x},${sceneInfo.baseCoords.y} ${sceneInfo.realms.reduce(
            (a, b) => a + `\n\t realm: ${b.serverName}-${b.layer} users: ${b.usersCount}`,
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
}

function getGlobalState(): RootState {
  return globalThis.globalStore.getState()
}

function parseWhisperExpression(expression: string) {
  const words = expression.split(' ')

  const userName = words[0].trim() // remove the leading '/'

  words.shift() // Remove userName from sentence

  const restOfMessage = words.join(' ')

  return [userName, restOfMessage]
}
