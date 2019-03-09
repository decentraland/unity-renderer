import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { createSchemaValidator } from '../../helpers/schemaValidator'
import { parseVerticalAlignment, parseHorizontalAlignment } from 'engine/entities/utils/parseAttrs'
import { UITextShape } from 'decentraland-ecs/src/decentraland/UIShapes'
import { UIControl } from './UIControl'

const schemaValidator = createSchemaValidator({
  id: { type: 'string', default: null },
  outlineWidth: { type: 'number', default: 0 },
  outlineColor: { type: 'string', default: '#fff' },
  color: { type: 'string', default: '#fff' },
  fontFamily: { type: 'string', default: 'Arial' },
  fontSize: { type: 'number', default: 100 },
  fontWeight: { type: 'string', default: 'normal' },
  opacity: { type: 'number', default: 1.0 },
  value: { type: 'string', default: '' },
  lineSpacing: { type: 'string', default: '0px' },
  lineCount: { type: 'number', default: 0 },
  resizeToFit: { type: 'boolean', default: false },
  textWrapping: { type: 'boolean', default: false },
  shadowBlur: { type: 'number', default: 0 },
  shadowOffsetX: { type: 'number', default: 0 },
  shadowOffsetY: { type: 'number', default: 0 },
  shadowColor: { type: 'string', default: '#fff' },
  zIndex: { type: 'number', default: 0 },
  hAlign: { type: 'string', default: 'center' },
  vAlign: { type: 'string', default: 'center' },
  hTextAlign: { type: 'string', default: 'center' },
  vTextAlign: { type: 'string', default: 'center' },
  width: { type: 'string', default: '100%' },
  height: { type: 'string', default: '100px' },
  top: { type: 'string', default: '0px' },
  left: { type: 'string', default: '0px' },
  paddingTop: { type: 'string', default: '0px' },
  paddingRight: { type: 'string', default: '0px' },
  paddingBottom: { type: 'string', default: '0px' },
  paddingLeft: { type: 'string', default: '0px' },
  visible: { type: 'boolean', default: true }
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
    this.control.color = this.data.color
    this.control.fontFamily = this.data.fontFamily
    this.control.fontSize = this.data.fontSize
    this.control.zIndex = this.data.zIndex
    this.control.shadowBlur = this.data.shadowBlur
    this.control.shadowOffsetX = this.data.shadowOffsetX
    this.control.shadowOffsetY = this.data.shadowOffsetY
    this.control.shadowColor = this.data.shadowColor
    this.control.lineSpacing = this.data.lineSpacing
    this.control.text = this.data.value
    this.control.textWrapping = this.data.textWrapping
    this.control.resizeToFit = this.data.resizeToFit
    this.control.horizontalAlignment = parseHorizontalAlignment(this.data.hAlign)
    this.control.verticalAlignment = parseVerticalAlignment(this.data.vAlign)
    this.control.textHorizontalAlignment = parseHorizontalAlignment(this.data.hTextAlign)
    this.control.textVerticalAlignment = parseVerticalAlignment(this.data.vTextAlign)
    this.control.outlineWidth = this.data.outlineWidth
    this.control.outlineColor = this.data.outlineColor
    this.control.fontWeight = this.data.fontWeight
    this.control.paddingTop = this.data.paddingTop
    this.control.paddingRight = this.data.paddingRight
    this.control.paddingBottom = this.data.paddingBottom
    this.control.paddingLeft = this.data.paddingLeft
    this.control.width = this.data.width
    this.control.height = this.data.height
    this.control.top = this.data.top
    this.control.left = this.data.left
    this.control.isVisible = this.data.visible

    this.setParent(this.data.parentComponent)
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_TEXT_SHAPE, UIText)
