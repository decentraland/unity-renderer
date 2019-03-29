const Hammer = require('hammerjs')
const { Ticker, Shape, Stage } = require('@createjs/easeljs')

import { createTween } from 'engine/components/helpers/tween'
import { ReadOnlyVector2 } from 'decentraland-ecs/src'

type JoystickOptions = {
  debug?: boolean
  joystickSize?: number
}

export type HammerEvent = {
  angle: number
  distance: number
  deltaX: number
  deltaY: number
  center: ReadOnlyVector2
  preventDefault: () => void
}

export default class Joystick {
  private options: JoystickOptions = {
    debug: false,
    joystickSize: 150
  }

  private dom: HTMLCanvasElement

  constructor(options?: JoystickOptions) {
    if (options) {
      this.options = { ...this.options, ...options }
    }

    const { joystickSize } = this.options

    const canvas = document.createElement('canvas')
    canvas.setAttribute('id', 'joystick-canvas')
    canvas.setAttribute('height', joystickSize!.toString())
    canvas.setAttribute('width', joystickSize!.toString())
    canvas.style.cssText = `border-radius:${joystickSize}px;-moz-border-radius:${joystickSize}px;-webkit-border-radius:${joystickSize}px;text-align:center;background-color:rgba(51, 51, 51, 0.5);font:24px/${joystickSize}pxHelvetica,Arial,sans-serif;cursor:all-scroll;user-select:none;border: 6px #e1e1e1 solid`

    this.dom = canvas
  }

  getCanvas(): HTMLCanvasElement {
    return this.dom
  }

  onMove(fn: (x: HammerEvent) => void): void {
    this.onMoveFn = fn
  }

  setup() {
    const { joystickSize } = this.options
    let xCenter = joystickSize! / 2
    let yCenter = joystickSize! / 2

    const stage = new Stage('joystick-canvas')
    const psp = new Shape()

    psp.graphics.beginFill('#e1e1e1').drawCircle(xCenter, yCenter, joystickSize! / 3)
    psp.graphics.beginFill('#241f20').drawCircle(xCenter, yCenter, joystickSize! / 4)

    stage.addChild(psp)

    if (this.options.debug) {
      let vertical = new Shape()
      let horizontal = new Shape()
      vertical.graphics.beginFill('#ff4d4d').drawRect(joystickSize! / 2, 0, 2, joystickSize)
      horizontal.graphics.beginFill('#ff4d4d').drawRect(0, joystickSize! / 2, joystickSize, 2)
      stage.addChild(vertical)
      stage.addChild(horizontal)
    }

    Ticker.framerate = 60
    Ticker.addEventListener('tick', stage)

    stage.update()

    const hammer = new Hammer(this.dom)
    let isMoving = false
    let lastEvent: HammerEvent | null = null

    Ticker.addEventListener('tick', () => {
      if (isMoving && 'onMoveFn' in this && lastEvent) {
        this.onMoveFn(lastEvent)
      }
    })

    hammer.on('panstart', () => {
      xCenter = psp.x
      yCenter = psp.y
      psp.alpha = 0.5
      stage.update()
      isMoving = true
    })

    hammer.on('panmove', (e: HammerEvent) => {
      const { x, y } = this.calculateCoords(e.angle, e.distance)
      psp.x = x
      psp.y = y
      psp.alpha = 0.5
      stage.update()
      lastEvent = e
    })

    hammer.on('panend', () => {
      isMoving = false
      psp.alpha = 1
      const tween = createTween(psp, 'elastic-out')
      tween.to({ x: xCenter, y: yCenter }, 750)
      tween.start()
    })
  }

  private onMoveFn: (e: HammerEvent) => void = () => void 0

  private calculateCoords(angle: number, distance: number) {
    const nDistance = Math.min(distance, this.options.joystickSize! / 3)
    let rads = (angle * Math.PI) / 180.0
    const x = nDistance * Math.cos(rads)
    const y = nDistance * Math.sin(rads)
    return { x, y }
  }
}
