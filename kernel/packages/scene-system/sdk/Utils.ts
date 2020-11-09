import { CLASS_ID, Transform } from "decentraland-ecs/src"
import { PB_Transform, PB_Vector3, PB_Quaternion } from "../../shared/proto/engineinterface_pb"

const pbTransform: PB_Transform = new PB_Transform()
const pbPosition: PB_Vector3 = new PB_Vector3()
const pbRotation: PB_Quaternion = new PB_Quaternion()
const pbScale: PB_Vector3 = new PB_Vector3()

export function generatePBObject(classId: CLASS_ID, json: string): string {
  if (classId === CLASS_ID.TRANSFORM) {
    const transform: Transform = JSON.parse(json)
    return serializeTransform(transform)
  }

  return json
}

export function generatePBObjectJSON(classId: CLASS_ID, json: any): string {
  if (classId === CLASS_ID.TRANSFORM) {
    return serializeTransform(json)
  }
  return JSON.stringify(json)
}

function serializeTransform(transform: Transform): string {
  pbPosition.setX(Math.fround(transform.position.x))
  pbPosition.setY(Math.fround(transform.position.y))
  pbPosition.setZ(Math.fround(transform.position.z))

  pbRotation.setX(transform.rotation.x)
  pbRotation.setY(transform.rotation.y)
  pbRotation.setZ(transform.rotation.z)
  pbRotation.setW(transform.rotation.w)

  pbScale.setX(Math.fround(transform.scale.x))
  pbScale.setY(Math.fround(transform.scale.y))
  pbScale.setZ(Math.fround(transform.scale.z))

  pbTransform.setPosition(pbPosition)
  pbTransform.setRotation(pbRotation)
  pbTransform.setScale(pbScale)

  let arrayBuffer: Uint8Array = pbTransform.serializeBinary()
  return btoa(String.fromCharCode(...arrayBuffer))
}