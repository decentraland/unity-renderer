export type Dictionary<T> = {
  [key: string]: T
}

export namespace DebugTelemetry {
  const initialDate = Date.now()

  export interface ITelemetryData {
    metric: string
    time: number
    data: { [key: string]: number | string }
  }

  export const _store: ITelemetryData[] = []
  export let _tags: Dictionary<string> = {}

  export let isEnabled: boolean = false

  /**
   * Sets the `isEnabled` flag to `true`
   */
  export function startTelemetry(tags: Dictionary<string> = {}): void {
    isEnabled = true
    _tags = tags
  }

  /**
   * Sets the `isEnabled` flag to `false` and returns the collected data.
   */
  export function stopTelemetry(): ITelemetryData[] {
    isEnabled = false
    const ret = _store.splice(0)
    _store.length = 0
    return ret
  }

  /**
   * Stores telemetry data in memory. Meant to be called on each frame.
   * @param metric The name of the time series
   * @param values object with number values (metrics)
   * @param tags dictionary with tags, used to filter the metric
   */
  export function collect(metric: string, values: Dictionary<number>, tags: Dictionary<string> = {}) {
    if (!isEnabled) return
    _store.push({
      time: initialDate + performance.now(),
      metric,
      data: { ...values, ...tags, ..._tags }
    })
  }

  function noopMeasure(values: Dictionary<number>) {
    /*noop*/
  }

  /**
   * Starts a measurement and returns a callback for when it is finalized.
   * The callback will receive values: Dictionary<number>
   * @param metric the name of the metric to measure
   * @param tags extra tags
   */
  export function measure(metric: string, tags: Dictionary<string> = {}): (values?: Dictionary<number>) => void {
    if (!isEnabled) return noopMeasure

    const start = performance.now()

    return function(values: Dictionary<number> = {}) {
      const end = performance.now()
      _store.push({
        time: initialDate + start,
        metric,
        data: { ...values, ...tags, ..._tags, duration: end - start }
      })
    }
  }

  /**
   * Dumps all the collected telemetry data into a JSON file that gets automatically
   * downloaded by the browser.
   */
  export function save() {
    const filename = `telemetry-${+Date.now()}.json`
    const data = JSON.stringify(_store, undefined, 4)

    const blob = new Blob([data], { type: 'text/json' })
    const e = document.createEvent('MouseEvents')
    const a = document.createElement('a')

    a.download = filename
    a.href = window.URL.createObjectURL(blob)
    a.dataset.downloadurl = ['text/json', a.download, a.href].join(':')
    e.initMouseEvent('click', true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null)
    a.dispatchEvent(e)
  }
}

global['dclTelemetry'] = DebugTelemetry

/**
 * This method is a function wrapper that adds measuring instrumentation based
 * in the DebugTelemetry.isEnabled flag. Use this to measure recurring STATIC
 * function like `updatePhysics`.
 * DO NO USE IT FOR DYNAMIC (inline) FUNCTIONS
 *
 * @param metricName the name of the metric to be recorded
 * @param fn the function to be measured
 * @returns a function with the same signature as the second parameter
 */
export function instrumentTelemetry<T extends Function>(metricName: string, fn: T): T {
  return (function() {
    if (DebugTelemetry.isEnabled) {
      const start = performance.now()
      const result = fn.apply(this, arguments)

      DebugTelemetry.collect(metricName, {
        duration: performance.now() - start
      })

      return result
    } else {
      return fn.apply(this, arguments)
    }
  } as any) as T
}

/**
 * Same as instrumentTelemetry, it decorates a class method
 */
export function instrumentMethodTelemetry(name: string) {
  return function<T extends Function>(
    target: Object,
    propertyKey: string | symbol,
    descriptor: TypedPropertyDescriptor<T>
  ) {
    const originalMethod = descriptor.value

    // editing the descriptor/value parameter
    descriptor.value = instrumentTelemetry(name, originalMethod)

    return descriptor
  }
}
