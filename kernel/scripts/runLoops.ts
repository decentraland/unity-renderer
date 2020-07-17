const puppeteer = require('puppeteer')

const position = process.env.BOT_POSITION ? process.env.BOT_POSITION : '20,20'
const host = process.env.HOST ? process.env.HOST : 'https://explorer.decentraland.zone'
const env = process.env.EXPLORER_ENV ? process.env : 'zone'

async function main() {
  const browser = await puppeteer.launch({
    args: ['--no-sandbox']
  })
  const page = await browser.newPage()

  page.on('console', (msg) => {
    for (let i = 0; i < msg.args().length; ++i) {
      console.info(`${i}: ${msg.args()[i]}`)
    }
  })

  await page.goto(`${host}/?position=${position}&ws=ws%3A%2F%2Flocalhost%3A5001%2Floop&NO_TUTORIAL&ENV=${env}`, {
    waitUntil: 'networkidle2'
  })

  await page.click('#agree-check')
  await page.click('#eth-login-confirm-button')

  while (true) {
    // await page.screenshot({ path: `example-${i++}.png` })

    await page.waitFor(30 * 1000)
  }
}

main().catch(console.error)
