dcl.subscribe('sceneStart');

dcl.addEntity('1');
dcl.setParent('1', '0');
dcl.updateEntityComponent('1', 'engine.transform', 1, JSON.stringify({"position":{"x":0,"y":0,"z":0},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('2');
dcl.setParent('2', '1');
dcl.componentCreated('gltfShape', 'engine.shape', 54);
dcl.componentUpdated('gltfShape', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/FloorBaseGrass_01/FloorBaseGrass_01.glb"}));
dcl.attachEntityComponent('2', 'engine.shape', 'gltfShape');
dcl.updateEntityComponent('2', 'engine.transform', 1, JSON.stringify({"position":{"x":8,"y":0,"z":8},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('3');
dcl.setParent('3', '1');
dcl.componentCreated('gltfShape_2', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_2', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Grass_01/Grass_01.glb"}));
dcl.attachEntityComponent('3', 'engine.shape', 'gltfShape_2');
dcl.updateEntityComponent('3', 'engine.transform', 1, JSON.stringify({"position":{"x":1.5,"y":0,"z":14},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('4');
dcl.setParent('4', '1');
dcl.componentCreated('gltfShape_3', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_3', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Grass_02/Grass_02.glb"}));
dcl.attachEntityComponent('4', 'engine.shape', 'gltfShape_3');
dcl.updateEntityComponent('4', 'engine.transform', 1, JSON.stringify({"position":{"x":13.5,"y":0,"z":13},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('5');
dcl.setParent('5', '1');
dcl.componentCreated('gltfShape_4', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_4', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Grass_03/Grass_03.glb"}));
dcl.attachEntityComponent('5', 'engine.shape', 'gltfShape_4');
dcl.updateEntityComponent('5', 'engine.transform', 1, JSON.stringify({"position":{"x":7,"y":0,"z":12.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('6');
dcl.setParent('6', '1');
dcl.attachEntityComponent('6', 'engine.shape', 'gltfShape_3');
dcl.updateEntityComponent('6', 'engine.transform', 1, JSON.stringify({"position":{"x":13.5,"y":0,"z":5.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('7');
dcl.setParent('7', '1');
dcl.attachEntityComponent('7', 'engine.shape', 'gltfShape_2');
dcl.updateEntityComponent('7', 'engine.transform', 1, JSON.stringify({"position":{"x":5.5,"y":0,"z":3.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('8');
dcl.setParent('8', '1');
dcl.componentCreated('gltfShape_5', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_5', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Grass_05/Grass_05.glb"}));
dcl.attachEntityComponent('8', 'engine.shape', 'gltfShape_5');
dcl.updateEntityComponent('8', 'engine.transform', 1, JSON.stringify({"position":{"x":12.5,"y":0,"z":3},"rotation":{"x":0,"y":-0.7071067811865478,"z":0,"w":0.7071067811865477},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('9');
dcl.setParent('9', '1');
dcl.attachEntityComponent('9', 'engine.shape', 'gltfShape_5');
dcl.updateEntityComponent('9', 'engine.transform', 1, JSON.stringify({"position":{"x":10,"y":0,"z":10},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('10');
dcl.setParent('10', '1');
dcl.attachEntityComponent('10', 'engine.shape', 'gltfShape_2');
dcl.updateEntityComponent('10', 'engine.transform', 1, JSON.stringify({"position":{"x":7.5,"y":0,"z":7},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('11');
dcl.setParent('11', '1');
dcl.componentCreated('gltfShape_6', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_6', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/JunglePlant_06/JunglePlant_06.glb"}));
dcl.attachEntityComponent('11', 'engine.shape', 'gltfShape_6');
dcl.updateEntityComponent('11', 'engine.transform', 1, JSON.stringify({"position":{"x":4,"y":0,"z":8},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('12');
dcl.setParent('12', '1');
dcl.componentCreated('gltfShape_7', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_7', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/JunglePlant_09/JunglePlant_09.glb"}));
dcl.attachEntityComponent('12', 'engine.shape', 'gltfShape_7');
dcl.updateEntityComponent('12', 'engine.transform', 1, JSON.stringify({"position":{"x":3,"y":0,"z":4},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('13');
dcl.setParent('13', '1');
dcl.attachEntityComponent('13', 'engine.shape', 'gltfShape_6');
dcl.updateEntityComponent('13', 'engine.transform', 1, JSON.stringify({"position":{"x":11,"y":0,"z":13.5},"rotation":{"x":0,"y":-0.5555702330196024,"z":0,"w":0.8314696123025453},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('14');
dcl.setParent('14', '1');
dcl.componentCreated('gltfShape_8', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_8', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Plant_02/Plant_02.glb"}));
dcl.attachEntityComponent('14', 'engine.shape', 'gltfShape_8');
dcl.updateEntityComponent('14', 'engine.transform', 1, JSON.stringify({"position":{"x":14,"y":0,"z":9},"rotation":{"x":0,"y":0.7071067811865475,"z":0,"w":0.7071067811865475},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('15');
dcl.setParent('15', '1');
dcl.attachEntityComponent('15', 'engine.shape', 'gltfShape_8');
dcl.updateEntityComponent('15', 'engine.transform', 1, JSON.stringify({"position":{"x":7.5,"y":0,"z":2.5},"rotation":{"x":0,"y":0.9807852804032303,"z":0,"w":0.19509032201612816},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('16');
dcl.setParent('16', '1');
dcl.attachEntityComponent('16', 'engine.shape', 'gltfShape_3');
dcl.updateEntityComponent('16', 'engine.transform', 1, JSON.stringify({"position":{"x":6,"y":0,"z":10},"rotation":{"x":0,"y":-0.5555702330196023,"z":0,"w":0.8314696123025452},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('17');
dcl.setParent('17', '1');
dcl.componentCreated('gltfShape_9', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_9', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Pebble_01/Pebble_01.glb"}));
dcl.attachEntityComponent('17', 'engine.shape', 'gltfShape_9');
dcl.updateEntityComponent('17', 'engine.transform', 1, JSON.stringify({"position":{"x":12,"y":0,"z":9},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('18');
dcl.setParent('18', '1');
dcl.componentCreated('gltfShape_10', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_10', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Pebble_02/Pebble_02.glb"}));
dcl.attachEntityComponent('18', 'engine.shape', 'gltfShape_10');
dcl.updateEntityComponent('18', 'engine.transform', 1, JSON.stringify({"position":{"x":3.5,"y":0,"z":12.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('19');
dcl.setParent('19', '1');
dcl.attachEntityComponent('19', 'engine.shape', 'gltfShape_9');
dcl.updateEntityComponent('19', 'engine.transform', 1, JSON.stringify({"position":{"x":3.5,"y":0,"z":3.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('20');
dcl.setParent('20', '1');
dcl.attachEntityComponent('20', 'engine.shape', 'gltfShape_10');
dcl.updateEntityComponent('20', 'engine.transform', 1, JSON.stringify({"position":{"x":8,"y":0,"z":2},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('21');
dcl.setParent('21', '1');
dcl.attachEntityComponent('21', 'engine.shape', 'gltfShape_9');
dcl.updateEntityComponent('21', 'engine.transform', 1, JSON.stringify({"position":{"x":14,"y":0,"z":2.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('22');
dcl.setParent('22', '1');
dcl.attachEntityComponent('22', 'engine.shape', 'gltfShape_9');
dcl.updateEntityComponent('22', 'engine.transform', 1, JSON.stringify({"position":{"x":9.5,"y":0,"z":6.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('23');
dcl.setParent('23', '1');
dcl.attachEntityComponent('23', 'engine.shape', 'gltfShape_9');
dcl.updateEntityComponent('23', 'engine.transform', 1, JSON.stringify({"position":{"x":2.5,"y":0,"z":10},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('24');
dcl.setParent('24', '1');
dcl.componentCreated('gltfShape_11', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_11', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Mushroom_02/Mushroom_02.glb"}));
dcl.attachEntityComponent('24', 'engine.shape', 'gltfShape_11');
dcl.updateEntityComponent('24', 'engine.transform', 1, JSON.stringify({"position":{"x":2.5,"y":0,"z":13.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

