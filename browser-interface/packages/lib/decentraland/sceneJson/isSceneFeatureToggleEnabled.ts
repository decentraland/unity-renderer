import { Scene } from '@dcl/schemas'
import { SceneFeatureToggle } from './types'

export function isSceneFeatureToggleEnabled(toggle: SceneFeatureToggle, sceneJsonData?: Scene): boolean {
  const featureToggles = sceneJsonData?.featureToggles
  let feature = featureToggles?.[toggle.name]

  if (!feature || (feature !== 'enabled' && feature !== 'disabled')) {
    // If not set or value is invalid, then use default
    feature = toggle.default
  }

  return feature === 'enabled'
}
