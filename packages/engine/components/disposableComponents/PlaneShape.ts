import { BasicShape, DisposableComponent } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { validators } from '../helpers/schemaValidator'
import { scene } from 'engine/renderer'

const defaultAttributes = {
  width: 1,
  height: 1,
  uvs: []
}

export class PlaneShape extends BasicShape<typeof defaultAttributes> {
  generateModel() {
    const uvs = this.data.uvs ? validators.floatArray(this.data.uvs, []) : null
    const mesh = BABYLON.MeshBuilder.CreatePlane(
      'plane-shape',
      {
        width: validators.float(this.data.width, defaultAttributes.width),
        height: validators.float(this.data.height, defaultAttributes.height),
        sideOrientation: 2,
        updatable: uvs ? true : false
      },
      scene
    )

    if (uvs) {
      mesh.updateVerticesData(BABYLON.VertexBuffer.UVKind, uvs)
    }

    return mesh
  }
}

DisposableComponent.registerClassId(CLASS_ID.PLANE_SHAPE, PlaneShape)
