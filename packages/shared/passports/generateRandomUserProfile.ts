import { getServerConfigurations } from '../../config/index'
import { Profile } from './types'
import { colorString } from './transformations/colorString'
import { fixWearableIds } from './transformations/processServerProfile'

export const sexes = ['female', 'male']
export const skins = ['7d5d47', '522c1c', 'cc9b77', 'f2c2a5', 'ffe4c6']
export const numbers = [
  '00001',
  '00002',
  '00003',
  '00004',
  '00005',
  '00006',
  '00007',
  '00008',
  '00009',
  '00010',
  '00011',
  '00012',
  '00013',
  '00014',
  '00015',
  '00016'
]

export function randomIn(array: any[]) {
  return array[Math.floor(Math.random() * array.length)]
}
export async function generateRandomUserProfile(userId: string): Promise<Profile> {
  const sex = randomIn(sexes)
  const skin = randomIn(skins)
  const _number = randomIn(numbers)
  const baseUrl = `${getServerConfigurations().avatar.presets}/${sex}/${skin}/${_number}`
  const response = await fetch(`${baseUrl}/avatar.json`)
  const avatarJson: any = await response.json()
  const avatarV2 = {
    bodyShape: fixWearableIds(avatarJson.bodyShape),
    skinColor: colorString(avatarJson.skin.color),
    hairColor: colorString(avatarJson.hair.color),
    eyeColor: colorString(avatarJson.eyes.color),
    wearables: avatarJson.wearables.map(fixWearableIds)
  }
  const name =
    'Guest-' +
    Math.random()
      .toFixed(6)
      .substr(2)
  return {
    userId,
    email: name.toLowerCase() + '@nowhere.com',
    inventory: ['dcl://base-exclusive/tropical_mask', 'dcl://base-exclusive/Serial_killer_mask'],
    version: 0,
    name,
    ethAddress: 'noeth',
    description: '',
    updatedAt: new Date().getDate(),
    createdAt: new Date().getDate(),
    avatar: avatarV2,
    snapshots: {
      face: `${baseUrl}/face.png`,
      body: `${baseUrl}/body.png`
    }
  }
}
