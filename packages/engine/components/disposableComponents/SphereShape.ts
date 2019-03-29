import { BasicShape, DisposableComponent } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { scene } from 'engine/renderer'

const base = BABYLON.MeshBuilder.CreateSphere(
  'sphere',
  {
    diameter: 1,
    updatable: false,
    segments: 8
  },
  scene
)
base.setEnabled(false)

export class SphereShape extends BasicShape<{}> {
  generateModel() {
    const ret = new BABYLON.Mesh('sphere-instance')

    base.geometry!.applyToMesh(ret)

    return ret
  }
}

DisposableComponent.registerClassId(CLASS_ID.SPHERE_SHAPE, SphereShape)
