import { triangles, bodies, entities, materials, textures, geometries } from './limits'
import type { IParcelSceneLimits } from './IParcelSceneLimits'

export function getParcelSceneLimits(parcelCount: number): IParcelSceneLimits {
  const log = Math.log2(parcelCount + 1)
  const lineal = parcelCount
  return {
    triangles: Math.floor(lineal * triangles),
    bodies: Math.floor(lineal * bodies),
    entities: Math.floor(lineal * entities),
    materials: Math.floor(log * materials),
    textures: Math.floor(log * textures),
    geometries: Math.floor(log * geometries)
  }
}
