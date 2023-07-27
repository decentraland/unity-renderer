import { LoadableScene } from 'shared/types'
import { forceStopScene, getSceneWorkerBySceneID, loadParcelSceneWorker } from 'shared/world/parcelSceneManager'
import { parseUrn, resolveContentUrl } from '@dcl/urn-resolver'
import { Entity } from '@dcl/schemas'
import { store } from 'shared/store/isolatedStore'
import { addScenePortableExperience, removeScenePortableExperience } from 'shared/portableExperiences/actions'
import { defaultPortableExperiencePermissions } from 'shared/apis/host/Permissions'
import { SceneWorker } from 'shared/world/SceneWorker'
import { dclWorldUrl } from '../shared/realm/resolver'
import { IRealmAdapter } from '../shared/realm/types'
import { removeQueryParamsFromUrn } from '../shared/portableExperiences/selectors'
import defaultLogger from '../lib/logger'

export type PortableExperienceHandle = {
  pid: string
  parentCid: string
  name: string
}

type PxPid = string
type Ens = string
const currentPortableExperiences: Map<PxPid, SceneWorker> = new Map()
export const ensPxMapping: Map<PxPid, Ens> = new Map()

export async function spawnPortableExperienceFromEns(ens: Ens, parentCid: string) {
  try {
    const worldUrl = dclWorldUrl(ens)
    const worldAbout: IRealmAdapter['about'] = await (await fetch(worldUrl + '/about')).json()
    const sceneUrn = worldAbout.configurations?.scenesUrn && worldAbout.configurations?.scenesUrn[0]

    if (worldAbout.healthy && sceneUrn) {
      const data = await spawnScenePortableExperienceSceneFromUrn(sceneUrn, parentCid)
      ensPxMapping.set(data.pid, ens)
      return data
    } else {
      throw new Error('Scene not available')
    }
  } catch (e) {
    defaultLogger.error('Error fetching scene', e)
    throw new Error('Error fetching scene')
  }
}

export async function spawnScenePortableExperienceSceneFromUrn(
  sceneUrn: string,
  parentCid: string
): Promise<PortableExperienceHandle> {
  const px = await getPortableExperienceFromUrn(sceneUrn)

  const data = {
    ...px,
    id: removeQueryParamsFromUrn(px.id),
    parentCid
  }

  store.dispatch(addScenePortableExperience(data))

  return {
    parentCid,
    pid: data.id,
    name: px.entity.metadata.display?.title ?? ''
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
    id: removeQueryParamsFromUrn(sceneUrn),
    entity,
    baseUrl,
    parentCid: 'main'
  }
}

export function getPortableExperiencesLoaded() {
  return new Set(currentPortableExperiences.values())
}

/**
 * Kills all portable experiences that are not present in the given list.
 * This function requires the rpcClient to be already connected.
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
      await spawnPortableExperience(sceneData)
    }
  }
}

async function spawnPortableExperience(spawnData: LoadableScene): Promise<PortableExperienceHandle> {
  const sceneId = spawnData.id
  if (currentPortableExperiences.has(sceneId) || getSceneWorkerBySceneID(sceneId)) {
    throw new Error(`Portable Experience: "${sceneId}" is already running.`)
  }
  if (!sceneId) debugger

  const scene = await loadParcelSceneWorker({
    ...spawnData,
    isPortableExperience: true,
    isGlobalScene: true,
    // portable experiences have no FPS limit
    useFPSThrottling: false
  })
  // add default permissions for portable experience based scenes
  defaultPortableExperiencePermissions.forEach(($) => scene.rpcContext.permissionGranted.add($))

  currentPortableExperiences.set(sceneId, scene)
  return { pid: sceneId, parentCid: spawnData.parentCid || '', name: scene.metadata.display?.title ?? '' }
}
