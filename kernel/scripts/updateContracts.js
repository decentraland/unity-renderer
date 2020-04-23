const fetch = require('node-fetch')
const fs = require('fs')

fetch('https://contracts.decentraland.org/addresses.json').then((res, error) => {
  if (error) {
    console.error(error)
    process.exit(-1)
  }
  res.json().then((json, parseError) => {
    if (error) {
      console.error(error)
      process.exit(-1)
    }
    fs.writeFileSync(
      'packages/config/contracts.ts',
      'export const contracts = ' + JSON.stringify(json, null, 2).replace(/"/g, `'`) + '\n'
    )
    process.exit(0)
  })
}).catch(error => {
  console.error(error)
  preocess.exit(-1)
})
