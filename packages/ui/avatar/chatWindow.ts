import { DecentralandInterface, IEvents } from 'decentraland-ecs/src/decentraland/Types'
import { Color4, Texture, OnEnter } from 'decentraland-ecs/src'
import { OnTextSubmit, OnBlur, OnChanged, OnClick, OnFocus } from 'decentraland-ecs/src/decentraland/UIEvents'

import {
  UIImage,
  UIInputText,
  UIText,
  UIContainerStack,
  UIContainerRect,
  UIFullScreen,
  UIShape,
  UIScrollRect
} from 'decentraland-ecs/src/decentraland/UIShapes'

import { MessageEntry } from 'shared/types'

import { execute } from './rpc'
import { screenSpaceUI } from './ui'

declare var dcl: DecentralandInterface
declare var require: any

const UI_CHAT = require('../../../static/images/ui-chat.png')
const uiChatTexture = new Texture(UI_CHAT)

const PRIMARY_TEXT_COLOR = Color4.White()
const COMMAND_COLOR = Color4.FromHexString('#ffd7a9ff')

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

function createCloseButton(parent: UIShape, click: (ev: IEvents['onClick']) => void) {
  const component = new UIImage(parent, uiChatTexture)
  component.name = 'close-icon'
  component.width = '20px'
  component.height = '20px'
  component.sourceWidth = 35
  component.sourceHeight = 35
  component.sourceTop = 5
  component.sourceLeft = 96
  component.hAlign = 'right'
  component.vAlign = 'bottom'
  component.positionX = '-10px'
  component.isPointerBlocker = true
  component.visible = false
  component.onClick = new OnClick(click)

  return { component }
}

function createTextInput(
  parent: UIShape,
  onChanged: (value?: any, pointerId?: Number) => void,
  onFocus: () => void,
  onBlur: () => void
) {
  const component = new UIInputText(parent)
  component.name = 'input'
  component.autoStretchWidth = false
  component.color = PRIMARY_TEXT_COLOR
  component.background = Color4.Clear()
  component.focusedBackground = Color4.Clear()
  component.placeholder = 'Say something to nearby people...'
  component.fontSize = 12
  component.width = 380
  component.height = 20
  component.thickness = 0
  component.vAlign = 'bottom'
  component.hAlign = 'left'
  component.vTextAlign = 'bottom'
  component.hTextAlign = 'left'
  component.positionX = '10px'
  component.positionY = '15px'
  component.value = ''
  component.isPointerBlocker = true
  component.onChanged = new OnChanged(onChanged)
  component.onFocus = new OnFocus(onFocus)
  component.onBlur = new OnBlur(onBlur)
  component.onTextSubmit = new OnTextSubmit(onInputSubmit)

  return { component }
}

function createMessage(parent: UIShape, props: { sender: string; message: string; isCommand?: boolean }) {
  const { sender, message, isCommand } = props
  const color = isCommand ? COMMAND_COLOR : PRIMARY_TEXT_COLOR
  const component = new UIText(parent)

  component.color = color
  component.value = `<b>${sender}:</b> ${message}`
  component.fontSize = 12
  component.vTextAlign = 'top'
  component.hTextAlign = 'left'
  component.vAlign = 'top'
  component.hAlign = 'left'
  component.adaptWidth = false
  component.adaptHeight = true
  component.width = '300px'
  component.textWrapping = true

  return { component: component }
}

// -------------------------------
const internalState = {
  commandsList: [] as Array<any>,
  messages: [] as Array<any>,
  isFocused: false,
  isSliderVisible: false
}

let isMaximized: boolean = false

const containerMinimized = initializeMinimizedChat(screenSpaceUI)

const container = new UIContainerRect(screenSpaceUI)
container.name = 'gui-container'
container.color = new Color4(0, 0, 0, 0.2)
container.vAlign = 'bottom'
container.hAlign = 'left'
container.width = 400
container.height = 320
container.positionX = 20
container.positionY = 0
container.thickness = 0
container.visible = false

const transparentContainer = new UIScrollRect(screenSpaceUI)
transparentContainer.name = 'gui-transparent-container'
transparentContainer.vAlign = 'bottom'
transparentContainer.hAlign = 'left'
transparentContainer.width = '400px'
transparentContainer.height = '250px'
transparentContainer.positionX = '20px'
transparentContainer.positionY = '70px'
transparentContainer.valueY = 1
transparentContainer.isVertical = false
transparentContainer.isHorizontal = false
transparentContainer.visible = true

const transparentMessageContainer = new UIContainerStack(transparentContainer)
transparentMessageContainer.vAlign = 'bottom'
transparentMessageContainer.hAlign = 'left'
transparentMessageContainer.width = '100%'
transparentMessageContainer.height = '100%'
transparentMessageContainer.positionX = '10px'
transparentMessageContainer.spacing = 6

const footerContainer = new UIContainerRect(screenSpaceUI)
footerContainer.adaptHeight = true
footerContainer.adaptWidth = true
footerContainer.vAlign = 'bottom'
footerContainer.hAlign = 'left'
footerContainer.width = 400
footerContainer.height = 25
footerContainer.positionX = 20
footerContainer.positionY = 20
footerContainer.isPointerBlocker = true

const textInput = createTextInput(footerContainer, onInputChanged, onInputFocus, onInputBlur)

createCloseButton(container, toggleChat)
setMaximized(isMaximized)

// Initialize chat scene

export async function initializeChat() {
  const chatCmds = await execute('ChatController', 'getChatCommands', [null])
  const commandsList = []

  for (let i in chatCmds) {
    commandsList.push(chatCmds[i])
  }
}

function toggleChat() {
  setMaximized(!isMaximized)
}

function setMaximized(maximized: boolean) {
  container.visible = maximized
  containerMinimized.visible = !maximized
  transparentContainer.isVertical = maximized
  isMaximized = maximized

  if (!maximized) {
    textInput.component.value = ''
  }
}

function onInputFocus() {
  setMaximized(true)
}

function onInputBlur() {
  setMaximized(false)
}

function onInputChanged(message: string) {
  // noop
}

async function onInputSubmit(e: { text: string }) {
  await sendMsg(e.text)
}

async function sendMsg(messageToSend: string) {
  if (messageToSend) {
    const message = await execute('ChatController', 'send', [messageToSend])

    if (message) {
      addMessage(message)
    }
  }
}

function addMessage(messageEntry: MessageEntry): void {
  internalState.messages.push(messageEntry)
  createMessage(transparentMessageContainer, messageEntry)
}

function initializeMinimizedChat(parent: UIFullScreen) {
  const containerMinimized = new UIContainerRect(parent)
  containerMinimized.name = 'gui-container-minimized'
  containerMinimized.adaptHeight = true
  containerMinimized.adaptWidth = true
  containerMinimized.vAlign = 'bottom'
  containerMinimized.hAlign = 'left'
  containerMinimized.positionX = 20
  containerMinimized.positionY = 15
  containerMinimized.thickness = 0

  const minimizedIcon = new UIImage(containerMinimized, uiChatTexture)
  minimizedIcon.name = 'minimize-icon'
  minimizedIcon.width = '230px'
  minimizedIcon.height = '55px'
  minimizedIcon.sourceWidth = 210
  minimizedIcon.sourceHeight = 50
  minimizedIcon.sourceTop = 50
  minimizedIcon.sourceLeft = 0
  minimizedIcon.hAlign = 'right'
  minimizedIcon.vAlign = 'top'
  minimizedIcon.isPointerBlocker = true
  minimizedIcon.onClick = new OnClick(toggleChat)
  minimizedIcon.onEnter = new OnEnter(toggleChat)

  return containerMinimized
}
