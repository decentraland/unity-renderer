import { BasicShape, DisposableComponent } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'

const base = BABYLON.MeshBuilder.CreateBox('box', {
  updatable: false
})
base.convertToUnIndexedMesh()
base.setEnabled(false)

export class BoxShape extends BasicShape<{}> {
  generateModel() {
    const ret = new BABYLON.Mesh('box-instance')

    base.geometry!.applyToMesh(ret)

    return ret
  }
}

DisposableComponent.registerClassId(CLASS_ID.BOX_SHAPE, BoxShape)
