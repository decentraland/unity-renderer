import { engine, Entity, BoxShape, Transform, log, Material, Color3 } from 'decentraland-ecs'

declare var dcl: any

let counter = 0
let entity: any = null
let shape: any = null
let material: any = null

function executeAction() {
  log(counter)
  switch (counter) {
    case 0:
      log('CREATE_ENTITY')
      entity = new Entity()
      engine.addEntity(entity)
      break
    case 1: {
      log('CREATE_SHAPE')
      shape = new BoxShape()
      entity.add(shape)
      break
    }
    case 2: {
      log('SET_TRANSFORM')
      const t = entity.getOrCreate(Transform)
      t.position.x = 5
      t.position.y = 5
      t.position.z = 5
      t.scale.x = 1.1
      t.scale.y = 1.1
      t.scale.z = 1.1
      break
    }
    case 3: {
      log('SET_MATERIAL')
      material = new Material()
      entity.add(material)
      break
    }
    case 4: {
      log('SET_MATERIAL_COLOR')
      material.albedoColor = Color3.FromHexString('#00FF00')
      break
    }
    case 5: {
      log('REMOVE_MATERIAL')
      entity.remove(material)
      // TODO: fix that `as any`
      engine.disposeComponent(material as any)
      material = null
      break
    }
    case 6: {
      log('REMOVE_SHAPE')
      entity.remove(shape)
      engine.disposeComponent(shape)
      shape = null
      break
    }
    case 7: {
      log('REMOVE_TRANSFORM')
      entity.remove(Transform)
      break
    }
    case 8: {
      log('REMOVE_ENTITY')
      engine.removeEntity(entity)
      break
    }
  }

  counter++

  if (counter === 9) {
    counter = 0
  }
}

dcl.subscribe('pointerDown')
dcl.onEvent((evt: any) => {
  if (evt.type === 'TEST_TRIGGER' || evt.type === 'pointerDown') {
    executeAction()
  }
})
