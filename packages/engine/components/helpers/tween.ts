import * as TWEEN from '@tweenjs/tween.js'
import { TimingFunction } from 'shared/types'

export const easingFunctions = {
  linear: TWEEN.Easing.Linear.None,
  'ease-in': TWEEN.Easing.Cubic.In,
  'ease-out': TWEEN.Easing.Cubic.Out,
  'ease-in-out': TWEEN.Easing.Cubic.InOut,
  'quadratic-in': TWEEN.Easing.Quadratic.In,
  'quadratic-out': TWEEN.Easing.Quadratic.Out,
  'quadratic-inout': TWEEN.Easing.Quadratic.InOut,
  'cubic-in': TWEEN.Easing.Cubic.In,
  'cubic-out': TWEEN.Easing.Cubic.Out,
  'cubic-inout': TWEEN.Easing.Cubic.InOut,
  'quartic-in': TWEEN.Easing.Quartic.In,
  'quartic-out': TWEEN.Easing.Quartic.Out,
  'quartic-inout': TWEEN.Easing.Quartic.InOut,
  'quintic-in': TWEEN.Easing.Quintic.In,
  'quintic-out': TWEEN.Easing.Quintic.Out,
  'quintic-inout': TWEEN.Easing.Quintic.InOut,
  'sin-in': TWEEN.Easing.Sinusoidal.In,
  'sin-out': TWEEN.Easing.Sinusoidal.Out,
  'sin-inout': TWEEN.Easing.Sinusoidal.InOut,
  'exponential-in': TWEEN.Easing.Exponential.In,
  'exponential-out': TWEEN.Easing.Exponential.Out,
  'exponential-inout': TWEEN.Easing.Exponential.InOut,
  'bounce-in': TWEEN.Easing.Bounce.In,
  'bounce-out': TWEEN.Easing.Bounce.Out,
  'bounce-inout': TWEEN.Easing.Bounce.InOut,
  'elastic-in': TWEEN.Easing.Elastic.In,
  'elastic-out': TWEEN.Easing.Elastic.Out,
  'elastic-inout': TWEEN.Easing.Elastic.InOut,
  'circular-in': TWEEN.Easing.Circular.In,
  'circular-out': TWEEN.Easing.Circular.Out,
  'circular-inout': TWEEN.Easing.Circular.InOut,
  'back-in': TWEEN.Easing.Back.In,
  'back-out': TWEEN.Easing.Back.Out,
  'back-inout': TWEEN.Easing.Back.InOut
}

export function createTween(target: any, easing: TimingFunction) {
  return new TWEEN.Tween(target).easing(easingFunctions[easing] || TWEEN.Easing.Linear.None)
}

export function removeTween(tween: TWEEN.Tween) {
  tween.stop()
  TWEEN.remove(tween)
}

export { TWEEN }
