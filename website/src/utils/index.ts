import { Kernel } from "../components/types";

const kernel = (window as Kernel).webApp;

export function filterInvalidNameCharacters(name: string) {
  return kernel.utils.filterInvalidNameCharacters(name);
}

export function isBadWord(name: string) {
  return kernel.utils.isBadWord(name);
}

export enum AuthType {
  GUEST = "guest",
  INJECTED = "injected",
  FORTMATIC = "fortmatic",
}
