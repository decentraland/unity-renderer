import { ILand, MappingsResponse, IScene, normalizeContentMappings } from 'shared/types'
import { jsonFetch } from 'atomicHelpers/jsonFetch'
import { error } from 'engine/logger'
import { options } from './config'

const localMap: Map<string, MappingsResponse | null> = new Map()
const cache: Map<string, Promise<ILand | null>> = new Map()

/// --- EXPORTS ---

/**
 * Returns the cached data from the marketplace.
 */
async function getParcelData(x: number, y: number) {
  const key = `${x},${y}`

  if (!localMap.has(key)) {
    await requestRange(x, y)
  }

  return localMap.get(key) || null
}

async function requestRange(x: number, y: number) {
  const requestRadius = Math.floor(options!.radius * 1.5)

  const nw = `${x - requestRadius},${y + requestRadius}`
  const se = `${x + requestRadius},${y - requestRadius}`

  const responseContent = await fetch(options!.contentServer + `/mappings?nw=${nw}&se=${se}`)

  if (!responseContent.ok) {
    error('Error in content.decentraland.org response!', responseContent)
    throw new Error('Error in content.decentraland.org response!')
  } else {
    const content = (await responseContent.json()) as MappingsResponse[]

    for (let X = x - requestRadius; X < x + requestRadius; X++) {
      for (let Y = y - requestRadius; Y < y + requestRadius; Y++) {
        localMap.set(`${X},${Y}`, null)
      }
    }

    content.forEach($ => {
      // TODO: Remove this after 5.1.0
      $.contents = normalizeContentMappings($.contents)
      localMap.set($.parcel_id, $)
    })
  }
}

async function loadParcelData(x: number, y: number): Promise<ILand | null> {
  const mappingsResponse = await getParcelData(x, y)

  if (!mappingsResponse) return null

  const sceneJsonMapping = mappingsResponse.contents.find($ => $.file === 'scene.json')

  if (!sceneJsonMapping) return null

  const baseUrl = options!.contentServer + '/contents/'
  const scene = (await jsonFetch(baseUrl + sceneJsonMapping.hash)) as IScene

  const data: ILand = {
    baseUrl,
    scene,
    mappingsResponse
  }

  return data
}

export async function getLand(x: number, y: number): Promise<ILand | null> {
  const parcelId = `${+x},${+y}`

  if (cache.has(parcelId)) {
    return (cache.get(parcelId) as any) as Promise<ILand>
  }

  const promise = loadParcelData(x, y)

  cache.set(parcelId, promise)

  return promise
}
