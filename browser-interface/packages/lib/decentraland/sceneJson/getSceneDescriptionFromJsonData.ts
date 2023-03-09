import { Scene } from '@dcl/schemas'

export function getSceneDescriptionFromJsonData(jsonData?: Scene) {
  return jsonData?.display?.description || ''
}
