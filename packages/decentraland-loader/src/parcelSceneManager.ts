import { error } from 'engine/logger'
import { ParcelScene } from './parcelScene'
import { getParcelsInRadius } from './utils'
import { parseParcelPosition } from 'atomicHelpers/parcelScenePositions'
import { sendParcelScenes, options } from './config'

const parcelSceneMap: Map<string, ParcelScene> = new Map()

let userX = 0
let userY = 0
let positionDidChange = false

/**
 * Given a list of visible parcels, returns the closest not-loaded parcelScene
 */
function getParcelSceneToLoad(parcelsInRadius: Map<string, number>): ParcelScene | null {
  const loadList: { dist: number; position: string }[] = []

  parcelsInRadius.forEach((dist, position) => {
    loadList.push({
      dist,
      position
    })
  })

  // we load first the closest parcels
  loadList.sort((a, b) => (a.dist < b.dist ? -1 : 1))

  for (let { position } of loadList) {
    const parcelScene = parcelSceneMap.get(position)
    if (parcelScene && parcelScene.isPending) {
      return parcelScene
    }
  }
  return null
}

/**
 * Given a list of visible parcels, ensures that all the parcelScenes exist in the parcelSceneMap
 */
async function ensureParcelScenesInMap(expectedParcelsInUserRadius: Map<string, number>) {
  for (let [position] of expectedParcelsInUserRadius) {
    if (!parcelSceneMap.has(position)) {
      const { x, y } = parseParcelPosition(position)
      let parcelScene: ParcelScene = new ParcelScene(x, y)
      parcelSceneMap.set(position, parcelScene)
    }
  }
}

/**
 * Given a parcelScene with multiple parcels, remove all the other parcelScenes from the parcelSceneMap
 * and copy the reference
 */
async function reconciliateParcelScene(parcelScene: ParcelScene) {
  if (parcelScene.isPending || !parcelScene.isValid) {
    throw new Error('Cannot reconciliate an parcelScene that is not valid or is loading')
  }

  parcelScene.parcels.forEach(({ x, y }) => {
    const position = `${x},${y}`
    parcelSceneMap.set(position, parcelScene)
  })
}

/**
 * Given a list of visible parcels, it loads and sends the loaded and valid parcelScenes to the client.
 * It does that iteratively until we receive a new user position or there are no more parcelScenes to load.
 */
async function loadAndSendCloseParcelScenes(expectedParcelsInUserRadius: Map<string, number>) {
  while (true) {
    if (positionDidChange) return

    /** Picks the closest not-loaded parcelScene and loads it */
    const parcelSceneToLoad = getParcelSceneToLoad(expectedParcelsInUserRadius)

    if (parcelSceneToLoad) {
      try {
        await parcelSceneToLoad.load()

        if (parcelSceneToLoad.isValid) {
          await reconciliateParcelScene(parcelSceneToLoad)
        }

        await sendLocalList(expectedParcelsInUserRadius)
      } catch (e) {
        error('Error loading parcelScene', e)
      }
    } else {
      break
    }
  }
}

// send the parcelScenes that are
//  1) loaded & valid
//  2) in range
async function sendLocalList(expectedParcelsInUserRadius: Map<string, number>) {
  const list = new Set<ParcelScene>()

  expectedParcelsInUserRadius.forEach((_, position) => {
    const parcelScene = parcelSceneMap.get(position)
    if (parcelScene instanceof ParcelScene && parcelScene.isValid) {
      list.add(parcelScene)
    }
  })

  const toSend = Array.from(list)

  await sendParcelScenes(toSend)
}

/// --- EXPORTS ---

/** positions in the world grid */
export function setUserPosition(x: number, y: number) {
  if (userX !== x && userY !== y) {
    userX = x
    userY = y
    positionDidChange = true
  }
}

export async function enableParcelSceneLoading() {
  async function iteration() {
    try {
      positionDidChange = false

      // get the expected parcels based on the radius of the user
      const expectedParcelsInUserRadius = getParcelsInRadius(userX, userY, options!.radius)

      await ensureParcelScenesInMap(expectedParcelsInUserRadius)
      await loadAndSendCloseParcelScenes(expectedParcelsInUserRadius)
      await sendLocalList(expectedParcelsInUserRadius)
    } catch (e) {
      error('error refreshing parcelScenes', e)
    }

    setTimeout(() => iteration().catch(error), 500)
  }

  iteration().catch(error)
}
