import { loadingTips } from './loading/types'
import { future, IFuture } from 'fp-future'
import { authenticate, updateTOS } from './session/actions'
import { StoreContainer } from './store/rootTypes'
import { LoadingState } from './loading/reducer'
import { ENABLE_WEB3, PREVIEW } from '../config'

declare const globalThis: StoreContainer
const isReact = !!(window as any).reactVersion
const loadingImagesCache: Record<string, IFuture<string>> = {}

export default class Html {
  static showEthLogin() {
    if (isReact) return
    const element = document.getElementById('eth-login')
    if (element) {
      element.style.display = 'block'
    }
  }

  static hideEthLogin() {
    if (isReact) return
    const element = document.getElementById('eth-login')
    if (element) {
      element.style.display = 'none'
    }
  }

  static hideProgressBar() {
    if (isReact) return
    const progressBar = document.getElementById('progress-bar')
    progressBar!.setAttribute('style', 'display: none !important')
  }

  static showErrorModal(targetError: string) {
    if (isReact) return
    document.getElementById('error-' + targetError)!.setAttribute('style', 'display: block !important')
  }

  static hideLoadingTips() {
    if (isReact) return
    const messages = document.getElementById('load-messages')
    const images = document.getElementById('load-images') as HTMLImageElement | null

    if (messages) {
      messages.style.cssText = 'display: none;'
    }
    if (images) {
      images.style.cssText = 'display: none;'
    }
  }

  static cleanSubTextInScreen() {
    if (isReact) return
    const subMessages = document.getElementById('subtext-messages')
    if (subMessages) {
      subMessages.innerText = 'Loading scenes...'
    }
    const progressBar = document.getElementById('progress-bar-inner')
    if (progressBar) {
      progressBar.style.cssText = `width: 0%`
    }
  }

  static async updateTextInScreen(status: LoadingState) {
    if (isReact) return
    const messages = document.getElementById('load-messages')
    const images = document.getElementById('load-images') as HTMLImageElement | null
    if (messages && images) {
      const loadingTip = loadingTips[status.helpText]
      if (messages.innerText !== loadingTip.text) {
        messages.innerText = loadingTip.text
      }

      if (!loadingImagesCache[loadingTip.image]) {
        const promise = (loadingImagesCache[loadingTip.image] = future())
        const response = await fetch(loadingTip.image)
        const blob = await response.blob()
        const url = URL.createObjectURL(blob)
        promise.resolve(url)
      }

      const url = await loadingImagesCache[loadingTip.image]
      if (url !== images.src) {
        images.src = url
      }
    }
    const subMessages = document.getElementById('subtext-messages')
    const progressBar = document.getElementById('progress-bar-inner')
    if (subMessages && progressBar) {
      const newMessage = status.pendingScenes > 0 ? status.message || 'Loading scenes...' : status.status
      if (newMessage !== subMessages.innerText) {
        subMessages.innerText = newMessage
      }
      const actualPercentage = Math.floor(
        Math.min(status.initialLoad ? (status.loadPercentage + status.subsystemsLoad) / 2 : status.loadPercentage, 100)
      )
      const newCss = `width: ${actualPercentage}%`
      if (newCss !== progressBar.style.cssText) {
        progressBar.style.cssText = newCss
      }
    }
  }

  static initializeTos(checked: boolean) {
    if (isReact) return
    const agreeCheck = document.getElementById('agree-check') as HTMLInputElement | undefined
    if (agreeCheck) {
      agreeCheck.checked = checked
      // @ts-ignore
      agreeCheck.onchange && agreeCheck.onchange()

      const originalOnChange = agreeCheck.onchange
      agreeCheck.onchange = (e) => {
        globalThis.globalStore && globalThis.globalStore.dispatch(updateTOS(agreeCheck.checked))
        // @ts-ignore
        originalOnChange && originalOnChange(e)
      }
    }
    Html.enableLogin()
  }

  static enableLogin() {
    if (isReact) return
    const wrapper = document.getElementById('eth-login-confirmation-wrapper')
    const spinner = document.getElementById('eth-login-confirmation-spinner')
    if (wrapper && spinner) {
      spinner.style.cssText = 'display: none;'
      wrapper.style.cssText = 'display: flex;'
    }
  }

  static bindLoginEvent() {
    if (isReact) return
    const button = document.getElementById('eth-login-confirm-button')
    button!.onclick = () => {
      globalThis.globalStore && globalThis.globalStore.dispatch(authenticate('Metamask'))
    }
  }

  static updateTLDInfo(tld: string, web3Net: string, tldNet: string) {
    if (isReact) return
    document.getElementById('tld')!.textContent = tld
    document.getElementById('web3Net')!.textContent = web3Net
    document.getElementById('web3NetGoal')!.textContent = tldNet
  }

  static showAwaitingSignaturePrompt(show: boolean) {
    if (isReact) return
    showElementById('check-wallet-prompt', show)
  }

  static showEthSignAdvice(show: boolean) {
    if (isReact) return
    showElementById('eth-sign-advice', show)
  }

  static showNetworkWarning() {
    if (isReact || (PREVIEW && !ENABLE_WEB3)) return
    const element = document.getElementById('network-warning')
    if (element) {
      element.style.display = 'block'
    }
  }

  static setLoadingScreen(shouldShow: boolean) {
    if (isReact) return
    document.getElementById('overlay')!.style.display = shouldShow ? 'block' : 'none'
    document.getElementById('load-messages-wrapper')!.style.display = shouldShow ? 'flex' : 'none'
    document.getElementById('progress-bar')!.style.display = shouldShow ? 'block' : 'none'
    const loadingAudio = document.getElementById('loading-audio') as HTMLAudioElement

    if (shouldShow) {
      loadingAudio &&
        loadingAudio.play().catch((e) => {
          /*Ignored. If this fails is not critical*/
        })
    } else {
      loadingAudio && loadingAudio.pause()
    }
  }

  static loopbackAudioElement() {
    return document.getElementById('voice-chat-audio') as HTMLAudioElement | undefined
  }

  static switchGameContainer(shouldShow: boolean) {
    showElementById('gameContainer', shouldShow, true)
  }

  static showTeleportAnimation() {
    document
      .getElementById('gameContainer')!
      .setAttribute(
        'style',
        'background: #151419 url(images/teleport.gif) no-repeat center !important; background-size: 194px 257px !important;'
      )
    document.body.setAttribute(
      'style',
      'background: #151419 url(images/teleport.gif) no-repeat center !important; background-size: 194px 257px !important;'
    )
  }

  static hideTeleportAnimation() {
    document.getElementById('gameContainer')!.setAttribute('style', 'background: #151419')
    document.body.setAttribute('style', 'background: #151419')
  }
}

function showElementById(id: string, show: boolean, force: boolean = false) {
  if (isReact && !force) return
  const element = document.getElementById(id)
  if (element) {
    element.style.display = show ? 'block' : 'none'
  }
}
