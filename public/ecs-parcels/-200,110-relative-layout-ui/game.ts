const SEND_ICON = './images/send-icon.png'

const ui = new UIScreenSpaceShape()
const uiEntity = new Entity()
uiEntity.addComponentOrReplace(ui)
engine.addEntity(uiEntity)

const container = new UIContainerRectShape(ui)
container.width = '100%'
container.height = '100%'
container.color = 'white' // we set global text color here
container.hAlign = 'center'
container.vAlign = 'center'

// We add separate rect container to act as a background with opacity
const bg = new UIContainerRectShape(container)
bg.opacity = 0.2
bg.thickness = 1
bg.cornerRadius = 10
bg.background = 'green'

// --- INVENTORY

const inventoryContainer = new UIContainerStackShape(container)
inventoryContainer.adaptWidth = true
// inventoryContainer.adaptHeight = true --- you can only set adaptWidth (X)OR adaptHeight for it to work correctly
inventoryContainer.width = '40%'
inventoryContainer.top = '100px'
inventoryContainer.left = '10px'
inventoryContainer.color = 'white'
inventoryContainer.background = 'blue'
inventoryContainer.hAlign = 'left'
inventoryContainer.vAlign = 'top'
inventoryContainer.vertical = true // when adapting height, set this to false

function generateInventoryItem(index: number) {
  const bg = new UIContainerRectShape(inventoryContainer)
  bg.id = `hmmm-${index}`
  bg.thickness = 1
  bg.background = 'green'
  // If `inventoryContainer.adaptWidth` OR `inventoryContainer.adaptHeight`
  // are set to true, you have to set width OR height of its children in pixels!
  bg.width = '100%'
  bg.height = '60px'
  bg.hAlign = 'center'
  bg.vAlign = 'top'

  const text = new UITextShape(bg)
  text.id = `hehe-${index}`
  text.value = `Item ${index}`
  text.vAlign = 'center'
  text.hAlign = 'center'
  text.resizeToFit = true
  text.fontSize = 10
  text.color = 'red'
}

generateInventoryItem(1)
generateInventoryItem(2)
generateInventoryItem(3)
generateInventoryItem(4)

// --- RIGHT SIDE OF THE UI

let inputTextState = ''

const input = new UIInputTextShape(container)
input.color = '#fff'
input.thickness = 1
input.fontFamily = 'Open Sans'
input.fontSize = 20
input.fontWeight = 'normal'
input.opacity = 1.0
input.placeholderColor = '#fff'
input.value = inputTextState
input.placeholder = 'write something...'
input.margin = '10px'
input.background = 'black'
input.focusedBackground = '#00a4a4'
input.shadowBlur = 10
input.shadowOffsetX = 5
input.shadowOffsetY = 5
input.shadowColor = '#c7c7c7'
input.hAlign = 'right'
input.vAlign = 'top'
input.width = '40%'
input.height = '30px'
input.top = '230px'
input.paddingRight = '20px'
// When you want to bind event listener to UI component, you have to
// create new entity and add component with event listener to it
const inputEntity = new Entity()
inputEntity.addComponentOrReplace(input)
inputEntity.addComponentOrReplace(
  new OnChanged((data: { value: string }) => {
    inputTextState = data.value
  })
)
engine.addEntity(inputEntity)

const sendButton = new Entity()
const sendButtonShape = new UIImageShape(container)
sendButtonShape.source = SEND_ICON
sendButtonShape.sourceWidth = '64px'
sendButtonShape.sourceHeight = '64px'
sendButtonShape.sourceTop = '0px'
sendButtonShape.sourceLeft = '0px'
sendButtonShape.width = '30px'
sendButtonShape.height = '30px'
sendButtonShape.hAlign = 'right'
sendButtonShape.top = '-40px'
sendButtonShape.left = '-20px'
sendButton.addComponentOrReplace(
  new OnClick(() => {
    if (inputTextState) {
      textFromInput.value = inputTextState
      // clear text from input
      input.value = ''
      inputTextState = ''
    }
  })
)
sendButton.addComponentOrReplace(sendButtonShape)
engine.addEntity(sendButton)

// --- SLIDERS

const valueFromSlider1 = new UITextShape(container)
valueFromSlider1.value = '0'
valueFromSlider1.vAlign = 'top'
valueFromSlider1.hAlign = 'right'
valueFromSlider1.width = '30px'
valueFromSlider1.fontSize = 30
valueFromSlider1.color = 'red'

const valueFromSlider2 = new UITextShape(container)
valueFromSlider2.top = '100px'
valueFromSlider2.value = '0'
valueFromSlider2.vAlign = 'top'
valueFromSlider2.hAlign = 'right'
valueFromSlider2.width = '30px'
valueFromSlider2.fontSize = 30
valueFromSlider2.color = 'black'

const slider1 = new Entity()
const sliderShape1 = new UISliderShape(container)
sliderShape1.minimum = 0
sliderShape1.maximum = 10
sliderShape1.color = '#fff'
sliderShape1.opacity = 1.0
sliderShape1.value = 0
sliderShape1.borderColor = '#fff'
sliderShape1.background = 'black'
sliderShape1.barOffset = '5px'
sliderShape1.thumbWidth = '30px'
sliderShape1.isThumbClamped = false
sliderShape1.isVertical = true
sliderShape1.hAlign = 'right'
sliderShape1.vAlign = 'top'
sliderShape1.width = '20px'
sliderShape1.height = '100px'
sliderShape1.top = '0px'
sliderShape1.left = '-60px'
slider1.addComponentOrReplace(
  new OnChanged((data: { value: number }) => {
    const value = Math.round(data.value)
    valueFromSlider1.value = value.toString()
  })
)
slider1.addComponentOrReplace(sliderShape1)
engine.addEntity(slider1)

const slider2 = new Entity()
const sliderShape2 = new UISliderShape(container)
sliderShape2.minimum = 0
sliderShape2.maximum = 10
sliderShape2.color = 'purple'
sliderShape2.opacity = 1.0
sliderShape2.value = 0
sliderShape2.borderColor = 'red'
sliderShape2.background = 'blue'
sliderShape2.barOffset = '5px'
sliderShape2.thumbWidth = '30px'
sliderShape2.isThumbCircle = true
sliderShape2.isThumbClamped = false
sliderShape2.isVertical = false
sliderShape2.hAlign = 'right'
sliderShape2.vAlign = 'top'
sliderShape2.width = '150px'
sliderShape2.height = '20px'
sliderShape2.top = '130px'
sliderShape2.left = '-40px'
sliderShape2.swapOrientation = true
slider2.addComponentOrReplace(
  new OnChanged((data: { value: number }) => {
    const value = Math.round(data.value)
    valueFromSlider2.value = value.toString()
  })
)
slider2.addComponentOrReplace(sliderShape2)
engine.addEntity(slider2)

const topText = new UITextShape(container)
topText.value = 'Some text'
topText.vAlign = 'top'
topText.fontSize = 20
topText.width = '200px'
topText.height = '25px'
topText.paddingTop = '10px'
topText.outlineWidth = 1
topText.outlineColor = 'lightblue'

const textFromInput = new UITextShape(container)
textFromInput.value = 'Type text to input and press send button'
textFromInput.hAlign = 'right'
textFromInput.vAlign = 'top'
textFromInput.top = '200px'
textFromInput.fontSize = 15
textFromInput.resizeToFit = true
textFromInput.paddingRight = '10px'

// --- CLOSE BUTTON

const closeButton = new Entity()
const closeShape = new UIButtonShape(container)
closeShape.text = 'Close UI'
closeShape.fontSize = 15
closeShape.color = 'black'
closeShape.background = 'yellow'
closeShape.cornerRadius = 10
closeShape.thickness = 1
closeShape.width = '120px'
closeShape.height = '30px'
closeShape.vAlign = 'bottom'
closeShape.top = '-80px'
closeShape.isPointerBlocker = false
closeButton.addComponentOrReplace(
  new OnClick(() => {
    ui.visible = false
  })
)
closeButton.addComponentOrReplace(closeShape)
engine.addEntity(closeButton)

// -----------------------------

// Let's place some trigger for opening some game UI (e.g. inventory) into the scene
// This TextShape is separate thing from screen-space UI
const instructionText = new TextShape('Open inventory by clicking on the box')
instructionText.width = 5
instructionText.fontSize = 40

const instructions = new Entity()
instructions.addComponentOrReplace(new Transform({ position: new Vector3(5, 1.5, 5) }))
instructions.addComponentOrReplace(instructionText)
engine.addEntity(instructions)

const uiTrigger = new Entity()
const transform = new Transform({ position: new Vector3(5, 1, 5), scale: new Vector3(0.3, 0.3, 0.3) })
uiTrigger.addComponentOrReplace(transform)

uiTrigger.addComponentOrReplace(
  new OnClick(() => {
    ui.visible = true
  })
)

uiTrigger.addComponentOrReplace(new BoxShape())

engine.addEntity(uiTrigger)
