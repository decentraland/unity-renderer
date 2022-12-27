import { LoadableScene } from '../shared/types'
import { forceStopScene, getSceneWorkerBySceneID, loadParcelSceneWorker } from 'shared/world/parcelSceneManager'
import { parseUrn, resolveContentUrl } from '@dcl/urn-resolver'
import { Entity } from '@dcl/schemas'
import { store } from 'shared/store/isolatedStore'
import { addScenePortableExperience, removeScenePortableExperience } from 'shared/portableExperiences/actions'
import { defaultPortableExperiencePermissions } from 'shared/apis/host/Permissions'
import { SceneWorker } from 'shared/world/SceneWorker'

export type PortableExperienceHandle = {
  pid: string
  parentCid: string
}

const currentPortableExperiences: Map<string, SceneWorker> = new Map()

export async function spawnScenePortableExperienceSceneFromUrn(
  sceneUrn: string,
  parentCid: string
): Promise<PortableExperienceHandle> {
  const data = await getPortableExperienceFromUrn(sceneUrn)

  store.dispatch(addScenePortableExperience(data))

  return {
    parentCid,
    pid: data.id
  }
}

export function killScenePortableExperience(urn: string) {
  store.dispatch(removeScenePortableExperience(urn))
}

export function getRunningPortableExperience(sceneId: string): SceneWorker | undefined {
  return currentPortableExperiences.get(sceneId)
}

export async function getPortableExperienceFromUrn(sceneUrn: string): Promise<LoadableScene> {
  const resolvedEntity = await parseUrn(sceneUrn)

  if (resolvedEntity === null || resolvedEntity.type !== 'entity') {
    throw new Error(`Could not resolve mappings for scene: ${sceneUrn}`)
  }

  const resolvedUrl = await resolveContentUrl(resolvedEntity)

  if (!resolvedUrl) {
    throw new Error('Could not resolve URL to download ' + sceneUrn)
  }

  const result = await fetch(resolvedUrl)
  const entity = (await result.json()) as Entity
  const baseUrl: string = resolvedEntity.baseUrl || new URL('.', resolvedUrl).toString()

  return {
    id: sceneUrn,
    entity,
    baseUrl,
    parentCid: 'main'
  }
}

export function getPortableExperiencesLoaded() {
  return new Set(currentPortableExperiences.values())
}

/**
 * Kills all portable experiences that are not present in the given list
 */
export async function declareWantedPortableExperiences(pxs: LoadableScene[]) {
  const immutableListOfRunningPx = new Set(currentPortableExperiences.keys())

  const wantedIds = pxs.map(($) => $.id)

  // kill portable experiences that are outside our "desired" list
  for (const sceneUrn of immutableListOfRunningPx) {
    if (!wantedIds.includes(sceneUrn)) {
      const scene = getRunningPortableExperience(sceneUrn)
      if (scene) {
        currentPortableExperiences.delete(sceneUrn)
        forceStopScene(sceneUrn)
      }
    }
  }

  // then load all the missing scenes
  for (const sceneData of pxs) {
    if (!getRunningPortableExperience(sceneData.id)) {
      spawnPortableExperience(sceneData)
    }
  }
}

function spawnPortableExperience(spawnData: LoadableScene): PortableExperienceHandle {
  const sceneId = spawnData.id
  if (currentPortableExperiences.has(sceneId) || getSceneWorkerBySceneID(sceneId)) {
    throw new Error(`Portable Experience: "${sceneId}" is already running.`)
  }
  if (!sceneId) debugger

  const scene = loadParcelSceneWorker(spawnData, undefined)
  // add default permissions for portable experience based scenes
  defaultPortableExperiencePermissions.forEach(($) => scene.rpcContext.permissionGranted.add($))
  scene.rpcContext.sceneData.isPortableExperience = true
  // portable experiences have no FPS limit
  scene.rpcContext.sceneData.useFPSThrottling = false

  currentPortableExperiences.set(sceneId, scene)

  return { pid: sceneId, parentCid: spawnData.parentCid || '' }
}
