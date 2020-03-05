import { expect } from 'chai'
import { processServerProfile } from 'shared/profiles/transformations/processServerProfile'

describe('legacy spec conversion', () => {
  it('works with a sample case', () => {
    const original = {
      userId: 'email|5d056a5302751940134de62c',
      email: 'john@doe.org',
      updatedAt: '1563514892830',
      profile: {
        name: 'john',
        avatar: {
          eyes: {
            color: {
              b: 0.25882354378700256,
              g: 0.3803921639919281,
              r: 0.5254902243614197
            }
          },
          hair: {
            color: {
              b: 0.05882352963089943,
              g: 0.1921568661928177,
              r: 0.35686275362968445
            }
          },
          skin: {
            color: {
              b: 0.46666666865348816,
              g: 0.6078431606292725,
              r: 0.800000011920929
            }
          },
          version: 1,
          bodyShape: 'dcl://base-avatars/BaseMale',
          wearables: [
            'dcl://base-avatars/male_body',
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
        created_at: 1562461667823,
        description: "Surfin' the metaverse."
      },
      snapshots: {
        face: 'https://s3.amazonaws.com/avatars-storage.decentraland.org/email%7C5d056a5302751940134de62c/face.png',
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
        face: 'https://s3.amazonaws.com/avatars-storage.decentraland.org/email%7C5d056a5302751940134de62c/face.png',
        body: 'https://s3.amazonaws.com/avatars-storage.decentraland.org/email%7C5d056a5302751940134de62c/body.png'
      }
    }

    expect(processServerProfile('email|5d056a5302751940134de62c', original)).to.deep.equal(expected)
  })
})
