import { Entity, GLTFShape, engine, Vector3, Transform } from 'decentraland-ecs/src'

let entity = new Entity();

let shape = new GLTFShape("models/PixelPlaza1.gltf");
entity.set(shape);

let transform = new Transform();
transform.position = new Vector3(10,0,10);
entity.set(transform);

engine.addEntity(entity);