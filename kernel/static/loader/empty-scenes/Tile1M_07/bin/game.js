dcl.subscribe('sceneStart');

dcl.addEntity('1');
dcl.setParent('1', '0');
dcl.updateEntityComponent('1', 'engine.transform', 1, JSON.stringify({"position":{"x":16,"y":0,"z":0},"rotation":{"x":0,"y":-0.7071067811865475,"z":0,"w":0.7071067811865476},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('2');
dcl.setParent('2', '1');
dcl.componentCreated('gltfShape', 'engine.shape', 54);
dcl.componentUpdated('gltfShape', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/FloorBaseGrass_01/FloorBaseGrass_01.glb"}));
dcl.attachEntityComponent('2', 'engine.shape', 'gltfShape');
dcl.updateEntityComponent('2', 'engine.transform', 1, JSON.stringify({"position":{"x":8,"y":0,"z":8},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));

dcl.addEntity('3');
dcl.setParent('3', '1');
dcl.updateEntityComponent('3', 'engine.transform', 1, JSON.stringify({"position":{"x":2.5,"y":0,"z":11.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));
dcl.componentCreated('gltfShape2', 'engine.shape', 54);
dcl.componentUpdated('gltfShape2', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/SnowRock_04.glb"}));
dcl.attachEntityComponent('3', 'engine.shape', 'gltfShape2');

dcl.addEntity('4');
dcl.setParent('4', '1');
dcl.updateEntityComponent('4', 'engine.transform', 1, JSON.stringify({"position":{"x":8,"y":0.0005483627319335938,"z":8},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));
dcl.componentCreated('gltfShape3', 'engine.shape', 54);
dcl.componentUpdated('gltfShape3', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Floor_Snow.glb"}));
dcl.attachEntityComponent('4', 'engine.shape', 'gltfShape3');

dcl.addEntity('5');
dcl.setParent('5', '1');
dcl.updateEntityComponent('5', 'engine.transform', 1, JSON.stringify({"position":{"x":8.5,"y":0.2476482391357422,"z":3},"rotation":{"x":-8.730236238452065e-16,"y":-0.5679166913032532,"z":6.770093108343644e-8,"w":0.8230860829353333},"scale":{"x":1,"y":1.3280184268951416,"z":1}}));
dcl.componentCreated('gltfShape4', 'engine.shape', 54);
dcl.componentUpdated('gltfShape4', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Grass_03/Grass_03.glb"}));
dcl.attachEntityComponent('5', 'engine.shape', 'gltfShape4');

dcl.addEntity('6');
dcl.setParent('6', '1');
dcl.updateEntityComponent('6', 'engine.transform', 1, JSON.stringify({"position":{"x":2.5,"y":0,"z":1.5},"rotation":{"x":3.3281885908289946e-16,"y":-0.3457911014556885,"z":4.1221504432087386e-8,"w":0.9383115768432617},"scale":{"x":1,"y":1,"z":1}}));
dcl.attachEntityComponent('6', 'engine.shape', 'gltfShape4');

dcl.addEntity('7');
dcl.setParent('7', '1');
dcl.updateEntityComponent('7', 'engine.transform', 1, JSON.stringify({"position":{"x":14,"y":0,"z":9.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));
dcl.attachEntityComponent('7', 'engine.shape', 'gltfShape4');

dcl.addEntity('8');
dcl.setParent('8', '1');
dcl.updateEntityComponent('8', 'engine.transform', 1, JSON.stringify({"position":{"x":2,"y":0,"z":7.909650802612305},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1.6005480289459229,"z":1}}));
dcl.attachEntityComponent('8', 'engine.shape', 'gltfShape4');

dcl.addEntity('9');
dcl.setParent('9', '1');
dcl.updateEntityComponent('9', 'engine.transform', 1, JSON.stringify({"position":{"x":12.5,"y":0,"z":13},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));
dcl.attachEntityComponent('9', 'engine.shape', 'gltfShape4');

dcl.addEntity('10');
dcl.setParent('10', '1');
dcl.updateEntityComponent('10', 'engine.transform', 1, JSON.stringify({"position":{"x":14,"y":0,"z":1.5},"rotation":{"x":-5.292598351518055e-17,"y":-0.6455901265144348,"z":7.69603332173574e-8,"w":0.763684093952179},"scale":{"x":1,"y":1,"z":1}}));
dcl.attachEntityComponent('10', 'engine.shape', 'gltfShape4');

dcl.addEntity('11');
dcl.setParent('11', '1');
dcl.updateEntityComponent('11', 'engine.transform', 1, JSON.stringify({"position":{"x":7.346514701843262,"y":0,"z":14.072413444519043},"rotation":{"x":2.032753924292364e-15,"y":-0.7190765738487244,"z":8.57205932902616e-8,"w":0.6949308514595032},"scale":{"x":1,"y":1,"z":1}}));
dcl.componentCreated('gltfShape5', 'engine.shape', 54);
dcl.componentUpdated('gltfShape5', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Plant_03/Plant_03.glb"}));
dcl.attachEntityComponent('11', 'engine.shape', 'gltfShape5');

dcl.addEntity('12');
dcl.setParent('12', '1');
dcl.updateEntityComponent('12', 'engine.transform', 1, JSON.stringify({"position":{"x":14.10966968536377,"y":0,"z":5},"rotation":{"x":-5.119466286172044e-15,"y":-0.9998422861099243,"z":1.1919047437913832e-7,"w":0.01776476390659809},"scale":{"x":1,"y":1,"z":1}}));
dcl.attachEntityComponent('12', 'engine.shape', 'gltfShape5');

dcl.addEntity('13');
dcl.setParent('13', '1');
dcl.updateEntityComponent('13', 'engine.transform', 1, JSON.stringify({"position":{"x":4,"y":0,"z":9.028438568115234},"rotation":{"x":6.154739918096759e-16,"y":-0.7383295297622681,"z":8.801572448646766e-8,"w":0.6744402050971985},"scale":{"x":1,"y":1,"z":1}}));
dcl.attachEntityComponent('13', 'engine.shape', 'gltfShape5');

dcl.addEntity('14');
dcl.setParent('14', '1');
dcl.updateEntityComponent('14', 'engine.transform', 1, JSON.stringify({"position":{"x":7,"y":0,"z":7},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":2.1069202423095703,"y":0.25070422887802124,"z":3.9693355560302734}}));
dcl.componentCreated('gltfShape6', 'engine.shape', 54);
dcl.componentUpdated('gltfShape6', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Snow_01.glb"}));
dcl.attachEntityComponent('14', 'engine.shape', 'gltfShape6');

dcl.addEntity('15');
dcl.setParent('15', '1');
dcl.updateEntityComponent('15', 'engine.transform', 1, JSON.stringify({"position":{"x":12,"y":0,"z":13},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":2.3168864250183105,"y":0.311171293258667,"z":2.3168864250183105}}));
dcl.componentCreated('gltfShape7', 'engine.shape', 54);
dcl.componentUpdated('gltfShape7', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Snow_02.glb"}));
dcl.attachEntityComponent('15', 'engine.shape', 'gltfShape7');

dcl.addEntity('16');
dcl.setParent('16', '1');
dcl.updateEntityComponent('16', 'engine.transform', 1, JSON.stringify({"position":{"x":10,"y":0,"z":6.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1.5,"y":1.5,"z":1.5}}));
dcl.componentCreated('gltfShape8', 'engine.shape', 54);
dcl.componentUpdated('gltfShape8', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/SnowRock_01.glb"}));
dcl.attachEntityComponent('16', 'engine.shape', 'gltfShape8');

dcl.addEntity('17');
dcl.setParent('17', '1');
dcl.updateEntityComponent('17', 'engine.transform', 1, JSON.stringify({"position":{"x":3.5,"y":0,"z":13.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));
dcl.componentCreated('gltfShape9', 'engine.shape', 54);
dcl.componentUpdated('gltfShape9', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Plant_01.glb"}));
dcl.attachEntityComponent('17', 'engine.shape', 'gltfShape9');

dcl.addEntity('18');
dcl.setParent('18', '1');
dcl.updateEntityComponent('18', 'engine.transform', 1, JSON.stringify({"position":{"x":5.5,"y":0,"z":3},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));
dcl.attachEntityComponent('18', 'engine.shape', 'gltfShape9');

dcl.addEntity('19');
dcl.setParent('19', '1');
dcl.updateEntityComponent('19', 'engine.transform', 1, JSON.stringify({"position":{"x":7,"y":0,"z":6.5},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));
dcl.componentCreated('gltfShape10', 'engine.shape', 54);
dcl.componentUpdated('gltfShape10', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Bush_02.glb"}));
dcl.attachEntityComponent('19', 'engine.shape', 'gltfShape10');

dcl.addEntity('20');
dcl.setParent('20', '1');
dcl.updateEntityComponent('20', 'engine.transform', 1, JSON.stringify({"position":{"x":4.5,"y":0,"z":11.5},"rotation":{"x":1.1778919573857106e-15,"y":-0.8205099105834961,"z":9.781239640460626e-8,"w":0.5716323852539062},"scale":{"x":0.5601911544799805,"y":0.5601911544799805,"z":0.5601911544799805}}));
dcl.componentCreated('gltfShape11', 'engine.shape', 54);
dcl.componentUpdated('gltfShape11', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Bush_03.glb"}));
dcl.attachEntityComponent('20', 'engine.shape', 'gltfShape11');

dcl.addEntity('21');
dcl.setParent('21', '1');
dcl.updateEntityComponent('21', 'engine.transform', 1, JSON.stringify({"position":{"x":9.5,"y":0,"z":6},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));
dcl.componentCreated('gltfShape12', 'engine.shape', 54);
dcl.componentUpdated('gltfShape12', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/SnowTree_03.glb"}));
dcl.attachEntityComponent('21', 'engine.shape', 'gltfShape12');

dcl.addEntity('22');
dcl.setParent('22', '1');
dcl.updateEntityComponent('22', 'engine.transform', 1, JSON.stringify({"position":{"x":11.296821594238281,"y":0,"z":10.5},"rotation":{"x":-4.510888254283761e-16,"y":0.4592313766479492,"z":-5.474464259691558e-8,"w":0.8883166909217834},"scale":{"x":1,"y":1,"z":1}}));
dcl.componentCreated('gltfShape13', 'engine.shape', 54);
dcl.componentUpdated('gltfShape13', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/SnowLog_02.glb"}));
dcl.attachEntityComponent('22', 'engine.shape', 'gltfShape13');

dcl.addEntity('23');
dcl.setParent('23', '1');
dcl.updateEntityComponent('23', 'engine.transform', 1, JSON.stringify({"position":{"x":4.5,"y":0,"z":2.5},"rotation":{"x":-2.077821715149348e-16,"y":-0.6574377417564392,"z":7.83726719078004e-8,"w":0.7535089254379272},"scale":{"x":1,"y":1,"z":1}}));
dcl.componentCreated('gltfShape14', 'engine.shape', 54);
dcl.componentUpdated('gltfShape14', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/SnowLog_01.glb"}));
dcl.attachEntityComponent('23', 'engine.shape', 'gltfShape14');

dcl.addEntity('24');
dcl.setParent('24', '1');
dcl.updateEntityComponent('24', 'engine.transform', 1, JSON.stringify({"position":{"x":7,"y":0,"z":12},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));
dcl.componentCreated('gltfShape15', 'engine.shape', 54);
dcl.componentUpdated('gltfShape15', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Fire_01.glb"}));
dcl.attachEntityComponent('24', 'engine.shape', 'gltfShape15');

dcl.addEntity('25');
dcl.setParent('25', '1');
dcl.updateEntityComponent('25', 'engine.transform', 1, JSON.stringify({"position":{"x":3,"y":0,"z":4},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));
dcl.componentCreated('gltfShape16', 'engine.shape', 54);
dcl.componentUpdated('gltfShape16', JSON.stringify({"withCollisions":true,"visible":true,"src":"models/Flower_01.glb"}));
dcl.attachEntityComponent('25', 'engine.shape', 'gltfShape16');

dcl.addEntity('26');
dcl.setParent('26', '1');
dcl.updateEntityComponent('26', 'engine.transform', 1, JSON.stringify({"position":{"x":15.101435661315918,"y":0,"z":10},"rotation":{"x":4.1047714255799906e-16,"y":-0.5086968541145325,"z":6.064138347028347e-8,"w":0.8609457015991211},"scale":{"x":1,"y":1,"z":1}}));
dcl.attachEntityComponent('26', 'engine.shape', 'gltfShape16');

dcl.addEntity('27');
dcl.setParent('27', '1');
dcl.updateEntityComponent('27', 'engine.transform', 1, JSON.stringify({"position":{"x":9.5,"y":0,"z":4},"rotation":{"x":0,"y":0,"z":0,"w":1},"scale":{"x":1,"y":1,"z":1}}));
dcl.attachEntityComponent('27', 'engine.shape', 'gltfShape16');
