import { encodeParcelPosition } from 'lib/decentraland/parcels/encodeParcelPosition'
import { parseParcelPosition } from 'lib/decentraland/parcels/parseParcelPosition'
import { jsonFetch } from 'lib/javascript/jsonFetch'
import { pickProperty } from 'lib/javascript/pickProperty'
import { reportScenesFromTiles } from 'shared/atlas/actions'
import { getPoiTiles, postProcessSceneName } from 'shared/atlas/selectors'
import { ensureRealmAdapter } from 'shared/realm/ensureRealmAdapter'
import { getFetchContentUrlPrefixFromRealmAdapter, getHotScenesService } from 'shared/realm/selectors'
import { fetchScenesByLocation } from 'shared/scene-loader/sagas'
import { getThumbnailUrlFromJsonDataAndContent } from 'lib/decentraland/sceneJson/getThumbnailUrlFromJsonDataAndContent'
import { store } from 'shared/store/isolatedStore'
import { getUnityInterface, HotSceneInfo, RealmInfo } from 'unity-interface/IUnityInterface'
import { getSceneNameFromJsonData } from 'lib/decentraland/sceneJson/getSceneNameFromJsonData'
import { getSceneDescriptionFromJsonData } from 'lib/decentraland/sceneJson/getSceneDescriptionFromJsonData'
import { getOwnerNameFromJsonData } from 'lib/decentraland/sceneJson/getOwnerNameFromJsonData'

export async function fetchHotScenes(): Promise<HotSceneInfo[]> {
  await ensureRealmAdapter()
  const url = getHotScenesService(store.getState())
  const info = await jsonFetch(url)
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
}

export async function reportHotScenes() {
  const [hotScenes, pois] = await Promise.all([
    fetchHotScenes(),
    // NOTE: we report POI as hotscenes for now, approach should change in next iteration
    fetchPOIsAsHotSceneInfo()
  ])
  const hotScenesIds = pickProperty(hotScenes, 'id')
  const isHotScene = (_: HotSceneInfo) => hotScenesIds.includes(_.id)
  const encodeBaseCoords = (_: HotSceneInfo) => encodeParcelPosition(_.baseCoords)

  const report = hotScenes.concat(pois.filter(isHotScene))
  store.dispatch(reportScenesFromTiles(report.map(encodeBaseCoords)))

  getUnityInterface().UpdateHotScenesList(report)
}

async function fetchPOIsAsHotSceneInfo(): Promise<HotSceneInfo[]> {
  const tiles = getPoiTiles(store.getState())
  const [scenesLandByLocation, bff] = await Promise.all([fetchScenesByLocation(tiles), ensureRealmAdapter()])
  const scenesLand = scenesLandByLocation.filter((_) => !!_.entity.metadata)
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
