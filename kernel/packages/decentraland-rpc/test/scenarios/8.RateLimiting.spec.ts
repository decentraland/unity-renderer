import { registerAPI, API, exposeMethod, rateLimit } from '../../lib/host'
import { testInWorker } from './support/Helpers'

@registerAPI('RateLimiter')
export class RateLimiter extends API {
  private calls: number = 0

  @rateLimit(100)
  @exposeMethod
  async everyTenthOfSecond() {
    console.log('ten per second', this.calls)
    this.calls++
  }
}

describe('RateLimiter', function() {
  testInWorker('test/out/fixtures/8.RateLimiting.js', {
    plugins: [RateLimiter],
    log: false
  })
})
