import { expect } from 'chai'
import { ensureAvatarCompatibilityFormat } from 'lib/decentraland/profiles/transformations/profileToServerFormat'

describe('legacy spec conversion', () => {
  it.skip('works with a sample case', () => {
    const original = {
      userId: 'email|5d056a5302751940134de62c',
      email: 'john@doe.org',
      name: 'john',
      version: 1,
      created_at: 1562461667823,
      updated_at: 1563514892830,
      description: "Surfin' the metaverse.",
      inventory: [],
      avatar: {
        eyeColor: '#876142',
        hairColor: '#5b310f',
        skinColor: '#cd9c77',
        bodyShape: 'dcl://base-avatars/BaseMale',
        wearables: [
          'dcl://base-avatars/BaseMale',
          'dcl://base-avatars/turtle_neck_sweater',
          'dcl://base-avatars/brown_pants',
          'dcl://base-avatars/sport_black_shoes',
          'dcl://base-avatars/tall_front_01',
          'dcl://base-avatars/eyes_09',
          'dcl://base-avatars/eyebrows_06',
          'dcl://base-avatars/mouth_07',
          'dcl://base-avatars/short_boxed_beard',
          'dcl://base-avatars/Thunder_earring'
        ]
      },
      snapshots: {
        face256: 'https://s3.amazonaws.com/avatars-storage.decentraland.org/email%7C5d056a5302751940134de62c/face.png',
        body: 'https://s3.amazonaws.com/avatars-storage.decentraland.org/email%7C5d056a5302751940134de62c/body.png'
      }
    }

    const expected = {
      userId: 'email|5d056a5302751940134de62c',
      email: 'john@doe.org',
      name: 'john',
      version: 1,
      created_at: 1562461667823,
      updated_at: 1563514892830,
      description: "Surfin' the metaverse.",
      inventory: [],
      avatar: {
        eyeColor: '#876142',
        hairColor: '#5b310f',
        skinColor: '#cd9c77',
        bodyShape: 'dcl://base-avatars/BaseMale',
        wearables: [
          'dcl://base-avatars/BaseMale',
          'dcl://base-avatars/turtle_neck_sweater',
          'dcl://base-avatars/brown_pants',
          'dcl://base-avatars/sport_black_shoes',
          'dcl://base-avatars/tall_front_01',
          'dcl://base-avatars/eyes_09',
          'dcl://base-avatars/eyebrows_06',
          'dcl://base-avatars/mouth_07',
          'dcl://base-avatars/short_boxed_beard',
          'dcl://base-avatars/Thunder_earring'
        ]
      },
      snapshots: {
        face256: 'https://s3.amazonaws.com/avatars-storage.decentraland.org/email%7C5d056a5302751940134de62c/face.png',
        body: 'https://s3.amazonaws.com/avatars-storage.decentraland.org/email%7C5d056a5302751940134de62c/body.png'
      }
    }

    expect(ensureAvatarCompatibilityFormat(original as any)).to.deep.equal(expected)
  })
})
