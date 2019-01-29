import { Entity, engine, OnClick, executeTask, IInteractionEvent } from 'decentraland-ecs/src'
import {
  UIImageShape,
  UIContainerRectShape,
  UITextShape,
  UIScreenSpaceShape,
  UIShape
} from 'decentraland-ecs/src/decentraland/UIShapes'
import { DecentralandInterface } from 'decentraland-ecs/src/decentraland/Types'

declare var dcl: DecentralandInterface
declare var require: any

const ATLAS_PATH = require('../../static/images/profile-ui.png')

type IState = {
  publicKey: string
  visible: boolean
  isMuted: boolean
  isBlocked: boolean
  avatarUrl: string
}

const internalState: IState = {
  publicKey: '',
  visible: false,
  isMuted: false,
  isBlocked: false,
  avatarUrl: ''
}

// -----------------------------

function createAvatar(parent: UIShape) {
  const component = new UIImageShape(parent)
  component.id = 'avatar'
  component.width = '128px'
  component.height = '128px'
  component.source = ATLAS_PATH
  component.sourceLeft = '0px'
  component.sourceTop = '0px'
  component.sourceWidth = '128px'
  component.sourceHeight = '128px'
  component.top = '-85px'
  component.visible = true

  return { component }
}

function createWinkButton(parent: UIShape, click: (event: IInteractionEvent) => void) {
  const component = new UIImageShape(parent)
  component.id = 'wink'
  component.width = '48px'
  component.height = '48px'
  component.source = ATLAS_PATH
  component.sourceLeft = '347px'
  component.sourceTop = '132px'
  component.sourceWidth = '48px'
  component.sourceHeight = '48px'
  component.isPointerBlocker = true

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnClick(click))
  engine.addEntity(entity)
  return { component, entity }
}

function createFriendButton(parent: UIShape, click: (event: IInteractionEvent) => void) {
  const component = new UIImageShape(parent)
  component.id = 'friend'
  component.width = '48px'
  component.height = '48px'
  component.source = ATLAS_PATH
  component.sourceLeft = '396px'
  component.sourceTop = '132px'
  component.sourceWidth = '48px'
  component.sourceHeight = '48px'
  component.left = '55px'
  component.isPointerBlocker = true

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnClick(click))
  engine.addEntity(entity)
  return { component, entity }
}

function createMuteButton(parent: UIShape, click: (event: IInteractionEvent) => void) {
  const component = new UIImageShape(parent)
  component.id = 'mute'
  component.width = '52px'
  component.height = '48px'
  component.source = ATLAS_PATH
  component.sourceLeft = '347px'
  component.sourceTop = '181px'
  component.sourceWidth = '52px'
  component.sourceHeight = '48px'
  component.isPointerBlocker = true

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnClick(click))
  engine.addEntity(entity)
  return { component, entity }
}

function createBlockButton(parent: UIShape, click: (event: IInteractionEvent) => void) {
  const component = new UIImageShape(parent)
  component.id = 'block'
  component.width = '52px'
  component.height = '48px'
  component.source = ATLAS_PATH
  component.sourceLeft = '400px'
  component.sourceTop = '181px'
  component.sourceWidth = '52px'
  component.sourceHeight = '48px'
  component.left = '55px'
  component.isPointerBlocker = true

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnClick(click))
  engine.addEntity(entity)
  return { component, entity }
}

function createCloseButton(parent: UIShape, click: (event: IInteractionEvent) => void) {
  const component = new UIImageShape(parent)
  component.id = 'close'
  component.width = '48px'
  component.height = '48px'
  component.source = ATLAS_PATH
  component.sourceLeft = '350px'
  component.sourceTop = '278px'
  component.sourceWidth = '48px'
  component.sourceHeight = '48px'
  component.left = '130px'
  component.top = '-170px'
  component.isPointerBlocker = true
  component.visible = false

  const entity = new Entity()
  entity.set(component)
  entity.set(new OnClick(click))
  engine.addEntity(entity)

  return { component, entity }
}

const hide = () => {
  dcl.log('hiding avatar-profile')
  internalState.visible = false
  closeButton.component.visible = false
  guiContainerComponent.visible = false
}

const toggleMute = async () => {
  const isMuted = !internalState.isMuted
  internalState.isMuted = isMuted
  muteButton.component.sourceTop = isMuted ? '230px' : '181px'

  if (isMuted) {
    await execute('SocialController', 'mute', [internalState.publicKey])
  } else {
    await execute('SocialController', 'unmute', [internalState.publicKey])
  }
}

const toggleBlock = async () => {
  const isBlocked = !internalState.isBlocked
  internalState.isBlocked = isBlocked
  blockButton.component.sourceTop = isBlocked ? '230px' : '181px'

  if (isBlocked) {
    await execute('SocialController', 'block', [internalState.publicKey])
  } else {
    await execute('SocialController', 'unblock', [internalState.publicKey])
  }
}

// -----------------------------------------

dcl.subscribe('SHOW_PROFILE')
dcl.subscribe('HIDE_PROFILE')
dcl.onEvent(event => {
  const eventType: string = event.type
  const eventData: any = event.data

  switch (eventType) {
    case 'SHOW_PROFILE':
      show(eventData)
      break

    case 'HIDE_PROFILE':
      hide()
      break

    default:
      break
  }
})

// ScreenSpace UI
const screenSpaceUI = new UIScreenSpaceShape()
screenSpaceUI.id = 'avatar-profile-ui'

// Main container
const guiContainerComponent = new UIContainerRectShape(screenSpaceUI)
guiContainerComponent.width = '300px'
guiContainerComponent.height = '400px'
guiContainerComponent.hAlign = 'right'
guiContainerComponent.cornerRadius = 40
guiContainerComponent.background = 'white'
guiContainerComponent.left = '-200px' // TODO: make it possible to do offsetX || '400px'
guiContainerComponent.visible = false

// background
const bgComponent = new UIImageShape(guiContainerComponent)
bgComponent.id = 'avatar_bg'
bgComponent.width = '96px'
bgComponent.height = '96px'
bgComponent.source = ATLAS_PATH
bgComponent.sourceLeft = '347px'
bgComponent.sourceTop = '1px'
bgComponent.sourceWidth = '96px'
bgComponent.sourceHeight = '96px'
bgComponent.top = '-80px'

let avatarIcon = createAvatar(guiContainerComponent)

// Display name
const displayNameComponent = new UITextShape(guiContainerComponent)
displayNameComponent.color = '#000'
displayNameComponent.fontSize = 24

const publicKeyComponent = new UITextShape(guiContainerComponent)
publicKeyComponent.color = '#999'
publicKeyComponent.fontSize = 18
publicKeyComponent.top = '30px'

// Friend, follow etc..
const friendshipsContainer = new UIContainerRectShape(guiContainerComponent)
friendshipsContainer.width = '450px'
friendshipsContainer.top = '150px'
friendshipsContainer.left = '-100px'

createWinkButton(friendshipsContainer, follow)
createFriendButton(friendshipsContainer, addFriend)

// Block, mute, etc...
const blockAndMuteContainer = new UIContainerRectShape(friendshipsContainer)
blockAndMuteContainer.left = '140px'

let muteButton = createMuteButton(blockAndMuteContainer, toggleMute)
let blockButton = createBlockButton(blockAndMuteContainer, toggleBlock)

// Close button
let closeButton = createCloseButton(guiContainerComponent, hide)

const show = (data: any) => {
  internalState.visible = true
  closeButton.component.visible = true
  guiContainerComponent.visible = true

  setAvatarIcon(data.avatarUrl)

  internalState.publicKey = data.publicKey

  const pubKeyShortened =
    data.publicKey.substring(0, 6) + '...' + data.publicKey.substring(data.publicKey.length - 6, data.publicKey.length)

  displayNameComponent.value = data.displayName
  publicKeyComponent.value = pubKeyShortened

  internalState.isMuted = data.isMuted
  muteButton.component.sourceTop = data.isMuted ? '230px' : '181px'

  internalState.isBlocked = data.isBlocked
  blockButton.component.sourceTop = data.isBlocked ? '230px' : '181px'
}

function follow() {
  // stub
}

function addFriend() {
  // stub
}

async function execute(controller: string, method: string, args: Array<any>) {
  return executeTask(async () => {
    return dcl.callRpc(controller, method, args)
  })
}

const setAvatarIcon = (url: string) => {
  avatarIcon.component.source = url
}

// ------------------------------------

// Initialize avatar profile scene

executeTask(async () => {
  await dcl.loadModule('@decentraland/SocialController')
})
