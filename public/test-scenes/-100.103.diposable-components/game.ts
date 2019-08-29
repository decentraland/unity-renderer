import { engine, DisposableComponent, Entity, log, DisposableComponentRemoved } from 'decentraland-ecs/src'
import { DecentralandInterface } from 'decentraland-ecs/src/decentraland/Types'

declare var dcl: DecentralandInterface

@DisposableComponent('engine.gltf-model', 55)
export class GLTFModel {
  src: string = ''

  onDispose() {
    log('onDispose')
  }
}

const model = new GLTFModel()
model.src = 'example.gltf'

const ent = new Entity()
;(ent as any).uuid = '__test_id__'

ent.addComponentOrReplace(model)

log('start')

engine.eventManager.addListener(DisposableComponentRemoved, {}, function() {
  log('DisposableComponentRemoved')
})

dcl.onUpdate(x => {
  if (x === 0) {
    log('addEntity')
    engine.addEntity(ent)
    throw new Error('onUpdate works')
  } else if (x === 1) {
    log('disposing')
    engine.disposeComponent(model)
    log('disposed')
  }
})
