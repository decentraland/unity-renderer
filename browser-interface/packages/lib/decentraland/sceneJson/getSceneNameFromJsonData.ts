import { Scene } from '@dcl/schemas'

export function getSceneNameFromJsonData(jsonData?: Scene): string {
  let title = jsonData?.display?.title
  if (title === 'interactive-text') {
    // avoid using autogenerated name
    title = undefined
  }

  return title || jsonData?.scene?.base || 'Unnamed'
}
