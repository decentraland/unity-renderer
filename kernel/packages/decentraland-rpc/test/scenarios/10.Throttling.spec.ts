import { registerAPI, API, exposeMethod, throttle } from '../../lib/host'
import { testInWorker } from './support/Helpers'

@registerAPI('Throttling')
export class Throttling extends API {
  private calls: number = 0

  @throttle(5, 100)
  @exposeMethod
  async fiveEveryHundredMilliseconds() {
    console.log('five every 100ms', this.calls)
    this.calls++
  }
}

describe('Throttling', function() {
  testInWorker('test/out/fixtures/10.Throttling.js', {
    plugins: [Throttling],
    log: false
  })
})
