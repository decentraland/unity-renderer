export type SerializedSceneState = {
  entities: SerializedEntity[]
}

export type SerializedEntity = {
  id: string
  components: SerializedComponent[]
}

export type SerializedComponent = {
  type: number
  value: any
}

export enum CONTENT_PATH {
  DEFINITION_FILE = 'scene-state-definition.json',
  SCENE_FILE = 'scene.json',
  MODELS_FOLDER = 'models',
  BUNDLED_GAME_FILE = 'bin/game.js'
}
