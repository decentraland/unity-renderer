import { Authenticator } from 'dcl-crypto'
import { ExplorerIdentity } from 'shared/session/types'
import { uuid } from 'decentraland-ecs/src/ecs/helpers'
import { ContentMapping } from '../../types'
import { BuilderAsset, BuilderManifest, BuilderProject, BuilderScene } from './types'
import { getDefaultTLD } from 'config'

export type AssetId = string

export type Asset = {
  id: AssetId
  model: string
  mappings: ContentMapping[]
  baseUrl: string
}

const BASE_DOWNLOAD_URL = 'https://builder-api.decentraland.org/v1/storage/contents'
const BASE_BUILDER_SERVER_URL_ROPSTEN = 'https://builder-api.decentraland.io/v1/'
const BASE_BUILDER_SERVER_URL = 'https://builder-api.decentraland.org/v1/'

export class BuilderServerAPIManager {
  private readonly assets: Map<AssetId, BuilderAsset> = new Map()

  async getAssets(assetIds: AssetId[]): Promise<Record<string, BuilderAsset>> {
    const unknownAssets = assetIds.filter((assetId) => !this.assets.has(assetId))
    // TODO: If there are too many assets, we might end up over the url limit, so we might need to send multiple requests
    if (unknownAssets.length > 0) {
      const queryParams = 'assets?id=' + unknownAssets.join('&id=')
      try {
        // Fetch unknown assets
        const response = await fetch(`${this.getBaseUrl()}${queryParams}`)
        const { data }: { data: BuilderAsset[] } = await response.json()
        data.map((builderAsset) => builderAsset).forEach((asset) => this.assets.set(asset.id, asset))
      } catch (e) {
        console.trace(e)
      }
    }
    const assets: Record<string, BuilderAsset> = {}

    assetIds.map((assetId) => {
      assets[assetId] = this.assets.get(assetId)!
    })
    return assets
  }

  async getConvertedAssets(assetIds: AssetId[]): Promise<Map<AssetId, Asset>> {
    await this.getAssets(assetIds)
    return new Map(assetIds.map((assetId) => [assetId, this.builderAssetToLocalAsset(this.assets.get(assetId)!)]))
  }

  private builderAssetToLocalAsset(webAsset: BuilderAsset): Asset {
    return {
      id: webAsset.id,
      model: webAsset.model,
      mappings: Object.entries(webAsset.contents).map(([file, hash]) => ({ file, hash })),
      baseUrl: BASE_DOWNLOAD_URL
    }
  }

  async getBuilderManifestFromProjectId(
    projectId: string,
    identity: ExplorerIdentity
  ): Promise<BuilderManifest | undefined> {
    try {
      // Fetch builder manifest by ID
      const queryParams = 'projects/' + projectId + '/manifest'
      const urlToFecth = `${this.getBaseUrl()}${queryParams}`

      let params: RequestInit = {
        headers: this.authorize(identity, 'get', '/' + queryParams)
      }

      const response = await fetch(urlToFecth, params)
      const data = await response.json()

      const manifest: BuilderManifest = data.data

      //If this manifest contains assets, we add them so we don't need to fetch them
      this.addAssetsFromManifest(manifest)
      return manifest
    } catch (e) {
      console.trace(e)
      return undefined
    }
  }

  async getBuilderManifestFromLandCoordinates(
    land: string,
    identity: ExplorerIdentity
  ): Promise<BuilderManifest | undefined> {
    try {
      // Fetch builder manifest by lands coordinates
      const queryParams = 'manifests?' + 'creation_coords_eq=' + land
      const urlToFecth = `${this.getBaseUrl()}${queryParams}`

      let params: RequestInit = {
        headers: this.authorize(identity, 'get', '/' + queryParams)
      }

      const response = await fetch(urlToFecth, params)
      const data = await response.json()

      if (data.data === false) {
        return undefined
      }
      const manifest: BuilderManifest = data.data

      //If this manifest contains assets, we add them so we don't need to fetch them
      this.addAssetsFromManifest(manifest)

      return manifest
    } catch (e) {
      console.trace(e)
      return undefined
    }
  }

  async updateProjectManifest(builderManifest: BuilderManifest, identity: ExplorerIdentity) {
    try {
      this.setManifestOnServer(builderManifest, identity)
    } catch (e) {
      console.trace(e)
      return undefined
    }
  }

  async createProjectWithCoords(coordinates: string, identity: ExplorerIdentity): Promise<BuilderManifest> {
    const builderManifest = this.createEmptyDefaultBuilderScene(coordinates, identity.rawAddress)
    try {
      this.setManifestOnServer(builderManifest, identity)
    } catch (e) {
      console.trace(e)
    }
    return builderManifest
  }

  private async setManifestOnServer(builderManifest: BuilderManifest, identity: ExplorerIdentity) {
    const queryParams = 'projects/' + builderManifest.project.id + '/manifest'
    const urlToFecth = `${this.getBaseUrl()}${queryParams}`

    const body = JSON.stringify({ manifest: builderManifest })
    const headers = this.authorize(identity, 'put', '/' + queryParams)
    headers['Content-Type'] = 'application/json'

    let params: RequestInit = {
      headers: headers,
      method: 'PUT',
      body: body
    }

    const response = await fetch(urlToFecth, params)
    const data = await response.json()
    return data
  }

  private getBaseUrl(): string {
    if (getDefaultTLD() === 'org') return BASE_BUILDER_SERVER_URL
    else return BASE_BUILDER_SERVER_URL_ROPSTEN
  }

  private addAssetsFromManifest(manifest: BuilderManifest) {
    Object.entries(manifest.scene.assets).forEach((asset) => {
      if (!this.assets.has(asset[0])) {
        this.assets.set(asset[0], asset[1])
      }
    })
  }

  private createEmptyDefaultBuilderScene(land: string, eth_address: string): BuilderManifest {
    let today = new Date().toISOString().slice(0, 10)
    let projectId = uuid()
    let sceneId = uuid()
    let builderProject: BuilderProject = {
      id: projectId,
      title: 'Builder ' + land,
      description: 'Scene created from the explorer builder',
      is_public: false,
      scene_id: sceneId,
      eth_address: eth_address,
      rows: 1,
      cols: 1,
      created_at: today,
      updated_at: today,
      created_location: land
    }

    let builderScene: BuilderScene = {
      id: sceneId,
      entities: {},
      components: {},
      assets: {},
      metrics: {
        textures: 0,
        triangles: 0,
        materials: 0,
        meshes: 0,
        bodies: 0,
        entities: 0
      },
      limits: {
        textures: 10,
        triangles: 10000,
        materials: 20,
        meshes: 200,
        bodies: 300,
        entities: 200
      },
      ground: {
        assetId: 'da1fed3c954172146414a66adfa134f7a5e1cb49c902713481bf2fe94180c2cf',
        componentId: 'b5edf28e-b4e4-4a27-b0ac-84b3d77eff8e'
      }
    }
    let builderManifest: BuilderManifest = {
      version: 10,
      project: builderProject,
      scene: builderScene
    }
    return builderManifest
  }

  private readonly AUTH_CHAIN_HEADER_PREFIX = 'x-identity-auth-chain-'

  createHeaders(idToken: string) {
    if (!idToken) return {}
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${idToken}`
    }
    return headers
  }

  authorize(identity: ExplorerIdentity, method: string = 'get', path: string = '') {
    const headers: Record<string, string> = {}

    if (identity) {
      const endpoint = (method + ':' + path).toLowerCase()
      const authChain = Authenticator.signPayload(identity, endpoint)
      for (let i = 0; i < authChain.length; i++) {
        headers[this.AUTH_CHAIN_HEADER_PREFIX + i] = JSON.stringify(authChain[i])
      }
    }
    return headers
  }
}
