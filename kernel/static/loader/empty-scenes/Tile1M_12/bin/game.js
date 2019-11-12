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
dcl.updateEntityComponent('3', 'engine.transform', 1, JSON.stringify({"position":{"x":2.5,"y":0,"z":13.5},"rotation":{"x":0,"y":0.8314696123025451,"z":0,"w":0.5555702330196022},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('4');
dcl.setParent('4', '1');
dcl.componentCreated('gltfShape_3', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_3', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Grass_02/Grass_02.glb"}));
dcl.attachEntityComponent('4', 'engine.shape', 'gltfShape_3');
dcl.updateEntityComponent('4', 'engine.transform', 1, JSON.stringify({"position":{"x":13.5,"y":0,"z":13.5},"rotation":{"x":0,"y":0.8314696123025451,"z":0,"w":0.5555702330196022},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('5');
dcl.setParent('5', '1');
dcl.componentCreated('gltfShape_4', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_4', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Grass_03/Grass_03.glb"}));
dcl.attachEntityComponent('5', 'engine.shape', 'gltfShape_4');
dcl.updateEntityComponent('5', 'engine.transform', 1, JSON.stringify({"position":{"x":7.5,"y":0,"z":6},"rotation":{"x":0,"y":-0.38268343236508984,"z":0,"w":0.9238795325112868},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('6');
dcl.setParent('6', '1');
dcl.attachEntityComponent('6', 'engine.shape', 'gltfShape_2');
dcl.updateEntityComponent('6', 'engine.transform', 1, JSON.stringify({"position":{"x":12.5,"y":0,"z":8},"rotation":{"x":0,"y":0.9807852804032302,"z":0,"w":0.19509032201612825},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('7');
dcl.setParent('7', '1');
dcl.attachEntityComponent('7', 'engine.shape', 'gltfShape_3');
dcl.updateEntityComponent('7', 'engine.transform', 1, JSON.stringify({"position":{"x":3,"y":0,"z":2.5},"rotation":{"x":0,"y":0.9807852804032302,"z":0,"w":-0.19509032201612814},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('8');
dcl.setParent('8', '1');
dcl.attachEntityComponent('8', 'engine.shape', 'gltfShape_2');
dcl.updateEntityComponent('8', 'engine.transform', 1, JSON.stringify({"position":{"x":9.5,"y":0,"z":3},"rotation":{"x":0,"y":0.9951847266721968,"z":0,"w":0.09801714032956052},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('9');
dcl.setParent('9', '1');
dcl.attachEntityComponent('9', 'engine.shape', 'gltfShape_4');
dcl.updateEntityComponent('9', 'engine.transform', 1, JSON.stringify({"position":{"x":14,"y":0,"z":3.5},"rotation":{"x":0,"y":-0.47139673682599775,"z":0,"w":0.8819212643483552},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('10');
dcl.setParent('10', '1');
dcl.attachEntityComponent('10', 'engine.shape', 'gltfShape_3');
dcl.updateEntityComponent('10', 'engine.transform', 1, JSON.stringify({"position":{"x":8,"y":0,"z":10.5},"rotation":{"x":0,"y":0.831469612302545,"z":0,"w":-0.555570233019602},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('11');
dcl.setParent('11', '1');
dcl.attachEntityComponent('11', 'engine.shape', 'gltfShape_2');
dcl.updateEntityComponent('11', 'engine.transform', 1, JSON.stringify({"position":{"x":4.5,"y":0,"z":7.5},"rotation":{"x":0,"y":-0.6343932841636456,"z":0,"w":0.773010453362737},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('12');
dcl.setParent('12', '1');
dcl.attachEntityComponent('12', 'engine.shape', 'gltfShape_4');
dcl.updateEntityComponent('12', 'engine.transform', 1, JSON.stringify({"position":{"x":3,"y":0,"z":12},"rotation":{"x":0,"y":-0.7071067811865477,"z":0,"w":0.7071067811865477},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('13');
dcl.setParent('13', '1');
dcl.attachEntityComponent('13', 'engine.shape', 'gltfShape_4');
dcl.updateEntityComponent('13', 'engine.transform', 1, JSON.stringify({"position":{"x":11,"y":0,"z":13},"rotation":{"x":0,"y":-0.6343932841636457,"z":0,"w":0.7730104533627371},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('14');
dcl.setParent('14', '1');
dcl.componentCreated('gltfShape_5', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_5', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/TreePine_01/TreePine_01.glb"}));
dcl.attachEntityComponent('14', 'engine.shape', 'gltfShape_5');
dcl.updateEntityComponent('14', 'engine.transform', 1, JSON.stringify({"position":{"x":6,"y":0,"z":9.5},"rotation":{"x":0,"y":0.5555702330196022,"z":0,"w":0.8314696123025452},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('15');
dcl.setParent('15', '1');
dcl.componentCreated('gltfShape_6', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_6', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/TreePine_03/TreePine_03.glb"}));
dcl.attachEntityComponent('15', 'engine.shape', 'gltfShape_6');
dcl.updateEntityComponent('15', 'engine.transform', 1, JSON.stringify({"position":{"x":12,"y":0,"z":4.5},"rotation":{"x":0,"y":-0.831469612302545,"z":0,"w":0.5555702330196022},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('16');
dcl.setParent('16', '1');
dcl.componentCreated('gltfShape_7', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_7', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/RockSmall_01/RockSmall_01.glb"}));
dcl.attachEntityComponent('16', 'engine.shape', 'gltfShape_7');
dcl.updateEntityComponent('16', 'engine.transform', 1, JSON.stringify({"position":{"x":9,"y":0,"z":4},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('17');
dcl.setParent('17', '1');
dcl.componentCreated('gltfShape_8', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_8', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/RockSmall_03/RockSmall_03.glb"}));
dcl.attachEntityComponent('17', 'engine.shape', 'gltfShape_8');
dcl.updateEntityComponent('17', 'engine.transform', 1, JSON.stringify({"position":{"x":11.5,"y":0,"z":3},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('18');
dcl.setParent('18', '1');
dcl.attachEntityComponent('18', 'engine.shape', 'gltfShape_7');
dcl.updateEntityComponent('18', 'engine.transform', 1, JSON.stringify({"position":{"x":12,"y":0,"z":6},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('19');
dcl.setParent('19', '1');
dcl.attachEntityComponent('19', 'engine.shape', 'gltfShape_8');
dcl.updateEntityComponent('19', 'engine.transform', 1, JSON.stringify({"position":{"x":6,"y":0,"z":11},"rotation":{"x":0,"y":-0.9951847266721974,"z":0,"w":-0.09801714032956049},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('20');
dcl.setParent('20', '1');
dcl.attachEntityComponent('20', 'engine.shape', 'gltfShape_7');
dcl.updateEntityComponent('20', 'engine.transform', 1, JSON.stringify({"position":{"x":5.5,"y":0,"z":8},"rotation":{"x":0,"y":0.7071067811865475,"z":0,"w":0.7071067811865475},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('21');
dcl.setParent('21', '1');
dcl.componentCreated('gltfShape_9', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_9', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/RockSmall_02/RockSmall_02.glb"}));
dcl.attachEntityComponent('21', 'engine.shape', 'gltfShape_9');
dcl.updateEntityComponent('21', 'engine.transform', 1, JSON.stringify({"position":{"x":9,"y":0,"z":10.5},"rotation":{"x":0,"y":-0.8314696123025456,"z":0,"w":0.5555702330196025},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('22');
dcl.setParent('22', '1');
dcl.componentCreated('gltfShape_10', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_10', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Pebble_01/Pebble_01.glb"}));
dcl.attachEntityComponent('22', 'engine.shape', 'gltfShape_10');
dcl.updateEntityComponent('22', 'engine.transform', 1, JSON.stringify({"position":{"x":4,"y":0,"z":3.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('23');
dcl.setParent('23', '1');
dcl.componentCreated('gltfShape_11', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_11', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Pebble_02/Pebble_02.glb"}));
dcl.attachEntityComponent('23', 'engine.shape', 'gltfShape_11');
dcl.updateEntityComponent('23', 'engine.transform', 1, JSON.stringify({"position":{"x":13,"y":0,"z":10.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('24');
dcl.setParent('24', '1');
dcl.attachEntityComponent('24', 'engine.shape', 'gltfShape_10');
dcl.updateEntityComponent('24', 'engine.transform', 1, JSON.stringify({"position":{"x":2.5,"y":0,"z":9.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('25');
dcl.setParent('25', '1');
dcl.componentCreated('gltfShape_12', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_12', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Mushroom_02/Mushroom_02.glb"}));
dcl.attachEntityComponent('25', 'engine.shape', 'gltfShape_12');
dcl.updateEntityComponent('25', 'engine.transform', 1, JSON.stringify({"position":{"x":7,"y":0,"z":8.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('26');
dcl.setParent('26', '1');
dcl.componentCreated('gltfShape_13', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_13', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Mushroom_01/Mushroom_01.glb"}));
dcl.attachEntityComponent('26', 'engine.shape', 'gltfShape_13');
dcl.updateEntityComponent('26', 'engine.transform', 1, JSON.stringify({"position":{"x":10.5,"y":0,"z":3.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('27');
dcl.setParent('27', '1');
dcl.attachEntityComponent('27', 'engine.shape', 'gltfShape_12');
dcl.updateEntityComponent('27', 'engine.transform', 1, JSON.stringify({"position":{"x":12,"y":0,"z":12},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('28');
dcl.setParent('28', '1');
dcl.componentCreated('gltfShape_14', 'engine.shape', 54);
dcl.componentUpdated('gltfShape_14', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Plant_02/Plant_02.glb"}));
dcl.attachEntityComponent('28', 'engine.shape', 'gltfShape_14');
dcl.updateEntityComponent('28', 'engine.transform', 1, JSON.stringify({"position":{"x":10,"y":0,"z":8},"rotation":{"x":0,"y":-0.5555702330196024,"z":0,"w":0.8314696123025453},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('29');
dcl.setParent('29', '1');
dcl.attachEntityComponent('29', 'engine.shape', 'gltfShape_7');
dcl.updateEntityComponent('29', 'engine.transform', 1, JSON.stringify({"position":{"x":2,"y":0,"z":3.5},"rotation":{"x":0,"y":-0.7730104533627369,"z":0,"w":0.6343932841636456},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('30');
dcl.setParent('30', '1');
dcl.attachEntityComponent('30', 'engine.shape', 'gltfShape_8');
dcl.updateEntityComponent('30', 'engine.transform', 1, JSON.stringify({"position":{"x":14,"y":0,"z":11.5},"rotation":{"x":0,"y":0.5555702330196022,"z":0,"w":0.8314696123025452},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('31');
dcl.setParent('31', '1');
dcl.attachEntityComponent('31', 'engine.shape', 'gltfShape_11');
dcl.updateEntityComponent('31', 'engine.transform', 1, JSON.stringify({"position":{"x":2.5,"y":0,"z":6.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('32');
dcl.setParent('32', '1');
dcl.attachEntityComponent('32', 'engine.shape', 'gltfShape_10');
dcl.updateEntityComponent('32', 'engine.transform', 1, JSON.stringify({"position":{"x":3,"y":0,"z":6.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

