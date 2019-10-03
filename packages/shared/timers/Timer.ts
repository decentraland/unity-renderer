export class Timer {
  private timerId: any
  private start: number
  private remaining: number

  constructor(private callback: Function, delay: number) {
    this.remaining = delay
    this.start = Date.now()
  }

  resume() {
    this.start = Date.now()
    window.clearTimeout(this.timerId)
    this.timerId = window.setTimeout(() => {
      this.callback(this)
    }, this.remaining)
  }

  pause() {
    window.clearTimeout(this.timerId)
    this.remaining -= Date.now() - this.start
  }

  cancel() {
    window.clearTimeout(this.timerId)
    this.remaining = 0
    this.start = 0
    this.timerId = undefined
  }
}
