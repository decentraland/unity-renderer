import { PREVIEW } from "../../config"

export const METHODS = ['info', 'log', 'warn', 'trace', 'error'] as const
type Method = (typeof METHODS)[number]

/**
 * @deprecated Only exported for testing. DO NOT USE
 * @internal
 */
export const _console = Object.assign({}, console)

function getPrefix(value?: string) {
  if (value) return value
  if (location.search.includes('DEBUG_LOGS')) return '*'
  if (PREVIEW) return 'kernel:scene'
  return 'kernel:scene'
}

export function wrap(testPrefix?: string) {
  const prefix = getPrefix(testPrefix)
  function logger(method: Method) {
    return function log(...args: any[]): void {
      const [logPrefix] = args

      function matchPrefix() {
        if (prefix === '*' || !prefix) {
          return true
        }
        const prefixes = prefix.split(',')

        if (typeof logPrefix === 'string') {
          return prefixes.find((p) => logPrefix.startsWith(p))
        }

        return false
      }
      if (matchPrefix()) {
        return _console[method](...args)
      }
    }
  }

  METHODS.forEach((method) => {
    console[method] = logger(method)
  })
}

export default wrap
