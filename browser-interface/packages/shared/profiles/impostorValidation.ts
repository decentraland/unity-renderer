import {Avatar} from "@dcl/schemas";
import {sha3} from "eth-connect";
import { ecdsaVerify } from 'ethereum-cryptography/secp256k1-compat'

export function isImpostor(avatar: Avatar, profileHash: string, profileSignedHash: string, signerPublicKey: string): boolean {
  let checksum = getProfileChecksum(avatar);
  return checksum !== profileHash || !verifySignature(profileHash, profileSignedHash, signerPublicKey)
}

export function getProfileChecksum(avatar: Avatar): string {
  const payload = JSON.stringify([avatar.name, avatar.hasClaimedName, ...avatar.avatar.wearables])
  return sha3(payload);
}

function verifySignature(hash: string, signedHash: string, publicKey: string): boolean {
  return ecdsaVerify(Buffer.from(signedHash, 'hex'),
    Buffer.from(hash, 'hex'),
    Buffer.from(publicKey, 'hex'))
}
