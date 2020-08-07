import { getUserProfile } from 'shared/comms/peers';
import { defaultLogger } from 'shared/logger';
import { Profile } from 'shared/profiles/types';
import { globalThis, isTheFirstLoading } from './dcl';

export function delightedSurvey() {
  // tslint:disable-next-line:strict-type-predicates
  if (typeof globalThis === 'undefined' || typeof globalThis !== 'object') {
    return;
  }
  const { analytics, delighted } = globalThis;
  if (!analytics || !delighted) {
    return;
  }
  const profile = getUserProfile().profile as Profile | null;
  if (!isTheFirstLoading && profile) {
    const payload = {
      email: profile.email || profile.ethAddress + '@dcl.gg',
      name: profile.name || 'Guest',
      properties: {
        ethAddress: profile.ethAddress,
        anonymous_id: analytics && analytics.user ? analytics.user().anonymousId() : null
      }
    };

    try {
      delighted.survey(payload);
    }
    catch (error) {
      defaultLogger.error('Delighted error: ' + error.message, error);
    }
  }
}
