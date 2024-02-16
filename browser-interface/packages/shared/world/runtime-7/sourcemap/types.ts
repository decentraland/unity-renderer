export type RawSourceMap = {
  version: number
  sources: string[]
  names: string[]
  sourceRoot?: string
  sourcesContent?: string[]
  mappings: string
  file: string
}
export type BasicSourceMapConsumer = SourceMapConsumer & {
  file: string
  sourceRoot: string
  sources: string[]
  sourcesContent: string[]
}

export type SourceMapConsumerConstructor = {
  prototype: SourceMapConsumer
  initialize(keys: { [key: string]: string }): void
  new (rawSourceMap: RawSourceMap, sourceMapUrl?: string): Promise<BasicSourceMapConsumer>
}

export type SourceMapConsumer = {
  originalPositionFor(generatedPosition: { line: number; column: number }): {
    source: string | null
    line: number | null
    column: number | null
    name: string | null
  }
}

export type Sourcemap = {
  parseError(error: Error): string | Error
}