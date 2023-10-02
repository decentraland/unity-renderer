import {Avatar} from "@dcl/schemas";
import {recoverAddressFromEthSignature} from "@dcl/crypto/dist/crypto";
import {sha3} from "eth-connect";

export function isImpostor(avatar: Avatar, profileHash: string, profileSignedHash: string, signer: string | undefined): boolean {
  let checksum = getProfileChecksum(avatar);
  return checksum != profileHash/* || getSigner(profileHash, profileSignedHash) != signer*/
}

export function getProfileChecksum(avatar: Avatar): string {
  const payload = JSON.stringify([avatar.name, avatar.hasClaimedName, ...avatar.avatar.wearables])
  return sha3(payload);
}

function getSigner(hash: string, signedHash: string): string {
  return recoverAddressFromEthSignature(signedHash, hash)
}
