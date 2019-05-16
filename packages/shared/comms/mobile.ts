import { DEBUG_MOBILE } from 'config'

export function isMobile() {
  return /Mobi/i.test(navigator.userAgent) || /Android/i.test(navigator.userAgent) || DEBUG_MOBILE
}
