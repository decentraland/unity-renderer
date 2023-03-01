/**
 * @public
 */
export type Signature = string

/**
 * @public
 */
export type EthAddress = string

/**
 * @public
 */
export type IdentityType = {
  privateKey: string
  publicKey: string
  address: string
}

/**
 * @public
 */
export type AuthChain = AuthLink[]

/**
 * @public
 */
export type AuthLink = {
  type: AuthLinkType
  payload: string
  signature: Signature
}

/**
 * @public
 */
export enum AuthLinkType {
  SIGNER = 'SIGNER',
  ECDSA_PERSONAL_EPHEMERAL = 'ECDSA_EPHEMERAL',
  ECDSA_PERSONAL_SIGNED_ENTITY = 'ECDSA_SIGNED_ENTITY',
  ECDSA_EIP_1654_EPHEMERAL = 'ECDSA_EIP_1654_EPHEMERAL',
  ECDSA_EIP_1654_SIGNED_ENTITY = 'ECDSA_EIP_1654_SIGNED_ENTITY'
}

/**
 * @public
 */
export type AuthIdentity = {
  ephemeralIdentity: IdentityType
  expiration: Date
  authChain: AuthChain
}
