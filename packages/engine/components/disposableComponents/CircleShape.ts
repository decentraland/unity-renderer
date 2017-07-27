import { BasicShape, DisposableComponent } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { validators } from '../helpers/schemaValidator'

const defaultAttributes = {
  segments: 36,
  arc: 360
}

export class CircleShape extends BasicShape<typeof defaultAttributes> {
  generateModel() {
    return BABYLON.MeshBuilder.CreateDisc('circle', {
      tessellation: validators.float(this.data.segments, defaultAttributes.segments),
      arc: (validators.float(this.data.arc, defaultAttributes.arc) * Math.PI) / 180,
      updatable: false,
      sideOrientation: 2
    })
  }
}

DisposableComponent.registerClassId(CLASS_ID.CIRCLE_SHAPE, CircleShape)
