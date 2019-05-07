import { UICanvas, Color4, UIContainerRect, UIImage, Texture, Color3, UIText } from 'decentraland-ecs/src'

import { OnClick } from 'decentraland-ecs/src/decentraland/UIEvents'

const ui = new UICanvas()

const panel = new UIContainerRect(ui)
panel.width = '50%'
panel.height = '50%'
panel.color = Color4.White()

const clickableImage = new UIImage(panel, new Texture('icon.png'))
clickableImage.name = 'clickable-image'
clickableImage.width = '92px'
clickableImage.height = '91px'
clickableImage.sourceWidth = 92
clickableImage.sourceHeight = 91
clickableImage.isPointerBlocker = true
clickableImage.onClick = new OnClick(() => {
  var randomColor = Color3.Random()
  panel.color = new Color4(randomColor.r, randomColor.g, randomColor.b, 1)
})

const imageChildPanel = new UIContainerRect(clickableImage)
imageChildPanel.height = '100px'
imageChildPanel.width = '100px'
imageChildPanel.positionX = '200px'
imageChildPanel.positionY = '-200px'
imageChildPanel.color = Color4.Red()

const text = new UIText(panel)
text.value = 'CLICK ON THE ICON'
text.color = Color4.Black()
text.fontSize = 15
text.hAlign = 'center'
text.vAlign = 'bottom'
