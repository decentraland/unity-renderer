const Hammer = require('hammerjs')
const { Ticker } = require('@createjs/easeljs')

import { HammerEvent } from './joystick'

export default class PlaneCanvasControl {
  private dom: HTMLCanvasElement
  private onMoveFn!: (e: HammerEvent) => void
  private onTouchFn!: (e: HammerEvent) => void

  constructor() {
    const canvas = document.createElement('canvas')
    canvas.setAttribute('id', 'rotation-control-canvas')
    canvas.setAttribute('height', '100%')
    canvas.setAttribute('width', '100%')
    this.dom = canvas
  }

  getCanvas(): HTMLCanvasElement {
    return this.dom
  }

  onMove(fn: (e: HammerEvent) => void): void {
    this.onMoveFn = fn
  }

  onTouch(fn: (e: HammerEvent) => void): void {
    this.onTouchFn = fn
  }

  setup() {
    // on debug we should draw lines or maybe a background opaque color to detect the canvas and movements
    const hammer = new Hammer(this.dom)
    let isMoving = false
    let lastEvent: HammerEvent | null = null

    Ticker.addEventListener('tick', () => {
      if (isMoving && 'onMoveFn' in this && lastEvent) {
        this.onMoveFn(lastEvent)
      }
    })

    hammer.on('panstart', () => {
      isMoving = true
    })

    hammer.on('panmove', (e: any) => {
      lastEvent = e
    })

    hammer.on('panend', () => {
      isMoving = false
    })

    hammer.on('tap', (e: any) => {
      if ('onTouchFn' in this) {
        this.onTouchFn(e)
      }
    })
  }
}
