import { UICanvas, UIContainerRect, Color4, UIInputText, log, OnTextSubmit, OnChanged } from 'decentraland-ecs/src'

const ui = new UICanvas()

const container = new UIContainerRect(ui)
container.name = 'testRectContainer'
container.color = Color4.Green()
container.width = '50%'
container.height = '50%'

const textInput = new UIInputText(container)
textInput.name = 'textInput'
textInput.width = '80%'
textInput.height = '25px'
textInput.vAlign = 'bottom'
textInput.hAlign = 'left'
textInput.fontSize = 10
textInput.placeholder = 'Write message here'
textInput.placeholderColor = Color4.Gray()
textInput.positionX = '10%'
textInput.positionY = '10px'
textInput.onTextSubmit = new OnTextSubmit(x => {
  container.color = Color4.Red()
  log('submitted text! ' + x.text)
})
textInput.onChanged = new OnChanged(x => {
  log('text changed: ' + x.value)
})
