import { UICanvas, UIContainerRect, Color4 } from 'decentraland-ecs/src'

//const SEND_ICON = './images/send-icon.png'

const ui = new UICanvas()

const container = new UIContainerRect(ui)
container.name = 'testRectContainer'
container.color = Color4.Green()
container.width = '50%'
container.height = '50%'

const innerPanelTopLeft = new UIContainerRect(container)
innerPanelTopLeft.name = 'innerPanelTopLeft'
innerPanelTopLeft.color = Color4.Red()
innerPanelTopLeft.width = '25%'
innerPanelTopLeft.height = '100px'
innerPanelTopLeft.vAlign = 'top'
innerPanelTopLeft.hAlign = 'left'

const innerPanelCenterLeft = new UIContainerRect(container)
innerPanelCenterLeft.name = 'innerPanelCenterLeft'
innerPanelCenterLeft.color = Color4.Red()
innerPanelCenterLeft.width = '25%'
innerPanelCenterLeft.height = '100px'
innerPanelCenterLeft.hAlign = 'left'

const innerPanelBottomLeft = new UIContainerRect(container)
innerPanelBottomLeft.name = 'innerPanelBottomLeft'
innerPanelBottomLeft.color = Color4.Red()
innerPanelBottomLeft.width = '25%'
innerPanelBottomLeft.height = '25%'
innerPanelBottomLeft.vAlign = 'bottom'
innerPanelBottomLeft.hAlign = 'left'

const innerPanelTopCenter = new UIContainerRect(container)
innerPanelTopCenter.name = 'innerPanelTopCenter'
innerPanelTopCenter.color = Color4.Red()
innerPanelTopCenter.width = '25%'
innerPanelTopCenter.height = '25%'
innerPanelTopCenter.vAlign = 'top'

const innerPanelCenterCenter = new UIContainerRect(container)
innerPanelCenterCenter.name = 'innerPanelCenterCenter'
innerPanelCenterCenter.color = Color4.Red()
innerPanelCenterCenter.width = '25px'
innerPanelCenterCenter.height = 25

const innerPanelBottomCenter = new UIContainerRect(container)
innerPanelBottomCenter.name = 'innerPanelBottomCenter'
innerPanelBottomCenter.color = Color4.Red()
innerPanelBottomCenter.width = '25%'
innerPanelBottomCenter.height = '25%'
innerPanelBottomCenter.vAlign = 'bottom'

const innerPanelTopRight = new UIContainerRect(container)
innerPanelTopRight.name = 'innerPanelTopRight'
innerPanelTopRight.color = Color4.Red()
innerPanelTopRight.width = 100
innerPanelTopRight.height = '25%'
innerPanelTopRight.vAlign = 'top'
innerPanelTopRight.hAlign = 'right'

const innerPanelCenterRight = new UIContainerRect(container)
innerPanelCenterRight.name = 'innerPanelCenterRight'
innerPanelCenterRight.color = Color4.Red()
innerPanelCenterRight.width = '25%'
innerPanelCenterRight.height = '25%'
innerPanelCenterRight.hAlign = 'right'

const innerPanelBottomRight = new UIContainerRect(container)
innerPanelBottomRight.name = 'innerPanelBottomRight'
innerPanelBottomRight.color = Color4.Red()
innerPanelBottomRight.width = '25%'
innerPanelBottomRight.height = '25%'
innerPanelBottomRight.vAlign = 'bottom'
innerPanelBottomRight.hAlign = 'right'

const innerPanel1 = new UIContainerRect(container)
innerPanel1.name = 'innerPanel1'
innerPanel1.color = Color4.Blue()
innerPanel1.width = '50%'
innerPanel1.height = '50%'
innerPanel1.positionX = '-20%'
innerPanel1.positionY = '20%'
