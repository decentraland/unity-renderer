import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src/decentraland/Components'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { isMobile } from 'shared/comms/mobile'
import { createSchemaValidator } from 'engine/components/helpers/schemaValidator'

const MAXIMIZE_BUTTON = require('../../../../../static/images/open-ui.png')

const mobile = isMobile()

const schemaValidator = createSchemaValidator({
  visible: { type: 'boolean', default: true }
})

export class UIScreenSpace extends DisposableComponent {
  data: { visible: boolean } = { visible: true }

  screenSpace: BABYLON.GUI.Rectangle = new BABYLON.GUI.Rectangle('screenspace')
  private _isEnabled: boolean = false
  private outerSpace: BABYLON.GUI.Rectangle = new BABYLON.GUI.Rectangle('outerspace')
  private maximizeButton: BABYLON.GUI.Image = new BABYLON.GUI.Image('maximize-button')
  private texture: BABYLON.GUI.AdvancedDynamicTexture = BABYLON.GUI.AdvancedDynamicTexture.CreateFullscreenUI(
    'screenspace'
  )

  set isEnabled(enabled: boolean) {
    this._isEnabled = enabled
    this.maximizeButton.isEnabled = enabled
    this.setVisible(!enabled)
  }

  get isEnabled() {
    return this._isEnabled
  }

  constructor(ctx: SharedSceneContext, uuid: string) {
    super(ctx, uuid)

    this.generateGeometry()
  }

  onAttach(_entity: BaseEntity): void {
    // noop
  }

  onDetach(_entity: BaseEntity): void {
    // noop
  }

  dispose() {
    if (this.texture) {
      this.texture.dispose()
      delete this.texture
    }

    if (this.outerSpace) {
      this.outerSpace.onPointerUpObservable.clear()
      this.outerSpace.dispose()
      delete this.outerSpace
    }

    if (this.maximizeButton) {
      this.maximizeButton.onPointerUpObservable.clear()
      this.maximizeButton.dispose()
      delete this.maximizeButton
    }

    if (this.screenSpace) {
      this.screenSpace.clearControls()
      this.screenSpace.dispose()
      delete this.screenSpace
    }

    super.dispose()
  }

  async updateData(data: { visible: boolean }): Promise<void> {
    this.data = schemaValidator(data)
    this.setVisible(this.data.visible)
  }

  setVisible(visible: boolean) {
    this.screenSpace.isVisible = this.isEnabled && visible
    this.outerSpace.isVisible = this.isEnabled && visible
    this.maximizeButton.isVisible = this.isEnabled && !visible
  }

  dispatchOnClick = (pointerId: number) => {
    this.entities.forEach($ => {
      $.dispatchUUIDEvent('onClick', { entityId: $.uuid })
    })
  }

  private generateGeometry() {
    this.outerSpace.width = '100%'
    this.outerSpace.height = '100%'
    this.outerSpace.thickness = 0
    this.outerSpace.background = 'transparent'
    this.outerSpace.isVisible = false
    this.outerSpace.onPointerUpObservable.add(async $ => {
      if (this.data.visible) {
        await this.updateData({ visible: false })
      }

      this.dispatchOnClick($.buttonIndex)
      document.exitPointerLock()
    })
    this.texture.addControl(this.outerSpace)

    this.maximizeButton.width = '30px'
    this.maximizeButton.height = '30px'
    this.maximizeButton.source = MAXIMIZE_BUTTON
    this.maximizeButton.sourceWidth = 50
    this.maximizeButton.sourceHeight = 50
    this.maximizeButton.sourceTop = 0
    this.maximizeButton.sourceLeft = 0
    this.maximizeButton.verticalAlignment = BABYLON.GUI.Control.VERTICAL_ALIGNMENT_TOP
    this.maximizeButton.horizontalAlignment = BABYLON.GUI.Control.HORIZONTAL_ALIGNMENT_RIGHT
    this.maximizeButton.left = '-20px'
    this.maximizeButton.top = '30px'
    this.maximizeButton.isVisible = true
    this.maximizeButton.onPointerUpObservable.add(async $ => {
      await this.updateData({ visible: true })
      this.dispatchOnClick($.buttonIndex)
      document.exitPointerLock()
    })
    this.texture.addControl(this.maximizeButton)

    this.screenSpace.width = '100%'
    this.screenSpace.height = '100%'
    this.screenSpace.background = 'transparent'
    this.screenSpace.thickness = 0
    this.screenSpace.horizontalAlignment = BABYLON.GUI.Control.HORIZONTAL_ALIGNMENT_RIGHT
    this.screenSpace.verticalAlignment = BABYLON.GUI.Control.VERTICAL_ALIGNMENT_TOP
    this.screenSpace.paddingTop = mobile ? '20%' : '2%'
    this.screenSpace.paddingRight = mobile ? '18%' : '3%'
    this.screenSpace.paddingBottom = mobile ? '15%' : '24%'
    this.screenSpace.paddingLeft = mobile ? '40%' : '30%'
    this.screenSpace.alpha = 0.8
    this.screenSpace.isVisible = false
    ;(this.screenSpace as any).clipChildren = true
    // Block pointer lock when clicking anywhere on screen space UI part
    this.screenSpace.onPointerUpObservable.add($ => {
      this.dispatchOnClick($.buttonIndex)
      document.exitPointerLock()
    })
    this.texture.addControl(this.screenSpace)
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_SCREEN_SPACE_SHAPE, UIScreenSpace)
