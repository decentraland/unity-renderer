import {
  Entity,
  engine,
  Transform,
  Vector3,
  Quaternion,
  GLTFShape,
  log,
  OnPointerDown,
  OnPointerUp
} from 'decentraland-ecs/src'

const _scene = new Entity("_scene");
engine.addEntity(_scene);
const transform = new Transform({
  position: new Vector3(0, 0, 0),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
});
_scene.addComponentOrReplace(transform);

const entity = new Entity("entity");
engine.addEntity(entity);
entity.setParent(_scene);
const gltfShape = new GLTFShape(
  "models/FloorBaseGrass_01/FloorBaseGrass_01.glb"
);
gltfShape.withCollisions = true;
gltfShape.visible = true;
entity.addComponentOrReplace(gltfShape);
const transform2 = new Transform({
  position: new Vector3(8, 0, 8),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
});
entity.addComponentOrReplace(transform2);

const cornerStoneBrickWall = new Entity("cornerStoneBrickWall");
engine.addEntity(cornerStoneBrickWall);
cornerStoneBrickWall.setParent(_scene);
const transform3 = new Transform({
  position: new Vector3(10.5, 0, 6.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
});
cornerStoneBrickWall.addComponentOrReplace(transform3);
const gltfShape2 = new GLTFShape(
  "models/Module_Stone_Curve_01/Module_Stone_Curve_01.glb"
);
gltfShape2.withCollisions = true;
gltfShape2.visible = true;
cornerStoneBrickWall.addComponentOrReplace(gltfShape2);

const mediumStoneWall = new Entity("mediumStoneWall");
engine.addEntity(mediumStoneWall);
mediumStoneWall.setParent(_scene);
const transform4 = new Transform({
  position: new Vector3(11, 0, 10.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
});
mediumStoneWall.addComponentOrReplace(transform4);
const gltfShape3 = new GLTFShape(
  "models/Wall_Stone_Medium/Wall_Stone_Medium.glb"
);
gltfShape3.withCollisions = true;
gltfShape3.isPointerBlocker = false;
gltfShape3.visible = true;
mediumStoneWall.addComponentOrReplace(gltfShape3);

const chest = new Entity("chest");
engine.addEntity(chest);
chest.setParent(_scene);
const transform5 = new Transform({
  position: new Vector3(9.5, 0, 9.789471626281738),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
});
chest.addComponentOrReplace(transform5);
const gltfShape4 = new GLTFShape("models/Trunk_01/Trunk_01.glb");
gltfShape4.withCollisions = true;
gltfShape4.isPointerBlocker = false;
gltfShape4.visible = true;
chest.addComponentOrReplace(gltfShape4);

chest.addComponent(
  new OnPointerDown(() => {
    log("CHEST ONPOINTER-DOWN HIT!");
  })
);

chest.addComponent(
  new OnPointerUp(() => {
    log("CHEST ONPOINTER-UP HIT!");
  })
);
