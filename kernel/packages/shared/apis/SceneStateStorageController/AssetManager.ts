import { ContentMapping } from '../../types'

export type AssetId = string

export type Asset = {
  id: AssetId
  model: string
  mappings: ContentMapping[]
  baseUrl: string
}

const BASE_DOWNLOAD_URL = 'https://content.decentraland.org/contents'
const BASE_ASSET_URL = 'https://builder-api.decentraland.org/v1/assets'

export class AssetManager {
  private readonly assets: Map<AssetId, Asset> = new Map()

  async getAssets(assetIds: AssetId[]): Promise<Map<AssetId, Asset>> {
    const unknownAssets = assetIds.filter((assetId) => !this.assets.has(assetId))
    // TODO: If there are too many assets, we might end up over the url limit, so we might need to send multiple requests
    const queryParams = 'id=' + unknownAssets.join('&id=')
    try {
      // Fetch unknown assets
      const response = await fetch(`${BASE_ASSET_URL}?${queryParams}`)
      const { data }: { data: WebAsset[] } = await response.json()
      data.map((webAsset) => this.webAssetToLocalAsset(webAsset)).forEach((asset) => this.assets.set(asset.id, asset))

      // return assets
      return new Map(assetIds.map((assetId) => [assetId, this.assets.get(assetId)!]))
    } catch (e) {
      return new Map()
    }
  }

  private webAssetToLocalAsset(webAsset: WebAsset): Asset {
    return {
      id: webAsset.id,
      model: webAsset.model,
      mappings: Object.entries(webAsset.contents).map(([file, hash]) => ({ file, hash })),
      baseUrl: BASE_DOWNLOAD_URL
    }
  }
}

type WebAsset = {
  id: string
  model: string
  contents: Record<string, string>
}
