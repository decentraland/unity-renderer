export const METHODS = ['error', 'info', 'log', 'warn', 'trace'] as const
type Method = (typeof METHODS)[number]

/**
 * @deprecated Only exported for testing. DO NOT USE
 * @internal
 */
export const _console = Object.assign({}, console)

export default function wrap(prefix: string) {
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
