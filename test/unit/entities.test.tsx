import { expect } from 'chai'
import { BaseEntity, findParentEntity } from 'engine/entities/BaseEntity'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { uuid } from 'atomicHelpers/math'
import { CLASS_ID } from 'decentraland-ecs/src'

/**
 * Recursively finds an entity by id (pre order)
 */
export function findEntityById(id: string, root: BaseEntity): BaseEntity | null {
  if (root.uuid === id) {
    return root
  }
  const children = root.childEntities()
  for (let i = 0; i < children.length; i++) {
    if (children[i] && children[i] instanceof BaseEntity) {
      const entity = findEntityById(id, children[i])
      if (entity) {
        return entity
      }
    }
  }
  return null
}

describe('entity tree tests', function() {
  const ctx = new SharedSceneContext('test', 'test')

  it('finding a parent in a non-attached entity should return null', () => {
    const entity = new BaseEntity(uuid(), ctx)

    expect(findParentEntity(entity)).to.equal(null)

    entity.dispose()
  })

  it('finding a parent in a attached entity should return the parent entity', () => {
    const parent = new BaseEntity(uuid(), ctx)
    const entity = new BaseEntity(uuid(), ctx)

    expect(findParentEntity(entity)).to.equal(null)

    entity.setParentEntity(parent)

    expect(findParentEntity(entity)).to.equal(parent)

    parent.dispose()
  })

  it('finding an entity by ID should return null if the entity does not exist in the tree', () => {
    const id = 'unexistent-id'
    const entity = new BaseEntity(uuid(), ctx)

    expect(findEntityById(id, entity)).to.equal(null)

    entity.dispose()
  })

  it('finding an entity by ID should return the entity if it has the desired ID', () => {
    const id = 'a1'
    const entity = new BaseEntity(id, ctx)

    expect(findEntityById(id, entity)).to.equal(entity)

    entity.dispose()
  })

  it('finding an entity by ID should return the entity if that entity is deep inside the entity tree', () => {
    const id = 'a1'

    const entity = new BaseEntity(id, ctx)

    const parent1 = new BaseEntity(uuid(), ctx)
    entity.setParentEntity(parent1)

    const parent = new BaseEntity(uuid(), ctx)
    parent1.setParentEntity(parent)

    expect(findEntityById(id, parent)).to.equal(entity)

    parent.dispose()
  })

  it('releasing an entity sub-tree should remove all the entities', () => {
    const entity = new BaseEntity(uuid(), ctx)
    const child1 = new BaseEntity(uuid(), ctx)
    const child2 = new BaseEntity(uuid(), ctx)
    const child3 = new BaseEntity(uuid(), ctx)
    let removedCalled = false

    child1.setParentEntity(entity)
    child2.setParentEntity(child1)
    child3.setParentEntity(child2)

    child3.onDisposeObservable.add(() => {
      removedCalled = true
    })

    expect(entity.isDisposed()).to.equal(false)
    expect(child1.isDisposed()).to.equal(false)
    expect(child2.isDisposed()).to.equal(false)
    expect(child3.isDisposed()).to.equal(false)

    entity.disposeTree()

    expect(removedCalled).to.equal(true)
    expect(entity.isDisposed()).to.equal(true)
    expect(child1.isDisposed()).to.equal(true)
    expect(child2.isDisposed()).to.equal(true)
    expect(child3.isDisposed()).to.equal(true)
  })

  it('releasing an entity sub-tree should release the components', () => {
    const entity = new BaseEntity(uuid(), ctx)
    const child = new BaseEntity(uuid(), ctx)
    let removedCalled = false

    child.updateComponent({
      name: 'transform',
      classId: CLASS_ID.TRANSFORM,
      entityId: null,
      json: JSON.stringify({
        position: { x: 1, y: 1, z: 1 },
        scale: { x: 1, y: 1, z: 1 },
        rotation: { x: 0, y: 0, z: 0, w: 1 }
      })
    })

    child.setParentEntity(entity)

    child.components.transform.detach = () => {
      removedCalled = true
    }

    expect(child.isDisposed()).to.equal(false)

    entity.disposeTree()

    expect(child.isDisposed()).to.equal(true)
    expect(entity.isDisposed()).to.equal(true)
    expect(removedCalled).to.equal(true)
  })
})
