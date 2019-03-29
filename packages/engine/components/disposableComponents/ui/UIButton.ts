import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { createSchemaValidator } from '../../helpers/schemaValidator'
import { parseVerticalAlignment, parseHorizontalAlignment } from 'engine/entities/utils/parseAttrs'
import { UIButtonShape } from 'decentraland-ecs/src/decentraland/UIShapes'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { UIControl } from './UIControl'

const schemaValidator = createSchemaValidator({
  id: { type: 'string', default: null },
  opacity: { type: 'number', default: 1 },
  cornerRadius: { type: 'number', default: 0 },
  fontFamily: { type: 'string', default: 'Arial' },
  fontSize: { type: 'number', default: 50 },
  fontWeight: { type: 'string', default: 'normal' },
  width: { type: 'string', default: '100%' },
  height: { type: 'string', default: '100%' },
  top: { type: 'string', default: '0px' },
  left: { type: 'string', default: '0px' },
  color: { type: 'string', default: 'white' },
  background: { type: 'string', default: 'black' },
  hAlign: { type: 'string', default: 'center' },
  vAlign: { type: 'string', default: 'center' },
  paddingTop: { type: 'string', default: '0px' },
  paddingRight: { type: 'string', default: '0px' },
  paddingBottom: { type: 'string', default: '0px' },
  paddingLeft: { type: 'string', default: '0px' },
  shadowBlur: { type: 'number', default: 0 },
  shadowOffsetX: { type: 'number', default: 0 },
  shadowOffsetY: { type: 'number', default: 0 },
  shadowColor: { type: 'string', default: '#fff' },
  thickness: { type: 'number', default: 0 },
  text: { type: 'string', default: 'button' },
  visible: { type: 'boolean', default: true },
  isPointerBlocker: { type: 'boolean', default: false }
})

class UIButton extends UIControl<UIButtonShape, BABYLON.GUI.Button> {
  control = new BABYLON.GUI.Button('button')
  textBlock: BABYLON.GUI.TextBlock

  constructor(ctx: SharedSceneContext, uuid: string) {
    super(ctx, uuid)

    this.textBlock = new BABYLON.GUI.TextBlock('button-text-' + uuid, '')
    this.textBlock.textWrapping = true
    this.textBlock.textHorizontalAlignment = BABYLON.GUI.Control.HORIZONTAL_ALIGNMENT_CENTER
    this.textBlock.paddingLeft = '10%'
    this.textBlock.paddingRight = '10%'
    this.control.addControl(this.textBlock)

    this.control.onPointerClickObservable.add($ => {
      this.dispatchOnClick($.buttonIndex)
    })
  }

  onAttach(_entity: BaseEntity): void {
    // noop
  }

  onDetach(_entity: BaseEntity): void {
    // noop
  }

  dispose() {
    if (this.control) {
      this.control.onPointerClickObservable.clear()
      this.control.dispose()
      delete this.control
    }

    if (this.textBlock) {
      this.textBlock.dispose()
      delete this.textBlock
    }

    super.dispose()
  }

  async updateData(data: UIButtonShape): Promise<void> {
    this.data = schemaValidator(data)

    this.control.alpha = Math.max(0, Math.min(1, this.data.opacity))
    this.control.cornerRadius = this.data.cornerRadius
    this.control.width = this.data.width
    this.control.height = this.data.height
    this.control.top = this.data.top
    this.control.left = this.data.left
    this.control.background = this.data.background
    this.control.verticalAlignment = parseVerticalAlignment(this.data.vAlign)
    this.control.horizontalAlignment = parseHorizontalAlignment(this.data.hAlign)
    this.control.paddingTop = this.data.paddingTop
    this.control.paddingRight = this.data.paddingRight
    this.control.paddingBottom = this.data.paddingBottom
    this.control.paddingLeft = this.data.paddingLeft
    this.control.shadowBlur = this.data.shadowBlur
    this.control.shadowOffsetX = this.data.shadowOffsetX
    this.control.shadowOffsetY = this.data.shadowOffsetY
    this.control.shadowColor = this.data.shadowColor
    this.control.thickness = this.data.thickness
    this.control.isVisible = this.data.visible
    this.control.isPointerBlocker = this.data.isPointerBlocker
    this.textBlock.isPointerBlocker = this.data.isPointerBlocker
    this.textBlock.fontFamily = this.data.fontFamily
    this.textBlock.fontSize = this.data.fontSize
    this.textBlock.fontWeight = this.data.fontWeight
    this.textBlock.text = this.data.text
    this.textBlock.color = this.data.color

    this.data.parentComponent && this.setParent(this.data.parentComponent)
  }

  dispatchOnClick = (pointerId: number) => {
    this.entities.forEach($ =>
      $.dispatchUUIDEvent('onClick', {
        entityId: $.uuid
      })
    )
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_BUTTON_SHAPE, UIButton)
