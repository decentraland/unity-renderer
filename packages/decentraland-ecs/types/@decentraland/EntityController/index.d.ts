declare module '@decentraland/EntityController' {
  /**
   * Returns scene limits
   */
  export function querySceneLimits(): Promise<{
    triangles: number
    entities: number
    bodies: number
    materials: number
    textures: number
  }>
}
