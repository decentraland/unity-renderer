import { DecentralandInterface } from 'decentraland-ecs/src/decentraland/Types'
import { Color4 } from 'decentraland-ecs/src'
import { OnTextSubmit, OnBlur, OnFocus } from 'decentraland-ecs/src/decentraland/UIEvents'

import {
  UIInputText,
  UIText,
  UIContainerStack,
  UIContainerRect,
  UIScrollRect
} from 'decentraland-ecs/src/decentraland/UIShapes'

import { MessageEntry } from 'shared/types'

import { screenSpaceUI } from './ui'
import { execute } from './rpc'

declare const dcl: DecentralandInterface

let isMaximized: boolean = false

let chatContainer: UIContainerRect
let chatInnerTopContainer: UIContainerRect
let messagesLogScrollContainer: UIScrollRect
let messagesLogStackContainer: UIContainerStack
let textInputContainer: UIContainerRect
let textInput: UIInputText
let messagesLogText: UIText

const MAX_LOGGED_MESSAGES = 50
const COMMAND_COLOR = '#80ffe5ff'
const INITIAL_INPUT_TEXT_COLOR = Color4.White()
const PRIMARY_TEXT_COLOR = Color4.White()

const internalState = {
  commandsList: [] as Array<any>,
  messages: [] as Array<any>,
  isFocused: false,
  isSliderVisible: false
}

function createUi() {
  // UI creators -------------------
  dcl.subscribe('MESSAGE_RECEIVED')
  dcl.subscribe('MESSAGE_SENT')
  dcl.onEvent(event => {
    const eventType: string = event.type
    const eventData: any = event.data
    if (eventType === 'MESSAGE_RECEIVED' || eventType === 'MESSAGE_SENT') {
      addMessage(eventData.messageEntry as MessageEntry)
    }
  })

  // -------------------------------

  chatContainer = new UIContainerRect(screenSpaceUI)
  chatContainer.name = 'chat-container'
  chatContainer.color = Color4.Clear()
  chatContainer.vAlign = 'bottom'
  chatContainer.hAlign = 'left'
  chatContainer.width = '380px'
  chatContainer.height = '250px'
  chatContainer.positionX = 10
  chatContainer.positionY = 10
  chatContainer.thickness = 0

  chatInnerTopContainer = new UIContainerRect(chatContainer)
  chatInnerTopContainer.color = Color4.Clear()
  chatInnerTopContainer.name = 'inner-top-container'
  chatInnerTopContainer.vAlign = 'top'
  chatInnerTopContainer.hAlign = 'left'
  chatInnerTopContainer.width = '100%'
  chatInnerTopContainer.height = '82.5%'

  messagesLogScrollContainer = new UIScrollRect(chatInnerTopContainer)
  messagesLogScrollContainer.name = 'messages-log-scroll-container'
  messagesLogScrollContainer.vAlign = 'top'
  messagesLogScrollContainer.hAlign = 'left'
  messagesLogScrollContainer.width = '100%'
  messagesLogScrollContainer.height = '90%'
  messagesLogScrollContainer.positionY = '-8px'
  messagesLogScrollContainer.positionX = -5
  messagesLogScrollContainer.valueY = 1
  messagesLogScrollContainer.isVertical = false
  messagesLogScrollContainer.isHorizontal = false
  messagesLogScrollContainer.visible = true

  messagesLogStackContainer = new UIContainerStack(messagesLogScrollContainer)
  messagesLogStackContainer.name = 'messages-log-stack-container'
  messagesLogStackContainer.vAlign = 'bottom'
  messagesLogStackContainer.hAlign = 'center'
  messagesLogStackContainer.width = '100%'
  messagesLogStackContainer.height = '100%'
  messagesLogStackContainer.spacing = 5 // inter message
  messagesLogStackContainer.positionX = 4 // position about the box

  textInputContainer = new UIContainerRect(chatContainer)
  textInputContainer.color = Color4.Clear()
  textInputContainer.name = 'input-text-container'
  textInputContainer.vAlign = 'bottom'
  textInputContainer.hAlign = 'left'
  textInputContainer.width = '100%'
  textInputContainer.height = '16%'

  textInput = new UIInputText(textInputContainer)
  textInput.name = 'input-text'
  textInput.autoStretchWidth = false
  textInput.color = INITIAL_INPUT_TEXT_COLOR
  textInput.background = Color4.Clear()
  textInput.focusedBackground = Color4.Clear()
  textInput.placeholder = 'Press enter and start talking...'
  textInput.fontSize = 14
  textInput.width = '90%'
  textInput.height = '16%'
  textInput.thickness = 0
  textInput.vAlign = 'center'
  textInput.hAlign = 'center'
  textInput.positionX = '-5px'
  textInput.vTextAlign = 'center'
  textInput.hTextAlign = 'left'
  textInput.value = ''
  textInput.textWrapping = true
  textInput.isPointerBlocker = true
  textInput.onFocus = new OnFocus(onInputFocus)
  textInput.onBlur = new OnBlur(onInputBlur)
  textInput.onTextSubmit = new OnTextSubmit(onInputSubmit)

  setMaximized(isMaximized)

  messagesLogText = new UIText(messagesLogStackContainer)
  messagesLogText.name = 'logged-message'
  messagesLogText.color = PRIMARY_TEXT_COLOR
  messagesLogText.fontSize = 14
  messagesLogText.vAlign = 'top'
  messagesLogText.hAlign = 'left'
  messagesLogText.vTextAlign = 'top'
  messagesLogText.hTextAlign = 'left'
  messagesLogText.width = '350px'
  messagesLogText.adaptWidth = false
  messagesLogText.adaptHeight = true
  messagesLogText.textWrapping = true
  messagesLogText.outlineColor = Color4.Black()

  const instructionsMessage = {
    id: '',
    isCommand: true,
    sender: 'Decentraland',
    timestamp: Date.now(),
    message: 'Type /help for info about controls'
  }

  addMessage(instructionsMessage)
}

export async function initializeChat() {
  createUi()

  const chatCmds = await execute('ChatController', 'getChatCommands', [null])
  const commandsList = []

  for (let i in chatCmds) {
    commandsList.push(chatCmds[i])
  }
}

function updateMessagesLog() {
  messagesLogText.value = ''
  for (let i = 0; i < internalState.messages.length; i++) {
    const currentMessage = internalState.messages[i]
    const color = currentMessage.isCommand ? COMMAND_COLOR : 'white'

    messagesLogText.value += `<color=${color}><b>${currentMessage.sender}:</b> ${currentMessage.message}</color>\n`
  }

  messagesLogScrollContainer.valueY = 0

  return { component: messagesLogText }
}

function setMaximized(newMaximizedValue: boolean) {
  if (isMaximized === newMaximizedValue) return

  if (newMaximizedValue && !isMaximized) {
    textInput.value = ''

    chatInnerTopContainer.color = new Color4(0, 0, 0, 0.2)
    textInputContainer.color = new Color4(0, 0, 0, 0.2)
  } else if (isMaximized) {
    chatInnerTopContainer.color = Color4.Clear()
    textInputContainer.color = Color4.Clear()
  }

  isMaximized = newMaximizedValue

  messagesLogScrollContainer.isVertical = isMaximized
}

function onInputFocus() {
  setMaximized(true)
}

function onInputBlur() {
  setMaximized(false)
}

async function onInputSubmit(e: { text: string }) {
  await sendMsg(e.text)
}

async function sendMsg(messageToSend: string) {
  if (messageToSend) {
    const message: MessageEntry | null = await execute('ChatController', 'send', [messageToSend])

    if (message) {
      addMessage(message)
    }
  }
}

function addMessage(messageEntry: MessageEntry): void {
  if (internalState.messages.length > MAX_LOGGED_MESSAGES) {
    // remove oldest message
    internalState.messages.shift()
  }

  internalState.messages.push(messageEntry)

  updateMessagesLog()
}
