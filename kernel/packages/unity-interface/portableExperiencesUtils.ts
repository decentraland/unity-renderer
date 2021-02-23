import {
  ContentMapping,
  EnvironmentData,
  LoadablePortableExperienceScene,
  MappingsResponse,
  SceneJsonData
} from '../shared/types'
import { getSceneNameFromJsonData } from '../shared/selectors'
import { parseParcelPosition } from '../atomicHelpers/parcelScenePositions'
import { UnityPortableExperienceScene } from './UnityParcelScene'
import { forceStopParcelSceneWorker, getSceneWorkerBySceneID, loadParcelScene } from 'shared/world/parcelSceneManager'
import { unityInterface } from './UnityInterface'
import { resolveUrlFromUrn } from '@dcl/urn-resolver'

declare var window: any
// TODO: Remove this when portable experiences are full-available
window['spawnPortableExperienceScene'] = spawnPortableExperienceScene
window['killPortableExperienceScene'] = killPortableExperienceScene

export type PortableExperienceHandle = {
  pid: string
  parentCid: string
}

let currentPortableExperiences: Map<string, string> = new Map()

export async function spawnPortableExperienceScene(
  sceneUrn: string,
  parentCid: string
): Promise<PortableExperienceHandle> {
  const peWorker = getSceneWorkerBySceneID(sceneUrn)
  if (peWorker) {
    throw new Error(`Portable Scene: "${sceneUrn}" is already running.`)
  }
  const scene = new UnityPortableExperienceScene(await getPortableExperienceFromS3Bucket(sceneUrn))
  loadParcelScene(scene, undefined, true)
  unityInterface.CreateGlobalScene({
    id: sceneUrn,
    name: scene.data.name,
    baseUrl: scene.data.baseUrl,
    contents: scene.data.data.contents,
    icon: scene.data.data.icon,
    isPortableExperience: true
  })
  currentPortableExperiences.set(sceneUrn, parentCid)

  return { pid: sceneUrn, parentCid: parentCid }
}

export async function killPortableExperienceScene(sceneUrn: string): Promise<boolean> {
  const peWorker = getSceneWorkerBySceneID(sceneUrn)
  if (peWorker) {
    forceStopParcelSceneWorker(peWorker)
    currentPortableExperiences.delete(sceneUrn)
    unityInterface.UnloadScene(sceneUrn)
    return true
  } else {
    return false
  }
}

export async function getPortableExperience(pid: string): Promise<PortableExperienceHandle | undefined> {
  const parentCid = currentPortableExperiences.get(pid)
  return parentCid ? { pid, parentCid } : undefined
}

export async function getPortableExperienceFromS3Bucket(sceneUrn: string) {
  const mappingsUrl = await resolveUrlFromUrn(sceneUrn)
  if (mappingsUrl === null) {
    throw new Error(`Could not resolve mappings for scene: ${sceneUrn}`)
  }
  const mappingsFetch = await fetch(mappingsUrl)
  const mappingsResponse = (await mappingsFetch.json()) as MappingsResponse

  const sceneJsonMapping = mappingsResponse.contents.find(($) => $.file === 'scene.json')

  if (sceneJsonMapping) {
    const baseUrl: string = new URL('.', mappingsUrl).toString()
    const sceneUrl = `${baseUrl}${sceneJsonMapping.hash}`
    const sceneResponse = await fetch(sceneUrl)

    if (sceneResponse.ok) {
      const scene = (await sceneResponse.json()) as SceneJsonData
      return getLoadablePortableExperience({
        sceneUrn: sceneUrn,
        baseUrl: baseUrl,
        mappings: mappingsResponse.contents,
        sceneJsonData: scene
      })
    } else {
      throw new Error('Could not load scene.json')
    }
  } else {
    throw new Error('Could not load scene.json')
  }
}

export async function getLoadablePortableExperience(data: {
  sceneUrn: string
  baseUrl: string
  mappings: ContentMapping[]
  sceneJsonData: SceneJsonData
}): Promise<EnvironmentData<LoadablePortableExperienceScene>> {
  const { sceneUrn, baseUrl, mappings, sceneJsonData } = data

  const sceneJsons = mappings.filter((land) => land.file === 'scene.json')
  if (!sceneJsons.length) {
    throw new Error('Invalid scene mapping: no scene.json')
  }
  // TODO: obtain sceneId from Content Server
  return {
    sceneId: sceneUrn,
    baseUrl: baseUrl,
    name: getSceneNameFromJsonData(sceneJsonData),
    main: sceneJsonData.main,
    useFPSThrottling: false,
    mappings,
    data: {
      id: sceneUrn,
      basePosition: parseParcelPosition(sceneJsonData.scene.base),
      name: getSceneNameFromJsonData(sceneJsonData),
      parcels:
        (sceneJsonData &&
          sceneJsonData.scene &&
          sceneJsonData.scene.parcels &&
          sceneJsonData.scene.parcels.map(parseParcelPosition)) ||
        [],
      baseUrl: baseUrl,
      baseUrlBundles: '',
      contents: mappings,
      icon: sceneJsonData.display?.favicon
    }
  }
}
