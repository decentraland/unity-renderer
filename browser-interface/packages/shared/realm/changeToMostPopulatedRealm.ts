import { getRealmAdapter } from 'shared/realm/selectors'
import { store } from 'shared/store/isolatedStore'
import { getAllCatalystCandidates } from '../catalystSelection/selectors'
import { Candidate } from '../catalystSelection/types'
import { changeRealm } from './changeRealm'

// TODO: unify this function with the one implementing the realm selection algorithm

export async function changeToMostPopulatedRealm(): Promise<void> {
  const realmAdapter = getRealmAdapter(store.getState())
  const allCandidates: Candidate[] = getAllCatalystCandidates(store.getState())
  let isDAORealm: boolean = false

  for (let i = 0; i < allCandidates.length; i++) {
    if (allCandidates[i].catalystName === realmAdapter?.about.configurations?.realmName) {
      isDAORealm = true
    }
  }

  //If a realm is found, then we are already on a DAO realm
  if (isDAORealm) {
    return
  }

  const sortedArray: Candidate[] = allCandidates.sort((n1, n2) => {
    if (n1.usersCount < n2.usersCount) {
      return 1
    }
    if (n1.usersCount > n2.usersCount) {
      return -1
    }
    return 0
  })

  await changeRealm(sortedArray[0].catalystName)
}
