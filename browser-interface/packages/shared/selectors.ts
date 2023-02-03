import { Scene } from '@dcl/schemas/dist/platform/scene/scene'
import { ContentMapping } from '@dcl/schemas/dist/misc/content-mapping'
import { SceneFeatureToggle } from './types'

export function getOwnerNameFromJsonData(jsonData?: Scene) {
  let ownerName = jsonData?.contact?.name
  if (ownerName === 'author-name') {
    // avoid using autogenerated name
    ownerName = undefined
  }

  return ownerName || 'Unknown'
}

export function isFeatureToggleEnabled(toggle: SceneFeatureToggle, sceneJsonData?: Scene): boolean {
  const featureToggles = sceneJsonData?.featureToggles
  let feature = featureToggles?.[toggle.name]

  if (!feature || (feature !== 'enabled' && feature !== 'disabled')) {
    // If not set or value is invalid, then use default
    feature = toggle.default
  }

  return feature === 'enabled'
}

export function getSceneDescriptionFromJsonData(jsonData?: Scene) {
  return jsonData?.display?.description || ''
}

export function getSceneNameFromJsonData(jsonData?: Scene): string {
  let title = jsonData?.display?.title
  if (title === 'interactive-text') {
    // avoid using autogenerated name
    title = undefined
  }

  return title || jsonData?.scene?.base || 'Unnamed'
}

export function getThumbnailUrlFromJsonDataAndContent(
  jsonData: Scene | undefined,
  contents: Array<ContentMapping> | undefined,
  downloadUrl: string
): string | undefined {
  if (!jsonData) {
    return undefined
  }

  if (!contents || !downloadUrl) {
    return getThumbnailUrlFromJsonData(jsonData)
  }

  let thumbnail: string | undefined = jsonData.display?.navmapThumbnail
  if (thumbnail && !thumbnail.startsWith('http')) {
    // We are assuming that the thumbnail is an uploaded file. We will try to find the matching hash
    const thumbnailHash = contents?.find(({ file }) => file === thumbnail)?.hash
    if (thumbnailHash) {
      thumbnail = `${downloadUrl}/${thumbnailHash}`
    } else {
      // If we couldn't find a file with the correct path, then we ignore whatever was set on the thumbnail property
      thumbnail = undefined
    }
  }

  if (!thumbnail) {
    thumbnail = getThumbnailUrlFromBuilderProjectId(jsonData.source?.projectId)
  }
  return thumbnail
}

export function getThumbnailUrlFromJsonData(jsonData?: Scene): string | undefined {
  if (!jsonData) {
    return undefined
  }

  return jsonData.display?.navmapThumbnail ?? getThumbnailUrlFromBuilderProjectId(jsonData.source?.projectId)
}

export function getThumbnailUrlFromBuilderProjectId(projectId: string | undefined): string | undefined {
  if (!projectId) {
    return undefined
  }

  return `https://builder-api.decentraland.org/v1/projects/${projectId}/media/preview.png`
}
