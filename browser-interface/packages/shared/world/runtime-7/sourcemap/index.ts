import { injectScript } from 'lib/browser/inject-script'
import { SourceMapConsumerConstructor, Sourcemap } from './types'

let initialized = false
declare const globalThis: {
  sourceMap: { SourceMapConsumer: SourceMapConsumerConstructor }
}

async function getSourcemap() {
  if (initialized) return globalThis.sourceMap
  initialized = true
  await injectScript('https://unpkg.com/source-map@0.7.3/dist/source-map.js')
  globalThis.sourceMap.SourceMapConsumer.initialize({
    'lib/mappings.wasm': 'https://unpkg.com/source-map@0.7.4/lib/mappings.wasm'
  })
  return globalThis.sourceMap
}

export async function initSourcemap(code: string): Promise<Sourcemap | void> {
  const sourceMap = await getSourcemap()
  const inlineSourceMapComment = code.match(/\/\/# sourceMappingURL=data:application\/json;base64,(.*)/)

  if (!inlineSourceMapComment || !inlineSourceMapComment[1]) return

  const decodedSourceMap = Buffer.from(inlineSourceMapComment[1], 'base64').toString('utf-8')
  const sourcemapConsumer = await new sourceMap.SourceMapConsumer(decodedSourceMap as any)

  /**
   * Because the scene-runtime uses an eval with a Function, it generates an offset of :2
   * in every error. So we need to fix that in the error code stack.
   */
  function adjustStackTrace(stackTrace: string) {
    const lines = stackTrace.split('\n')
    const adjustedLines = lines.map((line) => {
      // Check if the line contains a line number
      const match = line.match(/:(\d+):(\d+)\)$/)
      if (match) {
        const lineNumber = parseInt(match[1], 10)
        const adjustedLineNumber = lineNumber - 2 // Add 2 to each line number
        return line.replace(`:${lineNumber}:`, `:${adjustedLineNumber}:`)
      }
      return line
    })
    return adjustedLines.join('\n')
  }

  function parseError(error: Error) {
    if (!error.stack || !sourcemapConsumer) {
      return error
    }
    const stack = adjustStackTrace(error.stack.toString()).split('\n')
    const mappedStackTrace = stack.map((frame, index) => {
      // Show the error message
      if (index === 0) return frame

      // Fix all anonymous errors
      const match = frame.match(/<anonymous>:(\d+):(\d+)/)
      if (match) {
        const [, line, column] = match
        const originalPosition = sourcemapConsumer.originalPositionFor({
          line: parseInt(line, 10),
          column: parseInt(column, 10)
        })
        if (originalPosition.source) {
          const fileName = ` (${originalPosition.source}:${originalPosition.line}:${originalPosition.column})`
          return frame.replace(/ \(eval.*$/, fileName)
        }
      }
      return ''
    })
    return mappedStackTrace.join('\n')
  }
  return { parseError }
}
