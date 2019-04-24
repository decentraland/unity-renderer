import { SkeletalAnimationComponent, SkeletalAnimationValue } from '../../../shared/types'
import { BaseComponent } from '../BaseComponent'
import { validators } from '../helpers/schemaValidator'
import { scene } from '../../renderer'
import { disposeAnimationGroups } from 'engine/entities/utils/processModels'

function validateClip(clipDef: SkeletalAnimationValue): SkeletalAnimationValue | null {
  if ((clipDef as any) != null && typeof (clipDef as any) === 'object') {
    clipDef.weight = Math.max(Math.min(1, validators.float(clipDef.weight, 1)), 0)
    clipDef.loop = validators.boolean(clipDef.loop, true)
    clipDef.playing = validators.boolean(clipDef.playing, true)
    clipDef.speed = Math.max(0, validators.float(clipDef.speed, 1))

    if (!clipDef.playing) {
      clipDef.weight = 0
    }

    return clipDef
  }
  return null
}

/// --- EXPORTS ---

export class Animator extends BaseComponent<SkeletalAnimationComponent> {
  name = 'animator'

  currentAnimations: Map<string, BABYLON.AnimationGroup> = new Map()

  transformValue(value: SkeletalAnimationComponent) {
    const assetContainer = this.entity.assetContainer

    const usedClipsByName = new Set<string>()

    for (let clipIndex in value.states) {
      const state = validateClip(value.states[clipIndex])
      if (state) {
        value.states[clipIndex] = state
        usedClipsByName.add(value.states[clipIndex].name)
      }
    }

    if (assetContainer) {
      for (let animationAttributes of value.states) {
        let clip: BABYLON.AnimationGroup | void = this.currentAnimations.get(animationAttributes.name)

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

        if (animationAttributes.speed !== undefined) {
          clip.speedRatio = animationAttributes.speed
        }

        if (animationAttributes.shouldReset) {
          clip.reset()
        }

        if (animationAttributes.playing && !(clip as any).isPlaying) {
          clip.play(animationAttributes.loop)
        } else if (!animationAttributes.playing && (clip as any).isPlaying) {
          clip.pause()
        }

        if (animationAttributes.weight !== undefined) {
          clip.setWeightForAllAnimatables(animationAttributes.weight)
        }
      }
    }

    this.currentAnimations.forEach((value, key) => {
      if (!usedClipsByName.has(key)) {
        disposeAnimationGroups(value)
        this.currentAnimations.delete(key)
      }
    })

    return value
  }

  detach() {
    super.detach()
    this.currentAnimations.forEach(disposeAnimationGroups)
  }
}
