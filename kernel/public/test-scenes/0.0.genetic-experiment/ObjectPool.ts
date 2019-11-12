import { Entity } from 'decentraland-ecs/src'

export class Pool {
  pool: Entity[] = []
  max?: number = 1000
  constructor(max: number = 10) {
    this.pool = []
    this.max = max

    // generate initial instances
    for (let index = 0; index < max; index++) {
      this.newEntity()
    }
  }

  getEntity() {
    for (let i = 0; i < this.pool.length; i++) {
      const entity = this.pool[i]
      if (!entity.alive) {
        return entity
      }
    }

    if (this.max && this.pool.length < this.max) {
      return this.newEntity()
    }

    return null
  }

  newEntity() {
    const instance = new Entity()
    instance.name = (Math.random() * 10000).toString()
    this.pool.push(instance)
    return instance
  }
}
