import { BasicShape, DisposableComponent } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { scene } from 'engine/renderer'

export class SphereShape extends BasicShape<{}> {
  generateModel() {
    return BABYLON.MeshBuilder.CreateSphere('sphere', { diameter: 1 }, scene)
  }
}

DisposableComponent.registerClassId(CLASS_ID.SPHERE_SHAPE, SphereShape)
