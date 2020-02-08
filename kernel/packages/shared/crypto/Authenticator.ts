import { hash, sign, recover } from 'eth-crypto'

export type Signature = string
export type EthAddress = string

export type IdentityType = {
  privateKey: string
  publicKey: string
  address: string
}

export type AuthChain = AuthLink[]

export type AuthLink = {
  type: AuthLinkType
  payload: string
  signature: Signature
}

export enum AuthLinkType {
  SIGNER = 'SIGNER',
  ECDSA_EPHEMERAL = 'ECDSA_EPHEMERAL',
  ECDSA_SIGNED_ENTITY = 'ECDSA_SIGNED_ENTITY'
}

export type AuthIdentity = {
  ephemeralIdentity: IdentityType
  expiration: Date
  authChain: AuthChain
  address: EthAddress
}

export type AuditInfo = {
  version: EntityVersion
  deployedTimestamp: Timestamp

  authChain: AuthChain

  overwrittenBy?: EntityId

  isBlacklisted?: boolean
  blacklistedContent?: ContentFileHash[]

  originalMetadata?: {
    // This is used for migrations
    originalVersion: EntityVersion
    data: any
  }
}
export enum EntityVersion {
  V2 = 'v2',
  V3 = 'v3'
}

export type Timestamp = number

export type EntityId = ContentFileHash
export type ContentFileHash = string

export class Authenticator {
  /** Validate that the signature belongs to the Ethereum address */
  static async validateSignature(expectedFinalAuthority: string, authChain: AuthChain): Promise<boolean> {
    let currentAuthority: string = ''
    authChain.forEach(authLink => {
      const validator: ValidatorType = getValidatorByType(authLink.type)
      const { error, nextAuthority } = validator(currentAuthority, authLink)
      if (error) {
        return false
      }
      currentAuthority = nextAuthority || ''
    })
    return currentAuthority === expectedFinalAuthority
  }

  static createEthereumMessageHash(msg: string) {
    let msgWithPrefix: string = `\x19Ethereum Signed Message:\n${msg.length}${msg}`
    const msgHash = hash.keccak256(msgWithPrefix)
    return msgHash
  }

  static createSimpleAuthChain(finalPayload: string, ownerAddress: EthAddress, signature: Signature): AuthChain {
    return [
      {
        type: AuthLinkType.SIGNER,
        payload: ownerAddress,
        signature: ''
      },
      {
        type: AuthLinkType.ECDSA_SIGNED_ENTITY,
        payload: finalPayload,
        signature: signature
      }
    ]
  }

  static async initializeAuthChain(
    ethAddress: EthAddress,
    ephemeralIdentity: IdentityType,
    ephemeralMinutesDuration: number,
    signer: (message: string) => Promise<string>
  ): Promise<AuthIdentity> {
    let expiration = new Date()
    expiration.setMinutes(expiration.getMinutes() + ephemeralMinutesDuration)

    const ephemeralMessage = `Decentraland Login\nEphemeral address: ${
      ephemeralIdentity.address
    }\nExpiration: ${expiration.toISOString()}`
    const firstSignature = await signer(ephemeralMessage)

    const authChain: AuthChain = [
      { type: AuthLinkType.SIGNER, payload: ethAddress, signature: '' },
      { type: AuthLinkType.ECDSA_EPHEMERAL, payload: ephemeralMessage, signature: firstSignature }
    ]

    return {
      ephemeralIdentity,
      expiration,
      authChain,
      address: ethAddress.toLocaleLowerCase()
    }
  }

  static signPayload(authIdentity: AuthIdentity, entityId: string) {
    const secondSignature = Authenticator.createSignature(authIdentity.ephemeralIdentity, entityId)
    return [
      ...authIdentity.authChain,
      { type: AuthLinkType.ECDSA_SIGNED_ENTITY, payload: entityId, signature: secondSignature }
    ]
  }

  static createSignature(identity: IdentityType, message: string) {
    return this.doCreateSignature(identity.privateKey, message)
  }

  static doCreateSignature(privateKey: string, message: string) {
    return sign(privateKey, Authenticator.createEthereumMessageHash(message))
  }

  static ownerAddress(auditInfo: AuditInfo): EthAddress {
    if (auditInfo.authChain.length > 0) {
      if (auditInfo.authChain[0].type === AuthLinkType.SIGNER) {
        return auditInfo.authChain[0].payload
      }
    }
    return 'Invalid-Owner-Address'
  }
}

type ValidatorType = (authority: string, authLink: AuthLink) => { error?: boolean; nextAuthority?: string }

const SIGNER_VALIDATOR: ValidatorType = (authority: string, authLink: AuthLink) => {
  return { nextAuthority: authLink.payload }
}

const ECDSA_SIGNED_ENTITY_VALIDATOR: ValidatorType = (authority: string, authLink: AuthLink) => {
  try {
    const signerAddress = recover(authLink.signature, Authenticator.createEthereumMessageHash(authLink.payload))
    if (authority.toLocaleLowerCase() === signerAddress.toLocaleLowerCase()) {
      return { nextAuthority: authLink.payload }
    }
  } catch (e) {
    // nothing here
  }
  return { error: true }
}

const ECDSA_EPHEMERAL_VALIDATOR: ValidatorType = (authority: string, authLink: AuthLink) => {
  try {
    // authLink payload structure: <human-readable message>\nEphemeral address: <ephemeral-eth-address>\nExpiration: <timestamp>
    // authLink payload example  : Decentraland Login\nEphemeral address: 0x123456\nExpiration: 2020-01-20T22:57:11.334Z
    const payloadParts: string[] = authLink.payload.split('\n')
    const ephemeralAddress: string = payloadParts[1].substring('Ephemeral address: '.length)
    const expirationString: string = payloadParts[2].substring('Expiration: '.length)
    const expiration = Date.parse(expirationString)

    if (expiration > Date.now()) {
      const signerAddress = recover(authLink.signature, Authenticator.createEthereumMessageHash(authLink.payload))
      if (authority.toLocaleLowerCase() === signerAddress.toLocaleLowerCase()) {
        return { nextAuthority: ephemeralAddress }
      }
    }
  } catch (e) {
    // doing nothing here
  }
  return { error: true }
}

const ERROR_VALIDATOR: ValidatorType = (authority: string, authLink: AuthLink) => {
  return { error: true }
}

function getValidatorByType(type: AuthLinkType): ValidatorType {
  switch (type) {
    case AuthLinkType.SIGNER:
      return SIGNER_VALIDATOR
    case AuthLinkType.ECDSA_EPHEMERAL:
      return ECDSA_EPHEMERAL_VALIDATOR
    case AuthLinkType.ECDSA_SIGNED_ENTITY:
      return ECDSA_SIGNED_ENTITY_VALIDATOR
    default:
      return ERROR_VALIDATOR
  }
}
