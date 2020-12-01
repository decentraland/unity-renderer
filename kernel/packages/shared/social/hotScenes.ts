import { refreshCandidatesStatuses } from 'shared/dao'
import { Candidate } from 'shared/dao/types'
import { StoreContainer } from 'shared/store/rootTypes'
import { fetchSceneIds } from 'decentraland-loader/lifecycle/utils/fetchSceneIds'
import { fetchSceneJson } from 'decentraland-loader/lifecycle/utils/fetchSceneJson'
import { ILand, SceneJsonData } from 'shared/types'
import { reportScenesFromTiles } from 'shared/atlas/actions'
import { getSceneNameFromAtlasState, postProcessSceneName, getPoiTiles } from 'shared/atlas/selectors'
import { getHotScenesService } from 'shared/dao/selectors'
import defaultLogger from 'shared/logger'
import { getOwnerNameFromJsonData, getThumbnailUrlFromJsonData } from 'shared/selectors'

declare const globalThis: StoreContainer

declare const window: {
  unityInterface: {
    UpdateHotScenesList: (info: HotSceneInfo[]) => void
  }
}

type RealmInfo = {
  serverName: string
  layer: string
  usersCount: number
  usersMax: number
}

export type HotSceneInfo = {
  id: string
  name: string
  creator: string
  thumbnail: string
  baseCoords: { x: number; y: number }
  parcels: { x: number; y: number }[]
  usersTotalCount: number
  realms: RealmInfo[]
}

export async function fetchHotScenes(): Promise<HotSceneInfo[]> {
  try {
    const url = getHotScenesService(globalThis.globalStore.getState())
    const response = await fetch(url)
    if (response.ok) {
      const info = await response.json()
      return info.map((scene: any) => {
        return {
          ...scene,
          baseCoords: { x: scene.baseCoords[0], y: scene.baseCoords[1] },
          parcels: scene.parcels.map((parcel: [number, number]) => {
            return { x: parcel[0], y: parcel[1] }
          })
        } as HotSceneInfo
      })
    }
  } catch (e) {
    defaultLogger.log(e)
  }
  return fetchHotScenesNoLambdaFallback()
}

export async function reportHotScenes() {
  const hotScenes = await fetchHotScenes()

  // NOTE: we report POI as hotscenes for now, approach should change in next iteration
  const pois = await fetchPOIsAsHotSceneInfo()
  const report = hotScenes.concat(pois.filter((poi) => hotScenes.filter((scene) => scene.id === poi.id).length === 0))

  globalThis.globalStore.dispatch(
    reportScenesFromTiles(report.map((scene) => `${scene.baseCoords.x},${scene.baseCoords.y}`))
  )
  window.unityInterface.UpdateHotScenesList(report)
}

async function fetchHotScenesNoLambdaFallback(): Promise<HotSceneInfo[]> {
  const candidates = await refreshCandidatesStatuses()

  let crowdedScenes: Record<string, HotSceneInfo> = {}

  const filteredCandidates = candidates.filter(
    (candidate) => candidate.layer && candidate.layer.usersCount > 0 && candidate.layer.usersParcels
  )

  for (const candidate of filteredCandidates) {
    await fillHotScenesRecord(candidate, crowdedScenes)
  }

  const sceneValues = Object.values(crowdedScenes)
  sceneValues.forEach((scene) => scene.realms.sort((a, b) => (a.usersCount > b.usersCount ? -1 : 1)))

  return sceneValues
    .filter((scene) => {
      const strCoords = `${scene.baseCoords.x},${scene.baseCoords.y}`
      return (
        globalThis.globalStore.getState().atlas.tileToScene[strCoords] &&
        globalThis.globalStore.getState().atlas.tileToScene[strCoords].type !== 7
      ) // NOTE: filter roads
    })
    .sort((a, b) => (countUsers(a) > countUsers(b) ? -1 : 1))
}

function countUsers(a: HotSceneInfo) {
  return a.realms.reduce((total, realmInfo) => total + realmInfo.usersCount, 0)
}

async function fillHotScenesRecord(candidate: Candidate, crowdedScenes: Record<string, HotSceneInfo>) {
  const tiles =
    candidate.layer.usersParcels
      // tslint:disable:strict-type-predicates
      ?.filter((value) => typeof value[0] !== 'undefined' && typeof value[1] !== 'undefined')
      .map((value) => `${value[0]},${value[1]}`) ?? []

  const scenesId = await fetchSceneIds(tiles)

  for (let i = 0; i < tiles.length; i++) {
    const id = scenesId[i] ?? tiles[i]
    const land = scenesId[i] ? (await fetchSceneJson([scenesId[i]!]))[0] : undefined

    if (crowdedScenes[id]) {
      const realmInfo = crowdedScenes[id].realms.filter(
        (realm) => realm.serverName === candidate.catalystName && realm.layer === candidate.layer.name
      )

      if (realmInfo[0]) {
        realmInfo[0].usersCount += 1
      } else {
        crowdedScenes[id].realms.push(createRealmInfo(candidate, 1))
      }
      crowdedScenes[id].usersTotalCount++
    } else {
      crowdedScenes[id] = createHotSceneInfo(candidate, land?.sceneJsonData?.scene.base ?? tiles[i], id, land)
    }
  }
}

function createHotSceneInfo(
  candidate: Candidate,
  baseCoord: string,
  id: string,
  land: ILand | undefined
): HotSceneInfo {
  const sceneJsonData: SceneJsonData | undefined = land?.sceneJsonData
  const baseCoords = baseCoord.split(',').map((str) => parseInt(str, 10)) as [number, number]
  return {
    id: id,
    name: getSceneName(baseCoord, sceneJsonData),
    creator: getOwnerNameFromJsonData(sceneJsonData),
    thumbnail: getThumbnailUrlFromJsonData(sceneJsonData) ?? '',
    baseCoords: { x: baseCoords[0], y: baseCoords[1] },
    parcels: sceneJsonData
      ? sceneJsonData.scene.parcels.map((parcel) => {
        const coord = parcel.split(',').map((str) => parseInt(str, 10)) as [number, number]
        return { x: coord[0], y: coord[1] }
      })
      : [],
    realms: [createRealmInfo(candidate, 1)],
    usersTotalCount: 1
  }
}

function createRealmInfo(candidate: Candidate, usersCount: number): RealmInfo {
  return {
    serverName: candidate.catalystName,
    layer: candidate.layer.name,
    usersMax: candidate.layer.maxUsers,
    usersCount: usersCount
  }
}

function getSceneName(baseCoord: string, sceneJsonData: SceneJsonData | undefined): string {
  const sceneName =
    getSceneNameFromAtlasState(sceneJsonData) ?? globalThis.globalStore.getState().atlas.tileToScene[baseCoord]?.name
  return postProcessSceneName(sceneName)
}

async function fetchPOIsAsHotSceneInfo(): Promise<HotSceneInfo[]> {
  const tiles = getPoiTiles(globalThis.globalStore.getState())
  const scenesId = (await fetchSceneIds(tiles)).filter((id) => id !== null) as string[]
  const scenesLand = (await fetchSceneJson(scenesId)).filter((land) => land.sceneJsonData)

  return scenesLand.map((land) => {
    const baseCoords = land.sceneJsonData.scene.base.split(',').map((str) => parseInt(str, 10)) as [number, number]
    return {
      id: land.sceneId,
      name: getSceneName(land.sceneJsonData.scene.base, land.sceneJsonData),
      creator: getOwnerNameFromJsonData(land.sceneJsonData),
      thumbnail: getThumbnailUrlFromJsonData(land.sceneJsonData) ?? '',
      baseCoords: { x: baseCoords[0], y: baseCoords[1] },
      parcels: land.sceneJsonData
        ? land.sceneJsonData.scene.parcels.map((parcel) => {
          const coord = parcel.split(',').map((str) => parseInt(str, 10)) as [number, number]
          return { x: coord[0], y: coord[1] }
        })
        : [],
      realms: [{ serverName: '', layer: '', usersMax: 0, usersCount: 0 }],
      usersTotalCount: 0
    }
  })
}
