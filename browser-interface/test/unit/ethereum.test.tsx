import { expect } from 'chai'
import { convertMessageToObject, messageToString } from 'lib/web3/EthereumService'
import { toHex } from 'eth-connect'

describe('EthereumService tests', function() {
  it('calling convertMessageToObject()', async () => {
    const message = `# DCL Signed message
Attacker: 4
Defender: 387
Timestamp: 1531404397`

    const ret = await convertMessageToObject(message)
    expect(ret).to.be.an('object')
    expect(ret['Defender']).to.equal('387')
  })

  it('calling messageToString()', async () => {
    const dict = { ['First']: 'one', ['Second']: 'another one', Timestamp: 'DDMMYYYY' }

    const ret = await messageToString(dict)
    expect(ret).to.be.a('string')
    expect(ret).to.equal(`# DCL Signed message
First: one
Second: another one
Timestamp: DDMMYYYY`)
  })

  it('checks the hex of message', async () => {
    const message = `# DCL Signed message
Attacker: 4
Defender: 387
Timestamp: 1531404397`
    const hexEncodedMessage =
      '232044434c205369676e6564206d6573736167650a41747461636b65723a20340a446566656e6465723a203338370a54696d657374616d703a2031353331343034333937'

    const ret = toHex(message)
    expect(ret).to.be.a('string')
    expect(ret).to.equal(hexEncodedMessage)
  })
})
