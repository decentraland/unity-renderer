import * as BABYLON from 'babylonjs'
import { BaseEntity } from '../BaseEntity'

/// --- EXPORTS ---

export interface IAnimatedEntity extends BaseEntity {
  skeletons: BABYLON.Skeleton[]
  animationGroups: BABYLON.AnimationGroup[]
}
