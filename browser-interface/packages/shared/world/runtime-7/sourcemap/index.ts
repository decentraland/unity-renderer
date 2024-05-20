import { PREVIEW } from 'config'
import { SourceMapConsumerConstructor, Sourcemap } from './types'

import('./source-map@0.7.4.js')

let initialized = false
declare const globalThis: {
  sourceMap: { SourceMapConsumer: SourceMapConsumerConstructor }
}

async function getSourcemap() {
  if (initialized) return globalThis.sourceMap
  initialized = true

  if (!global.sourceMap) {
    return
  }

  globalThis.sourceMap.SourceMapConsumer.initialize({
    'lib/mappings.wasm': 'https://unpkg.com/source-map@0.7.4/lib/mappings.wasm'
  })

  return globalThis.sourceMap
}

export async function initSourcemap(code: string, inlineSourcemaps: boolean = true): Promise<Sourcemap | void> {
  const sourceMap = await getSourcemap()
  if (!sourceMap) return
  function decodeSourcemap(): any {
    // External source-maps .js.map file
    if (!inlineSourcemaps) {
      return code
    }
    // Inline sourcemap, find the source-map inside the file encoded in a base64
    const inlineSourceMapComment = code.match(/\/\/# sourceMappingURL=data:application\/json;base64,(.*)/)
    if (!inlineSourceMapComment || !inlineSourceMapComment[1]) return
    const decodedSourceMap = Buffer.from(inlineSourceMapComment[1], 'base64').toString('utf-8')
    return decodedSourceMap
  }
  const sourcemapCode = decodeSourcemap()
  if (!sourcemapCode) return
  const sourcemapConsumer = await new sourceMap.SourceMapConsumer(sourcemapCode)

  /**
   * Because the scene-runtime uses an eval with a Function, it generates an offset of :2
   * in every error. So we need to fix that in the error code stack.
   */
  function adjustStackTrace(stackTrace: string) {
    // Don't adjust the stack trace if we are in preview mode
    //  preview mode uses a plain eval wrapped, so this is not necessary
    //  see more https://github.com/decentraland/scene-runtime/pull/211
    if (PREVIEW) return stackTrace

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
