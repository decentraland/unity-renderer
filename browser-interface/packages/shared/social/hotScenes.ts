import { parseParcelPosition } from 'lib/decentraland/parcels/parseParcelPosition'
import { getOwnerNameFromJsonData } from 'lib/decentraland/sceneJson/getOwnerNameFromJsonData'
import { getSceneDescriptionFromJsonData } from 'lib/decentraland/sceneJson/getSceneDescriptionFromJsonData'
import { getSceneNameFromJsonData } from 'lib/decentraland/sceneJson/getSceneNameFromJsonData'
import { getThumbnailUrlFromJsonDataAndContent } from 'lib/decentraland/sceneJson/getThumbnailUrlFromJsonDataAndContent'
import { reportScenesFromTiles } from 'shared/atlas/actions'
import { getPoiTiles, postProcessSceneName } from 'shared/atlas/selectors'
import { getHotScenesService } from 'shared/dao/selectors'
import { ensureRealmAdapter } from 'shared/realm/ensureRealmAdapter'
import { getFetchContentUrlPrefixFromRealmAdapter } from 'shared/realm/selectors'
import { fetchScenesByLocation } from 'shared/scene-loader/sagas'
import { store } from 'shared/store/isolatedStore'
import { getUnityInstance, HotSceneInfo, RealmInfo } from 'unity-interface/IUnityInterface'

export async function fetchHotScenes(): Promise<HotSceneInfo[]> {
  await ensureRealmAdapter()
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
  const bff = await ensureRealmAdapter()
  const baseContentUrl = getFetchContentUrlPrefixFromRealmAdapter(bff)

  return scenesLand.map((land) => {
    return {
      id: land.id,
      name: postProcessSceneName(getSceneNameFromJsonData(land.entity.metadata)),
      creator: getOwnerNameFromJsonData(land.entity.metadata),
      description: getSceneDescriptionFromJsonData(land.entity.metadata),
      thumbnail: getThumbnailUrlFromJsonDataAndContent(land.entity.metadata, land.entity.content, baseContentUrl) ?? '',
      baseCoords: parseParcelPosition(land.entity.metadata.scene.base),
      parcels: land.entity.metadata ? land.entity.metadata.scene.parcels.map(parseParcelPosition) : [],
      realms: [{ serverName: '', layer: '', usersMax: 0, usersCount: 0, userParcels: [] }],
      usersTotalCount: 0
    }
  })
}
