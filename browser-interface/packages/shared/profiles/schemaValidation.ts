import { Avatar, AvatarInfo, generateLazyValidator, JSONSchema } from '@dcl/schemas'

/**
 * The schema validation requires strict IPFS "snapshots"
 * The following schema uses strings
 */
const stringSnapshotAvatarInfoSchema: JSONSchema<AvatarInfo> = {
  ...AvatarInfo.schema,
  properties: {
    ...AvatarInfo.schema.properties,
    snapshots: { type: 'object', additionalProperties: true, required: [] }
  }
} as any

const stringSnapshotAvatarSchema: JSONSchema<Avatar> = {
  ...Avatar.schema,
  properties: {
    ...Avatar.schema.properties,
    avatar: stringSnapshotAvatarInfoSchema
  }
} as any

export const validateAvatar = generateLazyValidator<Avatar>(stringSnapshotAvatarSchema)
