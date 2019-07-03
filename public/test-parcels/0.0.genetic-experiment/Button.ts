import { Component, ISystem, engine } from 'decentraland-ecs/src'

@Component('buttonData')
export class ButtonData {
  pressed: boolean
  xUp: number = 0
  xDown: number = 0
  fraction: number
  timeDownLeft: number
  totalTimeDown: number
  constructor(xUp: number, xDown: number, timeDown: number = 2) {
    this.xUp = xUp
    this.xDown = xDown
    this.pressed = false
    this.fraction = 0
    this.timeDownLeft = timeDown
    this.totalTimeDown = timeDown
  }
}

export const buttons = engine.getComponentGroup(ButtonData)

export class PushButton implements ISystem {
  /* update(dt: number) {

    for (let button of buttons.entities) {
      let transform = button.getComponent(Transform)
      let state = button.getComponent(ButtonData)
      if (state.pressed == true) {
        if (state.fraction < 1) {
          transform.position.x = Scalar.Lerp(
            state.xUp,
            state.xDown,
            state.fraction
          )
          state.fraction += 1 / 8
        }
        state.timeDownLeft -= dt
        if (state.timeDownLeft < 0) {
          state.pressed = false
          state.timeDownLeft = state.totalTimeDown
        }
      } else if (state.pressed == false && state.fraction > 0) {
        transform.position.x = Scalar.Lerp(
          state.xUp,
          state.xDown,
          state.fraction
        )
        state.fraction -= 1 / 8
      }
    }
  } */
}

