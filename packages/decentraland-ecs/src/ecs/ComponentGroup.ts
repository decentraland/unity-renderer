import { getComponentName, ComponentConstructor } from './Component'

// tslint:disable-next-line:whitespace
type Entity = import('./Entity').Entity

/**
 * @public
 */
export class ComponentGroup {
  readonly entities: ReadonlyArray<Entity> = []
  readonly requires!: ReadonlyArray<ComponentConstructor<any>>
  readonly requiresNames!: ReadonlyArray<string>

  active: boolean = false

  private _requiresNames: string[] = []

  constructor(...requires: ComponentConstructor<any>[]) {
    // validate requires list
    if (!requires) {
      throw new Error('ComponentGroup: Could not load the requires list')
    }
    if (!(requires instanceof Array)) {
      throw new Error('ComponentGroup: requires list is not an Array')
    }

    Object.defineProperty(this, 'requires', {
      get: function() {
        return requires.slice()
      }
    })

    Object.defineProperty(this, 'requiresNames', {
      get: function() {
        return this._requiresNames.slice()
      }
    })

    for (let ix = 0; ix < requires.length; ix++) {
      const component = requires[ix]
      let name: string | null = null

      if (!component) {
        throw new Error(`ComponentGroup: the required component at location ${ix} is invalid`)
      }

      try {
        name = getComponentName(component)
      } catch (e) {
        throw new Error(
          `ComponentGroup: the required component at location ${ix} is not registered as a @Component. Remember to provide the class of the component, not the name`
        )
      }

      if (this._requiresNames.some($ => $ === name)) {
        throw new Error(`ComponentGroup: the required component list has a repeated name ${name}`)
      }

      this._requiresNames.push(name)
    }
  }

  hasEntity(entity: Entity): boolean {
    if (!entity.isAddedToEngine()) return false

    return this.entities.indexOf(entity) !== -1
  }

  // @internal
  addEntity(entity: Entity) {
    if (!entity.isAddedToEngine()) {
      throw new TypeError('ComponentGroup: Cannot add a entity that is not added to the engine')
    }

    if (this.entities.indexOf(entity) === -1) {
      // tslint:disable-next-line:semicolon
      ;(this.entities as Entity[]).push(entity)
    }
  }

  // @internal
  removeEntity(entity: Entity) {
    const id = this.entities.indexOf(entity)

    if (id !== -1) {
      // tslint:disable-next-line:semicolon
      ;(this.entities as Entity[]).splice(id, 1)
    }
  }

  // @internal
  componentRemoved(entity: Entity, component: string) {
    if (this._requiresNames.indexOf(component) !== -1) {
      this.removeEntity(entity)
    }
  }

  // @internal
  meetsRequirements(entity: Entity) {
    for (let i = 0; i < this._requiresNames.length; i++) {
      const componentName = this._requiresNames[i]
      if (!(componentName in entity.components)) {
        return false
      }
    }
    return true
  }
}
