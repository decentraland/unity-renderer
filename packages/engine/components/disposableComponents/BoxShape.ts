import { BasicShape, DisposableComponent } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'

export class BoxShape extends BasicShape<{}> {
  generateModel() {
    return BABYLON.MeshBuilder.CreateBox('box', {
      updatable: false
    })
  }
}

DisposableComponent.registerClassId(CLASS_ID.BOX_SHAPE, BoxShape)
