import { ILand, SceneJsonData, MappingsResponse } from 'shared/types'
import { Vector2Component } from 'atomicHelpers/landHelpers'

const tutorialSceneContents = require('./tutorialSceneContents.json')

export const TUTORIAL_SCENE_COORDS = { x: 200, y: 200 }
export const TUTORIAL_SCENE_ID = 'TutorialScene'

export let _tutorialEnabled = false

export enum tutorialStepId {
  NONE = 0,
  INITIAL_SCENE = 1,
  GENESIS_PLAZA = 2,
  CHAT_AND_AVATAR_EXPRESSIONS = 3,
  FINISHED = 99
}

export function setTutorialEnabled(v: boolean) {
  _tutorialEnabled = v
}

let teleportCount: number = 0

export function isTutorial(): boolean {
  return teleportCount <= 1 && _tutorialEnabled
}

export function onTutorialTeleport() {
  teleportCount++
}

export function resolveTutorialPosition(position: Vector2Component, teleported: boolean): Vector2Component {
  if (teleported) {
    onTutorialTeleport()
  }
  return isTutorial() ? TUTORIAL_SCENE_COORDS : position
}

export function createTutorialILand(baseLocation: string): ILand {
  const coordinates = `${TUTORIAL_SCENE_COORDS.x},${TUTORIAL_SCENE_COORDS.y}`
  return {
    sceneId: TUTORIAL_SCENE_ID,
    baseUrl: baseLocation + '/loader/tutorial-scene/',
    baseUrlBundles: '',
    sceneJsonData: {
      display: { title: 'Tutorial Scene' },
      owner: '',
      main: 'bin/game.js',
      scene: { parcels: getSceneParcelsCoords(), base: coordinates },
      communications: { commServerUrl: '' },
      spawnPoints: [
        {
          name: 'spawnPoint',
          position: {
            x: 37,
            y: 2.5,
            z: 60.5
          }
        }
      ]
    } as SceneJsonData,
    mappingsResponse: {
      parcel_id: coordinates,
      contents: tutorialSceneContents,
      root_cid: TUTORIAL_SCENE_ID,
      publisher: '0x13371b17ddb77893cd19e10ffa58461396ebcc19'
    } as MappingsResponse
  }
}

function getSceneParcelsCoords() {
  let ret = []
  for (let i = 0; i < 6; i++) {
    for (let j = 0; j < 6; j++) {
      ret.push(`${TUTORIAL_SCENE_COORDS.x + i},${TUTORIAL_SCENE_COORDS.y + j}`)
    }
  }
  return ret
}
