import * as BABYLON from 'babylonjs'
import { scene } from '../renderer'
import { BaseEntity } from './BaseEntity'

// const schemaValidator = createSchemaValidator({
//   color: { type: 'string', default: '#000' },
//   fontFamily: { type: 'string', default: 'Arial' },
//   fontSize: { type: 'number', default: 48 },
//   value: { type: 'string', default: '' },
//   width: { type: 'number', default: 1 },
//   height: { type: 'number', default: 1 },
//   background: { type: 'string', default: '#fff' },
//   focusedBackground: { type: 'string', default: '#fff' },
//   outlineWidth: { type: 'number', default: 0 },
//   placeholder: { type: 'string', default: '' },
//   maxLength: { type: 'number', default: 140 }
// })

export class InputTextEntity extends BaseEntity {
  private mesh: BABYLON.Mesh
  private texture: BABYLON.GUI.AdvancedDynamicTexture
  private input: BABYLON.GUI.InputText

  dispose() {
    if (this.texture) {
      this.texture.removeControl(this.input)
      this.texture.dispose()
      delete this.texture
    }

    if (this.mesh) {
      this.mesh.dispose(false, true)
      delete this.mesh
    }

    super.dispose()
  }

  setAttributes(attrs) {
    // TODO: ECS
    // 1 const newAttrs = schemaValidator(Object.assign({}, this.attrs, attrs))
    // 1 super.setAttributes(newAttrs)

    if (!this.mesh) {
      this.generateGeometry()
    } else {
      this.setInputAttributes()
    }
  }

  generateGeometry() {
    this.mesh = BABYLON.MeshBuilder.CreatePlane(
      'plane-input',
      { width: this.attrs.width, height: this.attrs.height },
      scene
    )
    this.texture = BABYLON.GUI.AdvancedDynamicTexture.CreateForMesh(
      this.mesh,
      512 * this.attrs.width,
      512 * this.attrs.height,
      false
    )
    this.input = new BABYLON.GUI.InputText()

    this.input.onTextChangedObservable.add(inputText => {
      this.dispatchUUIDEvent('onChange', {
        value: inputText.text
      })
    })

    this.setInputAttributes()

    this.texture.addControl(this.input)

    this.setObject3D('mesh', this.mesh)
  }

  setInputAttributes() {
    this.input.color = this.attrs.color
    this.input.text = this.attrs.maxLength ? this.attrs.value.substr(0, this.attrs.maxLength) : this.attrs.value
    this.input.width = this.attrs.width
    this.input.height = this.attrs.height
    this.input.background = this.attrs.background
    this.input.focusedBackground = this.attrs.focusedBackground
    this.input.fontFamily = this.attrs.fontFamily
    this.input.fontSize = this.attrs.fontSize
    this.input.thickness = this.attrs.outlineWidth
    this.input.placeholderText = this.attrs.placeholder
  }
}
