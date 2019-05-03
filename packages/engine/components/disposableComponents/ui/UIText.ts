import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID, Color3 } from 'decentraland-ecs/src'
import { UIValue } from 'decentraland-ecs/src/ecs/UIValue'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { createSchemaValidator } from '../../helpers/schemaValidator'
import { parseVerticalAlignment, parseHorizontalAlignment } from 'engine/entities/utils/parseAttrs'
import { UIText as UITextShape } from 'decentraland-ecs/src/decentraland/UIShapes'
import { UIControl } from './UIControl'

const schemaValidator = createSchemaValidator({
  id: { type: 'string', default: null },
  visible: { type: 'boolean', default: true },
  hAlign: { type: 'string', default: 'center' },
  vAlign: { type: 'string', default: 'center' },
  zIndex: { type: 'number', default: 0 },
  positionX: { type: 'uiValue', default: new UIValue(0) },
  positionY: { type: 'uiValue', default: new UIValue(0) },
  width: { type: 'number', default: 100 },
  height: { type: 'number', default: 20 },
  isPointerBlocker: { type: 'boolean', default: false },

  paddingTop: { type: 'number', default: 0 },
  paddingRight: { type: 'number', default: 0 },
  paddingBottom: { type: 'number', default: 0 },
  paddingLeft: { type: 'number', default: 0 },

  color: { type: 'color', default: Color3.White() },
  opacity: { type: 'number', default: 1.0 },

  outlineWidth: { type: 'number', default: 0 },
  outlineColor: { type: 'color', default: Color3.White() },
  fontFamily: { type: 'string', default: 'Arial' },
  fontSize: { type: 'number', default: 100 },
  fontWeight: { type: 'string', default: 'normal' },
  value: { type: 'string', default: '' },
  lineSpacing: { type: 'string', default: '0px' },
  lineCount: { type: 'number', default: 0 },
  adaptWidth: { type: 'boolean', default: false },
  adaptHeight: { type: 'boolean', default: false },
  textWrapping: { type: 'boolean', default: false },
  shadowBlur: { type: 'number', default: 0 },
  shadowOffsetX: { type: 'number', default: 0 },
  shadowOffsetY: { type: 'number', default: 0 },
  shadowColor: { type: 'color', default: BABYLON.Color3.White() },
  hTextAlign: { type: 'string', default: 'center' },
  vTextAlign: { type: 'string', default: 'center' }
})

class UIText extends UIControl<UITextShape, BABYLON.GUI.TextBlock> {
  control = new BABYLON.GUI.TextBlock('text', '')

  onAttach(_entity: BaseEntity): void {
    // noop
  }

  onDetach(_entity: BaseEntity): void {
    // noop
  }

  dispose() {
    if (this.control) {
      this.control.dispose()
      delete this.control
    }

    super.dispose()
  }

  async updateData(data: UITextShape): Promise<void> {
    this.data = schemaValidator(data)

    this.control.alpha = Math.max(0, Math.min(1, this.data.opacity))
    this.control.color = this.data.color.toHexString()
    this.control.fontFamily = 'Arial'
    this.control.fontSize = this.data.fontSize
    this.control.shadowBlur = this.data.shadowBlur
    this.control.shadowOffsetX = this.data.shadowOffsetX
    this.control.shadowOffsetY = this.data.shadowOffsetY
    this.control.shadowColor = this.data.shadowColor.toHexString()
    this.control.lineSpacing = this.data.lineSpacing
    this.control.text = this.data.value
    this.control.textWrapping = this.data.textWrapping
    this.control.horizontalAlignment = parseHorizontalAlignment(this.data.hAlign)
    this.control.verticalAlignment = parseVerticalAlignment(this.data.vAlign)
    this.control.textHorizontalAlignment = parseHorizontalAlignment(this.data.hTextAlign)
    this.control.textVerticalAlignment = parseVerticalAlignment(this.data.vTextAlign)
    this.control.outlineWidth = this.data.outlineWidth
    this.control.outlineColor = this.data.outlineColor.toHexString()
    this.control.fontWeight = this.data.fontWeight
    this.control.paddingTop = this.data.paddingTop
    this.control.paddingRight = this.data.paddingRight
    this.control.paddingBottom = this.data.paddingBottom
    this.control.paddingLeft = this.data.paddingLeft
    this.control.width = this.data.width
    this.control.height = this.data.height
    this.control.top = -this.data.positionY
    this.control.left = this.data.positionX
    this.control.isVisible = this.data.visible

    this.data.parentComponent && this.setParent(this.data.parentComponent)
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_TEXT_SHAPE, UIText)
