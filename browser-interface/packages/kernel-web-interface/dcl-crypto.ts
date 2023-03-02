/**
 * @public
 */
type Signature = string

/**
 * @public
 */
type IdentityType = {
  privateKey: string
  publicKey: string
  address: string
}

/**
 * @public
 */
type AuthChain = AuthLink[]

/**
 * @public
 */
type AuthLink = {
  type: AuthLinkType
  payload: string
  signature: Signature
}

/**
 * @public
 */
enum AuthLinkType {
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
