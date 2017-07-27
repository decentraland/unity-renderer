import { BasicShape, DisposableComponent } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { validators } from '../helpers/schemaValidator'
import { scene } from 'engine/renderer'

const defaultAttributes = {
  radiusTop: 0,
  radiusBottom: 1,
  segmentsHeight: 1,
  segmentsRadial: 36,
  openEnded: false,
  radius: null,
  arc: 360
}

export class ConeShape extends BasicShape<typeof defaultAttributes> {
  generateModel() {
    return BABYLON.MeshBuilder.CreateCylinder(
      'cone',
      {
        diameterTop:
          validators.number(
            this.data.radiusTop !== undefined ? this.data.radiusTop : this.data.radius,
            defaultAttributes.radiusTop
          ) * 2,
        diameterBottom:
          validators.number(
            this.data.radiusBottom !== undefined ? this.data.radiusBottom : this.data.radius,
            defaultAttributes.radiusBottom
          ) * 2,
        enclose: validators.boolean(this.data.openEnded, defaultAttributes.openEnded),
        subdivisions: validators.number(this.data.segmentsHeight, defaultAttributes.segmentsHeight),
        tessellation: validators.number(this.data.segmentsRadial, defaultAttributes.segmentsRadial),
        arc: (validators.number(this.data.arc, defaultAttributes.arc) * Math.PI) / 180,
        updatable: false
      },
      scene
    )
  }
}

DisposableComponent.registerClassId(CLASS_ID.CONE_SHAPE, ConeShape)
