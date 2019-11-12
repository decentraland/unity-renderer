import { UICanvas, UIInputText, UIScrollRect, Color4, UIText, UIContainerRect } from 'decentraland-ecs/src'

import { OnTextSubmit } from 'decentraland-ecs/src/decentraland/UIEvents'

const ui = new UICanvas()

const container = new UIScrollRect(ui)
container.width = '50%'
container.height = '50%'
container.backgroundColor = Color4.Gray()
container.isVertical = true

const rt = new UIContainerRect(ui)
rt.width = '50%'
rt.height = '50%'
rt.color = Color4.Clear()
rt.isPointerBlocker = false

let curOffset = 0

const textInput = new UIInputText(rt)
textInput.width = '80%'
textInput.height = '25px'
textInput.vAlign = 'bottom'
textInput.hAlign = 'center'
textInput.fontSize = 10
textInput.placeholder = 'Write message here'
textInput.placeholderColor = Color4.Gray()
textInput.positionX = '25px'
textInput.positionY = '25px'
textInput.isPointerBlocker = true

textInput.onTextSubmit = new OnTextSubmit(x => {
  const text = new UIText(container)
  text.value = '<USER-ID> ' + x.text
  text.width = '100%'
  text.height = '20px'
  text.vAlign = 'top'
  text.hAlign = 'left'
  text.positionY = curOffset
  container.valueY = 1
  curOffset -= 25
})
