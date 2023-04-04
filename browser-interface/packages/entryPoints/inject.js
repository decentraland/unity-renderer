globalThis.global = globalThis
globalThis.Buffer = require('buffer').Buffer
globalThis.process = {
  browser: true,
  env: {},
  nextTick(fn, ...args) {
    queueMicrotask(() => fn(...args))
  }
}