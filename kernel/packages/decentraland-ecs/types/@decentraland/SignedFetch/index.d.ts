declare module '@decentraland/SignedFetch' {
  export type FlatFetchResponse = {
    ok: boolean
    status: number
    statusText: string
    headers: Record<string, string>
    json?: any
    text?: string
  }

  export type BodyType = 'json' | 'text'

  export type FlatFetchInit = RequestInit & { responseBodyType?: BodyType }

  export function signedFetch(url: string, init?: FlatFetchInit): Promise<FlatFetchResponse>
}
