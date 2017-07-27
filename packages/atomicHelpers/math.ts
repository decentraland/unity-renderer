export const DEG2RAD = Math.PI / 180
export const RAD2DEG = 360 / (Math.PI * 2)

export function quaternionToRotation(x: number, y: number, z: number, w: number) {
  const roll = Math.atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z) * RAD2DEG
  const pitch = Math.atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z) * RAD2DEG
  const yaw = Math.asin(2 * x * y + 2 * z * w) * RAD2DEG

  return { x: pitch, y: roll, z: yaw }
}

export function quaternionToRotationBABYLON(quat: BABYLON.Quaternion, rotation: BABYLON.Vector3) {
  const { x, y, z, w } = quat

  const roll = Math.atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z) * RAD2DEG
  const pitch = Math.atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z) * RAD2DEG
  const yaw = Math.asin(2 * x * y + 2 * z * w) * RAD2DEG

  rotation.set(pitch, roll, yaw)
}

export function uuid() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    let r = (Math.random() * 16) | 0
    let v = c === 'x' ? r : (r & 0x3) | 0x8
    return v.toString(16)
  })
}
