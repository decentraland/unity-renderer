import { getDefaultTLD } from 'config'
import { queueTrackingEvent } from 'shared/analytics'
import { ExplorerIdentity } from 'shared/session/types'
import { defaultLogger } from 'shared/logger'
import { saveToLocalStorage, getFromLocalStorage, removeFromLocalStorage } from '../../atomicHelpers/localStorage'

const REFERRAL_KEY = 'dcl-referral'

declare var location: any
declare var history: any

export function getReferralEndpoint() {
  switch (getDefaultTLD()) {
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

export function saveReferral() {
  const params = new URLSearchParams(location.search)
  const code = params.get('referral')
  if (code) {
    saveToLocalStorage(REFERRAL_KEY, code)
    queueTrackingEvent('referral_save', { code })

    params.delete('referral')
    history.replaceState(history.state, document.title, '?' + params.toString())
  }
}

export function referUser(identity: ExplorerIdentity) {
  const code = getFromLocalStorage(REFERRAL_KEY)
  const endpoint = getReferralEndpoint()
  if (code && endpoint && identity && identity.authChain) {
    const options: RequestInit = {
      method: 'POST',
      headers: {
        'Authorization': 'Bearer ' + btoa(JSON.stringify(identity.authChain)),
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
          queueTrackingEvent('referral_save', { code, address, referral_of })
          removeFromLocalStorage(REFERRAL_KEY)
        }
      })
      .catch((err) => defaultLogger.error(err.message, err))
  }
}
