declare module '@decentraland/ParcelIdentity' {

  export type ILand = {
    sceneJsonData: SceneJsonData
  }

  export type SceneJsonData = {
    display?: SceneDisplay
    owner?: string
    contact?: SceneContact
    tags?: string[]
    scene: SceneParcels
    spawnPoints?: SceneSpawnPoint[]
    requiredPermissions?: string[]
  }

  export type SceneDisplay = {
    title?: string
    favicon?: string
    description?: string
    navmapThumbnail?: string
  }

  export type SceneContact = {
    name?: string
    email?: string
    url?: string
  }

  export type SceneParcels = {
    base: string
    parcels: string[]
  }

  export type SceneSpawnPoint = {
    name?: string
    position: {
      x: number | number[]
      y: number | number[]
      z: number | number[]
    }
    default?: boolean
  }

  /**
   * Returns the current parcel data
   */
  export function getParcel(): Promise<{ land: ILand; cid: string }>
}
