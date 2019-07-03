import { Entity, engine, Vector3, Transform, OBJShape, Quaternion } from 'decentraland-ecs/src'

function makeOBJ(src: string, position: Vector3, scale: Vector3, rotation?: Quaternion) {
  const ent = new Entity()
  ent.addComponentOrReplace(new OBJShape(src))
  ent.addComponentOrReplace(
    new Transform({
      position,
      scale,
      rotation
    })
  )
  engine.addEntity(ent)
  return ent
}

makeOBJ('models/pine-tree.obj', new Vector3(8.912, 3.403, 6.661), new Vector3(2, 2, 2))
makeOBJ('models/pine-tree.obj', new Vector3(-2.835, 3.449, 15.471), new Vector3(2, 2, 2))
makeOBJ('models/pine-tree.obj', new Vector3(-1.662, 7.042, 28.722), new Vector3(1.5, 1.5, 1.5))
makeOBJ('models/pine-tree.obj', new Vector3(-0.781, 4.05, 10.748), new Vector3(2.5, 2.5, 2.5))
makeOBJ('models/pine-tree.obj', new Vector3(10.119, 3.215, -0.827), new Vector3(1.8, 1.8, 1.8))
makeOBJ('models/pine-tree.obj', new Vector3(4.654, 3.403, -2.98), new Vector3(2, 2, 2))
makeOBJ('models/pine-tree.obj', new Vector3(9.993, 6.759, 11.236), new Vector3(4, 4, 4))
makeOBJ(
  'models/candy-cane.obj',
  new Vector3(1.988, 0.932, -0.224),
  new Vector3(0.05, 0.05, 0.05),
  Quaternion.Euler(-34.20558036931015, 0, 0)
)
makeOBJ(
  'models/candy-cane.obj',
  new Vector3(2.178, 0.906, -0.086),
  new Vector3(0.05, 0.05, 0.05),
  Quaternion.Euler(-34.20558036931015, -94.88181087366432, 0)
)
makeOBJ(
  'models/candy-cane.obj',
  new Vector3(2.09, 1.022, -0.086),
  new Vector3(0.05, 0.05, 0.05),
  Quaternion.Euler(52.54, -96.658, -8.021)
)
makeOBJ(
  'models/candy-cane.obj',
  new Vector3(2.066, 1.084, 0.004),
  new Vector3(0.05, 0.05, 0.05),
  Quaternion.Euler(85.37071147449267, 113.445643435903, -63.36913214146905)
)
makeOBJ(
  'models/candy-cane.obj',
  new Vector3(2.066, 1.056, -0.386),
  new Vector3(0.05, 0.05, 0.05),
  Quaternion.Euler(85.37071147449267, -162.14705602202298, -63.36913214146905)
)
makeOBJ('models/bowl.obj', new Vector3(2.043, 1.209, -0.128), new Vector3(3, 3, 3))

// These can not be ported to the ECS (no separate mtl support)
// <obj-model position="-2.032 2.328 0" scale="7 7 7" mtl="https://gateway.ipfs.io/ipfs/QmXcvyrf7XPBsUZtbxLbihQwXBDhc56PooCwj4QecQp8YQ/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmPUFagJ5za35vcYu5bACno66XEoNzqjMijQSuR3nQd9nk/model-triangulated.obj"></obj-model>
//   <obj-model position="8.823 1.345 22.482" rotation="0 166.9026057216088 0" scale="3 3 3" mtl="https://gateway.ipfs.io/ipfs/QmbtmasxqFa6D8uHUSXaAb84BitJJGuBtbhaaCmh33CBS9/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmVCNVFuvLetxQ7EujW4f9bzSiuGHgn8bebfLr2xbTWmXJ/model.obj"></obj-model>
//   <obj-model position="0 1.499 2.999" mtl="https://gateway.ipfs.io/ipfs/QmQoZnfZW6KxjQyKQo36KVcMKRAv4hTNukxRWCE8hvVvt7/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/Qma1x9wotKW6tiSckW5WanQ4xmqf32d4wj4DGqJobLo9Rb/model-triangulated.obj"></obj-model>
//   <obj-model position="-2.918 6.124 -0.313" rotation="-19.595156593474155 74.94287960311168 1.7188733853924696" scale="0.01 0.01 0.01"
//     material="emissive:#ffff00;color:#ffff00" src="https://gateway.ipfs.io/ipfs/Qmc8o992DHC2MfMzCSC6AQpX3uxZYNQcqBD4HgTQxNgRes/Star_01.obj"></obj-model>
//   <obj-model position="4.448 2.652 6.478" rotation="0 -67.20794936884556 0" scale="4 4 4" mtl="https://gateway.ipfs.io/ipfs/QmUZ2h3KVAWuBaqgACAsEpMN5mP4BsEqDMMkqNNeu4y8jN/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmSnhf8RE4RBLM73poDnySgTKFk14yZx94c6FKCzAhhnBf/model-triangulated.obj"></obj-model>
//   <obj-model position="7.19 1.088 1.409" rotation="0 75.688 0" scale="2 2 2" mtl="https://gateway.ipfs.io/ipfs/QmbtmasxqFa6D8uHUSXaAb84BitJJGuBtbhaaCmh33CBS9/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmYwfuThL4Z7yWuETq8AX77P5SzeYsUmuAmp6mkrcaVJR8/model.obj"></obj-model>
//   <obj-model position="-2.342 0.926 3.086" scale="2 2 2" mtl="https://gateway.ipfs.io/ipfs/QmUZyyAgJGoAmXStWmP5Tfxyf7Z3Tg7kyyXLxmMCpTUsyN/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmNfaRigfgWeLQfxtUsiCYgHwCSGqyZajGA24x2vVf8CHF/model-triangulated.obj"></obj-model>
//   <obj-model position="6.185 0.512 2.152" scale="0.5 0.5 0.5" mtl="https://gateway.ipfs.io/ipfs/QmbtmasxqFa6D8uHUSXaAb84BitJJGuBtbhaaCmh33CBS9/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmQjkAPHPkgdVx6DV3ajCyrgCLd8kS8juxUWUDcWFBnCgJ/model.obj"></obj-model>
//   <obj-model position="2.266 1.017 -0.452" rotation="46.18039828754436 43.54479242994257 73.28130199723229" scale="2 2 2" mtl="https://gateway.ipfs.io/ipfs/QmZs3qyMFxMfx8xBGSMDaa9AwA6LFiHMZkRxuSMNgNLTFD/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmXXXQCxnRLY1Z1DZoU8iLkmSuRFTYYxT3n2SD8fdnk1Vq/model-triangulated.obj"></obj-model>
//   <obj-model position="4.443 5.422 0" mtl="https://gateway.ipfs.io/ipfs/QmVRtMnxPuNZwkVKXrBc6aEX9ERh8FNjzju43weMDm3VPo/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmbJNZuzCfAT6UaU6ZJpGNJGvRMD8q246GFyCcwXQMr5nw/model-triangulated.obj"></obj-model>
//   <obj-model position="4.517 0.29 3.871" rotation="0 114.36237590811231 0" mtl="https://gateway.ipfs.io/ipfs/QmbtmasxqFa6D8uHUSXaAb84BitJJGuBtbhaaCmh33CBS9/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmPiU2vPFYyq59JDVmx7cQ8bf29APzMpg7rMfeSxf4MXDt/model.obj"></obj-model>
//   <obj-model shadow="cast:true;receive:true" position="0.573 0.5 2.562" rotation="0 -99.179 0" scale="1 1 1" mtl="https://gateway.ipfs.io/ipfs/QmNZ3spCMpEAH1jKCZLiwS5cY5MMLzhXZ6pKCBeSDzNV6e/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmQR3LrFdbZhPN6X2ZpkqYHXxcTNH8NZns35rE7zbNwxCe/model-triangulated.obj"></obj-model>
//   <obj-model shadow="cast:true;receive:true" position="4.701 5.576 6.652" rotation="-3.6096341093241864 7.792226013779197 24.579889411112315"
//     scale="1.3 1.3 1.3" mtl="https://gateway.ipfs.io/ipfs/QmNZ3spCMpEAH1jKCZLiwS5cY5MMLzhXZ6pKCBeSDzNV6e/materials.mtl" src="https://gateway.ipfs.io/ipfs/QmQR3LrFdbZhPN6X2ZpkqYHXxcTNH8NZns35rE7zbNwxCe/model-triangulated.obj"></obj-model>
//   <obj-model position="3.784 5.356 6.89" rotation="0 0 180" scale="1.4 1.4 1.4" mtl="https://gateway.ipfs.io/ipfs/QmXjp8e3Qywsc8wcenMbVNTsU7ephGADNftSnKh3RRTsx6/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmcMUTbgbsjmJBzZ4A38i6XDhSp7nJeXfAghT3A5c1Hv4E/model-triangulated.obj"></obj-model>
//   <obj-model position="4.052 2.125 25.224" rotation="0 -178.24717006519913 0" scale="12 12 12" mtl="https://gateway.ipfs.io/ipfs/QmWEWHJmbouhBk1upVcBkBVXD7syp7bE4tnAJiYHrMk2Pj/materials.mtl"
//     src="https://gateway.ipfs.io/ipfs/QmdxBqw4m3XUURxjkxKPNXALmdo2qWW325JG51kpF3rT5G/model-triangulated.obj"></obj-model>
