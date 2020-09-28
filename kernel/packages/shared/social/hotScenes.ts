import { refreshCandidatesStatuses } from 'shared/dao'
import { Candidate } from 'shared/dao/types'
import { StoreContainer } from 'shared/store/rootTypes'
import { fetchSceneIds } from 'decentraland-loader/lifecycle/utils/fetchSceneIds'
import { fetchSceneJson } from 'decentraland-loader/lifecycle/utils/fetchSceneJson'
import { SceneJsonData } from 'shared/types'
import { reportScenesFromTiles } from 'shared/atlas/actions'
import { getSceneNameFromAtlasState, postProcessSceneName, getPoiTiles } from 'shared/atlas/selectors'

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

type HotSceneInfoRaw = {
  name: string
  baseCoord: string
  realmsInfo: RealmInfo[]
}

export type HotSceneInfo = {
  baseCoords: { x: number; y: number }
  usersTotalCount: number
  realms: RealmInfo[]
}

export async function fetchHotScenes(): Promise<HotSceneInfoRaw[]> {
  const candidates = await refreshCandidatesStatuses()

  let crowdedScenes: Record<string, HotSceneInfoRaw> = {}

  const filteredCandidates = candidates.filter(
    (candidate) => candidate.layer && candidate.layer.usersCount > 0 && candidate.layer.usersParcels
  )

  for (const candidate of filteredCandidates) {
    await fillHotScenesRecord(candidate, crowdedScenes)
  }

  const sceneValues = Object.values(crowdedScenes)
  sceneValues.forEach((scene) => scene.realmsInfo.sort((a, b) => (a.usersCount > b.usersCount ? -1 : 1)))

  return sceneValues.sort((a, b) => (countUsers(a) > countUsers(b) ? -1 : 1))
}

export async function reportHotScenes() {
  const hotScenes = (await fetchHotScenes()).filter(
    (scene) =>
      globalThis.globalStore.getState().atlas.tileToScene[scene.baseCoord] &&
      globalThis.globalStore.getState().atlas.tileToScene[scene.baseCoord].type !== 7 // NOTE: filter roads
  )

  // NOTE: we report POI as hotscenes for now, approach should change in next iteration
  const pois = await fetchPOIsAsHotSceneInfoRaw()
  const report = hotScenes.concat(
    pois.filter((poi) => hotScenes.filter((scene) => scene.baseCoord === poi.baseCoord).length === 0)
  )

  globalThis.globalStore.dispatch(reportScenesFromTiles(report.map((scene) => scene.baseCoord)))
  window.unityInterface.UpdateHotScenesList(report.map((scene) => hotSceneInfoFromRaw(scene)))
}

function countUsers(a: HotSceneInfoRaw) {
  return a.realmsInfo.reduce((total, realmInfo) => total + realmInfo.usersCount, 0)
}

async function fillHotScenesRecord(candidate: Candidate, crowdedScenes: Record<string, HotSceneInfoRaw>) {
  const tiles =
    candidate.layer.usersParcels
      // tslint:disable:strict-type-predicates
      ?.filter((value) => typeof value[0] !== 'undefined' && typeof value[1] !== 'undefined')
      .map((value) => `${value[0]},${value[1]}`) ?? []

  const scenesId = await fetchSceneIds(tiles)

  for (let i = 0; i < tiles.length; i++) {
    const id = scenesId[i] ?? tiles[i]
    const land = scenesId[i] ? (await fetchSceneJson([scenesId[i]!]))[0] : null

    if (crowdedScenes[id]) {
      const realmInfo = crowdedScenes[id].realmsInfo.filter(
        (realm) => realm.serverName === candidate.catalystName && realm.layer === candidate.layer.name
      )

      if (realmInfo[0]) {
        realmInfo[0].usersCount += 1
      } else {
        crowdedScenes[id].realmsInfo.push(createRealmInfo(candidate, 1))
      }
    } else {
      crowdedScenes[id] = createHotSceneInfoRaw(
        candidate,
        land?.sceneJsonData?.scene.base ?? tiles[i],
        land?.sceneJsonData
      )
    }
  }
}

function createHotSceneInfoRaw(
  candidate: Candidate,
  baseCoord: string,
  sceneJsonData: SceneJsonData | undefined
): HotSceneInfoRaw {
  return {
    name: getSceneName(baseCoord, sceneJsonData),
    baseCoord: baseCoord,
    realmsInfo: [createRealmInfo(candidate, 1)]
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

function hotSceneInfoFromRaw(hotSceneInfoRaw: HotSceneInfoRaw): HotSceneInfo {
  const baseCoord = hotSceneInfoRaw.baseCoord.split(',').map((str) => parseInt(str, 10)) as [number, number]
  return {
    baseCoords: { x: baseCoord[0], y: baseCoord[1] },
    usersTotalCount: countUsers(hotSceneInfoRaw),
    realms: hotSceneInfoRaw.realmsInfo
  }
}

async function fetchPOIsAsHotSceneInfoRaw(): Promise<HotSceneInfoRaw[]> {
  const tiles = getPoiTiles(globalThis.globalStore.getState())
  const scenesId = (await fetchSceneIds(tiles)).filter((id) => id !== null) as string[]
  const scenesLand = (await fetchSceneJson(scenesId)).filter((land) => land.sceneJsonData)

  return scenesLand.map((land) => {
    return {
      name: getSceneName(land.sceneJsonData.scene.base, land.sceneJsonData),
      baseCoord: land.sceneJsonData.scene.base,
      realmsInfo: [{ serverName: '', layer: '', usersMax: 0, usersCount: 0 }]
    }
  })
}
