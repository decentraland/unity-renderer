import { MOBILE_DEBUG } from 'config'

export function isMobile() {
  return /Mobi/i.test(navigator.userAgent) || /Android/i.test(navigator.userAgent) || MOBILE_DEBUG
}
