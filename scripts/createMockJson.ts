// tslint:disable:no-console
// this file uses console.log because it is a helper

import glob = require('glob')
import path = require('path')
import multihashes = require('multihashes')
import fs = require('fs-extra')
import keccak = require('keccakjs')

const ipfsMock = path.resolve(__dirname, '../public/local-ipfs/mappings')
const ipfsMockContent = path.resolve(__dirname, '../public/local-ipfs/contents/')
console.log(`> generating IPFS mock at: ${ipfsMock}`)

const IPFSMock = []

glob
  .sync(path.resolve(__dirname, '../public/test-parcels/*/scene.json'), { absolute: true })
  .concat(glob.sync(path.resolve(__dirname, '../public/ecs-parcels/*/scene.json'), { absolute: true }))
  .forEach(file => {
    const manifest = require(file)
    const dirName = path.dirname(file)
    const rootCid = hash(dirName.replace(process.cwd(), ''))
    const mappings = generateIPFSMock(path.dirname(file))

    for (let p in manifest.scene.parcels) {
      const position = manifest.scene.parcels[p]

      IPFSMock.push({
        publisher:
          (manifest._blockhainMock && manifest._blockhainMock.operator) ||
          (manifest._blockhainMock && manifest._blockhainMock.owner) ||
          manifest.owner ||
          '0x0f5d2fb29fb7d3cfee444a200298f468908cc942',
        parcel_id: position,
        root_cid: rootCid,
        contents: mappings
      })
    }
  })

fs.writeFile(ipfsMock, JSON.stringify(IPFSMock, null, 2), function(err) {
  if (err) {
    console.log(err)
    process.exit(1)
  }
})

function hash(str: string) {
  const hash = new keccak()
  hash.update(str)

  const cid = 'Qm' + multihashes.toB58String(new Buffer(hash.digest('hex'), 'hex'), 'sha2-256')
  return cid
}

function generateIPFSMock(folder: string) {
  // THIS IS NOT THE WAY CIDS ARE CALCULATED, I JUST CAME UP WITH IT

  const mappings: Array<{ file: string; hash: string }> = []
  console.log(folder + '/**/*')

  const files = new Set<string>(glob.sync(folder + '/**/*', { absolute: true }))

  files.forEach(file => {
    try {
      if (!fs.statSync(file).isFile()) return
    } catch (err) {
      return
    }

    const key = file.replace(folder + '/', '')
    const cid = hash(file.replace(process.cwd(), ''))
    const targetFile = ipfsMockContent + '/' + cid

    mappings.push({ file: key.toLowerCase(), hash: cid })

    try {
      fs.ensureSymlinkSync(file, targetFile, 'file')
    } catch (e) {
      try {
        fs.removeSync(targetFile)
      } finally {
        fs.ensureSymlinkSync(file, targetFile, 'file')
      }
    }
  })

  return mappings
}
