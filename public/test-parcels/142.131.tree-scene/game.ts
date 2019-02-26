import { Entity, GLTFShape, engine, Vector3, Transform } from 'decentraland-ecs/src'

const entity = new Entity();
entity.add(new GLTFShape("models/Tree_Scene.glb"));
entity.add(new Transform({position: new Vector3(8,0,8)}));
engine.addEntity(entity);