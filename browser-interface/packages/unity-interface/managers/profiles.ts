import { Avatar, generateLazyValidator, JSONSchema } from '@dcl/schemas'
import defaultLogger from 'lib/logger'
import { trackEvent } from 'shared/analytics/trackEvent'
import { saveProfileDelta } from 'shared/profiles/actions'
import { store } from 'shared/store/isolatedStore'

/** Message from renderer sent to save the profile in the catalyst */
export type RendererSaveProfile = {
  avatar: {
    name: string
    bodyShape: string
    skinColor: {
      r: number
      g: number
      b: number
      a: number
    }
    hairColor: {
      r: number
      g: number
      b: number
      a: number
    }
    eyeColor: {
      r: number
      g: number
      b: number
      a: number
    }
    wearables: string[]
    emotes: {
      slot: number
      urn: string
    }[]
  }
  face256: string
  body: string
  isSignUpFlow?: boolean
}

const color3Schema: JSONSchema<{ r: number; g: number; b: number; a: number }> = {
  type: 'object',
  required: ['r', 'g', 'b', 'a'],
  properties: {
    r: { type: 'number', nullable: false },
    g: { type: 'number', nullable: false },
    b: { type: 'number', nullable: false },
    a: { type: 'number', nullable: false }
  }
} as any
const emoteSchema: JSONSchema<{ slot: number; urn: string }> = {
  type: 'object',
  required: ['slot', 'urn'],
  properties: {
    slot: { type: 'number', nullable: false },
    urn: { type: 'string', nullable: false }
  }
}

export const rendererSaveProfileSchema: JSONSchema<RendererSaveProfile> = {
  type: 'object',
  required: ['avatar', 'body', 'face256'],
  properties: {
    face256: { type: 'string' },
    body: { type: 'string' },
    isSignUpFlow: { type: 'boolean', nullable: true },
    avatar: {
      type: 'object',
      required: ['bodyShape', 'eyeColor', 'hairColor', 'name', 'skinColor', 'wearables'],
      properties: {
        bodyShape: { type: 'string' },
        name: { type: 'string' },
        eyeColor: color3Schema,
        hairColor: color3Schema,
        skinColor: color3Schema,
        wearables: { type: 'array', items: { type: 'string' } },
        emotes: { type: 'array', items: emoteSchema }
      }
    }
  }
} as any
const validateRendererSaveProfile = generateLazyValidator<RendererSaveProfile>(rendererSaveProfileSchema)

export function handleSaveUserAvatar(changes: RendererSaveProfile) {
  if (validateRendererSaveProfile(changes as RendererSaveProfile)) {
    const update: Partial<Avatar> = {
      avatar: {
        bodyShape: changes.avatar.bodyShape,
        eyes: { color: changes.avatar.eyeColor },
        hair: { color: changes.avatar.hairColor },
        skin: { color: changes.avatar.skinColor },
        wearables: changes.avatar.wearables,
        snapshots: {
          body: changes.body,
          face256: changes.face256
        },
        emotes: changes.avatar.emotes
      }
    }
    store.dispatch(saveProfileDelta(update))
  } else {
    const errors = validateRendererSaveProfile.errors ?? validateRendererSaveProfile.errors
    defaultLogger.error('error validating schema', errors)
    trackEvent('invalid_schema', {
      schema: 'SaveUserAvatar',
      payload: changes,
      errors: (errors ?? []).map(($) => $.message).join(',')
    })
    defaultLogger.error('Unity sent invalid profile' + JSON.stringify(changes) + ' Errors: ' + JSON.stringify(errors))
  }
}
