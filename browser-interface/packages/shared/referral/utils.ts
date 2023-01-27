import { getTLD } from 'config'
import { trackEvent } from 'shared/analytics'
import { ExplorerIdentity } from 'shared/session/types'
import { defaultLogger } from 'shared/logger'
import {
  saveToPersistentStorage,
  getFromPersistentStorage,
  removeFromPersistentStorage
} from '../../atomicHelpers/persistentStorage'

const REFERRAL_KEY = 'dcl-referral'

declare let location: any
declare let history: any

export function getReferralEndpoint() {
  switch (getTLD()) {
    case 'org':
      return `https://referral.decentraland.org`

    case 'today':
      return `https://referral.decentraland.net`

    case 'zone':
      return `https://referral.decentraland.io`

    default:
      return undefined
  }
}

export async function saveReferral() {
  const params = new URLSearchParams(location.search)
  const code = params.get('referral')
  if (code) {
    await saveToPersistentStorage(REFERRAL_KEY, code)
    trackEvent('referral_save', { code })

    params.delete('referral')
    history.replaceState(history.state, document.title, '?' + params.toString())
  }
}

export async function referUser(identity: ExplorerIdentity) {
  const code = await getFromPersistentStorage(REFERRAL_KEY)
  const endpoint = getReferralEndpoint()
  if (code && endpoint && identity && identity.authChain) {
    const options: RequestInit = {
      method: 'POST',
      headers: {
        Authorization: 'Bearer ' + btoa(JSON.stringify(identity.authChain)),
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ code })
    }

    fetch(endpoint + '/api/referralOf', options)
      .then((response) => response.json())
      .then((result) => {
        if (result.ok) {
          const address = identity.address
          const referral_of = result.data.referral_of
          trackEvent('referral_save', { code, address, referral_of })
          return removeFromPersistentStorage(REFERRAL_KEY)
        }
      })
      .catch((err) => defaultLogger.error(err.message, err))
  }
}
