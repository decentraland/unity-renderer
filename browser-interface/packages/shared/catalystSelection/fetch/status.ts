import { ServerConnectionStatus, Candidate, Parcel } from '../types'
import { ask } from './ping'

export async function fetchCatalystStatus(
  domain: string,
  denylistedCatalysts: string[],
  askFunction: typeof ask
): Promise<Candidate | undefined> {
  if (denylistedCatalysts.includes(domain)) return undefined

  const [aboutResponse, parcelsResponse] = await Promise.all([
    askFunction(`${domain}/about`),
    askFunction(`${domain}/stats/parcels`)
  ])

  const result = aboutResponse.result
  if (
    aboutResponse.status === ServerConnectionStatus.OK &&
    result &&
    result.comms &&
    result.configurations &&
    result.bff
  ) {
    const { comms, configurations, bff } = result

    // TODO(hugo): this is kind of hacky, the original representation is much better,
    // but I don't want to change the whole pick-realm algorithm now
    const usersParcels: Parcel[] = []

    if (parcelsResponse.result && parcelsResponse.result.parcels) {
      for (const {
        peersCount,
        parcel: { x, y }
      } of parcelsResponse.result.parcels) {
        const parcel: Parcel = [x, y]
        for (let i = 0; i < peersCount; i++) {
          usersParcels.push(parcel)
        }
      }
    }

    return {
      protocol: comms.protocol,
      catalystName: configurations.realmName,
      domain: domain,
      status: aboutResponse.status,
      elapsed: aboutResponse.elapsed!,
      usersCount: bff.userCount || comms.usersCount || 0,
      maxUsers: 2000,
      usersParcels
    }
  }

  return undefined
}
