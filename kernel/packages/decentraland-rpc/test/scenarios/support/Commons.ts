import { registerAPI, API, exposeMethod, ScriptingHost } from '../../../lib/host'
import { future } from './Helpers'
import './MessageBusManager'

@registerAPI('Logger')
export class Logger extends API {
  @exposeMethod
  async error(message: string) {
    console.error.call(console, message)
  }

  @exposeMethod
  async log(message: string) {
    console.log.call(console, message)
  }

  @exposeMethod
  async warn(message: string) {
    console.warn.call(console, message)
  }

  @exposeMethod
  async info(message: string) {
    console.info.call(console, message)
  }
}

@registerAPI('Methods')
export class Methods extends API {
  store: { [key: string]: any } = {}

  @exposeMethod
  async setValue(key: string, value: any) {
    this.store[key] = value
  }

  @exposeMethod
  async getValue(key: string) {
    return this.store[key]
  }

  @exposeMethod
  async bounce(...args: any[]) {
    return args
  }

  @exposeMethod
  async enable() {
    return 1
  }

  @exposeMethod
  async singleBounce(arg: any) {
    return arg
  }

  @exposeMethod
  async ret0() {
    return 0
  }

  @exposeMethod
  async retNull() {
    return null
  }

  @exposeMethod
  async retFalse() {
    return false
  }

  @exposeMethod
  async retTrue() {
    return true
  }

  @exposeMethod
  async retEmptyStr() {
    return ''
  }

  @exposeMethod
  async getRandomNumber() {
    return Math.random()
  }

  @exposeMethod
  async fail() {
    throw new Error('A message')
  }

  @exposeMethod
  async receiveObject(obj: any) {
    if (typeof obj !== 'object') {
      throw new Error('Did not receive an object')
    }
    return { received: obj }
  }

  @exposeMethod
  async failsWithoutParams() {
    if (arguments.length !== 1) {
      throw new Error(`Did not receive an argument. got: ${JSON.stringify(arguments)}`)
    }
    return { args: arguments }
  }

  @exposeMethod
  async failsWithParams() {
    if (arguments.length !== 0) {
      throw new Error(`Did receive arguments. got: ${JSON.stringify(arguments)}`)
    }
    return { args: arguments }
  }
}

@registerAPI('Test')
export class Test extends API {
  future = future<{ pass: boolean; arg: any }>()

  async waitForPass() {
    const result = await this.future

    if (!result.pass) {
      throw Object.assign(new Error('WebWorker test failed. The worker did not report error data.'), result.arg || {})
    }

    return result.arg
  }

  @exposeMethod
  async fail(arg: any) {
    this.future.resolve({ pass: false, arg })
  }

  @exposeMethod
  async pass(arg: any) {
    this.future.resolve({ pass: true, arg })
  }
}

export function setUpPlugins(worker: ScriptingHost) {
  worker.getAPIInstance(Logger)
  worker.getAPIInstance(Methods)
  worker.getAPIInstance(Test)
}
