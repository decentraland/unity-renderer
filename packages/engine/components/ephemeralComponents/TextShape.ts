import { createSchemaValidator, validators } from '../helpers/schemaValidator'
import { BaseComponent } from '../BaseComponent'
import { scene } from 'engine/renderer'
import { parseVerticalAlignment, parseHorizontalAlignment } from 'engine/entities/utils/parseAttrs'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { CLASS_ID, TextShape as ECSTextShape } from 'decentraland-ecs/src'

const schemaValidator = createSchemaValidator({
  outlineWidth: { type: 'number', default: 0 },
  outlineColor: { type: 'string', default: '#ffffff' },
  color: { type: 'string', default: '#ffffff' },
  fontFamily: { type: 'string', default: 'Arial' },
  fontSize: { type: 'number', default: 100 },
  fontWeight: { type: 'number', default: 'normal' },
  opacity: { type: 'number', default: 1.0 },
  value: { type: 'string', default: '' },
  lineSpacing: { type: 'string', default: '0px' },
  lineCount: { type: 'number', default: 0 },
  resizeToFit: { type: 'boolean', default: false },
  textWrapping: { type: 'boolean', default: false },
  shadowBlur: { type: 'number', default: 0 },
  shadowOffsetX: { type: 'number', default: 0 },
  shadowOffsetY: { type: 'number', default: 0 },
  shadowColor: { type: 'string', default: '#ffffff' },
  zIndex: { type: 'number', default: 0 },
  hAlign: { type: 'string', default: 'center' },
  vAlign: { type: 'string', default: 'center' },
  width: { type: 'number', default: 1 },
  height: { type: 'number', default: 1 },
  paddingTop: { type: 'number', default: 0 },
  paddingRight: { type: 'number', default: 0 },
  paddingBottom: { type: 'number', default: 0 },
  paddingLeft: { type: 'number', default: 0 }
})

export class TextShape extends BaseComponent<ECSTextShape> {
  currentFont: any = null
  textBlock!: BABYLON.GUI.TextBlock | void
  texture!: BABYLON.GUI.AdvancedDynamicTexture | void
  private mesh!: BABYLON.Mesh | void

  transformValue(value: any) {
    return schemaValidator(value)
  }

  update() {
    this.generateGeometry()
    this.setTextAttributes()
  }

  generateGeometry() {
    if (this.mesh || !this.value) return

    this.mesh = BABYLON.MeshBuilder.CreatePlane(
      'text-plane',
      { width: this.value.width, height: this.value.height },
      scene
    )

    this.texture = BABYLON.GUI.AdvancedDynamicTexture.CreateForMesh(
      this.mesh,
      512 * this.value.width,
      512 * this.value.height,
      false
    )

    this.textBlock = new BABYLON.GUI.TextBlock()
    this.setTextAttributes()
    this.texture.addControl(this.textBlock)

    this.entity.setObject3D('text', this.mesh)

    this.textBlock.onPointerUpObservable.add($ => {
      this.entity.dispatchUUIDEvent('onClick', {
        entityId: this.entity.uuid
      })
    })
  }

  detach() {
    this.entity.removeObject3D('text')

    if (this.texture && this.textBlock) {
      this.texture.removeControl(this.textBlock)
      this.textBlock.dispose()
      this.texture.dispose()
      delete this.texture
    }

    if (this.mesh) {
      this.mesh.dispose(false, true)
      delete this.mesh
    }

    if (this.currentFont) {
      delete this.currentFont
    }
  }

  private setTextAttributes() {
    if (!this.textBlock) return
    if (this.value) {
      this.textBlock.alpha = Math.max(0, Math.min(1, this.value.opacity))
      this.textBlock.color = validators.color(this.value.color, BABYLON.Color3.Black()).toHexString()
      this.textBlock.fontFamily = this.value.fontFamily
      this.textBlock.fontSize = this.value.fontSize
      this.textBlock.zIndex = this.value.zIndex
      this.textBlock.shadowBlur = this.value.shadowBlur
      this.textBlock.shadowOffsetX = this.value.shadowOffsetX
      this.textBlock.shadowOffsetY = this.value.shadowOffsetY
      this.textBlock.shadowColor = validators.color(this.value.shadowColor, BABYLON.Color3.Black()).toHexString()
      this.textBlock.lineSpacing = this.value.lineSpacing
      this.textBlock.text = this.value.value || ''
      this.textBlock.textWrapping = this.value.textWrapping
      this.textBlock.resizeToFit = this.value.resizeToFit
      this.textBlock.textHorizontalAlignment = parseHorizontalAlignment(this.value.hAlign)
      this.textBlock.textVerticalAlignment = parseVerticalAlignment(this.value.vAlign)
      this.textBlock.outlineWidth = this.value.outlineWidth
      this.textBlock.outlineColor = validators.color(this.value.outlineColor, BABYLON.Color3.Black()).toHexString()
      this.textBlock.fontWeight = this.value.fontWeight
      this.textBlock.paddingTop = this.value.paddingTop
      this.textBlock.paddingRight = this.value.paddingRight
      this.textBlock.paddingBottom = this.value.paddingBottom
      this.textBlock.paddingLeft = this.value.paddingLeft

      if (this.mesh) {
        this.mesh.isPickable = this.value.isPickable
        this.mesh.billboardMode = this.value.billboard ? 7 : 0
      }
    }
  }
}

export function setEntityText(entity: BaseEntity, textComponent: Partial<ECSTextShape>) {
  entity.context.UpdateEntityComponent({
    classId: CLASS_ID.TEXT_SHAPE,
    entityId: entity.id,
    json: JSON.stringify(textComponent),
    name: 'text'
  })
}
