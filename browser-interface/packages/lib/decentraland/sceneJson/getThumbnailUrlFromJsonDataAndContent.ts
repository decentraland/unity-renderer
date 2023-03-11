import { ContentMapping, Scene } from '@dcl/schemas'

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

function getThumbnailUrlFromJsonData(jsonData?: Scene): string | undefined {
  if (!jsonData) {
    return undefined
  }

  return jsonData.display?.navmapThumbnail ?? getThumbnailUrlFromBuilderProjectId(jsonData.source?.projectId)
}

function getThumbnailUrlFromBuilderProjectId(projectId: string | undefined): string | undefined {
  if (!projectId) {
    return undefined
  }

  return `https://builder-api.decentraland.org/v1/projects/${projectId}/media/preview.png`
}
