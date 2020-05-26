import { ScriptingHost } from '../../../lib/host'
import { Test } from './Commons'
import { WebWorkerTransport } from '../../../lib/client'
import { ScriptingTransport } from '../../../lib/common/json-rpc/types'

export type IFuture<T> = Promise<T> & {
  resolve: (x: T) => void
  reject?: (x: Error) => void
}

export type ITestInWorkerOptions = {
  log?: boolean
  validateResult?: (result: any, worker: ScriptingHost) => void
  execute?: (worker: ScriptingHost) => void
  plugins?: any[]
}

export function wait(ms: number): Promise<void> {
  return new Promise(ok => {
    setTimeout(ok, ms)
  })
}

export function future<T = any>(): IFuture<T> {
  let resolver: (x: T) => void = (x: T) => {
    throw new Error('Error initilizing mutex')
  }
  let rejecter: (x: Error) => void = (x: Error) => {
    throw x
  }

  const promise: any = new Promise((ok, err) => {
    resolver = ok
    rejecter = err
  })

  promise.resolve = resolver
  promise.reject = rejecter

  return promise as IFuture<T>
}

export function testInWorker(file: string, options: ITestInWorkerOptions = {}) {
  let worker: Worker | null = null

  it(`creates a worker for ${file}`, () => {
    worker = new Worker(file)
  })

  it(`tests the worker ${file}`, () => {
    return testWithTransport(file, options, WebWorkerTransport(worker!))
  })
}

export async function testWithTransport(
  file: string,
  options: ITestInWorkerOptions = {},
  transport: ScriptingTransport
) {
  const system = await ScriptingHost.fromTransport(transport)

  if (options.log) {
    system.setLogging({ logConsole: true })
  }

  options.plugins && options.plugins.forEach($ => system.getAPIInstance($))

  system.enable()

  options.execute && options.execute(system)

  const TestPlugin = system.getAPIInstance(Test)

  if (!TestPlugin) throw new Error('Cannot get the Test plugin instance')

  const result = await TestPlugin.waitForPass()

  options.validateResult && options.validateResult(result, system)

  system.unmount()
}
