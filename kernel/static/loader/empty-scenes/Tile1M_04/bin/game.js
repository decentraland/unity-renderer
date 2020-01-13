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
dcl.updateEntityComponent('3', 'engine.transform', 1, JSON.stringify({"position":{"x":13.5,"y":0,"z":2.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('4');
dcl.setParent('4', '1');
dcl.attachEntityComponent('4', 'engine.shape', 'gltfShape_2');
dcl.updateEntityComponent('4', 'engine.transform', 1, JSON.stringify({"position":{"x":10,"y":0,"z":12.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('5');
dcl.setParent('5', '1');
dcl.attachEntityComponent('5', 'engine.shape', 'gltfShape_2');
dcl.updateEntityComponent('5', 'engine.transform', 1, JSON.stringify({"position":{"x":3.5,"y":0,"z":5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('6');
dcl.setParent('6', '1');
dcl.componentCreated('gltfShape_3', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_3', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Grass_02/Grass_02.glb"}));
dcl.attachEntityComponent('6', 'engine.shape', 'gltfShape_3');
dcl.updateEntityComponent('6', 'engine.transform', 1, JSON.stringify({"position":{"x":3,"y":0,"z":13.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('7');
dcl.setParent('7', '1');
dcl.attachEntityComponent('7', 'engine.shape', 'gltfShape_3');
dcl.updateEntityComponent('7', 'engine.transform', 1, JSON.stringify({"position":{"x":13,"y":0,"z":9},"rotation":{"x":0,"y":0.8314696123025451,"z":0,"w":0.5555702330196022},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('8');
dcl.setParent('8', '1');
dcl.componentCreated('gltfShape_4', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_4', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Grass_03/Grass_03.glb"}));
dcl.attachEntityComponent('8', 'engine.shape', 'gltfShape_4');
dcl.updateEntityComponent('8', 'engine.transform', 1, JSON.stringify({"position":{"x":7,"y":0,"z":10.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('9');
dcl.setParent('9', '1');
dcl.attachEntityComponent('9', 'engine.shape', 'gltfShape_4');
dcl.updateEntityComponent('9', 'engine.transform', 1, JSON.stringify({"position":{"x":10,"y":0,"z":2},"rotation":{"x":0,"y":-0.2902846772544624,"z":0,"w":0.9569403357322089},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('10');
dcl.setParent('10', '1');
dcl.attachEntityComponent('10', 'engine.shape', 'gltfShape_2');
dcl.updateEntityComponent('10', 'engine.transform', 1, JSON.stringify({"position":{"x":1,"y":0,"z":1},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('11');
dcl.setParent('11', '1');
dcl.attachEntityComponent('11', 'engine.shape', 'gltfShape_3');
dcl.updateEntityComponent('11', 'engine.transform', 1, JSON.stringify({"position":{"x":7.5,"y":0,"z":5.5},"rotation":{"x":0,"y":0.9807852804032302,"z":0,"w":0.19509032201612825},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('12');
dcl.setParent('12', '1');
dcl.componentCreated('gltfShape_5', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_5', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/TreeRoundPine_02/TreeRoundPine_02.glb"}));
dcl.attachEntityComponent('12', 'engine.shape', 'gltfShape_5');
dcl.updateEntityComponent('12', 'engine.transform', 1, JSON.stringify({"position":{"x":12,"y":0,"z":6},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('13');
dcl.setParent('13', '1');
dcl.componentCreated('gltfShape_6', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_6', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/TreeRoundPine_01/TreeRoundPine_01.glb"}));
dcl.attachEntityComponent('13', 'engine.shape', 'gltfShape_6');
dcl.updateEntityComponent('13', 'engine.transform', 1, JSON.stringify({"position":{"x":2.5,"y":0,"z":6},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('14');
dcl.setParent('14', '1');
dcl.componentCreated('gltfShape_7', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_7', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/TreeRoundPine_03/TreeRoundPine_03.glb"}));
dcl.attachEntityComponent('14', 'engine.shape', 'gltfShape_7');
dcl.updateEntityComponent('14', 'engine.transform', 1, JSON.stringify({"position":{"x":7,"y":0,"z":13},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('15');
dcl.setParent('15', '1');
dcl.componentCreated('gltfShape_8', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_8', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/RockSmall_01/RockSmall_01.glb"}));
dcl.attachEntityComponent('15', 'engine.shape', 'gltfShape_8');
dcl.updateEntityComponent('15', 'engine.transform', 1, JSON.stringify({"position":{"x":2.5,"y":0,"z":4},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('16');
dcl.setParent('16', '1');
dcl.componentCreated('gltfShape_9', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_9', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/RockMedium_02/RockMedium_02.glb"}));
dcl.attachEntityComponent('16', 'engine.shape', 'gltfShape_9');
dcl.updateEntityComponent('16', 'engine.transform', 1, JSON.stringify({"position":{"x":12,"y":0,"z":7.5},"rotation":{"x":0,"y":0.9951847266721967,"z":0,"w":0.09801714032956056},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('17');
dcl.setParent('17', '1');
dcl.componentCreated('gltfShape_10', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_10', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/RockMedium_01/RockMedium_01.glb"}));
dcl.attachEntityComponent('17', 'engine.shape', 'gltfShape_10');
dcl.updateEntityComponent('17', 'engine.transform', 1, JSON.stringify({"position":{"x":6,"y":0,"z":13.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('18');
dcl.setParent('18', '1');
dcl.attachEntityComponent('18', 'engine.shape', 'gltfShape_9');
dcl.updateEntityComponent('18', 'engine.transform', 1, JSON.stringify({"position":{"x":3.5,"y":0,"z":7.5},"rotation":{"x":0,"y":0.7071067811865475,"z":0,"w":0.7071067811865476},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('19');
dcl.setParent('19', '1');
dcl.componentCreated('gltfShape_11', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_11', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Mushroom_02/Mushroom_02.glb"}));
dcl.attachEntityComponent('19', 'engine.shape', 'gltfShape_11');
dcl.updateEntityComponent('19', 'engine.transform', 1, JSON.stringify({"position":{"x":3.5,"y":0,"z":5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('20');
dcl.setParent('20', '1');
dcl.componentCreated('gltfShape_12', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_12', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Mushroom_01/Mushroom_01.glb"}));
dcl.attachEntityComponent('20', 'engine.shape', 'gltfShape_12');
dcl.updateEntityComponent('20', 'engine.transform', 1, JSON.stringify({"position":{"x":13,"y":0,"z":6},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('21');
dcl.setParent('21', '1');
dcl.componentCreated('gltfShape_13', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_13', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Pebble_01/Pebble_01.glb"}));
dcl.attachEntityComponent('21', 'engine.shape', 'gltfShape_13');
dcl.updateEntityComponent('21', 'engine.transform', 1, JSON.stringify({"position":{"x":13,"y":0,"z":5.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('22');
dcl.setParent('22', '1');
dcl.componentCreated('gltfShape_14', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_14', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Pebble_02/Pebble_02.glb"}));
dcl.attachEntityComponent('22', 'engine.shape', 'gltfShape_14');
dcl.updateEntityComponent('22', 'engine.transform', 1, JSON.stringify({"position":{"x":11.5,"y":0,"z":4.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('23');
dcl.setParent('23', '1');
dcl.componentCreated('gltfShape_15', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_15', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Bush_01/Bush_01.glb"}));
dcl.attachEntityComponent('23', 'engine.shape', 'gltfShape_15');
dcl.updateEntityComponent('23', 'engine.transform', 1, JSON.stringify({"position":{"x":8.5,"y":0,"z":14},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('24');
dcl.setParent('24', '1');
dcl.componentCreated('gltfShape_16', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_16', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Bush_03/Bush_03.glb"}));
dcl.attachEntityComponent('24', 'engine.shape', 'gltfShape_16');
dcl.updateEntityComponent('24', 'engine.transform', 1, JSON.stringify({"position":{"x":6.5,"y":0,"z":2.5},"rotation":{"x":0,"y":-0.47139673682599775,"z":0,"w":0.8819212643483552},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('25');
dcl.setParent('25', '1');
dcl.componentCreated('gltfShape_17', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_17', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Plant_02/Plant_02.glb"}));
dcl.attachEntityComponent('25', 'engine.shape', 'gltfShape_17');
dcl.updateEntityComponent('25', 'engine.transform', 1, JSON.stringify({"position":{"x":12.5,"y":0,"z":12.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('26');
dcl.setParent('26', '1');
dcl.componentCreated('gltfShape_18', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_18', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/RockMedium_03/RockMedium_03.glb"}));
dcl.attachEntityComponent('26', 'engine.shape', 'gltfShape_18');
dcl.updateEntityComponent('26', 'engine.transform', 1, JSON.stringify({"position":{"x":8.5,"y":0,"z":9},"rotation":{"x":0,"y":-0.9951847266721972,"z":0,"w":0.0980171403295609},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('27');
dcl.setParent('27', '1');
dcl.attachEntityComponent('27', 'engine.shape', 'gltfShape_12');
dcl.updateEntityComponent('27', 'engine.transform', 1, JSON.stringify({"position":{"x":2,"y":0,"z":4.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

