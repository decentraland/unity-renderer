import { setTimeout } from 'timers'
import { defaultLogger } from 'shared/logger'
import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { StoreContainer } from 'shared/store/rootTypes'
import { ensureRendererEnabled } from 'shared/world/worldState'

const TIMEOUT_MS = 10 * 60 * 1000

declare const globalThis: StoreContainer & { analytics: any; delighted: any }

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
  ensureRendererEnabled()
    .then(() => {
      // tslint:disable-next-line:strict-type-predicates
      if (typeof globalThis === 'undefined' || typeof globalThis !== 'object') {
        return
      }
      const { analytics, delighted } = globalThis
      if (!analytics || !delighted) {
        return
      }
      const profile = getCurrentUserProfile(globalThis.globalStore.getState())
      if (profile) {
        const payload = {
          email: profile.userId + '@dcl.gg',
          name: profile.name || 'Guest',
          properties: {
            userId: profile.userId,
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
