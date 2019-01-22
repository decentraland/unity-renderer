import { SkeletalAnimationComponent, SkeletalAnimationValue } from '../../../shared/types'
import { BaseComponent } from '../BaseComponent'
import { validators } from '../helpers/schemaValidator'
import { scene } from '../../renderer'

function validateClip(clipDef: SkeletalAnimationValue): SkeletalAnimationValue {
  if ((clipDef as any) != null && typeof (clipDef as any) === 'object') {
    clipDef.weight = validators.float(clipDef.weight, 1)
    clipDef.loop = validators.boolean(clipDef.loop, true)
    clipDef.playing = validators.boolean(clipDef.playing, true)
    clipDef.speed = validators.float(clipDef.speed, 1)

    if (!clipDef.playing) {
      clipDef.weight = 0
    }

    return clipDef
  }
}

/// --- EXPORTS ---

export class Animator extends BaseComponent<SkeletalAnimationComponent> {
  name = 'animator'

  currentAnimations: Map<string, BABYLON.AnimationGroup>

  transformValue(value: SkeletalAnimationComponent) {
    this.currentAnimations = this.currentAnimations || new Map()
    const assetContainer = this.entity.assetContainer

    const usedClipsByName = new Set<string>()

    for (let clipIndex in value.states) {
      value.states[clipIndex] = validateClip(value.states[clipIndex])
      usedClipsByName.add(value.states[clipIndex].name)
    }

    if (assetContainer) {
      for (let animationAttributes of value.states) {
        let clip: BABYLON.AnimationGroup = this.currentAnimations.get(animationAttributes.name)

        if (!clip) {
          const base = assetContainer.animationGroups.find($ => $.name === animationAttributes.clip)
          clip = new BABYLON.AnimationGroup(animationAttributes.name, scene)
          if (base) {
            const targeted = base.targetedAnimations
            for (let i in targeted) {
              clip.addTargetedAnimation(targeted[i].animation.clone(), targeted[i].target)
            }
          }
          this.currentAnimations.set(animationAttributes.name, clip)
        }

        clip.speedRatio = animationAttributes.speed

        if (animationAttributes.playing && !(clip as any).isPlaying) {
          clip.play(animationAttributes.loop)
        } else if (!animationAttributes.playing && (clip as any).isPlaying) {
          clip.pause()
        }

        clip.setWeightForAllAnimatables(animationAttributes.weight)
      }
    }

    this.currentAnimations.forEach((value, key) => {
      if (!usedClipsByName.has(key)) {
        value.stop()
        value.dispose()
        this.currentAnimations.delete(key)
      }
    })

    return value
  }

  detach() {
    super.detach()
    this.currentAnimations.forEach($ => $.dispose())
  }
}
