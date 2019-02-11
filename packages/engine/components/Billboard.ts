import { BaseComponent } from '../components'
import { validators } from './helpers/schemaValidator'
import { BaseEntity } from '../entities/BaseEntity'

export class Billboard extends BaseComponent<number> {
  transformValue(x) {
    return Math.max(Math.min(7, validators.int(x, 0)), 0)
  }

  update() {
    this.entity.billboardMode = this.value
  }

  detach() {
    this.entity.billboardMode = 0
  }
}
