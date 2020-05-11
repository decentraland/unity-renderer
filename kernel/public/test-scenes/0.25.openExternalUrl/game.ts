import { Entity, engine, Transform, Vector3, BoxShape, OnPointerDown, openExternalURL } from 'decentraland-ecs/src'

const entity = new Entity()
entity.addComponent(new BoxShape())
const transform = new Transform({ position: new Vector3(4, 0, 4) })
entity.addComponent(transform)

entity.addComponent(
  new OnPointerDown(() => {
    transform.position.set(4, 1, 4)
    openExternalURL('https://docs.microsoft.com/en-us/dotnet/api/system.uri?redirectedfrom=MSDN&view=netcore-3.1')
  })
)

engine.addEntity(entity)
