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

export type DeploymentResult = { ok: true } | { ok: false; error: string }

export type BuilderManifest = {
  version: number
  project: BuilderProject
  scene: BuilderScene
}

export type BuilderProject = {
  id: string
  title: string
  description: string
  is_public: boolean
  scene_id: string
  eth_address: string
  rows: number
  cols: number
  created_at: string
  updated_at: string
  created_location: string
}

export type BuilderScene = {
  id: string
  entities: Record<string, BuilderEntity>
  components: Record<string, BuilderComponent>
  assets: Record<string, BuilderAsset>
  metrics: BuilderMetric
  limits: BuilderMetric
  ground: BuilderGround
}

export type BuilderEntity = {
  id: string
  components: string[]
}

export type BuilderComponent = {
  id: string
  type: string
  data: any
}

export type BuilderAsset = {
  id: string
  assetPackId: string
  name: string
  model: string
  script: string
  thumbnail: string
  tags: string[]
  category: string
  contents: Record<string, string>
  metrics: BuilderMetric
  parameters: any
  actions: any
}

export type BuilderMetric = {
  triangles: number
  materials: number
  meshes: number
  bodies: number
  entities: number
  textures: number
}

export type BuilderGround = {
  assetId: string
  componentId: string
}
