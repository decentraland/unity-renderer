import { Engine } from './Engine'
import { Entity } from './Entity'

/**
 * @public
 */
export interface ISystem {
  active?: boolean

  activate?(engine: Engine): void
  deactivate?(): void

  update?(dt: number): void

  onAddEntity?(entity: Entity): void
  onRemoveEntity?(entity: Entity): void
}
