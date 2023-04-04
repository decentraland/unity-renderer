import Sinon, * as sinon from 'sinon'
import * as wrapConsole from 'lib/logger/wrap'
import { defaultLogger } from 'lib/logger'

describe('Wrapped Logger', () => {
  wrapConsole.METHODS.forEach((method) => {
    describe(`Wrapped Console.${method}`, () => {
      beforeEach(() => {
        sinon.reset()
        sinon.restore()
      })

      it('should log everything without prefix', () => {
        const spy = sinon.spy(wrapConsole._console, method)

        wrapConsole.default('*')
        const message = 'Some'
        console[method](message)
        Sinon.assert.calledWith(spy, message)
      })

      it('should NOT log if the message doenst match the prefix', () => {
        const spy = sinon.spy(wrapConsole._console, method)
        const prefix = 'kernel: '
        wrapConsole.default(prefix)

        // No prefix
        const message = 'Some message without prefix'
        console[method](message)
        Sinon.assert.notCalled(spy)

        // Prefix with multiple args
        console[method](prefix, message)
        Sinon.assert.calledWith(spy, prefix, message)

        // Prefix with single arg
        console[method](prefix + message)
        Sinon.assert.calledWith(spy, prefix + message)
      })

      it('should log with multiple prefixes', () => {
        const spy = sinon.spy(wrapConsole._console, method)
        const kernelPrefix = 'kernel: '
        const unityPrefix = 'unity: '
        const prefix = `${kernelPrefix},${unityPrefix}`
        wrapConsole.default(prefix)
        const message = 'Some message without prefix'

        // No prefix
        console[method](message)
        Sinon.assert.notCalled(spy)

        // Kernel prefix
        console[method](kernelPrefix, message)
        Sinon.assert.calledWith(spy, kernelPrefix, message)

        // Unity prefix
        console[method](unityPrefix + message)
        Sinon.assert.calledWith(spy, unityPrefix + message)
      })

      it('should log an object correctly', () => {
        const spy = sinon.spy(wrapConsole._console, method)
        if (method === 'warn') return
        const prefix = '*'
        wrapConsole.default(prefix)
        const message = { someMessage: true }
        defaultLogger[method](message as any)

        Sinon.assert.calledWith(spy, 'kernel: ', '', message)
      })
    })
  })
})
