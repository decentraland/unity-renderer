const PR = Math.round(window.devicePixelRatio || 1)

const WIDTH = 80 * PR
const HEIGHT = 48 * PR
const TEXT_X = 3 * PR
const TEXT_Y = 2 * PR
const GRAPH_X = 3 * PR
const GRAPH_Y = 15 * PR
const GRAPH_WIDTH = 74 * PR
const GRAPH_HEIGHT = 30 * PR

interface Colors {
  textColor: string
  backgroundColor: string
}

export class Panel {
  private min = Infinity
  private max = 0
  private canvas: HTMLCanvasElement
  private context: CanvasRenderingContext2D
  private colors: Colors
  private name: string

  constructor(name: string, textColor: string, backgroundColor: string) {
    this.colors = { textColor, backgroundColor }
    this.name = name
    this.initCanvas()
  }

  initCanvas(): void {
    const { textColor, backgroundColor } = this.colors
    const canvas = document.createElement('canvas')
    canvas.width = WIDTH
    canvas.height = HEIGHT
    canvas.style.cssText = 'width:80px;height:48px'

    const context = canvas.getContext('2d')
    context.font = 'bold ' + 9 * PR + 'px Helvetica,Arial,sans-serif'
    context.textBaseline = 'top'

    context.fillStyle = backgroundColor
    context.fillRect(0, 0, WIDTH, HEIGHT)

    context.fillStyle = textColor
    context.fillText(this.name, TEXT_X, TEXT_Y)
    context.fillRect(GRAPH_X, GRAPH_Y, GRAPH_WIDTH, GRAPH_HEIGHT)

    context.fillStyle = backgroundColor
    context.globalAlpha = 1
    context.fillRect(GRAPH_X, GRAPH_Y, GRAPH_WIDTH, GRAPH_HEIGHT)
    this.canvas = canvas
    this.context = context
  }

  getDOM(): HTMLCanvasElement {
    return this.canvas
  }

  update(value: number, maxValue: number): void {
    const { textColor, backgroundColor } = this.colors
    const { context } = this

    this.min = Math.min(this.min, value)
    this.max = Math.max(this.max, value)

    context.fillStyle = backgroundColor
    context.globalAlpha = 1
    context.fillRect(0, 0, WIDTH, GRAPH_Y)
    context.fillStyle = textColor
    context.fillText(
      Math.round(value) + ' ' + this.name + ' (' + Math.round(this.min) + '-' + Math.round(this.max) + ')',
      TEXT_X,
      TEXT_Y
    )

    context.drawImage(
      this.canvas,
      GRAPH_X + PR,
      GRAPH_Y,
      GRAPH_WIDTH - PR,
      GRAPH_HEIGHT,
      GRAPH_X,
      GRAPH_Y,
      GRAPH_WIDTH - PR,
      GRAPH_HEIGHT
    )

    context.fillRect(GRAPH_X + GRAPH_WIDTH - PR, GRAPH_Y, PR, GRAPH_HEIGHT)

    context.fillStyle = backgroundColor
    context.globalAlpha = 1
    context.fillRect(GRAPH_X + GRAPH_WIDTH - PR, GRAPH_Y, PR, Math.round((1 - value / maxValue) * GRAPH_HEIGHT))
  }
}
