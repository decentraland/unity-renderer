import {Avatar} from "@dcl/schemas";
import {sha3} from "eth-connect";
import { ecdsaRecover } from 'ethereum-cryptography/secp256k1-compat'

export function isImpostor(avatar: Avatar, profileHash: string, profileSignedHash: string, signer: string | undefined): boolean {
  let checksum = getProfileChecksum(avatar);
  const recoveredSigner = getSigner(profileHash, profileSignedHash);
  return checksum != profileHash || recoveredSigner != signer
}

export function getProfileChecksum(avatar: Avatar): string {
  const payload = JSON.stringify([avatar.name, avatar.hasClaimedName, ...avatar.avatar.wearables])
  return sha3(payload);
}

function getSigner(hash: string, signedHash: string): string {
  const encoder = new TextEncoder();
  const decoder = new TextDecoder("utf-8");
  const signature = encoder.encode(signedHash);
  return decoder.decode(ecdsaRecover(signature, signature[64] === 0x1c ? 1 : 0, encoder.encode(hash)))
}
