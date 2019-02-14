import { saveScreenshot, enableVisualTests, wait } from '../testHelpers'
import { PaymentEntity, PaymentAttributes } from 'dcl/entities/payment/PaymentEntity'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { gridToWorld } from 'atomicHelpers/parcelScenePositions'

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
    gridToWorld(40, 0.2, ui.position)
    ui.position.y = 1.5
    ui.showApprovalUI()
  })

  wait(1000)

  saveScreenshot(`cc-payment-ui-approval.png`, { from: [40, 1.5, 0], lookAt: [40, 1.5, 0.2] })

  it('creates a UI for buying a CC', () => {
    ui.dispose()

    ui = new PaymentEntity('http://localhost:8080/', context)
    ui.setAttributes(options)
    gridToWorld(42, 0.2, ui.position)
    ui.position.y = 1.5
    ui.showPaymentUI()
  })

  wait(1000)

  saveScreenshot(`cc-payment-ui-buy.png`, { from: [42, 1.5, 0], lookAt: [42, 1.5, 0.2] })

  it('creates a UI for processing payment', () => {
    ui.dispose()
    ui = new PaymentEntity('http://localhost:8080/', context)
    ui.setAttributes({ ...options, processedTx })
    gridToWorld(44, 0.2, ui.position)
    ui.position.y = 1.5
    ui.showPendingTxUI(processedTx)
  })

  wait(1000)

  saveScreenshot(`cc-payment-ui-processing.png`, { from: [44, 1.5, 0], lookAt: [44, 1.5, 0.2] })

  it('creates a UI for successful payment', () => {
    ui.dispose()
    ui = new PaymentEntity('http://localhost:8080/', context)
    ui.setAttributes({ ...options, processedTx })
    gridToWorld(46, 0.2, ui.position)
    ui.position.y = 1.5
    ui.showPaymentSuccessUI(processedTx)
  })

  wait(1000)

  saveScreenshot(`cc-payment-ui-payment-success.png`, { from: [46, 1.5, 0], lookAt: [46, 1.5, 0.2] })

  it('creates a UI for failed payment', () => {
    ui.dispose()
    ui = new PaymentEntity('http://localhost:8080/', context)
    ui.setAttributes({ ...options, processedTx })
    gridToWorld(48, 0.2, ui.position)
    ui.position.y = 1.5
    ui.showPaymentFailedUI(processedTx)
  })

  wait(1000)

  saveScreenshot(`cc-payment-ui-payment-failed.png`, { from: [48, 1.5, 0], lookAt: [48, 1.5, 0.2] })

  it('disposes everything', () => {
    ui.dispose()
    context.dispose()
  })
})
