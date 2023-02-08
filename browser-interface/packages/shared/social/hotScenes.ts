import { reportScenesFromTiles } from 'shared/atlas/actions'
import { postProcessSceneName, getPoiTiles } from 'shared/atlas/selectors'
import { getHotScenesService } from 'shared/dao/selectors'
import {
  getOwnerNameFromJsonData,
  getThumbnailUrlFromJsonDataAndContent,
  getSceneDescriptionFromJsonData,
  getSceneNameFromJsonData
} from 'shared/selectors'
import { getUnityInstance, HotSceneInfo, RealmInfo } from 'unity-interface/IUnityInterface'
import { store } from 'shared/store/isolatedStore'
import { ensureRealmAdapterPromise, getFetchContentUrlPrefixFromRealmAdapter } from 'shared/realm/selectors'
import { fetchScenesByLocation } from 'shared/scene-loader/sagas'

export async function fetchHotScenes(): Promise<HotSceneInfo[]> {
  await ensureRealmAdapterPromise()
  const url = getHotScenesService(store.getState())
  const response = await fetch(url)
  if (response.ok) {
    const info = await response.json()
    return info.map((scene: any) => {
      return {
        ...scene,
        baseCoords: { x: scene.baseCoords[0], y: scene.baseCoords[1] },
        parcels: scene.parcels.map((parcel: [number, number]) => {
          return { x: parcel[0], y: parcel[1] }
        }),
        realms: scene.realms.map((realm: any) => {
          return {
            ...realm,
            userParcels: realm.userParcels.map((parcel: [number, number]) => {
              return { x: parcel[0], y: parcel[1] }
            })
          } as RealmInfo
        })
      } as HotSceneInfo
    })
  } else {
    throw new Error(`Error fetching hot scenes. Response not OK. Status: ${response.status}`)
  }
}

export async function reportHotScenes() {
  const hotScenes = await fetchHotScenes()

  // NOTE: we report POI as hotscenes for now, approach should change in next iteration
  const pois = await fetchPOIsAsHotSceneInfo()
  const report = hotScenes.concat(pois.filter((poi) => hotScenes.filter((scene) => scene.id === poi.id).length === 0))

  store.dispatch(reportScenesFromTiles(report.map((scene) => `${scene.baseCoords.x},${scene.baseCoords.y}`)))

  getUnityInstance().UpdateHotScenesList(report)
}

async function fetchPOIsAsHotSceneInfo(): Promise<HotSceneInfo[]> {
  const tiles = getPoiTiles(store.getState())
  const scenesLand = (await fetchScenesByLocation(tiles)).filter((land) => land.entity.metadata)
  const bff = await ensureRealmAdapterPromise()
  const baseContentUrl = getFetchContentUrlPrefixFromRealmAdapter(bff)

  return scenesLand.map((land) => {
    return {
      id: land.id,
      name: postProcessSceneName(getSceneNameFromJsonData(land.entity.metadata)),
      creator: getOwnerNameFromJsonData(land.entity.metadata),
      description: getSceneDescriptionFromJsonData(land.entity.metadata),
      thumbnail: getThumbnailUrlFromJsonDataAndContent(land.entity.metadata, land.entity.content, baseContentUrl) ?? '',
      baseCoords: TileStringToVector2(land.entity.metadata.scene.base),
      parcels: land.entity.metadata
        ? land.entity.metadata.scene.parcels.map((parcel) => {
            const coord = parcel.split(',').map((str) => parseInt(str, 10)) as [number, number]
            return { x: coord[0], y: coord[1] }
          })
        : [],
      realms: [{ serverName: '', layer: '', usersMax: 0, usersCount: 0, userParcels: [] }],
      usersTotalCount: 0
    }
  })
}

function TileStringToVector2(tileValue: string): { x: number; y: number } {
  const tile = tileValue.split(',').map((str) => parseInt(str, 10)) as [number, number]
  return { x: tile[0], y: tile[1] }
}
