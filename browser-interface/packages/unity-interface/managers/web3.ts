import { ethereumConfigurations } from 'config'
import { defaultLogger } from 'lib/logger'
import { getERC20Balance } from 'lib/web3/EthereumService'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { ensureRealmAdapter } from 'shared/realm/ensureRealmAdapter'
import { getFetchContentUrlPrefixFromRealmAdapter } from 'shared/realm/selectors'
import { getCurrentIdentity } from 'shared/session/selectors'
import { store } from 'shared/store/isolatedStore'
import { fetchENSOwnerProfile } from 'unity-interface/fetchENSOwnerProfile'
import { getUnityInstance } from 'unity-interface/IUnityInterface'

let lastBalanceOfMana: number | undefined = undefined
export async function handleFetchBalanceOfMANA() {
  try {
    const identity = getCurrentIdentity(store.getState())

    if (!identity?.hasConnectedWeb3) {
      return
    }
    const net = getSelectedNetwork(store.getState())
    const balance = (await getERC20Balance(identity.address, ethereumConfigurations[net].MANAToken)).toNumber()
    if (lastBalanceOfMana !== balance) {
      lastBalanceOfMana = balance
      getUnityInstance().UpdateBalanceOfMANA(`${balance}`)
    }
  } catch (err: any) {
    defaultLogger.error(err)
  }
}

export async function handleSearchENSOwner(data: { name: string; maxResults?: number }) {
  async function work() {
    const adapter = await ensureRealmAdapter()
    const fetchContentServerWithPrefix = getFetchContentUrlPrefixFromRealmAdapter(adapter)

    try {
      const profiles = await fetchENSOwnerProfile(data.name, data.maxResults)
      getUnityInstance().SetENSOwnerQueryResult(data.name, profiles, fetchContentServerWithPrefix)
    } catch (error: any) {
      getUnityInstance().SetENSOwnerQueryResult(data.name, undefined, fetchContentServerWithPrefix)
      defaultLogger.error(error)
    }
  }

  work().catch(defaultLogger.error)
}
