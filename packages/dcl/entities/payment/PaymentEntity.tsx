import * as BABYLON from 'babylonjs'
import blockies = require('ethereum-blockies')
import { scene } from 'engine/renderer'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { isRunningTest, networkConfigurations, ETHEREUM_NETWORK } from 'config'
import { parseHorizontalAlignment, parseVerticalAlignment } from 'engine/entities/utils/parseAttrs'

const avatarColors = ['red', 'orange', 'yellow', 'green', 'blue']

export interface IAvatarProps {
  seed?: string
  color: string
  bgcolor?: string
  size?: number
  scale?: number
  spotcolor?: string | number
}

export type PaymentAttributes = {
  processedTx: string
  name: string
  amount: string
  currency: string
  allowed: string
  marketplaceOrderId: string
  balance: string
  marketplaceNameSpace: string
  erc20Contract: string
}

export class PaymentEntity extends BaseEntity {
  attrs: PaymentAttributes = {
    name: 'CryptoCollectible',
    erc20Contract: '0x0000000000000000',
    amount: '0.0',
    currency: 'MANA',
    marketplaceOrderId: '0x0000000000000000',
    marketplaceNameSpace: 'Collectible',
    allowed: '0',
    balance: '0',
    processedTx: ''
  }
  ui: any = {}
  private mesh: BABYLON.Mesh
  private uiTexture: BABYLON.GUI.AdvancedDynamicTexture

  // x-payment-view
  constructor(uuid: string, context) {
    super(uuid, context)

    this.initUI()
  }

  setAttributes(data: Partial<PaymentAttributes>): any {
    this.attrs = { ...this.attrs, ...data }
  }

  changeItemsVisibility(val: boolean) {
    for (let key in this.ui) {
      this.ui[key].isVisible = val
    }
    // leave BG
    this.ui.bg.isVisible = true
    this.ui.name.isVisible = true
  }

  showPaymentUI() {
    this.changeItemsVisibility(false)

    // Show payment stuff
    this.ui.avatar.isVisible = true
    this.ui.orderId.isVisible = true

    this.ui.youHave.isVisible = true
    this.ui.balance.isVisible = true
    this.ui.aboutToPay.isVisible = true
    this.ui.amount.isVisible = true

    this.ui.payButton.isVisible = true
    this.ui.cancelButton.isVisible = true
  }

  showApprovalUI() {
    this.changeItemsVisibility(false)

    this.ui.avatar.isVisible = true
    this.ui.orderId.isVisible = true

    // Show approval stuff
    this.ui.approveLabel1.isVisible = true
    this.ui.approveLabel2.isVisible = true
    this.ui.approveLabel3.isVisible = true
    this.ui.approveLabel4.isVisible = true
    this.ui.approveLabel5.isVisible = true

    this.ui.approveButton.isVisible = true
    this.ui.cancelButton.isVisible = true
  }

  showPaymentSuccessUI(txHash: string) {
    this.changeItemsVisibility(false)

    // Make status info visible
    this.ui.paymentSuccessImg.isVisible = true
    this.ui.paymentSuccessMsg.isVisible = true
    this.ui.closeButton.isVisible = true

    this.handleCheckTx(txHash)
  }

  showPaymentFailedUI(txHash: string) {
    this.changeItemsVisibility(false)

    // Make status info visible
    this.ui.paymentFailedImg.isVisible = true
    this.ui.paymentFailedMsg.isVisible = true
    this.ui.closeButton.isVisible = true

    this.handleCheckTx(txHash)
  }

  showApprovalSuccessUI(txHash: string) {
    this.changeItemsVisibility(false)

    // Make status info visible
    this.ui.paymentSuccessImg.isVisible = true
    this.ui.paymentSuccessMsg.isVisible = true
    this.ui.closeButton.isVisible = true

    this.handleCheckTx(txHash)
  }

  showApprovalFailedUI(txHash: string) {
    this.changeItemsVisibility(false)

    // Make status info visible
    this.ui.paymentFailedImg.isVisible = true
    this.ui.paymentFailedMsg.isVisible = true
    this.ui.closeButton.isVisible = true

    this.handleCheckTx(txHash)
  }

  showPendingTxUI(txHash: string) {
    this.changeItemsVisibility(false)

    // Make status info visible
    this.ui.paymentLoadingImg.isVisible = true
    this.ui.paymentProcessing.isVisible = true
    this.ui.closeButton.isVisible = true

    this.handleCheckTx(txHash)
  }

  dispose() {
    if (this.ui) {
      // TODO(michal): please create an array of IDisposable things and dispose every item
      // always
      for (let key in this.ui) {
        if (this.ui[key] && 'dispose' in this.ui[key]) {
          this.ui[key].dispose()
        }
      }
      this.ui = null
    }

    if (this.mesh) {
      this.mesh.dispose(false, true)
      this.mesh = null
    }

    if (this.uiTexture) {
      this.uiTexture.executeOnAllControls(control => {
        this.uiTexture.removeControl(control)
        control.dispose()
      })
      this.uiTexture.dispose()
      this.uiTexture = null
    }

    this.removeObject3D('payment_entity')

    super.dispose()
  }

  private handleCheckTx(txHash: string) {
    let url = `${networkConfigurations[ETHEREUM_NETWORK.MAINNET].etherscan}/tx/${txHash}`

    this.ui.checkTx = this.addButton('check-tx-button', 'CHECK TX', 'primary', 35, 'right', 'bottom', -80, -200)
    this.ui.checkTx.onPointerUpObservable.add(() => {
      window.open(url)
    })
  }

  private addText(
    value: string,
    size: number = 60,
    horizontalAlign: string = 'center',
    verticalAlign: string = 'center',
    fromTop: number = 0,
    fromLeft: number = 0,
    visible: boolean = true
  ) {
    const textBlock = new BABYLON.GUI.TextBlock()

    textBlock.color = '#fff'
    textBlock.fontFamily = 'Helvetica'
    textBlock.fontSize = size
    textBlock.text = value
    textBlock.textHorizontalAlignment = parseHorizontalAlignment(horizontalAlign)
    textBlock.textVerticalAlignment = parseVerticalAlignment(verticalAlign)
    textBlock.top = fromTop
    textBlock.left = fromLeft
    textBlock.textWrapping = true
    textBlock.isVisible = visible

    this.uiTexture.addControl(textBlock)
    return textBlock
  }

  private addImage(
    imageName: string,
    src: string,
    width: number = 1,
    height: number = 1,
    horizontalAlign: string = 'center',
    verticalAlign: string = 'center',
    fromTop: number = 0,
    fromLeft: number = 0,
    visible: boolean = true
  ) {
    const url = this.context.resolveUrl(src)
    const image = new BABYLON.GUI.Image(imageName, url)
    image.width = width
    image.height = height
    image.horizontalAlignment = parseHorizontalAlignment(horizontalAlign)
    image.verticalAlignment = parseVerticalAlignment(verticalAlign)
    image.top = fromTop
    image.left = fromLeft
    this.uiTexture.addControl(image)
    return image
  }

  private addButton(
    name: string,
    value: string,
    style: string,
    fontSize: number,
    horizontalAlign: string = 'center',
    verticalAlign: string = 'center',
    fromTop: number = 0,
    fromLeft: number = 0,
    visible: boolean = true
  ) {
    const button = new BABYLON.GUI.Button(name)
    button.adaptWidthToChildren = true
    button.adaptHeightToChildren = true
    button.thickness = 0
    button.horizontalAlignment = parseHorizontalAlignment(horizontalAlign)
    button.verticalAlignment = parseVerticalAlignment(verticalAlign)
    button.top = fromTop
    button.left = fromLeft
    button.isVisible = visible

    const wrapper = new BABYLON.GUI.Rectangle()
    wrapper.cornerRadius = 10
    wrapper.adaptWidthToChildren = true
    wrapper.adaptHeightToChildren = true

    const textBlock = new BABYLON.GUI.TextBlock()
    textBlock.text = value
    textBlock.color = 'white'
    textBlock.width = '270px'
    textBlock.height = '80px'
    textBlock.fontSize = fontSize

    if (style === 'primary') {
      wrapper.color = '#ff4f4f'
      wrapper.thickness = 2
      wrapper.background = '#ff4f4f'
      textBlock.color = 'white'
    } else {
      wrapper.color = 'white'
      wrapper.thickness = 2
      textBlock.color = '#ff4f4f'
    }

    wrapper.addControl(textBlock)
    button.addControl(wrapper)
    this.uiTexture.addControl(button)

    return button
  }

  private addAvatar(visible: boolean = true) {
    let options: IAvatarProps = { color: avatarColors[Math.floor(Math.random() * avatarColors.length)] }
    if (isRunningTest) {
      options = { ...options, seed: 'testseed', color: 'red', bgcolor: '#000', spotcolor: -1 }
    }
    const icon = blockies.create(options)

    const avatarImage = icon.toDataURL()
    const avatar = new BABYLON.GUI.Image('avatar', avatarImage)
    avatar.width = 0.15
    avatar.height = 0.15
    avatar.verticalAlignment = BABYLON.GUI.Control.VERTICAL_ALIGNMENT_TOP
    avatar.top = 200
    this.uiTexture.addControl(avatar)

    return avatar
  }

  private initUI() {
    // --- MAIN UI STUFF
    this.mesh = BABYLON.MeshBuilder.CreatePlane(
      'payment plane',
      { width: 1.6, height: 1.6, sideOrientation: BABYLON.Mesh.FRONTSIDE },
      scene
    )

    this.uiTexture = BABYLON.GUI.AdvancedDynamicTexture.CreateForMesh(this.mesh, 1024, 1024, false)
    this.uiTexture.name = 'PaymentUITexture'

    this.ui.bg = new BABYLON.GUI.Rectangle()
    this.ui.bg.width = 0.75
    this.ui.bg.cornerRadius = 60
    this.ui.bg.color = '#00002f'
    this.ui.bg.thickness = 4
    this.ui.bg.background = '#00002f'
    this.ui.bg.alpha = 0.9
    this.uiTexture.addControl(this.ui.bg)

    // --- UI ELEMENTS THAT ARE ALWAYS PRESENT
    this.ui.name = this.addText(this.attrs.name, 55, 'center', 'top', 80)

    // --- UI ELEMENTS WITH CHANGING VISIBILITY
    this.ui.avatar = this.addAvatar()
    // Generate UI parts
    this.generateApprovalUI()
    this.generatePaymentUI()
    this.generateButtons()
    this.generateTxStates()

    // Show appropriate part
    if (this.attrs.allowed === '0') {
      this.showApprovalUI()
    } else {
      this.showPaymentUI()
    }
    // If the payment is processing, show "pending tx" UI
    if (this.attrs.processedTx !== '') {
      this.showPendingTxUI(this.attrs.processedTx)
    }

    this.setObject3D('payment_entity', this.mesh)
  }

  private generatePaymentUI() {
    const orderId = this.attrs.marketplaceOrderId
    const orderIdShortened = `${orderId.slice(0, 6)}..${orderId.slice(orderId.length - 4, orderId.length)}`
    this.ui.orderId = this.addText(orderIdShortened, 35, 'center', 'center', -90)

    this.ui.youHave = this.addText('You have:', 50, 'center', 'center', 20)
    this.ui.balance = this.addText(`${this.attrs.balance} ${this.attrs.currency}`, 35, 'center', 'center', 70)

    this.ui.aboutToPay = this.addText('You are about to pay:', 40, 'center', 'center', 150)

    this.ui.amount = new BABYLON.GUI.Rectangle()
    this.ui.amount.cornerRadius = 10
    this.ui.amount.color = '#ff4f4f'
    this.ui.amount.thickness = 2
    this.ui.amount.top = 80
    this.ui.amount.adaptWidthToChildren = true
    this.ui.amount.adaptHeightToChildren = true
    this.uiTexture.addControl(this.ui.amount)

    const amountValue = new BABYLON.GUI.TextBlock()
    this.ui.amount.top = 250
    amountValue.fontFamily = 'Helvetica'
    amountValue.fontSize = 50
    amountValue.color = '#fff'
    amountValue.text = `${this.attrs.amount} ${this.attrs.currency}`
    amountValue.width = '450px'
    amountValue.height = '100px'
    this.ui.amount.addControl(amountValue)
  }

  private generateApprovalUI() {
    this.ui.approveLabel1 = this.addText(`It seems you have't authorized`, 35, 'center', 'center', 0, 0, false)
    this.ui.approveLabel2 = this.addText(`${this.attrs.currency} usage.`, 35, 'center', 'center', 50, 0, false)
    this.ui.approveLabel3 = this.addText('Authorize:', 50, 'center', 'center', 120, 0, false)
    this.ui.approveLabel4 = this.addText(`${this.attrs.marketplaceNameSpace}`, 40, 'center', 'center', 180, 0, false)
    this.ui.approveLabel5 = this.addText(
      `to operate ${this.attrs.currency} on your behalf.`,
      40,
      'center',
      'center',
      250,
      0,
      false
    )
  }

  // TODO: ECS events

  private generateButtons() {
    this.ui.payButton = this.addButton('pay-button', 'PAY', 'primary', 35, 'right', 'bottom', -80, -200)
    this.ui.payButton.onPointerUpObservable.add(() => {
      // 1 this.dispatchEvent(new EntityEvent('confirmPayment', { detail: null, bubbles: true, target: this }))
    })

    this.ui.approveButton = this.addButton('approve-button', 'APPROVE', 'primary', 35, 'right', 'bottom', -80, -200)
    this.ui.approveButton.onPointerUpObservable.add(() => {
      // this.dispatchEvent(
      //   new EntityEvent('approveERC20Token', {
      //     detail: null,
      //     bubbles: true,
      //   1  target: this
      //   })
      // )
    })

    this.ui.cancelButton = this.addButton('cancel-button', 'CANCEL', 'secondary', 35, 'left', 'bottom', -80, 200)
    this.ui.cancelButton.onPointerUpObservable.add(() => {
      this.dispose()
      // 1this.dispatchEvent(new EntityEvent('removePayment', { detail: null, bubbles: true, target: this }))
    })

    this.ui.closeButton = this.addButton('close-button', 'CLOSE', 'secondary', 35, 'left', 'bottom', -80, 200)
    this.ui.closeButton.onPointerUpObservable.add(() => {
      this.dispose()
      // 1this.dispatchEvent(new EntityEvent('removePayment', { detail: null, bubbles: true, target: this }))
    })
  }

  private generateTxStates() {
    this.ui.paymentLoadingImg = this.addImage(
      'payment-loading',
      '/images/payment-loading.png',
      0.2,
      0.2,
      'center',
      'center',
      -70,
      0,
      false
    )
    this.ui.paymentProcessing = this.addText('Processing...', 40, 'center', 'center', 120, 0, false)
    this.ui.paymentSuccessImg = this.addImage(
      'success-tx',
      '/images/success-tx.png',
      0.2,
      0.2,
      'center',
      'center',
      -70,
      0,
      false
    )
    this.ui.paymentSuccessMsg = this.addText('Payment success!', 40, 'center', 'center', 120, 0, false)
    this.ui.paymentFailedImg = this.addImage(
      'failed-tx',
      '/images/failed-tx.png',
      0.2,
      0.2,
      'center',
      'center',
      -70,
      0,
      false
    )
    this.ui.paymentFailedMsg = this.addText('Payment failed!', 40, 'center', 'center', 120, 0, false)
    this.ui.txHash = this.addText('', 40, 'center', 'center', -250, 0, false)
  }
}
