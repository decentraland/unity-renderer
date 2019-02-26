import { Entity, GLTFShape, engine, Vector3, Transform } from 'decentraland-ecs/src'

let entity = new Entity();

let shape = new GLTFShape("models/Forest_30-01-19.gltf");
entity.set(shape);

let transform = new Transform();
transform.position = new Vector3(15,0,15);
entity.set(transform);

engine.addEntity(entity);