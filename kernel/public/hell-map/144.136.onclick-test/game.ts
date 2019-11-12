import {
  Entity,
  ISystem,
  engine,
  Vector3,
  Transform,
  Material,
  BoxShape,
  PlaneShape,
  Color3,
  Camera,
  OnClick
} from 'decentraland-ecs/src'

class FaceCameraSystem implements ISystem {
  update() {
    onclickObjects.forEach(element => {
      element.update()
    })
  }
}

class OnClickObject {
  private materialIndex: number
  private planeTransform: Transform

  constructor(pos: Vector3, startingMaterialIndex: number) {
    const cube = new Entity()
    cube.addComponent(new Transform({ position: pos }))
    cube.addComponent(new BoxShape())
    cube.addComponentOrReplace(materials[startingMaterialIndex])

    const plane = new Entity()
    this.planeTransform = new Transform({ position: pos.add(new Vector3(0, 1, 0)), scale: new Vector3(0.25, 0.25, 1) })
    plane.addComponent(new PlaneShape())
    plane.addComponent(this.planeTransform)
    plane.addComponentOrReplace(materials[GetNextMaterialIndex(startingMaterialIndex)])

    engine.addEntity(cube)
    engine.addEntity(plane)
    engine.addSystem(new FaceCameraSystem())

    this.materialIndex = startingMaterialIndex

    cube.addComponent(
      new OnClick(() => {
        this.materialIndex = GetNextMaterialIndex(this.materialIndex)
        cube.removeComponent(Material)
        plane.removeComponent(Material)
        cube.addComponentOrReplace(materials[this.materialIndex])
        plane.addComponentOrReplace(materials[GetNextMaterialIndex(this.materialIndex)])
      })
    )
  }

  update() {
    let target = this.planeTransform.position.add(Vector3.Forward().rotate(Camera.instance.rotation))
    let up = Vector3.Up().rotate(Camera.instance.rotation)
    this.planeTransform.lookAt(target, up)
  }
}

const materials: Material[] = []
materials.push(CreateMaterial(Color3.Blue()))
materials.push(CreateMaterial(Color3.Gray()))
materials.push(CreateMaterial(Color3.Green()))
materials.push(CreateMaterial(Color3.Magenta()))
materials.push(CreateMaterial(Color3.Teal()))
materials.push(CreateMaterial(Color3.Yellow()))
materials.push(CreateMaterial(Color3.Purple()))
materials.push(CreateMaterial(Color3.Red()))
materials.push(CreateMaterial(Color3.White()))
materials.push(CreateMaterial(Color3.Black()))
materials.push(CreateMaterial(Color3.Green()))
materials.push(CreateMaterial(Color3.Magenta()))
materials.push(CreateMaterial(Color3.Teal()))
materials.push(CreateMaterial(Color3.Yellow()))
materials.push(CreateMaterial(Color3.Purple()))
materials.push(CreateMaterial(Color3.Red()))
materials.push(CreateMaterial(Color3.White()))
materials.push(CreateMaterial(Color3.Green()))
materials.push(CreateMaterial(Color3.Teal()))
materials.push(CreateMaterial(Color3.Purple()))

const onclickObjects: OnClickObject[] = []
onclickObjects.push(new OnClickObject(new Vector3(1, 1, 1), 0))
onclickObjects.push(new OnClickObject(new Vector3(1, 1, 4.5), 2))
onclickObjects.push(new OnClickObject(new Vector3(1, 1, 9), 4))
onclickObjects.push(new OnClickObject(new Vector3(4.5, 1, 1), 6))
onclickObjects.push(new OnClickObject(new Vector3(4.5, 1, 4.5), 8))
onclickObjects.push(new OnClickObject(new Vector3(4.5, 1, 9), 10))
onclickObjects.push(new OnClickObject(new Vector3(9, 1, 1), 12))
onclickObjects.push(new OnClickObject(new Vector3(9, 1, 4.5), 14))
onclickObjects.push(new OnClickObject(new Vector3(9, 1, 9), 16))

function GetNextMaterialIndex(currentIndex: number): number {
  return currentIndex + 1 >= materials.length ? 0 : currentIndex + 1
}

function CreateMaterial(color: Color3): Material {
  const ret = new Material()
  ret.albedoColor = color
  return ret
}
