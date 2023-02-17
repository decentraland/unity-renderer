import { resolveUrlFromUrn } from '@dcl/urn-resolver'

export async function resolveBaseUrl(urn: string): Promise<string> {
  if (urn.startsWith('urn:')) {
    const t = await resolveUrlFromUrn(urn)
    if (t) {
      return (t + '/').replace(/(\/)+$/, '/')
    }
    throw new Error('Cannot resolve content for URN ' + urn)
  }
  return (urn + '/').replace(/(\/)+$/, '/')
}
