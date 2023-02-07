import { defaultLogger } from 'lib/logger'
import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { store } from 'shared/store/isolatedStore'

const TIMEOUT_MS = 10 * 60 * 1000

declare const globalThis: { analytics: any; delighted: any }

let timer: ReturnType<typeof setTimeout> | null = null

export function setDelightedSurveyEnabled(enabled: boolean) {
  if (enabled && !timer) {
    timer = setTimeout(delightedSurvey, TIMEOUT_MS)
  } else if (!enabled && timer) {
    clearTimeout(timer)
    timer = null
  }
}

function delightedSurvey() {
  if (typeof globalThis === 'undefined' || typeof globalThis !== 'object') {
    return
  }
  const { analytics, delighted } = globalThis
  if (!analytics || !delighted) {
    return
  }
  const profile = getCurrentUserProfile(store.getState())
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
    } catch (error: any) {
      defaultLogger.error('Delighted error: ' + error.message, error)
    }
  }
}
