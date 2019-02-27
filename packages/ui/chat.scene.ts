import { DecentralandInterface, IEvents } from 'decentraland-ecs/src/decentraland/Types'
import { Entity, engine, OnChanged, OnClick, executeTask } from 'decentraland-ecs/src'
import {
  UIImageShape,
  UIInputTextShape,
  UITextShape,
  UIContainerStackShape,
  UISliderShape,
  UIContainerRectShape,
  UIFullScreenShape,
  UIShape
} from 'decentraland-ecs/src/decentraland/UIShapes'

declare var dcl: DecentralandInterface
declare var require: any

const UI_CHAT = require('../../static/images/ui-chat.png')

// ScreenSpace UI
const parent = new UIFullScreenShape()

const MAX_CHARS = 94
const PRIMARY_TEXT_COLOR = 'white'
const COMMAND_COLOR = '#d7a9ff'

type MessageEntry = {
  id?: string
  sender: string
  message: string
  isCommand?: boolean
}

// UI creators -------------------

function createMinimizeButton(parent: UIShape, click: (ev: IEvents['onClick']) => void) {
  const component = new UIImageShape(parent)
  component.id = 'minimize-icon'
  component.width = '20px'
  component.height = '20px'
  component.source = UI_CHAT
  component.sourceWidth = '40px'
  component.sourceHeight = '40px'
  component.sourceTop = '10px'
  component.sourceLeft = '130px'
  component.hAlign = 'right'
  component.top = '0px'
  component.left = '-10px'
  component.isPointerBlocker = true

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnClick(click))
  engine.addEntity(entity)

  return { entity, component }
}

function createSendButton(parent: UIShape, click: (ev: IEvents['onClick']) => void) {
  const component = new UIImageShape(parent)
  component.id = 'send-icon'
  component.width = '23px'
  component.height = '23px'
  component.source = UI_CHAT
  component.sourceWidth = '48px'
  component.sourceHeight = '48px'
  component.sourceTop = '0px'
  component.sourceLeft = '48px'
  component.hAlign = 'right'
  component.left = '-10px'
  component.isPointerBlocker = true

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnClick(click))
  engine.addEntity(entity)

  return { entity, component }
}

function createHelpButton(parent: UIShape, click: (ev: IEvents['onClick']) => void) {
  const component = new UIImageShape(parent)
  component.id = 'help-icon'
  component.width = '23px'
  component.height = '23px'
  component.source = UI_CHAT
  component.sourceWidth = '48px'
  component.sourceHeight = '48px'
  component.sourceTop = '0px'
  component.sourceLeft = '0px'
  component.hAlign = 'right'
  component.left = '-10px'
  component.isPointerBlocker = true

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnClick(click))
  engine.addEntity(entity)

  return { entity, component }
}

function createCloseButton(parent: UIShape, click: (ev: IEvents['onClick']) => void) {
  const component = new UIImageShape(parent)
  component.id = 'close-icon'
  component.width = '20px'
  component.height = '20px'
  component.source = UI_CHAT
  component.sourceWidth = '35px'
  component.sourceHeight = '35px'
  component.sourceTop = '5px'
  component.sourceLeft = '96px'
  component.hAlign = 'right'
  component.left = '-10px'
  component.isPointerBlocker = true
  component.visible = false

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnClick(click))
  engine.addEntity(entity)

  return { entity, component }
}

function createHelpCloseButton(parent: UIShape, click: (data: IEvents['onClick']) => void) {
  const component = new UIImageShape(parent)
  component.id = 'help-close-icon'
  component.width = '25px'
  component.height = '25px'
  component.source = UI_CHAT
  component.sourceWidth = '59px'
  component.sourceHeight = '60px'
  component.sourceTop = '-5px'
  component.sourceLeft = '75px'
  component.hAlign = 'right'
  component.left = '-10px'
  component.top = '-5px'
  component.isPointerBlocker = true

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnClick(click))
  engine.addEntity(entity)

  return { entity, component }
}

function createTextInput(parent: UIShape, changed: (ev: IEvents['onChange']) => void) {
  const component = new UIInputTextShape(parent)
  component.id = 'input'
  component.autoStretchWidth = false
  component.color = PRIMARY_TEXT_COLOR
  component.background = 'black'
  component.focusedBackground = 'black'
  component.placeholder = 'Type a message...'
  component.fontSize = 15
  component.width = '400px'
  component.height = '40px'
  component.thickness = 0
  component.vAlign = 'bottom'
  component.hAlign = 'left'
  component.left = '5px'
  component.value = ''
  component.isPointerBlocker = true

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnChanged(changed))
  engine.addEntity(entity)

  return { component }
}

function renderSender(parent: UIShape, props: { color: string; sender: string }) {
  const component = new UITextShape(parent)
  component.color = props.color
  component.value = `${props.sender}: `
  component.fontSize = 14
  component.fontWeight = 'bold'
  component.hTextAlign = 'left'
  component.vTextAlign = 'top'
  component.hAlign = 'left'
  component.vAlign = 'top'
  component.resizeToFit = true

  return { component }
}

function renderMessage(parent: UIShape, props: { color: string; message: string }) {
  const component = new UITextShape(parent)
  component.width = '320px'
  component.color = props.color
  component.value = props.message
  component.fontSize = 14
  component.left = '10px'
  component.height = '30px'
  component.vTextAlign = 'top'
  component.hTextAlign = 'left'
  component.textWrapping = true

  return { component }
}

function createMessage(parent: UIShape, props: { sender: string; message: string; isCommand?: boolean }) {
  const { sender, message, isCommand } = props
  const color = isCommand ? COMMAND_COLOR : PRIMARY_TEXT_COLOR

  const stack = new UIContainerStackShape(parent)
  stack.vertical = false
  stack.hAlign = 'left'
  stack.vAlign = 'bottom'
  stack.height = '30px'
  stack.width = '400px'
  stack.top = '-50px'

  renderSender(stack, { color, sender })
  renderMessage(stack, { color, message })

  return { component: stack }
}

function createMessagesScrollbar(parent: UIShape, changed: (ev: IEvents['onChange']) => void) {
  const component = new UISliderShape(parent)
  component.id = 'slider'
  component.height = '170px'
  component.width = '20px'
  component.left = '185px'
  component.minimum = -45
  component.isVertical = true
  component.maximum = -45
  component.value = -45
  component.paddingLeft = '0px'
  component.visible = false
  component.isThumbCircle = true
  component.thumbWidth = '15px'
  component.barOffset = '8px'
  component.color = '#333333'
  component.background = '#262626'
  component.isPointerBlocker = true

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnChanged(changed))
  engine.addEntity(entity)

  return { entity, component }
}

function createChatHeader(parent: UIShape) {
  const container = new UIContainerRectShape(parent)
  container.id = 'gui-container-header'
  container.vAlign = 'top'
  container.hAlign = 'left'
  container.width = '400px'
  container.height = '45px'
  container.thickness = 0
  container.background = 'black'

  const headerTextComponent = new UITextShape(parent)
  headerTextComponent.color = PRIMARY_TEXT_COLOR
  headerTextComponent.value = 'Chat'
  headerTextComponent.fontSize = 17
  headerTextComponent.hAlign = 'left'
  headerTextComponent.vAlign = 'top'
  headerTextComponent.hTextAlign = 'left'
  headerTextComponent.vTextAlign = 'top'
  headerTextComponent.top = '15px'
  headerTextComponent.left = '15px'
  headerTextComponent.width = '100px'
  headerTextComponent.height = '40px'

  return { container, headerTextComponent }
}

function createCommandHelper(parent: UIShape, props: { name: string; description: string }) {
  const container = new UIContainerStackShape(parent)
  container.height = '55px'

  const cmdNameComponent = new UITextShape(container)
  cmdNameComponent.color = COMMAND_COLOR
  cmdNameComponent.value = `/${props.name}`
  cmdNameComponent.fontSize = 14
  cmdNameComponent.fontWeight = 'bold'
  cmdNameComponent.width = '100%'
  cmdNameComponent.height = '25px'
  cmdNameComponent.hTextAlign = 'left'

  const cmdDescriptionComponent = new UITextShape(container)
  cmdDescriptionComponent.color = '#7d8499'
  cmdDescriptionComponent.value = props.description
  cmdDescriptionComponent.fontSize = 13
  cmdDescriptionComponent.textWrapping = true
  cmdDescriptionComponent.width = '100%'
  cmdDescriptionComponent.height = '30px'
  cmdDescriptionComponent.vTextAlign = 'top'
  cmdDescriptionComponent.hTextAlign = 'left'
}

// -------------------------------
const messageHeight: number = 30
const internalState = {
  commandsList: [] as Array<any>,
  messages: [] as Array<any>,
  isFocused: false,
  sliderMin: -45,
  sliderMax: -45,
  isSliderVisible: false,
  sliderValue: -45,
  helpPanelTop: 50,
  isHelpVisible: false,
  rotate: false
}

dcl.subscribe('MESSAGE_RECEIVED')
dcl.subscribe('MESSAGE_SENT')
dcl.onEvent(event => {
  const eventType: string = event.type
  const eventData: any = event.data
  if (eventType === 'MESSAGE_RECEIVED' || eventType === 'MESSAGE_SENT') {
    addMessage(eventData.messageEntry)
  }
})

const containerMinimized = initializeMinimizedChat(parent)

function openHelp() {
  internalState.isHelpVisible = true

  container!.visible = false
  containerMinimized!.visible = false
  helpContainer!.visible = true
}

function closeHelp() {
  internalState.isHelpVisible = false

  container!.visible = true
  containerMinimized!.visible = false
  helpContainer!.visible = false
}

function toggleChat() {
  const visible = container!.visible

  container!.visible = !visible
  containerMinimized!.visible = visible
}

function onSliderChanged(data: any) {
  const value = Math.round(data.value)
  sliderOpenedChat.component.value = value
  messageContainer!.top = `${value}px`
}

function onHelpSliderChanged(data: any) {
  const value = Math.round(data.value)
  helpSliderComponent.value = value
  commandsContainerStack.top = `${-value}px`
}

function onInputChanged(data: any) {
  const { value } = textInput.component

  // set proper color
  if (value.charAt(0) === '/') {
    textInput.component.color = COMMAND_COLOR
  } else {
    textInput.component.color = PRIMARY_TEXT_COLOR
  }

  if (value.length < MAX_CHARS) {
    textInput.component.value = data.value
  } else {
    textInput.component.value = data.value.slice(0, MAX_CHARS)
  }
}

async function sendMsg() {
  const currentMessage = textInput.component.value

  if (currentMessage) {
    const cmd = await execute('ChatController', 'send', [currentMessage])
    // If the command was recognized, add the confirming message to the list
    if (cmd) {
      addMessage(cmd as MessageEntry)
    }

    // Clear input
    textInput.component.value = ''
  }
}

function addMessage(messageEntry: MessageEntry): void {
  if (internalState.messages.length <= 6) {
    internalState.messages.push(messageEntry)
    if (internalState.messages.length > 0) {
      addEntryAndResize(messageEntry)
    }
  } else {
    internalState.messages = [...internalState.messages, messageEntry]
    sliderOpenedChat.component.maximum = getMessagesListHeight() - 160 // makes it always scroll to latest msg
    sliderOpenedChat.component.value = -45
    sliderOpenedChat.component.visible = true
    addEntryAndResize(messageEntry)
  }
}

function addEntryAndResize(messageEntry: MessageEntry) {
  messageContainer!.height = `${getMessagesListHeight()}px`
  createMessage(messageContainer, messageEntry)
}

const container = new UIContainerRectShape(parent)
container.id = 'gui-container'
container.vAlign = 'bottom'
container.hAlign = 'left'
container.width = '400px'
container.height = '250px'
container.cornerRadius = 20
container.left = '20px'
container.top = '-20px'
container.color = PRIMARY_TEXT_COLOR
container.thickness = 0
container.background = 'black'
container.visible = false

const messageContainer = new UIContainerStackShape(container)
messageContainer.vAlign = 'bottom'
messageContainer.hAlign = 'left'
messageContainer.top = '-105px'
messageContainer.left = '15px'
messageContainer.height = '200px'

const footerContainer = new UIContainerRectShape(container)
footerContainer.adaptHeight = true
footerContainer.adaptWidth = true
footerContainer.vAlign = 'bottom'
footerContainer.hAlign = 'left'

const textInput = createTextInput(footerContainer, onInputChanged)

createHelpButton(footerContainer, openHelp)
createSendButton(footerContainer, sendMsg)
createCloseButton(footerContainer, toggleChat)

// Slider for opened chat
const sliderOpenedChat = createMessagesScrollbar(container, onSliderChanged)

// Chat header text
const chatHeader = createChatHeader(container)
createMinimizeButton(chatHeader.container, toggleChat)

function initializeMinimizedChat(parent: UIFullScreenShape) {
  const containerMinimized = new UIContainerRectShape(parent)
  containerMinimized.id = 'gui-container-minimized'
  containerMinimized.adaptHeight = true
  containerMinimized.adaptWidth = true
  containerMinimized.vAlign = 'bottom'
  containerMinimized.hAlign = 'left'
  containerMinimized.left = '20px'
  containerMinimized.top = '-15px'
  containerMinimized.thickness = 0
  containerMinimized.background = 'transparent'

  const minimizedIcon = new UIImageShape(containerMinimized)
  minimizedIcon.id = 'minimize-icon'
  minimizedIcon.width = '230px'
  minimizedIcon.height = '55px'
  minimizedIcon.source = UI_CHAT
  minimizedIcon.sourceWidth = '210px'
  minimizedIcon.sourceHeight = '50px'
  minimizedIcon.sourceTop = '50px'
  minimizedIcon.sourceLeft = '0px'
  minimizedIcon.hAlign = 'right'
  minimizedIcon.vAlign = 'top'
  minimizedIcon.isPointerBlocker = true

  const minimizedIconEntity = new Entity()
  minimizedIconEntity.set(minimizedIcon)
  minimizedIconEntity.set(new OnClick(toggleChat))
  engine.addEntity(minimizedIconEntity)

  const helpIcon = createHelpButton(containerMinimized, openHelp)
  helpIcon.component.top = '-5px'

  return containerMinimized
}

const helpContainer = new UIContainerRectShape(parent)
helpContainer.id = 'gui-container-commands'
helpContainer.vAlign = 'bottom'
helpContainer.hAlign = 'left'
helpContainer.width = '400px'
helpContainer.height = '250px'
helpContainer.cornerRadius = 20
helpContainer.left = '20px'
helpContainer.top = '-20px'
helpContainer.color = PRIMARY_TEXT_COLOR
helpContainer.thickness = 0
helpContainer.background = 'black'
helpContainer.visible = false

const commandsContainerStack = new UIContainerStackShape(helpContainer)
commandsContainerStack.vAlign = 'top'
commandsContainerStack.hAlign = 'left'
commandsContainerStack.top = '50px'
commandsContainerStack.left = '15px'
commandsContainerStack.height = `55px`
commandsContainerStack.width = '320px'

const helpSliderComponent = new UISliderShape(helpContainer)
helpSliderComponent.id = 'help-slider'
helpSliderComponent.height = '170px'
helpSliderComponent.width = '20px'
helpSliderComponent.left = '185px'
helpSliderComponent.minimum = 0
helpSliderComponent.isVertical = true
helpSliderComponent.value = 0
helpSliderComponent.paddingLeft = '0px'
helpSliderComponent.top = '10px'
helpSliderComponent.swapOrientation = true
helpSliderComponent.isThumbCircle = true
helpSliderComponent.thumbWidth = '15px'
helpSliderComponent.barOffset = '8px'
helpSliderComponent.color = '#333333'
helpSliderComponent.background = '#262626'
helpSliderComponent.isPointerBlocker = true

const sliderEntity = new Entity()
sliderEntity.set(helpSliderComponent)
sliderEntity.set(new OnChanged(onHelpSliderChanged))
engine.addEntity(sliderEntity)

const closeButtonContainer = new UIContainerRectShape(helpContainer)
closeButtonContainer.adaptHeight = true
closeButtonContainer.adaptWidth = true
closeButtonContainer.vAlign = 'bottom'
closeButtonContainer.hAlign = 'right'

createHelpCloseButton(closeButtonContainer, closeHelp)

const headerContainer = new UIContainerRectShape(helpContainer)
headerContainer.id = 'gui-container-header'
headerContainer.vAlign = 'top'
headerContainer.hAlign = 'left'
headerContainer.width = '400px'
headerContainer.height = '45px'
headerContainer.thickness = 0
headerContainer.background = 'black'

const headerTextComponent = new UITextShape(helpContainer)
headerTextComponent.color = PRIMARY_TEXT_COLOR
headerTextComponent.value = 'Commands'
headerTextComponent.fontSize = 17
headerTextComponent.hAlign = 'left'
headerTextComponent.vAlign = 'top'
headerTextComponent.hTextAlign = 'left'
headerTextComponent.vTextAlign = 'top'
headerTextComponent.top = '15px'
headerTextComponent.left = '15px'
headerTextComponent.height = '40px'

async function execute(controller: string, method: string, args: Array<any>) {
  return executeTask(async () => {
    return dcl.callRpc(controller, method, args)
  })
}

function getMessagesListHeight() {
  return internalState.messages.length * messageHeight
}

// ------------------------------------

// Initialize chat scene

async function initializeCommandsHelp() {
  const chatCmds = await execute('ChatController', 'getChatCommands', [null])
  const commandsList = []

  for (let i in chatCmds) {
    commandsList.push(chatCmds[i])
  }

  if (commandsList.length > 0) {
    commandsList.map(cmd => {
      createCommandHelper(commandsContainerStack, cmd)
    })
  }

  const commandHeight = 55
  const commandsListHeight = commandsList.length * commandHeight

  commandsContainerStack.height = `${commandsListHeight}px`
  helpSliderComponent.maximum = commandsListHeight - commandHeight
}

executeTask(async () => {
  await Promise.all([dcl.loadModule('@decentraland/ChatController'), dcl.loadModule('@decentraland/Identity')])
  await initializeCommandsHelp()
})
