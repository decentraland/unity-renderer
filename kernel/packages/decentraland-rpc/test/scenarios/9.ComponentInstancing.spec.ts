import { registerAPI, API, APIOptions, exposeMethod } from '../../lib/host'
import { testInWorker } from './support/Helpers'

@registerAPI('Greeter')
export class Greeter extends API {
  greet(name: string) {
    return `Hello ${name}`
  }
}

@registerAPI('Instancer')
export class Instancer extends API {
  private Greeter: Greeter

  constructor(options: APIOptions) {
    super(options)
    this.Greeter = this.options.getAPIInstance(Greeter)
  }

  @exposeMethod
  async doSomething() {
    return this.Greeter.greet('World')
  }
}

describe('Intance a Component from another Component', function() {
  testInWorker('test/out/fixtures/9.ComponentInstancing.js', {
    plugins: [Instancer],
    log: false
  })
})
