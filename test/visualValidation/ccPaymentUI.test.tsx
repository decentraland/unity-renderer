import { saveScreenshot, enableVisualTests, wait } from '../testHelpers'
import { PaymentEntity, PaymentAttributes } from 'dcl/entities/payment/PaymentEntity'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'

enableVisualTests(`Collectible's payment UI validation`, function(root) {
  const options: Partial<PaymentAttributes> = {
    name: 'TestCC',
    amount: '10',
    currency: 'MANA',
    allowed: '1',
    marketplaceOrderId: '0x0000000000000',
    balance: '1,000'
  }

  const processedTx = '0x0000000000000'

  let context: SharedSceneContext

  let ui: PaymentEntity

  it('creates a UI for marketplace approval', () => {
    context = new SharedSceneContext('http://localhost:8080/', 'payment-entity-test', false)

    const notAllowed = '0'
    ui = new PaymentEntity('http://localhost:8080/', context)
    ui.setAttributes({ ...options, allowed: notAllowed })
    ui.position.set(400, 1.5, 2)
    ui.showApprovalUI()
  })

  wait(1000)

  saveScreenshot(`cc-payment-ui-approval.png`, { from: [400, 1.5, 0], lookAt: [400, 1.5, 2] })

  it('creates a UI for buying a CC', () => {
    ui.dispose()

    ui = new PaymentEntity('http://localhost:8080/', context)
    ui.setAttributes(options)
    ui.position.set(420, 1.5, 2)
    ui.showPaymentUI()
  })

  wait(1000)

  saveScreenshot(`cc-payment-ui-buy.png`, { from: [420, 1.5, 0], lookAt: [420, 1.5, 2] })

  it('creates a UI for processing payment', () => {
    ui.dispose()
    ui = new PaymentEntity('http://localhost:8080/', context)
    ui.setAttributes({ ...options, processedTx })
    ui.position.set(440, 1.5, 2)
    ui.showPendingTxUI(processedTx)
  })

  wait(1000)

  saveScreenshot(`cc-payment-ui-processing.png`, { from: [440, 1.5, 0], lookAt: [440, 1.5, 2] })

  it('creates a UI for successful payment', () => {
    ui.dispose()
    ui = new PaymentEntity('http://localhost:8080/', context)
    ui.setAttributes({ ...options, processedTx })
    ui.position.set(460, 1.5, 2)
    ui.showPaymentSuccessUI(processedTx)
  })

  wait(1000)

  saveScreenshot(`cc-payment-ui-payment-success.png`, { from: [460, 1.5, 0], lookAt: [460, 1.5, 2] })

  it('creates a UI for failed payment', () => {
    ui.dispose()
    ui = new PaymentEntity('http://localhost:8080/', context)
    ui.setAttributes({ ...options, processedTx })
    ui.position.set(480, 1.5, 2)
    ui.showPaymentFailedUI(processedTx)
  })

  wait(1000)

  saveScreenshot(`cc-payment-ui-payment-failed.png`, { from: [480, 1.5, 0], lookAt: [480, 1.5, 2] })

  it('disposes everything', () => {
    ui.dispose()
    context.dispose()
  })
})
