import { getUserProfile } from 'shared/comms/peers'
import { defaultLogger } from 'shared/logger'
import { Profile } from 'shared/profiles/types'
import { ensureWorldRunning } from 'shared/world/worldState'

const TIMEOUT_MS = 10 * 60 * 1000

declare const globalThis: any

let timer: NodeJS.Timeout | null = null

export function setDelightedSurveyEnabled(enabled: boolean) {
  if (enabled && !timer) {
    timer = setTimeout(delightedSurvey, TIMEOUT_MS)
  } else if (!enabled && timer) {
    clearTimeout(timer)
    timer = null
  }
}

function delightedSurvey() {
  ensureWorldRunning()
    .then(() => {
      // tslint:disable-next-line:strict-type-predicates
      if (typeof globalThis === 'undefined' || typeof globalThis !== 'object') {
        return
      }
      const { analytics, delighted } = globalThis
      if (!analytics || !delighted) {
        return
      }
      const profile = getUserProfile().profile as Profile | null
      if (profile) {
        const payload = {
          email: profile.ethAddress + '@dcl.gg',
          name: profile.name || 'Guest',
          properties: {
            ethAddress: profile.ethAddress,
            anonymous_id: analytics && analytics.user ? analytics.user().anonymousId() : null
          }
        }

        try {
          delighted.survey(payload)
        } catch (error) {
          defaultLogger.error('Delighted error: ' + error.message, error)
        }
      }
    })
    .catch()
}
